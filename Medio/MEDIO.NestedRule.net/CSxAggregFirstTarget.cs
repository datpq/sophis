using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NSREnums;
using sophis.gui;
using sophis.instrument;
using sophis.portfolio;
using sophis.utils;
using Sophis.DataAccess;
using Oracle.DataAccess.Client;
using Sophis.Data;


namespace MEDIO.NestedRule.net
{
    public class CSxAggregFirstTarget : CSMPortfolioColumn
    {

        public int refreshVersion = -1;
        string sTargetColumn = "";
        protected Dictionary<int, double> columnCache = new Dictionary<int, double>();

        public override void GetPositionCell(CSMPosition position, int activePortfolioCode, int portfolioCode, CSMExtraction extraction, int underlyingCode, int instrumentCode, ref SSMCellValue cellValue, SSMCellStyle cellStyle, bool onlyTheValue)
        {

            try
            {
                //TODO Get portfolio column Param...
                // Getportfoliocell from param column..
                double cellVal = 0.0;

                cellStyle.kind = eMDataType.M_dDouble;
                cellStyle.alignment = eMAlignmentType.M_aRight;
                cellStyle.@decimal = 2;


                if (CSMPortfolioColumn.GetRefreshVersion() != refreshVersion)
                {
                    sTargetColumn = getTargetColumn();
                    columnCache.Clear();
                    refreshVersion = CSMPortfolioColumn.GetRefreshVersion();
                }
                if (!string.IsNullOrEmpty(sTargetColumn))
                {

                     CSMPortfolioColumn cTargetColumn = CSMPortfolioColumn.GetCSRPortfolioColumn(sTargetColumn);

                     if (cTargetColumn != null)
                     {
                         int posIdent = position.GetIdentifier();

                         if (columnCache.TryGetValue(posIdent, out cellVal) == false)
                         {
                             SSMCellValue val = new SSMCellValue();
                             SSMCellStyle style = new SSMCellStyle();
                             cTargetColumn.GetPortfolioCell(activePortfolioCode, portfolioCode, extraction, ref val, style, false);
                             
                            // cellStyle = style;
                             cellVal = val.doubleValue;
                             cellValue.integerValue = val.integerValue;
                             cellValue.SetString(val.GetString());

                             columnCache.Add(posIdent, cellVal);
                         }

                         cellValue.doubleValue = cellVal; 
                     }
                }
            }
            catch(Exception ex)
            {
                sophis.utils.CSMLog.Write("CSxAggregFirstTarget", "GetPositionCell", CSMLog.eMVerbosity.M_error, "Exception Caught :" + ex.Message);
            }
        }

                private string getTargetColumn()
                {
                    string retval = "";

                    //TODO Query...
                    using (var cmd = DBContext.Connection.CreateCommand())
                    {
                        cmd.CommandText = "SELECT CONFIG_VALUE FROM MEDIO_TKT_CONFIG WHERE CONFIG_NAME='MEDIO_AggregFirst_Target_Col'";
                        retval = cmd.ExecuteScalar().ToString();
                    }

                    return retval;
                }

    }
}
