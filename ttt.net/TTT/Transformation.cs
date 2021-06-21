using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using TTT.Models;

namespace TTT
{
    public enum TransName
    {
        ThirdPartyName,
        ThirdPartyEntity,
        Share,
        YieldCurve,
        Benchmark
    }

    public static class Transformation
    {
        private const string DEFAULT_CONFIG = "ttt.xml";
        private const string CSV_SEPARATED_CHARS = ",|;";
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static String GetCsvVal(String[] csvHeaders, String[] csvVals, String columnName)
        {
            int idx = Enumerable.Range(0, csvHeaders.Length)
                    .Where(i => columnName.Equals(csvHeaders[i])).FirstOrDefault();                        

            return csvVals[idx];
        }

        public static TttTransformation[] InitializeAndSaveConfig(string configFile = DEFAULT_CONFIG)
        {
            var Settings = new[] {
				new TttTransformation {
                    name = TransName.ThirdPartyName.ToString(),
					label = "ThirdParty Names",
					templateFile = "Import_ThirdParty_names.xml",
					category = "Third Parties",
					columns = new [] {
                        new TttColumn {
                            name = "Reference",
                            isRequired = true,
                            isRelativeToRootNode = false,
                            destPaths = new [] {
                                "//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'reference')]/text()",
//										"//*:partyId[contains(@*:partyIdScheme, 'reference')]"
                            }
                        },
                        new TttColumn {
                            name = "Name",
                            isRequired = true,
                            isRelativeToRootNode = false,
                            destPaths = new [] {
                                "//*[local-name() = 'partyId'][contains(@*[local-name() = 'partyIdScheme'], 'name')]/text()",
                                "//*[local-name() = 'partyName']/text()"
//										"//*:partyId[contains(@*:partyIdScheme, 'name')]",
//										"//*:partyName"
                            }
                        },
                        new TttColumn {
                            name = "Location",
                            isRequired = false,
                            isRelativeToRootNode = false,
                            destPaths = new [] {
                                "//*[local-name() = 'location']"
//										"//party:location"
                            }
                        }
					}
				},
				new TttTransformation {
                    name = TransName.ThirdPartyEntity.ToString(),
					label = "ThirdParty Entities",
					templateFile = "Import_ThirdParty_Enty.xml",
					category = "Third Parties",
				},
				new TttTransformation {
					name = TransName.Share.ToString(),
					label = "Share",
					templateFile = "Import_Share.xml",
					category = "Instruments",
					columns = new [] {
                        new TttColumn {
                            name = "Reference",
                            isRequired = true,
                            isRelativeToRootNode = false,
                            destPaths = new String[] {
                                "//*[local-name() = 'identifier']/*[local-name() = 'reference'][@*[local-name() = 'name'] = 'Reference']/text()",
                            }
                        },
                        new TttColumn {
                            name = "Name",
                            isRequired = true,
                            isRelativeToRootNode = false,
                            destPaths = new [] {
                                "//*[local-name() = 'share']/*[local-name() = 'name']/text()",
                            }
                        }
					}
				},
				new TttTransformation {
					name = TransName.YieldCurve.ToString(),
					label = "Yield Curve",
					templateFile = "importYieldCurve.xml",
					category = "Market Data",
					repeatingRootPath = "//*[local-name() = 'points']",
					repeatingChildrenPath = "//*[local-name() = 'points']/*[local-name() = 'point']",
					columns = new [] {
                        new TttColumn {
						    name = "CURVE_NAME",
							isRequired = true,
							isRelativeToRootNode = false,
							destPaths = new [] {
                                "//*[local-name() = 'name']/text()",
                            },
                        },
						new TttColumn {
						    name = "MARKET_FAMILY",
							isRequired = true,
							isRelativeToRootNode = false,
							destPaths = new [] {
							    "//*[local-name() = 'family']/text()",
							}
                        },
						new TttColumn {
						    name = "CURVE_DATE",
							isRequired = true,
							isRelativeToRootNode = true,
							destPaths = new [] {
                                "//*[local-name() = 'date']/text()",
                            }
                        },
						new TttColumn {
						    name = "CURVE_YIELD",
							isRequired = true,
							isRelativeToRootNode = true,
							destPaths = new [] {
							    "//*[local-name() = 'yield']/text()",
							}
                        },
						new TttColumn {
						    name = "CURVE_ISBOND",
							isRequired = true,
							isRelativeToRootNode = true,
							destPaths = new [] {
							    "//*[local-name() = 'isBond']/text()",
                            }
                        }
					}
				},
				new TttTransformation {
                    name = TransName.Benchmark.ToString(),
					label = "Benchmark",
					templateFile = "Import_Benchmark.xml",
					category = "Instruments",
					repeatingRootPath = "//*[local-name() = 'standardComponents']",
					repeatingChildrenPath = "//*[local-name() = 'instrumentStdComponent']",					
					columns = new [] {
                        new TttColumn {
                            name = "Reference",
                            isRequired = true,
                            isRelativeToRootNode = false,
                            destPaths = new [] {
                                "//*[local-name() = 'identifier']/*[local-name() = 'reference'][@*[local-name() = 'name'] = 'Sophisref']/text()",
                                //"//*[local-name() = 'identifier']/*[local-name() = 'reference'][@*[local-name() = 'name'] = 'Reference']/text()",
                            }
                        },
                        new TttColumn {
                            name = "Name",
                            isRequired = true,
                            isRelativeToRootNode = false,
                            destPaths = new [] {
                                "//*[local-name() = 'name']/text()",
                            }
                        },
                        new TttColumn {
                            name = "Currency",
                            isRequired = true,
                            isRelativeToRootNode = false,
                            destPaths = new [] {
                                "//*[local-name() = 'currency']/text()",
                            }
                        },
                        new TttColumn {
                            name = "Market",
                            isRequired = true,
                            isRelativeToRootNode = false,
                            destPaths = new [] {
                                "//*[local-name() = 'market']/*[local-name() = 'sophis']/text()",
                            }
                        },
                        new TttColumn {
                            name = "Definition_type",
                            isRequired = true,
                            isRelativeToRootNode = false,
                            destPaths = new [] {
                                "//*[local-name() = 'definitionType']/text()",
                            }
                        },
                        new TttColumn {
                            name = "Is_drifted",
                            isRequired = true,
                            isRelativeToRootNode = false,
                            destPaths = new [] {
                                "//*[local-name() = 'drifted']/text()",
                            }
                        },		
                        new TttColumn {
                            name = "Pricing",
                            isRequired = true,
                            isRelativeToRootNode = false,
                            destPaths = new [] {
                                "//*[local-name() = 'pricingMethod']/text()",
                            }
                        },
                        new TttColumn {
                            name = "Hedge_ratio",
                            isRequired = true,
                            isRelativeToRootNode = false,
                            destPaths = new [] {
                                "//*[local-name() = 'hedgeRatio']/text()",
                            }
                        },
                        new TttColumn {
                            name = "Record_date",
                            isRequired = true,
                            isRelativeToRootNode = false,
                            destPaths = new [] {
                                "//*[local-name() = 'recordDate']/text()",
                            }
                        },
                        new TttColumn {
                            name = "Return_computation",
                            isRequired = true,
                            isRelativeToRootNode = false,
                            destPaths = new [] {
                                "//*[local-name() = 'standardComposition']/*[local-name() = 'useComponentsReturn']/text()",
                            }
                        },	
                        new TttColumn {
                            name = "Cash_computation",
                            isRequired = true,
                            isRelativeToRootNode = false,
                            destPaths = new [] {
                                "//*[local-name() = 'standardComposition']/*[local-name() = 'includeCashSinceRecordStart']/text()",
                            }
                        },
                        new TttColumn {
                            name = "Resize",
                            isRequired = true,
                            isRelativeToRootNode = false,
                            destPaths = new [] {
                                "//*[local-name() = 'standardComposition']/*[local-name() = 'resize']/text()",
                            }
                        },
                        new TttColumn {
                            name = "Resize_to",
                            isRequired = true,
                            isRelativeToRootNode = false,
                            destPaths = new [] {
                                "//*[local-name() = 'standardComposition']/*[local-name() = 'resizingType']/text()",
                            }
                        },
                        new TttColumn {
                            name = "Instrument",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                "//*[local-name() = 'instrumentStdComponent']/*[local-name() = 'instrument']/*[local-name() = 'sophis']/text()",
                            }
                        },	
                        new TttColumn {
                            name = "Weight",
                            isRequired = true,
                            isRelativeToRootNode = true,
                            destPaths = new [] {
                                "//*[local-name() = 'instrumentStdComponent']/*[local-name() = 'weight']/text()",
                            }
                        }						
					}
				}
		    };

            var serializer = new XmlSerializer(typeof(TttTransformation[]));
            TextWriter writer = new StreamWriter(configFile);
            serializer.Serialize(writer, Settings);
            writer.Close();

            return Settings;
        }

        public static TttTransformation[] LoadConfigFromFile(String configFile = DEFAULT_CONFIG)
        {
            try
            {
                TttTransformation[] Settings;
                XmlSerializer serializer = new XmlSerializer(typeof(TttTransformation[]));
                if (File.Exists(configFile))
                {
                    StreamReader reader = new StreamReader(configFile);
                    Settings = (TttTransformation[])serializer.Deserialize(reader);
                    reader.Close();
                }
                else
                {
                    Logger.Debug("Config file does not exist. Finding in resource...");
                    var resourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames().FirstOrDefault(x => x.EndsWith("." + configFile));
                    if (resourceName == null) throw new ArgumentException(string.Format("Config file not found in resource: {0}", configFile));
                    var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
                    Settings = (TttTransformation[])serializer.Deserialize(stream);
                }
                return Settings;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }
        }

        public static void Transform(TttTransformation[] Settings, String transName, int lineNumber, String inputCsvFile, String outputXmlFile)
        {
            TttTransformation trans = Settings.Where(x => x.name == transName).FirstOrDefault();
            if (trans == null) throw new ArgumentException(string.Format("Transformation name not found: {0}", transName));

            //Build DOM
            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = true; // to preserve the same formatting

            var resourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames().FirstOrDefault(x => x.EndsWith("." + trans.templateFile));
            if (resourceName == null) throw new ArgumentException(string.Format("Template file not found in resource: {0}", trans.templateFile));
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            doc.Load(stream);

            String[] lines;
            if (File.Exists(inputCsvFile))
            {
                lines = File.ReadAllLines(inputCsvFile);
            }
            else
            {
                Logger.Debug("InputCsv file does not exist. Finding in resource...");
                resourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames().FirstOrDefault(x => x.EndsWith("." + inputCsvFile));
                if (resourceName == null) throw new ArgumentException(string.Format("InputCsv file not found in resource: {0}", inputCsvFile));
                lines = ReadLines(() => Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName), Encoding.UTF8).ToArray();
            }
            resourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames().FirstOrDefault(x => x.EndsWith("." + trans.templateFile));
            if (resourceName == null) throw new ArgumentException(string.Format("Template file not found: {0}", trans.templateFile));

            String[] csvHeaders = lines[0].Split(CSV_SEPARATED_CHARS.ToCharArray());
            String[] csvVals = lines[lineNumber].Split(CSV_SEPARATED_CHARS.ToCharArray());

            //transformation for the absolute columns whose values are the same for all lines
            TttColumn[] arrAbsoluteColumns = trans.columns.Where(x => !x.isRelativeToRootNode).ToArray();
            foreach (TttColumn col in arrAbsoluteColumns)
            {
                String colVal = GetCsvVal(csvHeaders, csvVals, col.name);
                foreach (String destPath in col.destPaths)
                {
                    XmlNode nodeDest = doc.SelectSingleNode(destPath);
                    Logger.Debug("src={}, dest={}, textContent={}", colVal, nodeDest.Value, nodeDest.InnerText);
                    nodeDest.InnerText = colVal;
                    Logger.Debug("new value dest={}, textContent={}", nodeDest.Value, nodeDest.InnerText);
                }
            }

            //transformation for the relative columns whose values are different between lines
            string repeatingRootPath = trans.repeatingRootPath;
            string repeatingChildrenPath = trans.repeatingChildrenPath;

            if (repeatingRootPath != null && !string.IsNullOrEmpty(repeatingRootPath)
                    && repeatingChildrenPath != null && !string.IsNullOrEmpty(repeatingChildrenPath))
            {
                XmlNode repeatingRootNode = doc.SelectSingleNode(repeatingRootPath); // un seul noeud
                XmlNodeList repeatingChildrenNodes = doc.SelectNodes(repeatingChildrenPath); // tous les noeuds qui ont le meme Path
                XmlNode templateNode = repeatingChildrenNodes.Item(0);
                for (int i = 1; i < repeatingChildrenNodes.Count; i++)
                {
                    repeatingRootNode.RemoveChild(repeatingChildrenNodes.Item(i));
                }

                //get list of columns whose values need to be filled in root node
                TttColumn[] arrRelativeColumns = trans.columns.Where(x => x.isRelativeToRootNode).ToArray();
                List<XmlNode> transformedNodes = new List<XmlNode>();
                for (int i = 1; i < lines.Length; i++)
                {
                    csvVals = lines[i].Split(CSV_SEPARATED_CHARS.ToCharArray());
                    String msg = String.Format("Line {0}: ", i);
                    foreach (TttColumn col in arrRelativeColumns)
                    {
                        String colVal = GetCsvVal(csvHeaders, csvVals, col.name);
                        foreach (String destPath in col.destPaths)
                        {

                            XmlNode nodeDest = doc.SelectSingleNode(destPath);
                            nodeDest.InnerText = colVal;
                            msg = msg + colVal + ", ";
                        }
                    }
                    Logger.Debug(msg.Substring(0, msg.Length - 2));
                    transformedNodes.Add(templateNode.Clone());
                }

                foreach (XmlNode node in transformedNodes)
                {
                    repeatingRootNode.InsertBefore(node, templateNode);
                }
                repeatingRootNode.RemoveChild(templateNode);
            }

            // save the output file
            doc.Save(outputXmlFile);
            Logger.Debug("Transform.END");
        }

        private static IEnumerable<string> ReadLines(Func<Stream> streamProvider, Encoding encoding)
        {
            using (var stream = streamProvider())
            using (var reader = new StreamReader(stream, encoding))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }
    }
}
