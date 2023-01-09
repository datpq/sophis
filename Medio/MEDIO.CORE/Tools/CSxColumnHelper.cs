using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using sophis.portfolio;
using sophis.utils;

namespace MEDIO.CORE.Tools
{
    public class CSxColumnHelper
    {
        
        public static SSMCellValue GetPositionColumn(int positionID, int folioId, string columnName)
        {
            SSMCellValue res = new SSMCellValue();
            using (CSMLog Log = new CSMLog())
            {
                Log.Begin("CSxColumnHelper", MethodBase.GetCurrentMethod().Name);

                CSMPosition position = CSMPosition.GetCSRPosition(positionID, folioId);

                if (position == null)
                {
                    Log.Write(CSMLog.eMVerbosity.M_warning, String.Format("Position #{0} cannot be cast as CSMPosition object. Return 0", positionID));
                    return res;
                }

                CSMPortfolioColumn column = GetFolioColumn(columnName);
                if (column == null)
                {
                    Log.Write(CSMLog.eMVerbosity.M_warning, String.Format("Column {0} cannot be found", columnName));
                    return res;
                }
                using (CSMPortfolio folio = CSMPortfolio.GetCSRPortfolio(position.GetPortfolioCode()))
                {
                    if (folio == null)
                    {
                        Log.Write(CSMLog.eMVerbosity.M_warning, String.Format("Portfolio of position #{0} cannot be found", positionID));
                        return res;
                    }
                    SSMCellStyle cellStyle = new SSMCellStyle();

                    if (folio.GetExtraction() == null)
                    {
                        Log.Write(CSMLog.eMVerbosity.M_warning, String.Format("Portfolio extraction of position #{0} cannot be found", positionID));
                        return res;
                    }
                    column.GetPositionCell(position, folio.GetCode(), folio.GetCode(), folio.GetExtraction(), 0, position.GetInstrumentCode(), ref res, cellStyle, true);
                    Log.Write(CSMLog.eMVerbosity.M_debug, String.Format("Got position #{0} column value!", positionID));
                }

                Log.End();
                return res;
            }
        }

        public static SSMCellValue GetPortfolioColumn(int folioID, CSMExtraction extraction, string columnName)
        {
            SSMCellValue res = new SSMCellValue();
            using (CSMLog Log = new CSMLog())
            {
                Log.Begin("CSxColumnHelper", MethodBase.GetCurrentMethod().Name);
                CSMPortfolioColumn column = GetFolioColumn(columnName);
                if (column == null)
                {
                    Log.Write(CSMLog.eMVerbosity.M_warning, String.Format("Column {0} cannot be found", columnName));
                    return res;
                }
                using (CSMPortfolio folio = CSMPortfolio.GetCSRPortfolio(folioID, extraction))
                {
                    if (folio != null)
                    {
                        column.GetPortfolioCell(folioID, folioID, extraction, ref res, new SSMCellStyle(), true);
                    }
                }
            }
            return res;
        }

        public static SSMCellValue GetPositionColumn(CSMPosition position, int folioID, CSMExtraction extraction, string columnName)
        {
            SSMCellValue res = new SSMCellValue();
            using (CSMLog Log = new CSMLog())
            {
                Log.Begin("CSxColumnHelper", MethodBase.GetCurrentMethod().Name);
                CSMPortfolioColumn column = GetFolioColumn(columnName);
                if (column == null)
                {
                    Log.Write(CSMLog.eMVerbosity.M_warning,
                        String.Format("Column {0} cannot be found", columnName));
                    return res;
                }
                using (CSMPortfolio folio = CSMPortfolio.GetCSRPortfolio(folioID, extraction))
                {
                    if (folio != null)
                    {
                        column.GetPositionCell(position, folioID, folioID, extraction, position.GetInstrumentCode(), position.GetInstrumentCode(), ref res, new SSMCellStyle(), true);
                    }
                }
            }
            return res;
        }

        private static CSMPortfolioColumn GetFolioColumn(string columnName)
        {
            using (CSMLog Log = new CSMLog())
            {
                Log.Begin("CSxColumnHelper", MethodBase.GetCurrentMethod().Name);
                CSMPortfolioColumn res = CSMPortfolioColumn.GetCSRPortfolioColumn(columnName);
                if (res == null)
                {
                    Log.Write(CSMLog.eMVerbosity.M_error, "Column " + columnName + " not found!");
                    Log.End(); return null;
                }
                Log.End(); return res;
            }
        }

    }
}
