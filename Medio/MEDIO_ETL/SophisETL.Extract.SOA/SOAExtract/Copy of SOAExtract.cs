using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SophisETL.Common;
using SophisETL.Engine.IntegrationService;
using SophisETL.Engine.IntegrationService.Sophis.SOA.MethodDesigner;
using SophisETL.Common.Logger;
using System.Xml.Linq;
using SophisETL.Extract.SOAExtract.Xml;

namespace SophisETL.Extract.SOAExtract
{
    public class NS
    {
        static public XNamespace fpml = "http://www.fpml.org/2005/FpML-4-2";
        static public XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
        static public XNamespace nsDataExchange = "http://sophis.net/sophis/gxml/dataExchange";
        static public XNamespace nsCommon = "http://www.sophis.net/common";
        static public XNamespace nsCommon2 = "http://sophis.net/sophis/common";
        static public XNamespace nsR = "http://www.sophis.net/reporting";
        static public XNamespace nsNetR = "http://www.sophis.net/DotNetReporting";
        static public XNamespace nsInstrument = "http://www.sophis.net/Instrument";
        static public XNamespace nsFund = "http://www.sophis.net/fund";
        static public XNamespace nsValuation = "http://www.sophis.net/valuation";
        static public XNamespace nsDividend = "http://www.sophis.net/dividend";
        static public XNamespace nsVolatility = "http://www.sophis.net/volatility";
        static public XNamespace nsTrade = "http://www.sophis.net/trade";
        static public XNamespace nsReporting = "http://www.sophis.net/reporting";
        static public XNamespace nsFolio = "http://www.sophis.net/folio";
    }


	
    [SettingsType(typeof(Settings), "_Settings")]
    public class SOAExtract : IExtractStep
    {
        private Settings _Settings { get; set;}
        private int _RecordsExtractedCount = 0;
        private string logClassName = null;
        private SoaMethodsRequest _SoaMethodsRequest;


       #region MethodDesignerExtract Chain Parameters
        public string Name { get; set; }
        // Only an Output Queue exists
        public IETLQueue TargetQueue { get; set; }
        #endregion


        private IntegrationServiceEngine _ISEngine;


        public void Init()
        {
            logClassName = "[Transform/SOA/" + this.Name + "]";
            LogManager.Instance.Log(logClassName + " Starting Step...");
            // Request the initialization of the Integration Service Engine and Test that it works
            _ISEngine = IntegrationServiceEngine.Instance;
            _ISEngine.CheckMethodDesignerService();            

            // Add a callback to our own NoMoreRecords event to dispose the Service
            //this.NoMoreRecords += new EventHandler(AddBenchmarkComposition_NoMoreRecords);
        }

        
        // Start of our thread, we load and push
        public event EventHandler NoMoreRecords;

        public void Start()
        {
            int count = _Settings.soaParameters.Length;
            List<SoaMethodParameter> paramList = new List<SoaMethodParameter>(count);
            for (int i = 0; i < count; i++)
            {
                SoaMethodParameter param = new SoaMethodParameter();
                param.name = _Settings.soaParameters[i].soaParamName;
                param.Value = _Settings.soaParameters[i].soaParamValue;
                paramList.Add(param);
            }

            LogManager.Instance.Log(logClassName + " try to execute method:" + _Settings.soaMethod + " with parameters:" + paramList);
            XDocument resultXdoc = _ISEngine.MethodDesignerExecuteMethod(_Settings.soaMethod, paramList);
            if (resultXdoc != null)
            {
                try
                {
                    foreach (XElement node in resultXdoc.Root.Elements(NS.nsR + "default0Result"))
                    {
                        Record record = Record.NewRecord(++_RecordsExtractedCount);
                        foreach (XNode e in node.Nodes())
                        {
                            XElement elt = e as XElement;
                            string fieldName = elt.Name.LocalName;
                            string fieldValue = elt.Value.ToString();
                            if (string.IsNullOrEmpty(fieldValue))
                            {
                                record.Fields.Add(fieldName, "");
                                record.Fields.Add(fieldName + "_IsNull", true);
                            }
                            else
                            {
                                record.Fields.Add(fieldName, fieldValue);
                                record.Fields.Add(fieldName + "_IsNull", false);
                            }

                        }
                        TargetQueue.Enqueue(record);
                    }
                }
                catch (Exception e)
                {

                }
            }

            NoMoreRecords(this, null);
        }

    }
}
