using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Oracle.DataAccess.Client;

using Eff.UpgradeUtilities;
using sophis;
using sophis.portfolio;
using sophis.instrument;
using sophis.market_data;
using sophis.static_data;
using sophis.utils;

namespace dnPortfolioColumn
{
    //Concentrations
    public class PC_Top3Positions : sophis.portfolio.CSMColumnConsolidate
    {
        public override void GetPortfolioCell(int activePortfolioCode, int portfolioCode,
                                 sophis.portfolio.CSMExtraction extraction,
                                 ref SSMCellValue cellValue, SSMCellStyle cellStyle,
                                 bool onlyTheValue)
        {
            //e_top3pos:
            try
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(activePortfolioCode={0}, portfolioCode={1}, extraction={2})",
                    activePortfolioCode, portfolioCode, extraction.GetModelName());
                // get the portfolio from its code and the extraction
                CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                if (portfolio == null)
                    return;

                CSMPosition position;

                double AV1 = 0;
                double AV2 = 0;
                double AV3 = 0;
                double AVtemp;

                int positionNumber = portfolio.GetTreeViewPositionCount();
                for (int index = 0; index < positionNumber; index++)
                {
                    position = portfolio.GetNthTreeViewPosition(index);
                    double fxspot = CSMMarketData.GetCurrentMarketData().GetForex(position.GetCurrency());
                    if (position.GetInstrumentCount() != 0)//On ne tient compte que des positions ouvertes
                    {
                        AVtemp = position.GetAssetValue() * fxspot;
                        if (AVtemp >= AV1)
                        {
                            AV3 = AV2;
                            AV2 = AV1;
                            AV1 = AVtemp;
                        }
                        else
                        {
                            if (AVtemp >= AV2)
                            {
                                AV3 = AV2;
                                AV2 = AVtemp;
                            }
                            else
                            {
                                if (AVtemp >= AV3)
                                {
                                    AV3 = AVtemp;
                                }
                            }
                        }
                    }
                }
                cellValue.doubleValue = AV1 + AV2 + AV3;
                cellValue.doubleValue = cellValue.doubleValue * 1000;

                if (cellValue.doubleValue.Equals(double.NaN)) { cellValue.doubleValue = 0; }

                //Set style
                cellStyle.kind = NSREnums.eMDataType.M_dDouble;
                cellStyle.@decimal = 0;
                cellStyle.alignment = sophis.gui.eMAlignmentType.M_aRight;
                cellStyle.style = sophis.gui.eMTextStyleType.M_tsBold;
                cellStyle.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
                sophis.gui.SSMRgbColor col = new sophis.gui.SSMRgbColor();
                col.green = 0;
                col.blue = 65000;
                col.red = 0;
                cellStyle.color = col;
            }
            catch (Exception e)
            {
                cellValue.doubleValue = 0;
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_error, e.ToString());
            }
            finally
            {
                if (double.IsNaN(cellValue.doubleValue) || double.IsInfinity(cellValue.doubleValue))
                {
                    UpgradeExtensions.Log(CSMLog.eMVerbosity.M_warning, "NaN value replaced by Zero");
                    cellValue.doubleValue = 0;
                }
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "END");
            }
        }
    }
    
    public class PC_Top5Positions : sophis.portfolio.CSMColumnConsolidate
    {
        public override void GetPortfolioCell(int activePortfolioCode, int portfolioCode,
                                 sophis.portfolio.CSMExtraction extraction,
                                 ref SSMCellValue cellValue, SSMCellStyle cellStyle,
                                 bool onlyTheValue)
        {
            //e_top5pos:
            try
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(activePortfolioCode={0}, portfolioCode={1}, extraction={2})",
                    activePortfolioCode, portfolioCode, extraction.GetModelName());
                // get the portfolio from its code and the extraction
                CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                if (portfolio == null)
                    return;

                CSMPosition position;

                double AV1 = 0;
                double AV2 = 0;
                double AV3 = 0;
                double AV4 = 0;
                double AV5 = 0;
                double AVtemp;

                int positionNumber = portfolio.GetTreeViewPositionCount();
                for (int index = 0; index < positionNumber; index++)
                {
                    position = portfolio.GetNthTreeViewPosition(index);
                    double fxspot = CSMMarketData.GetCurrentMarketData().GetForex(position.GetCurrency());
                    if (position.GetInstrumentCount() != 0)//On ne tient compte que des positions ouvertes
                    {
                        AVtemp = position.GetAssetValue() * fxspot;
                        if (AVtemp >= AV1)
                        {
                            AV5 = AV4;
                            AV4 = AV3;
                            AV3 = AV2;
                            AV2 = AV1;
                            AV1 = AVtemp;
                        }
                        else
                        {
                            if (AVtemp >= AV2)
                            {
                                AV5 = AV4;
                                AV4 = AV3;
                                AV3 = AV2;
                                AV2 = AVtemp;
                            }
                            else
                            {
                                if (AVtemp >= AV3)
                                {
                                    AV5 = AV4;
                                    AV4 = AV3;
                                    AV3 = AVtemp;
                                }
                                else
                                {
                                    if (AVtemp >= AV4)
                                    {
                                        AV5 = AV4;
                                        AV4 = AVtemp;
                                    }
                                    else
                                    {
                                        if (AVtemp >= AV5)
                                        {
                                            AV5 = AVtemp;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                cellValue.doubleValue = AV1 + AV2 + AV3 + AV4 + AV5;
                cellValue.doubleValue = cellValue.doubleValue * 1000;

                if (cellValue.doubleValue.Equals(double.NaN)) { cellValue.doubleValue = 0; }

                //Set style
                cellStyle.kind = NSREnums.eMDataType.M_dDouble;
                cellStyle.@decimal = 0;
                cellStyle.alignment = sophis.gui.eMAlignmentType.M_aRight;
                cellStyle.style = sophis.gui.eMTextStyleType.M_tsBold;
                cellStyle.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
                sophis.gui.SSMRgbColor col = new sophis.gui.SSMRgbColor();
                col.green = 0;
                col.blue = 65000;
                col.red = 0;
                cellStyle.color = col;
            }
            catch (Exception e)
            {
                cellValue.doubleValue = 0;
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_error, e.ToString());
            }
            finally
            {
                if (double.IsNaN(cellValue.doubleValue) || double.IsInfinity(cellValue.doubleValue))
                {
                    UpgradeExtensions.Log(CSMLog.eMVerbosity.M_warning, "NaN value replaced by Zero");
                    cellValue.doubleValue = 0;
                }
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "END");
            }
        }
    }

    //Analyses de performance
    public class PC_NetReturnSinceInception : sophis.portfolio.CSMColumnConsolidate
    {
        public override void GetPortfolioCell(int activePortfolioCode, int portfolioCode,
                         sophis.portfolio.CSMExtraction extraction,
                         ref SSMCellValue cellValue, SSMCellStyle style,
                         bool onlyTheValue)
        {
            //e_netreturnsinceinception:
            try
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(activePortfolioCode={0}, portfolioCode={1}, extraction={2})",
                    activePortfolioCode, portfolioCode, extraction.GetModelName());
                // get the portfolio from its code and the extraction
                CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                if (portfolio == null)
                    return;

                // Set Value
                double minNAV;
                double lastNAV;
                double folioSicovam;
                //ORCLFactory.ORCLFactory CS_ORCL = new ORCLFactory.ORCLFactory();
                ORCLFactory.ORCLFactory.Initialize();
                folioSicovam = ORCLFactory.ORCLFactory.getResultI("select SICOVAM from titres where type='Z'and REFERENCE = '" + portfolio.GetName() + "'");
                if (folioSicovam > 0)
                {
                    minNAV = ORCLFactory.ORCLFactory.getResultD("select NAV from FUND_NAVFOREOD where SICOVAM = '" + folioSicovam + "'and NAV_DATE in (select Min(NAV_DATE) from FUND_NAVFOREOD where sicovam='" + folioSicovam + "')");
                    lastNAV = ORCLFactory.ORCLFactory.getResultD("select NAV from FUND_NAVFOREOD where SICOVAM = '" + folioSicovam + "' and NAV_DATE in (select Max(NAV_DATE) from FUND_NAVFOREOD where SICOVAM='" + folioSicovam + "' and NAV_DATE<=num_to_date('" + CSMMarketData.GetCurrentMarketData().GetDate() + "'))");
                    cellValue.doubleValue = lastNAV / minNAV - 1;
                }
                cellValue.doubleValue = cellValue.doubleValue * 100;
                //ORCLFactory.ORCLFactory.CloseAll();


                // Set style
                style.kind = NSREnums.eMDataType.M_dDouble;
                style.@decimal = 2;
                style.alignment = sophis.gui.eMAlignmentType.M_aRight;
                style.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
                style.style = sophis.gui.eMTextStyleType.M_tsBold;

            }
            catch (Exception e)
            {
                cellValue.doubleValue = 0;
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_error, e.ToString());
            }
            finally
            {
                if (double.IsNaN(cellValue.doubleValue) || double.IsInfinity(cellValue.doubleValue))
                {
                    UpgradeExtensions.Log(CSMLog.eMVerbosity.M_warning, "NaN value replaced by Zero");
                    cellValue.doubleValue = 0;
                }
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "END");
            }
        }
    }
    
    public class PC_AnnualizedReturnSinceInception : sophis.portfolio.CSMColumnConsolidate
    {
        public override void GetPortfolioCell(int activePortfolioCode, int portfolioCode,
                         sophis.portfolio.CSMExtraction extraction,
                         ref SSMCellValue cellValue, SSMCellStyle style,
                         bool onlyTheValue)
        {
            //e_annreturnsinceinception:
            try
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(activePortfolioCode={0}, portfolioCode={1}, extraction={2})",
                    activePortfolioCode, portfolioCode, extraction.GetModelName());
                // get the portfolio from its code and the extraction
                CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                if (portfolio == null)
                    return;

                // Set Value
                double minNAV;
                double lastNAV;
                double minDate;
                double LastDate;
                double folioSicovam;
                //ORCLFactory.ORCLFactory CS_ORCL = new ORCLFactory.ORCLFactory();
                ORCLFactory.ORCLFactory.Initialize();
                folioSicovam = ORCLFactory.ORCLFactory.getResultI("select SICOVAM from titres where type='Z'and REFERENCE = '" + portfolio.GetName() + "'");
                if (folioSicovam > 0)
                {
                    minNAV = ORCLFactory.ORCLFactory.getResultD("select NAV from FUND_NAVFOREOD where SICOVAM = '" + folioSicovam + "'and NAV_DATE in (select Min(NAV_DATE) from FUND_NAVFOREOD where sicovam='" + folioSicovam + "')");
                    minDate = ORCLFactory.ORCLFactory.getResultD("select date_to_num(Min(NAV_DATE)) from FUND_NAVFOREOD where SICOVAM ='" + folioSicovam + "'");
                    lastNAV = ORCLFactory.ORCLFactory.getResultD("select NAV from FUND_NAVFOREOD where SICOVAM = '" + folioSicovam + "' and NAV_DATE in (select Max(NAV_DATE) from FUND_NAVFOREOD where SICOVAM='" + folioSicovam + "' and NAV_DATE<=num_to_date('" + CSMMarketData.GetCurrentMarketData().GetDate() + "'))");
                    LastDate = ORCLFactory.ORCLFactory.getResultD("select date_to_num(Max(NAV_DATE)) from FUND_NAVFOREOD where SICOVAM='" + folioSicovam + "' and NAV_DATE<=num_to_date('" + CSMMarketData.GetCurrentMarketData().GetDate() + "')");
                    cellValue.doubleValue = lastNAV / minNAV - 1;//Performance non annualisée
                    cellValue.doubleValue = cellValue.doubleValue * 365 / (LastDate - minDate);
                }
                cellValue.doubleValue = cellValue.doubleValue * 100;
                //ORCLFactory.ORCLFactory.CloseAll();

                // Set style
                style.kind = NSREnums.eMDataType.M_dDouble;
                style.@decimal = 2;
                style.alignment = sophis.gui.eMAlignmentType.M_aRight;
                style.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
                style.style = sophis.gui.eMTextStyleType.M_tsBold;

            }
            catch (Exception e)
            {
                cellValue.doubleValue = 0;
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_error, e.ToString());
            }
            finally
            {
                if (double.IsNaN(cellValue.doubleValue) || double.IsInfinity(cellValue.doubleValue))
                {
                    UpgradeExtensions.Log(CSMLog.eMVerbosity.M_warning, "NaN value replaced by Zero");
                    cellValue.doubleValue = 0;
                }
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "END");
            }
        }
    }
    
    public class PC_AvMonthNetReturn : sophis.portfolio.CSMColumnConsolidate
    {
        public override void GetPortfolioCell(int activePortfolioCode, int portfolioCode,
                 sophis.portfolio.CSMExtraction extraction,
                 ref SSMCellValue cellValue, SSMCellStyle style,
                 bool onlyTheValue)
        {
            //e_avmonthnetreturn:
            try
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(activePortfolioCode={0}, portfolioCode={1}, extraction={2})",
                    activePortfolioCode, portfolioCode, extraction.GetModelName());
                // get the portfolio from its code and the extraction
                CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                if (portfolio == null)
                    return;

                // Set Value
                int Count;
                int folioSicovam;
                //ORCLFactory.ORCLFactory CS_ORCL = new ORCLFactory.ORCLFactory();
                ORCLFactory.ORCLFactory.Initialize();
                OracleCommand SqlCommand = new OracleCommand();
                folioSicovam = ORCLFactory.ORCLFactory.getResultI("select SICOVAM from titres where type='Z'and REFERENCE = '" + portfolio.GetName() + "'");
                if (folioSicovam > 0)
                {
                    double[,] tab;//Tableau pour stoker les résultats de la requete i.e. 
                    Count = ORCLFactory.ORCLFactory.getResultI("select count(*) from fund_navforeod where SICOVAM='" + folioSicovam + "' and NAV_DATE<=num_to_date('" + CSMMarketData.GetCurrentMarketData().GetDate() + "')");
                    tab = new double[Count, 2];//Count lignes, 2 colonnes
                    tab = ORCLFactory.ORCLFactory.getResultTab2("select cast(to_char(to_date(NAV_DATE),'MM') as int), NAV from fund_navforeod where SICOVAM='" + folioSicovam + "' and NAV_DATE<=num_to_date('" + CSMMarketData.GetCurrentMarketData().GetDate() + "')", Count, 2);

                    int i = 0;
                    double MonthPerf;//Perf net mensuelle
                    int MonthLast;//Indice de la dernière valeur du mois précédent
                    int nbMonthPerf = 0;//Nombre de performance mensuelles mesurées

                    //On se place sur le 1er indice du mois suivant le mois de 
                    //création pour éviter d'avoir une 1ère perf fausse si pas un mois complet
                    while (tab[i, 0] == tab[i + 1, 0]) { i++; }
                    MonthLast = i;
                    i++;//A ce stade, i est l'indice de la 1ere valeur du mois suivant le mois de création
                    cellValue.doubleValue = 0;
                    while (i < (Count - 1))
                    {
                        while (i < (Count - 1) && tab[i, 0] == tab[i + 1, 0]) { i++; }
                        if (i < (Count - 1))
                        {
                            //A ce stade, i est la dernière valeur du mois courant
                            MonthPerf = tab[i, 1] / tab[MonthLast, 1] - 1;
                            cellValue.doubleValue += MonthPerf;
                            nbMonthPerf++;
                            MonthLast = i;//Indice de la 1ère valeur du mois suivant
                            i++;
                        }
                    }
                    cellValue.doubleValue = cellValue.doubleValue / nbMonthPerf;
                    cellValue.doubleValue = cellValue.doubleValue * 100;
                    //Le dernier mois n'est pas compté dans cette boucle 
                }
                //ORCLFactory.ORCLFactory.CloseAll();


                // Set style
                style.kind = NSREnums.eMDataType.M_dDouble;
                style.@decimal = 2;
                style.alignment = sophis.gui.eMAlignmentType.M_aRight;
                style.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
                style.style = sophis.gui.eMTextStyleType.M_tsBold;

            }
            catch (Exception e)
            {
                cellValue.doubleValue = 0;
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_error, e.ToString());
            }
            finally
            {
                if (double.IsNaN(cellValue.doubleValue) || double.IsInfinity(cellValue.doubleValue))
                {
                    UpgradeExtensions.Log(CSMLog.eMVerbosity.M_warning, "NaN value replaced by Zero");
                    cellValue.doubleValue = 0;
                }
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "END");
            }
        }
    }
    
    public class PC_PositiveMonths : sophis.portfolio.CSMColumnConsolidate
    {
        public override void GetPortfolioCell(int activePortfolioCode, int portfolioCode,
                 sophis.portfolio.CSMExtraction extraction,
                 ref SSMCellValue cellValue, SSMCellStyle style,
                 bool onlyTheValue)
        {
            //e_positivemonths:
            try
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(activePortfolioCode={0}, portfolioCode={1}, extraction={2})",
                    activePortfolioCode, portfolioCode, extraction.GetModelName());
                // get the portfolio from its code and the extraction
                CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                if (portfolio == null)
                    return;

                // Set Value
                double[,] tab;//Tableau pour stoker les résultats de la requete i.e. les dates et les NAV correspondantes
                int Count;//Nombre de dates pour lesquels le fond concerné affiche une NAV

                //Pour récupérer les résultats de la requete sous forme de tableau, on utilise un OracleReader
                //ORCLFactory.ORCLFactory CS_ORCL = new ORCLFactory.ORCLFactory();
                ORCLFactory.ORCLFactory.Initialize();
                OracleCommand SqlCommand = new OracleCommand();

                int i;//Compteur sur le nombre de lignes de tab
                int nbPerf;//Nombre de perf mensuelles calculées
                double MonthPerf;//Perf net mensuelle
                int MonthLast;//Indice de la dernière valeur du mois précédent
                int folioSicovam;
                folioSicovam = ORCLFactory.ORCLFactory.getResultI("select SICOVAM from titres where type='Z'and REFERENCE = '" + portfolio.GetName() + "'");
                if (folioSicovam > 0)
                {
                    Count = ORCLFactory.ORCLFactory.getResultI("select count(*) from fund_navforeod where SICOVAM='" + folioSicovam + "' and NAV_DATE<=num_to_date('" + CSMMarketData.GetCurrentMarketData().GetDate() + "')");
                    tab = new double[Count, 2];//'Count' lignes, 2 colonnes
                    tab = ORCLFactory.ORCLFactory.getResultTab2("select cast(to_char(to_date(NAV_DATE),'MM') as int), NAV from fund_navforeod where SICOVAM='" + folioSicovam + "' and NAV_DATE<=num_to_date('" + CSMMarketData.GetCurrentMarketData().GetDate() + "')", Count, 2);

                    //On se place sur le 1er indice du mois suivant le mois de 
                    //création pour éviter d'avoir une 1ère perf fausse si pas un mois complet
                    i = 0;
                    nbPerf = 0;
                    while (tab[i, 0] == tab[i + 1, 0]) { i++; }
                    MonthLast = i;
                    i++;//A ce stade, i est l'indice de la 1ere valeur du mois suivant le mois de création
                    cellValue.doubleValue = 0;
                    while (i < (Count - 1))
                    {
                        while (i < (Count - 1) && tab[i, 0] == tab[i + 1, 0]) { i++; }
                        if (i < (Count - 1))
                        {
                            //A ce stade, i est la dernière valeur du mois courant
                            MonthPerf = tab[i, 1] / tab[MonthLast, 1] - 1;
                            if (MonthPerf >= 0) { cellValue.doubleValue++; }
                            nbPerf++;
                            MonthLast = i;//Indice de la 1ère valeur du mois suivant
                            i++;
                        }
                    }
                    cellValue.doubleValue = 100 * cellValue.doubleValue / nbPerf;
                    //Le dernier mois n'est pas compté dans cette boucle 
                }
                //ORCLFactory.ORCLFactory.CloseAll();


                // Set style
                style.kind = NSREnums.eMDataType.M_dDouble;
                style.@decimal = 2;
                style.alignment = sophis.gui.eMAlignmentType.M_aRight;
                style.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
                style.style = sophis.gui.eMTextStyleType.M_tsBold;

            }
            catch (Exception e)
            {
                cellValue.doubleValue = 0;
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_error, e.ToString());
            }
            finally
            {
                if (double.IsNaN(cellValue.doubleValue) || double.IsInfinity(cellValue.doubleValue))
                {
                    UpgradeExtensions.Log(CSMLog.eMVerbosity.M_warning, "NaN value replaced by Zero");
                    cellValue.doubleValue = 0;
                }
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "END");
            }
        }
    }

}
