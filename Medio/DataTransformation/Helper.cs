using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using DataTransformation.Models;
using NLog;
using Oracle.DataAccess.Client;

namespace DataTransformation
{
    public static class Helper
    {
        private static Dictionary<string, string> CacheLookupValuesFile = new Dictionary<string, string>();
        //private static Dictionary<string, string> CacheLookupValuesLines = new Dictionary<string, string>();

        // {table name, {key, columns[]}}
        private static Dictionary<string, Dictionary<string, string[]>> lookupTables = new Dictionary<string, Dictionary<string, string[]>>();
        private static Dictionary<string, DateTime> lookupTableExpires = new Dictionary<string, DateTime>();
        private static Dictionary<string, string> CacheSQLValues = new Dictionary<string, string>();

        const string E_ERROR = "ERROR: ";
        const string E_WARN = "WARN: ";
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static string evaluateExpression(string name, string expression, Dictionary<string, string> Variables, string Val, string lineVal = "", string extraCode = null)
        {
            if (!string.IsNullOrEmpty(expression))
            {
                Logger.Debug(string.Format("{0}, Val={1}, Evaluate: {2}", name, Val, expression));
                if (Variables != null)
                {
                    foreach (var Var in Variables)
                    {
                        expression = expression.Replace("$" + Var.Key, Var.Value.ToLiteral());
                    }
                }
                try
                {
                    //Val = await CSharpScript.EvaluateAsync<string>(destCol.expression.Replace("colVal", $"\"{Val}\""));
                    Val = Compiler.Evaluate(expression, Val, lineVal, extraCode);
                    Logger.Debug(string.Format("{0}, Val={1}", name, Val));
                    if (Val.StartsWith(E_ERROR)) throw new Exception(Val.Substring(E_ERROR.Length));
                    if (Val.StartsWith(E_WARN))
                    {
                        Val = string.Empty;
                        Logger.Warn("Set Val to empty");
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Error when evaluating expression");
                    throw e;
                }
            }
            return Val;
        }

        public static string computeLookup(string name, string Val, string[] lines, Dictionary<string, string> Variables, PdtColumnLookup lookup, string dirName, PdtLookupTable[] tables, string inputLine, string extraCode = null)
        {
            Logger.Debug($"BEGIN(name={name}, Val={Val})");
            while (lookup != null)
            {
                if (!string.IsNullOrEmpty(lookup.Table) && lookup.Table.StartsWith("SQL")) // Lookup by SQL query from database
                {
                    var sqlQuery = evaluateExpression(name, lookup.Expression, Variables, Val, inputLine, extraCode);
                    Logger.Debug(string.Format("sqlQuery = {0}", sqlQuery));
                    if (CacheSQLValues.ContainsKey(sqlQuery))
                    {
                        Val = CacheSQLValues[sqlQuery];
                        Logger.Debug($"Return value from Cache: Val={Val}");
                    }
                    else
                    {
                        using (var command = new OracleCommand(sqlQuery, DataTransformationService.DbConnection))
                        {
                            var newVal = command.ExecuteScalar();
                            if (newVal != null)
                            {
                                Val = newVal.ToString();
                                Logger.Info("Val = ", Val);
                            }
                            else
                            {
                                //Val = string.Empty;
                                Val = newVal as string;
                                Logger.Warn("SQL query returned nothing. Set Val to null");
                            }
                            CacheSQLValues.Add(sqlQuery, Val);
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(lookup.Table)) // Lookup by Table as a mapping cache stored in memory
                {
                    if (lookupTables.ContainsKey(lookup.Table) && DateTime.Compare(lookupTableExpires[lookup.Table], DateTime.Now) < 0)
                    {
                        Logger.Debug($"Lookup Table {lookup.Table} expired. ExpiredTime = {lookupTableExpires[lookup.Table]}");
                        lookupTables.Remove(lookup.Table);
                        lookupTableExpires.Remove(lookup.Table);
                    }
                    if (!lookupTables.ContainsKey(lookup.Table))
                    {
                        var table = tables.Single(x => x.Name == lookup.Table);
                        Logger.Debug(string.Format("Building table = {0}, from file = {1}", table.Name, table.File));
                        var rows = new Dictionary<string, string[]>();
                        if (table.File == "SQL")
                        {
                            using (var command = new OracleCommand(table.keyExpression, DataTransformationService.DbConnection))
                            {
                                using (OracleDataReader reader = command.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        var rowKey = reader.GetString(0);
                                        string[] rowCols = new string[reader.FieldCount - 1];
                                        for (int i = 1; i < reader.FieldCount; i++)
                                        {
                                            rowCols[i] = reader.GetString(i);
                                        }
                                        rows[rowKey] = rowCols;
                                    }
                                }
                            }
                        }
                        else
                        {
                            var lookupFile = (new DirectoryInfo(dirName)).GetFiles(table.File).OrderByDescending(x => x.LastWriteTime).FirstOrDefault();
                            if (lookupFile == null)
                            {
                                Logger.Debug(string.Format("Could not find file {0} in folder {1}. Try getting from base directory...", table.File, dirName));
                                lookupFile = (new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory)).GetFiles(table.File).OrderByDescending(x => x.LastWriteTime).FirstOrDefault();
                            }
                            if (lookupFile == null)
                            {
                                Logger.Error($"Error searching files {table.File}. No file found.");
                            }
                            else
                            {
                                Logger.Debug($"lookupFile.Name = {lookupFile.Name}");
                                var lookupLines = File.ReadAllLines(lookupFile.FullName);
                                foreach (var lineVal in lookupLines)
                                {
                                    string newLineVal = lineVal;
                                    if (table.csvSeparator != '\0')
                                    {
                                        var csvVals = Regex.Matches(newLineVal, $"(?:^|{table.csvSeparator})(\"(?:[^\"]+|\"\")*\"|[^{table.csvSeparator}]*)").Cast<Match>().Select(m => m.Value.TrimStart(table.csvSeparator)).ToArray();

                                        //bool preProcessed = false;
                                        for (int j = 0; j < csvVals.Length; j++)
                                        {
                                            if (csvVals[j].StartsWith("\"") && csvVals[j].EndsWith("\""))
                                            {
                                                csvVals[j] = csvVals[j].Substring(1, csvVals[j].Length - 2);
                                                //preProcessed = true;
                                            }
                                        }
                                        //if (preProcessed)
                                        //{
                                        newLineVal = string.Join(Transformation.DEFAULT_CSV_SEPARATOR.ToString(), csvVals);
                                        Logger.Debug($"lineVal: {lineVal}");
                                        Logger.Debug($"newLineVal: {newLineVal}");
                                        //}
                                    }

                                    if (!string.IsNullOrEmpty(table.processingCondition))
                                    {
                                        var result = evaluateExpression("processingCondition", table.processingCondition, null, string.Empty, newLineVal, extraCode);
                                        Logger.Debug(string.Format("result={0}", result));
                                        if (!bool.Parse(result)) continue;
                                    }

                                    //Logger.Debug(string.Format("lineVal={0}", lineVal));
                                    var rowKey = evaluateExpression(string.Format("Table {0}.Key", table.Name), table.keyExpression, null, string.Empty, newLineVal, extraCode);
                                    if (table.columnsExpression != null)
                                    {
                                        string[] rowCols = new string[table.columnsExpression.Length];
                                        for (int i = 0; i < table.columnsExpression.Length; i++)
                                        {
                                            rowCols[i] = evaluateExpression(string.Format("Table {0}.Col.{1}", table.Name, i), table.columnsExpression[i], null, string.Empty, newLineVal, extraCode);
                                        }
                                        rows[rowKey] = rowCols;
                                    }
                                    else rows[rowKey] = null;
                                }
                            }
                        }
                        lookupTables.Add(table.Name, rows);
                        var expiredTime = DateTime.Now.AddMinutes(table.Expires == 0 ? 24 * 60 : table.Expires);
                        lookupTableExpires.Add(table.Name, expiredTime);
                        Logger.Debug($"Table {table.Name}, {rows.Count} rows. Expires at {expiredTime}");
                    }
                    var newVal = string.Empty;
                    if (lookupTables[lookup.Table].ContainsKey(Val))
                    {
                        newVal = int.Parse(lookup.ColumnIndex) < 0 ? Val : lookupTables[lookup.Table][Val][int.Parse(lookup.ColumnIndex)];
                    }
                    else
                    {
                        Logger.Warn($"Key {Val} is not found in the table {lookup.Table}.");
                    }
                    if (!string.IsNullOrEmpty(lookup.Expression))
                    {
                        Logger.Debug(string.Format("evaluate newVal={0}", newVal));
                        newVal = evaluateExpression(string.Format("newVal={0}", newVal), lookup.Expression, Variables, newVal, string.Empty, extraCode);
                    }
                    Logger.Debug(string.Format("Getting value from Table={0}, Key={1}, ColumnIndex={2}, Value={3}", lookup.Table, Val, lookup.ColumnIndex, newVal));
                    Val = newVal;
                }
                else if (string.IsNullOrEmpty(lookup.File)) // Lookup by another File served as dictionary stored in disk
                {
                    Logger.Debug(string.Format("Evaluate Lookup: {0}, colVal={1}", lookup.Expression, Val));
                    foreach (var lineVal in lines)
                    {
                        var lookupResult = evaluateExpression(name, lookup.Expression, Variables, Val, lineVal, extraCode);
                        if (!string.IsNullOrWhiteSpace(lookupResult))
                        {
                            Val = lookupResult;
                            Logger.Debug(string.Format("{0}, Val={1}", name, Val));
                            break;
                        }
                    }
                }
                else // Lookup in the same file
                {
                    var key = string.Format("File={0},colVal={1},Expression={2}", lookup.File, Val, lookup.Expression);
                    if (Variables != null)
                    {
                        foreach (var Var in Variables)
                        {
                            key = key.Replace("$" + Var.Key, Var.Value.ToLiteral());
                        }
                    }
                    if (!CacheLookupValuesFile.ContainsKey(key))
                    {
                        var fileEntries = Directory.GetFiles(dirName, lookup.File);
                        if (fileEntries.Length == 0)
                        {
                            Logger.Debug(string.Format("Could not find file {0} in folder {1}. Try getting from base directory...", lookup.File, dirName));
                            fileEntries = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, lookup.File);
                        }
                        if (fileEntries.Length == 1)
                        {
                            var lookupLines = File.ReadAllLines(fileEntries[0]);
                            Logger.Debug(string.Format("Evaluate Lookup: {0}, colVal={1}, File={2}", lookup.Expression, Val, lookup.File));
                            string lookupResult = null;
                            foreach (var lineVal in lookupLines)
                            {
                                lookupResult = evaluateExpression(name, lookup.Expression, Variables, Val, lineVal, extraCode);
                                if (!string.IsNullOrWhiteSpace(lookupResult))
                                {
                                    Val = lookupResult;
                                    Logger.Debug(string.Format("{0}, Val={1}", name, Val));
                                    break;
                                }
                            }
                            if (string.IsNullOrWhiteSpace(lookupResult))
                            {
                                Logger.Warn($"lookup not found. Take the default value.");
                            }
                            Logger.Debug(string.Format("Evaluate Lookup END: Val={0}", Val));
                        }
                        else
                        {
                            Logger.Error(string.Format("Error searching files {0}, count={1}", lookup.File, fileEntries.Length));
                        }
                        CacheLookupValuesFile[key] = Val;
                    }
                    else
                    {
                        Val = CacheLookupValuesFile[key];
                        Logger.Debug(string.Format("Return value from cache={0}, key={1}", Val, key));
                    }
                }
                lookup = lookup.Lookup;
            }
            Logger.Debug($"END(name={name}, Val={Val})");
            return Val;
        }

        public static PdtTransformationSetting LoadConfigFromFile(string configFile)
        {
            try
            {
                Logger.Info("BEGIN");
                PdtTransformationSetting Setting;
                XmlSerializer serializer = new XmlSerializer(typeof(PdtTransformationSetting));
                if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFile)))
                {
                    StreamReader reader = new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFile));
                    Setting = (PdtTransformationSetting)serializer.Deserialize(reader);
                    reader.Close();
                }
                else
                {
                    Logger.Debug("Config file does not exist. Finding in resource...");
                    var resourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames().FirstOrDefault(x => x.EndsWith("." + configFile));
                    if (resourceName == null) throw new ArgumentException(string.Format("Config file not found in resource: {0}", configFile));
                    var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
                    Setting = (PdtTransformationSetting)serializer.Deserialize(stream);
                }
                Logger.Info("END");
                return Setting;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }
        }

    }
}
