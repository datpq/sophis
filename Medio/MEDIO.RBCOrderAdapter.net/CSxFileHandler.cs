using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using sophis.log;
using System.IO;


namespace MEDIO.RBCOrderAdapter
{
    public class CSxFileHandler
    {
        private readonly string fFilePath;
        private readonly string fFileExtension;
        private readonly char fSeparator;
        private readonly bool fPadding=false;
        
        public CSxFileHandler(string filePath, string fileExtension,char separator)
        {
            using (Logger log = new Logger(this, "RBCFileManager"))
            {
                log.log(Severity.debug, "Start");

                fFilePath = filePath;
                fFileExtension = fileExtension;
                fSeparator = separator;
                //fPadding = padding;

                if (!GetDirectory(fFilePath))
                {
                    log.log(Severity.error, string.Format("Cannot Find Path [{0}]", fFilePath));
                    log.end();
                    throw new Exception(string.Format("Cannot Find Path [{0}]", fFilePath));
                }

                log.log(Severity.debug, "End");
                log.end();
            }
        }
        

        private bool GetDirectory(string path)
        {
            using (Logger log = new Logger(this, "GetDirectory"))
            {
                log.log(Severity.debug, "Start");
                bool result = true;
                try
                {
                    log.log(Severity.debug, string.Format("Checking Directory [{0}]", path));
                    DirectoryInfo info = new DirectoryInfo(path);
                    log.log(Severity.debug, string.Format("Got Directory [{0}]", info));
                    if (!info.Exists)
                    {
                        log.log(Severity.error, string.Format("Directory [{0}] Does Not Exist", path));
                        result = false;
                    }
                }
                catch (Exception ex)
                {
                    log.log(Severity.error, ex.Message);
                    result = false;
                }

                log.log(Severity.debug, "End");
                log.end();
                return result;
            }
        }

        public enum eColumnType
        {
            eString,
            eDouble,
            eInteger
        }
        public sealed class Column
        {
            public int length;
            public eColumnType type;
            public double doubleContent;
            public int intContent;
            public string stringContent;
            public int precision;

            public Column(int iLength, eColumnType iType, double iDoubleContent, int iIntContent, string iStringContent, int iPrecision)
            {
                length = iLength;
                type = iType;
                doubleContent = iDoubleContent;
                intContent = iIntContent;
                stringContent = iStringContent;
                precision = iPrecision;
            }
            public void SetDouble(int iLength, double iDoubleContent, int iPrecision)
            {
                length = iLength;
                type = eColumnType.eDouble;
                doubleContent = iDoubleContent;
                intContent = 0;
                stringContent = "";
                precision = iPrecision;
            }
            public void SetInteger(int iLength, int iIntegerContent)
            {
                length = iLength;
                type = eColumnType.eInteger;
                doubleContent = .0;
                intContent = iIntegerContent;
                stringContent = "";
                precision = 0;
            }
            public void SetString(int iLength, string iStringContent)
            {
                length = iLength;
                type = eColumnType.eString;
                doubleContent = .0;
                intContent = 0;
                stringContent = iStringContent;
                precision = 0;
            }
            public void SetDate(DateTime iDate)
            {
                length = 8;
                type = eColumnType.eString;
                doubleContent = .0;
                intContent = 0;
                if (iDate != DateTime.MinValue)
                {
                    stringContent = iDate.ToString("dd/MM/yyyy", CultureInfo.CurrentCulture);
                }
                else
                {
                    stringContent = "";
                }
                precision = 0;
            }
        }


        protected string GetFileFullPath(string path, string name, string extension)
        {
            using (Logger log = new Logger(this, "GetFileFullPath"))
            {
                log.log(Severity.debug, "Start");
                string result = "";
               
                try
                {
                    DirectoryInfo info = new DirectoryInfo(path);
                    string filename = string.Format("{0}.{1}", name, extension);
                    FileInfo[] files = info.GetFiles(filename, SearchOption.TopDirectoryOnly);
                    if (files != null && files.Length > 0)
                    {
                        // already exists
                        log.log(Severity.debug, string.Format("File [{0}.{1}] Already Exists in Directory [{2}]", name, extension, path));
                        FileInfo[] filesChanged = info.GetFiles(string.Format("{0}_*.{1}", name, extension), SearchOption.TopDirectoryOnly);
                        if (filesChanged == null || filesChanged.Length == 0)
                        {
                            // only 1 file
                            name += "_1";
                        }
                        else
                        {
                            // more than 1 file
                            string lastFileName = (from file in filesChanged
                                                   where file.Name.Contains(name)
                                                   orderby file.Name.Length descending, file.Name descending
                                                   select file.Name).First();
                            string nameIndex = lastFileName.Substring(lastFileName.LastIndexOf('_') + 1).Replace("." + extension, "");
                            int index = 1;
                            if (Int32.TryParse(nameIndex, out index))
                            {
                                index++;
                                name += string.Format("_{0}", index);
                            }
                            else
                            {
                                name += "_1";
                            }
                        }
                        log.log(Severity.debug, string.Format("File Name = [{0}]", name));
                    }
                    name = string.Format("{0}.{1}", name, extension);
                    result = Path.Combine(path, name);
                }
                catch (Exception ex)
                {
                    log.log(Severity.error, ex.Message);
                }

                log.log(Severity.debug, string.Format("File's Full Path [{0}]", result));
                log.log(Severity.debug, "End");
                log.end();
                return result;
            }
        }

        protected string GetFileNameWithDateAndTime(string name)
        {
            using (Logger log = new Logger(this, "GetFileNameWithDateAndTime"))
            {
                log.log(Severity.debug, "Start");

                string result = "";

                DateTime now = DateTime.Now;
               
                result = string.Format("{0}_{1}", name, now.ToString("yyyyMMddHHmmff"));
                log.log(Severity.debug, "End");
                log.end();

                return result;
            }
        }

        public bool WriteToFile(int orderId, List<Column> record, out string fileName)
        {
            using (Logger log = new Logger(this, "WriteToFile"))
            {
                log.log(Severity.debug, "Start");
                

                bool result = false;
                const int maxRetries = 5;
                int retry = 0;
                fileName = "";

                while (!result && retry < maxRetries)
                {
                    try
                    {
                        ////handle file name here
                        string file = "";
                        string complement;
                        string nameSect="MEDIO_TRADES_GFP";

                        if (retry != 0)
                        {
                            complement = string.Format("{0}_{1}_{2}", nameSect, orderId, retry);
                        }
                        else
                        {
                            complement = string.Format("{0}_{1}", nameSect, orderId); ;
                        }
                        string fileNameSting = GetFileNameWithDateAndTime(complement);
                        file = GetFileFullPath(fFilePath, fileNameSting,fFileExtension);
                        
                        using (StreamWriter writer = new StreamWriter(file, false))
                        {
                            bool firstColumn = true;
                            int i = 1;
                            foreach (Column column in record)
                            {
                                if (firstColumn)
                                {
                                    firstColumn = false;
                                }
                                else
                                {
                                    writer.Write(fSeparator);
                                }
                                string field = buildString(column);
                                log.log(Severity.debug, string.Format("Write Field {0} = [{1}]", i, field));
                                i++;
                                writer.Write(field);

                            }
                            log.log(Severity.debug, string.Format("Order {0} - Success in Writing File to Directory [{1}]", orderId, RBCConfigurationSectionGroup.RBCFileSection.ToRBCFolder));
                            result = true;
                            fileName = fileNameSting;
                        }
                        // Create an empty ok file
                        string okFile = String.Format("{0}.{1}", file, "ok");
                        File.Create(okFile).Dispose();
                    }
                    catch (Exception ex)
                    {
                        log.log(Severity.warning, string.Format("Order {0} - Failed to Write File to Directory [{1}]: [{2}]", orderId, fFilePath, ex.Message));
                        result = false;
                        retry++;
                    }
                }

                if (!result)
                {
                    log.log(Severity.error, string.Format("Order {0} - Failed to Write to File", orderId));
                }
                

                log.log(Severity.debug, "End");
                log.end();
                return result;
            }
        }


        private string buildString(Column column)
        {
            using (Logger log = new Logger(this, "buildString"))
            {
                string result = "";
                switch (column.type)
                {
                    case eColumnType.eDouble:
                        string formatString = String.Concat("{0:F", column.precision, "}");
                        result = string.Format(formatString, column.doubleContent);
                        break;
                    case eColumnType.eInteger:
                       // result = string.Format("0", column.intContent);
                        result = column.intContent.ToString();
                        break;
                    case eColumnType.eString:
                        if (String.IsNullOrEmpty(column.stringContent))
                        {
                            result = "";
                        }
                        else
                        {
                            result = column.stringContent;
                        }
                        break;
                    default:
                        log.log(Severity.error, "Unknown Type");
                        break;
                }

                // pad if needed
                if (fPadding)
                {
                    switch (column.type)
                    {
                        case eColumnType.eDouble:
                        case eColumnType.eInteger:
                            result.PadRight(column.length);
                            break;
                        case eColumnType.eString:
                            result.PadLeft(column.length);
                            break;
                    }
                }

                return result;
            }
        }
    }
}
