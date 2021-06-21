using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NLog;
using Oracle.ManagedDataAccess.Client;
namespace TKOPriceFeed
{
    public class PriceFeedWatcher : FileSystemWatcher
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const string NodeSophis = "sophis";
        private const string ValueDate = "valueDate";
        private const string ValueMultisource = "valueMultisource";
        private const string NumericValue = "numericValue";

        public string BackupDir { get; set; }
        public string BackupSuffix { get; set; }
        public string OutputDir { get; set; }
        public string DoneDir { get; set; }

        public string XmlTemplate { get; set; }
        public string RepeatingNode { get; set; }

        //SQL connection string

        public string ConnectionStrings { get; set; }
        public void Processing(string fileName)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Processing file: {0}", fileName);
            }

            var suffix = DateTime.Now.ToString(BackupSuffix);
            var backupFileName = System.IO.Path.Combine(BackupDir, string.Format("{0}{1}{2}",
                System.IO.Path.GetFileNameWithoutExtension(fileName), suffix, System.IO.Path.GetExtension(fileName)));
            var outputFileXml = System.IO.Path.Combine(DoneDir, string.Format("{0}{1}.xml",
                System.IO.Path.GetFileNameWithoutExtension(fileName), suffix));
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Moving file to: {0}", backupFileName);
            }
            File.Move(fileName, backupFileName);

            Transforming(backupFileName, outputFileXml);

            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Copying to {0}", OutputDir);
            }
            File.Copy(outputFileXml, System.IO.Path.Combine(OutputDir, System.IO.Path.GetFileName(outputFileXml)));
        }
        private void Transforming(string inputFileCsv, string outputFileXml)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Transforming to {0}", outputFileXml);
                }
                var xDoc = XDocument.Load(XmlTemplate);
                var repeatingNode = xDoc.DescendantNodes().OfType<XElement>()
                    .FirstOrDefault(x => x.Name.LocalName.Equals(RepeatingNode));
                if (repeatingNode == null)
                {
                    throw new Exception("Repeating node not found");
                }
                var parentNode = repeatingNode.Parent;
                if (parentNode == null)
                {
                    throw new Exception("Parent node not found");
                }
                parentNode.RemoveNodes();


                OracleConnection objConn = new OracleConnection(ConnectionStrings);
                try
                {
                    objConn.Open();
                    Logger.Info("Connection Open");

                    if (inputFileCsv.Contains("OV_ETACPA_TIKEHAU"))
                    {//CSV File format «ETACPA_TIKEHAU.csv »
                        var arrLines = File.ReadAllLines(inputFileCsv).Skip(1);//skip Header Line
                        arrLines.ToList().Select(x => x.Split(';')).ToList().ForEach(fields =>
                        {
                            var lineNode = new XElement(repeatingNode);
                            var sophisNode = lineNode.DescendantNodes().OfType<XElement>()
                                .FirstOrDefault(x => x.Name.LocalName.Equals(NodeSophis));
                            if (sophisNode == null)
                            {
                                Logger.Info("Sophis node not found");
                            }
                            else
                            {
                                string queryString = String.Format("SELECT SICOVAM FROM TITRES WHERE LIBELLE ='{0}'", fields[0]);
                                Logger.Info("Query String  {0}", queryString);
                                OracleCommand command = new OracleCommand(queryString, objConn);
                                var reference = command.ExecuteScalar();
                                if (reference != null)
                                {
                                    sophisNode.Value = reference.ToString();
                                }
                                else
                                {
                                    sophisNode.Value = "";
                                    Logger.Info("Titre not found for LIBELLE '{0}' in Data Base ", fields[0]);
                                }
                                   
                            }
                            var valueDate = lineNode.DescendantNodes().OfType<XElement>()
                                .FirstOrDefault(x => x.Name.LocalName.Equals(ValueDate));
                            if (valueDate == null)
                            {
                                Logger.Info("ValueDate node not found");
                            }
                            else
                            {
                                //format is YYYYMMDD
                                try
                                {
                                    DateTime dt = DateTime.ParseExact(fields[1], "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                                    string Year = fields[1].Substring(0, 4);
                                    string Month = fields[1].Substring(4, 2);
                                    string Day = fields[1].Substring(6, 2);
                                    valueDate.Value = Year + "-" + Month + "-" + Day;
                                }
                                catch
                                {
                                    Logger.Info("Date '{0}' not in correct format YYYYMMDD", fields[1]);
                                    // valueDate.Value = "YYYYMMDD";
                                    valueDate.Value = "";
                                }
                            }
                            var numericValue = lineNode.DescendantNodes().OfType<XElement>()
                               .FirstOrDefault(x => x.Name.LocalName.Equals(NumericValue));
                            if (numericValue == null)
                            {
                                Logger.Info("numericValue node not found");
                            }
                            else
                            {
                                var style = System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands | System.Globalization.NumberStyles.AllowDecimalPoint;

                                double outvalue;
                                if (Double.TryParse(fields[2], style, System.Globalization.CultureInfo.InvariantCulture, out outvalue))
                                {
                                    numericValue.Value = fields[2];
                                }
                                else
                                {
                                    Logger.Info("NumericValue  '{0}' not in correct format ", fields[2]);
                                    numericValue.Value = "0.0";
                                }
                            }
                            var valueMultisource = lineNode.DescendantNodes().OfType<XElement>()
                               .FirstOrDefault(x => x.Name.LocalName.Equals(ValueMultisource));
                            if (valueMultisource == null)
                            {
                                Logger.Info("valueMultisource node not found");
                            }
                            valueMultisource.Value = "LOW";
                            parentNode.Add(lineNode);
                        });
                    }
                    else
                    { //CSV File format «OV_ETACPA_TIKEHAU.csv »
                        var arrLines = File.ReadAllLines(inputFileCsv).Skip(1);//skip Header Line
                        arrLines.ToList().Select(x => x.Split(';')).ToList().ForEach(fields =>
                        {
                            var lineNode = new XElement(repeatingNode);
                            var sophisNode = lineNode.DescendantNodes().OfType<XElement>()
                                .FirstOrDefault(x => x.Name.LocalName.Equals(NodeSophis));
                            if (sophisNode == null)
                            {
                                Logger.Info("Sophis node not found");
                            }
                            else
                            {
                                string queryString = String.Format("SELECT SICOVAM FROM TITRES WHERE LIBELLE ='{0}'", fields[0]);

                                Logger.Info("Query String  {0}", queryString);
                                OracleCommand command = new OracleCommand(queryString, objConn);
                                var reference = command.ExecuteScalar();
                                    if (reference != null)
                                    {
                                    sophisNode.Value = reference.ToString();
                                    Logger.Info("Resultat  {0}", reference.ToString());
                                }
                                else
                                {
                                    sophisNode.Value = "";
                                    Logger.Info("Titre not found for LIBELLE '{0}' in Data Base ", fields[0]);
                                }
                                
                            }

                            var valueDate = lineNode.DescendantNodes().OfType<XElement>()
                                .FirstOrDefault(x => x.Name.LocalName.Equals(ValueDate));
                            if (valueDate == null)
                            {

                                Logger.Info("Value Date node not found");
                            }
                            else
                            { //format is DD/MM/YYYY

                                try
                                {
                                    DateTime dt = DateTime.ParseExact(fields[2], "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                                    string Day = fields[2].Substring(0, 2);
                                    string Month = fields[2].Substring(3, 2);
                                    string Year = fields[2].Substring(6, 4);
                                    valueDate.Value = Year + "-" + Month + "-" + Day;
                                }
                                catch
                                {
                                    Logger.Info("Date '{0}' not in correct format DD/MM/YYYY ", fields[2]);
                                    //valueDate.Value = "DD/MM/YYYY";
                                    valueDate.Value = "";
                                }
                            }

                            var numericValue = lineNode.DescendantNodes().OfType<XElement>()
                               .FirstOrDefault(x => x.Name.LocalName.Equals(NumericValue));
                            if (numericValue == null)
                            {
                                Logger.Info("NumericValue node not found");
                            }
                            else
                            {
                                var style = System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands | System.Globalization.NumberStyles.AllowDecimalPoint;
                                double outvalue;
                                if (Double.TryParse(fields[3], style, System.Globalization.CultureInfo.InvariantCulture, out outvalue))
                                {
                                    numericValue.Value = fields[3];
                                }
                                else
                                {
                                    Logger.Info("NumericValue '{0}' not in correct format ", fields[3]);
                                    numericValue.Value = "0.0";
                                }

                            }
                            var valueMultisource = lineNode.DescendantNodes().OfType<XElement>()
                               .FirstOrDefault(x => x.Name.LocalName.Equals(ValueMultisource));
                            if (valueMultisource == null)
                            {
                                Logger.Info("ValueMultisource node not found");
                            }
                            else
                            {
                                valueMultisource.Value = fields[18];
                            }

                            parentNode.Add(lineNode);
                        });

                    } // end else

                    objConn.Close();
                    Logger.Info("Connection Close");

                } // end try connection DB
                catch (Exception ex)
                {
                    Logger.Info(" Oracle Connection Error {0}", ex.Message);
                }

                xDoc.Save(outputFileXml);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error when transforming {0}", e.Message);
            }
        }
    }
}
