using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sophis.gui;
using Sophis.Util.GUI;
using Dapper;
using Eff.DataModels;

namespace Eff
{
    namespace Utils
    {
        public static class ColumnExplainer
        {
            public static void ReloadFromDatabase()
            {
                try
                {
                    var arrPortfolioColumnsTooltip = PortfolioColumnsTooltipMgr.GetInstance().PortfolioColumnsTooltip;
                    var connection = Sophis.DataAccess.DBContext.Connection;
                    var arrDefCols = connection.Query<EMC_DEFINITION_COLUMN>(@"
SELECT T1.ID_COLUMN, T1.NAME_COLUMN, NVL(T2.DESCRIPTION, T1.DEFINITION) DEFINITION
FROM EMC_DEFINITION_COLUMN T1
FULL JOIN EXPRESSION_COLUMN T2 ON T1.NAME_COLUMN = T2.COLUMNNAME", null);
                    foreach (var defCol in arrDefCols)
                    {
                        arrPortfolioColumnsTooltip[defCol.NAME_COLUMN] = defCol.DEFINITION;
                    }
                }
                catch (Exception e)
                {
                    EmcLog.Error(e.ToString());
                    MessageBox.Show(string.Format("Error: {0}", e.Message), MainClass.CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
