using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using SophisETL.Common;
using SophisETL.Common.Logger;
//using SophisETL.Engine.IntegrationService.WebApi;
using SophisETL.Transform.MedioDBFilter.Xml;

namespace SophisETL.Transform.MedioDBFilter
{
    [SettingsType(typeof(Settings), "_Settings")]
    public class MedioDBFilter : ITransformStep
    {
        #region private members
        private Settings _Settings { get; set; }
        private List<IETLQueue> m_sourceQueues = new List<IETLQueue>();
        private List<IETLQueue> m_targetQueues = new List<IETLQueue>();
        #endregion

        #region properties
        public List<IETLQueue> SourceQueues { get { return m_sourceQueues; } }
        public List<IETLQueue> TargetQueues { get { return m_targetQueues; } }
        public string Name { get; set; }
        private int _RecordsTransformedCount = 0;
        #endregion

        // Start of our thread, we load and push
        public event EventHandler NoMoreRecords;
        private OracleConnection _DBCon;    // opened in Init()

        public void Init()
        {
            LogManager.Instance.Log(" Initializing Step " + this.Name + "...");

            // Make sure we have only 1 Source / Target Queue
            if (m_sourceQueues.Count != 1)
            {
                throw new Exception(String.Format("Transform step [{1}/{0}] must have one source queue", Name,
                    GetType().Name));
            }
            if (m_targetQueues.Count != 1)
            {
                throw new Exception(String.Format("Transform step [{1}/{0}] must have only one target Queue", Name,
                    GetType().Name));
            }

            try
            {
                string connectionString =
                    "Data Source=" + _Settings.dbConnection.instance + ";" +
                    "User Id=" + _Settings.dbConnection.login + ";" +
                    "Password=" + _Settings.dbConnection.password;
                _DBCon = new OracleConnection(connectionString);
                _DBCon.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw new Exception("Error: can not open connection to specified database", e);
            }
        }

        public void Start()
        {
            var records = GetRecords();
            foreach (var record in records)
            {
                _RecordsTransformedCount++;

                if (!_Settings.turnOn)
                {
                    m_targetQueues[0].Enqueue(record);
                }
                else
                {
                    using (var cmd = _DBCon.CreateCommand())
                    {
                        cmd.CommandText = _Settings.query;
                        cmd.BindByName = true;
                        cmd.Parameters.Clear();
                        foreach (var param in _Settings.parameters)
                        {
                            if (param.paramName.ToUpper().Contains("DATE"))
                            {
                                DateTime fieldValue = (DateTime)record.Fields[param.paramName];
                                var value = fieldValue.ToString("dd-MM-yy");
                                OracleParameter parameter = new OracleParameter(param.paramName, value);
                                cmd.Parameters.Add(parameter);
                            }
                            else
                            {
                                var value = record.Fields[param.paramName];
                                OracleParameter parameter = new OracleParameter(param.paramName, value);
                                cmd.Parameters.Add(parameter);
                            }
                        }
                        var result = cmd.ExecuteScalar() == DBNull.Value ? 0 : cmd.ExecuteScalar();
                        double price = 0;
                        if (IsNumeric(result, ref price))
                        {
                            if (price != 0)
                            {
                                _RecordsTransformedCount--;
                                LogManager.Instance.LogDebug("This record is going to be filterd out, as a price has been found on this day");
                            }
                            else
                            {
                                m_targetQueues[0].Enqueue(record);
                            }
                        }
                        else
                        {
                            m_targetQueues[0].Enqueue(record);
                        }
                    }
                }
            }

            LogManager.Instance.Log("Transform/" + Name + ": step finished - "
                                    + _RecordsTransformedCount + " record(s) transformed");
        }

        // check if an object is a numeric type and store the value in dValue
        private bool IsNumeric(object Expression, ref double dValue)
        {
            bool bIsNum = false;
            double dRetNum = .0;
            bIsNum = Double.TryParse(Convert.ToString(Expression),
                System.Globalization.NumberStyles.Any,
                System.Globalization.NumberFormatInfo.InvariantInfo,
                out dRetNum);

            dValue = dRetNum;
            return bIsNum;
        }

        private List<Record> GetRecords()
        {
            List<Record> theRecordSet = new List<Record>();

            // get all the records and add them to the list (only use the first Queue as there is only one)
            while (m_sourceQueues[0].HasMore)
            {
                Record record = m_sourceQueues[0].Dequeue();
                if (record != null)
                    theRecordSet.Add(record);
            }
            return theRecordSet;
        }


    }
}
