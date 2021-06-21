using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

using sophis;
using sophis.portfolio;
using sophis.misc;
using sophis.instrument;
using sophis.market_data;
using sophis.static_data;
using sophis.utils;

namespace dnPortfolioColumn
{
    public class FonctionAdd
    {

        //Fonction qui retourne une valeur de type double provenant de Sophis d'après le nom de la colonne
        //et des infos de l'instrument.
        public static double GetValuefromSophisDouble(CSMPosition position, CSMInstrument instrument, string namecolum)
        { // position = position // activePortfolioCode = 0 // portFolioCode = 18649 // extraction = // underlyingCode = 0 // instrumentCode = //style = à définir selon le cas//onlyThe value =false //
            //Var
            double Svalue = 1;
            int activePortfolioCode = 0;
            int portfolioCode = position.GetPortfolio().GetCode();
            int underlyingCode = 0;
            int instrumentCode = instrument.GetCode();
            bool onlyTheValue = true;

            //paramètre
            SSMCellValue cvalue = new SSMCellValue();
            SSMCellStyle cstyle = new SSMCellStyle();
            CSMExtraction extraction = new CSMExtraction();
            extraction.fExtractionType = eMExtractionType.M_tUnknown;

            cstyle.kind = NSREnums.eMDataType.M_dDouble;
            cstyle.@decimal = 3;

            //Création de la colonne pour récupérer les données
            CSMPortfolioColumn col = CSMPortfolioColumn.GetCSRPortfolioColumn(namecolum);
            if (col != null)
            {
                col.GetPositionCell(position, activePortfolioCode, portfolioCode, extraction, underlyingCode, instrumentCode, ref cvalue, cstyle, onlyTheValue);
                Svalue = cvalue.doubleValue;
                return Svalue;
            }
            else return 0;

        }

        //Fonction qui retourne une valeur de type string provenant de Sophis d'après le nom de la colonne
        //et des infos de l'instrument.
        public static string GetValuefromSophisString(CSMPosition position, CSMInstrument instrument, string namecolum)
        {
            //Var
            string Svalue = "";
            int activePortfolioCode = 0;
            int portfolioCode = position.GetPortfolio().GetCode();
            int underlyingCode = 0;
            int instrumentCode = instrument.GetCode();
            bool onlyTheValue = false;

            //paramètre
            SSMCellValue cvalue = new SSMCellValue();
            SSMCellStyle cstyle = new SSMCellStyle();
            CSMExtraction extraction = new CSMExtraction();

            cstyle.kind = NSREnums.eMDataType.M_dNullTerminatedString;

            //Création de la colonne pour récupérer les données
            CSMPortfolioColumn col = CSMPortfolioColumn.GetCSRPortfolioColumn(namecolum);

            if (col != null)
            {
                col.GetPositionCell(position, activePortfolioCode, portfolioCode, extraction, underlyingCode, instrumentCode, ref cvalue, cstyle, onlyTheValue);
                Svalue = cvalue.GetString();
                return Svalue;
            }
            else return "";

        }

        //Focntion qui récupère une date dans les colonnes sophis
        public static int GetValuefromSophisDate(CSMPosition position, CSMInstrument instrument, string namecolum)
        { // position = position // activePortfolioCode = 0 // portFolioCode = 18649 // extraction = // underlyingCode = 0 // instrumentCode = //style = à définir selon le cas//onlyThe value =false //
            //Var
            int activePortfolioCode = 1;
            int portfolioCode = position.GetPortfolio().GetCode();//C'est la ou ca va pas :)
            int underlyingCode = 0;
            int instrumentCode = instrument.GetCode();
            bool onlyTheValue = true;

            //paramètre
            SSMCellValue cvalue = new SSMCellValue();
            SSMCellStyle cstyle = new SSMCellStyle();
            CSMExtraction extraction = new CSMExtraction();
            extraction.fExtractionType = eMExtractionType.M_tUnknown;

            cstyle.kind = NSREnums.eMDataType.M_dDate;
            //cstyle.@decimal = 3;

            //Création de la colonne pour récupérer les données
            CSMPortfolioColumn col = CSMPortfolioColumn.GetCSRPortfolioColumn(namecolum);
            if (col != null)
            {
                col.GetPositionCell(position, activePortfolioCode, portfolioCode, extraction, underlyingCode, instrumentCode, ref cvalue, cstyle, onlyTheValue);
                return cvalue.integerValue;
            }
            else return 0;

        }


        //Fonction qui retourne la valeur de la NAV pour les folios concernés
        //
        //V1 : Suivant le nom du dossier.
        public static double GetNAVFund(CSMPortfolio pf, CSMExtraction extemp)//Version 1. a remanier 
        {
            //Variables
            CSMPortfolio pftemp;

            try
            {
                if (pf.GetName() == "Strategy")//si le folio parent est déja le bon
                {
                    pftemp = CSMPortfolio.GetCSRPortfolio(pf.GetParentCode(), extemp);
                    return 1000 * pftemp.GetNetAssetValue();
                }
                else if (pf.GetName() == "Bonds" || pf.GetName() == "Legacy Bonds" || pf.GetName() == "CDS" || pf.GetName() == "FX Trades" || pf.GetName() == "Fx Trades" || pf.GetName() == "Fx trades" || pf.GetName() == "Hedges" || pf.GetName() == "Treasury Management" || pf.GetName() == "OPCVM"
                   || (pf.GetName() == "Fonds" && pf.GetLevel() > 3) || (pf.GetName() == "Fonds TIM" && pf.GetLevel() > 3) || pf.GetName() == "Equity" || pf.GetName() == "Private Equity" || pf.GetName() == "Monetary" || pf.GetName() == "Hedge" || pf.GetName() == "Kaufman & Broad"
                    || pf.GetName() == "Loans" || pf.GetName() == "Structured Products" || pf.GetName() == "T-ORA")
                {
                    pftemp = CSMPortfolio.GetCSRPortfolio(pf.GetParentCode(), extemp);
                    if (pftemp.GetName() == "Strategy")//si le folio parent est déja le bon
                    {
                        pftemp = CSMPortfolio.GetCSRPortfolio(pftemp.GetParentCode(), extemp);
                        return 1000 * pftemp.GetNetAssetValue();
                    }
                    else return 1000 * pf.GetNetAssetValue();

                }
                else if (pf.GetName() == "Financieres Senior" || pf.GetName() == "HY Court" || pf.GetName() == "IG Court")
                {
                    pftemp = CSMPortfolio.GetCSRPortfolio(pf.GetParentCode(), extemp);
                    pftemp = CSMPortfolio.GetCSRPortfolio(pftemp.GetParentCode(), extemp);
                    if (pftemp.GetName() == "Strategy")//si le folio parent est déja le bon
                    {
                        pftemp = CSMPortfolio.GetCSRPortfolio(pftemp.GetParentCode(), extemp);
                        return 1000 * pftemp.GetNetAssetValue();
                    }
                    else return 1000 * pf.GetNetAssetValue();

                }
                else //Sinon on retourn la NAv du folio
                {
                    return 1000 * pf.GetNetAssetValue();
                }
            }
            catch (Exception e)
            {
                return 0;
            }

        }

        public static string getbucketextend(double mat_time)
        {
            string tmp = "";

            if (mat_time < 92) tmp = "< 3 months";
            else if (mat_time >= 92 && mat_time < 365) tmp = "3 months - 1 Year";
            else if (mat_time >= 365 && mat_time < 1095) tmp = "1-3 Years";
            else if (mat_time >= 1095 && mat_time < 1825) tmp = "3-5 Years";
            else if (mat_time >= 1825 && mat_time < 2555) tmp = "5-7 Years";
            else if (mat_time >= 2555 && mat_time < 3650) tmp = "7-10 Years";
            else if (mat_time >= 3650 && mat_time < 5475) tmp = "10-15 Years";
            else if (mat_time >= 5475 && mat_time < 7300) tmp = "15-20 Years";
            else if (mat_time >= 7300 && mat_time < 10950) tmp = "20-30 Years";
            else if (mat_time >= 10950 && mat_time < 14600) tmp = "30-40 Years";
            else if (mat_time >= 14600 && mat_time < 18250) tmp = "40-50 Years";
            else if (mat_time > 18250) tmp = "> 50 Years";

            return "   " + tmp;
        }
        /*
        //Fonction qui va retourner la pondération associée 
        public static double GetPondGearing(CSMPortfolio portfolio, DataSourceGearing DataSource_a, CSMExtraction extraction)
        {
            double val = 0;
            CSMPosition position;
            CSMInstrument instrument;
            //On boucle sur les folios 
            double valNav = 0;


            if (portfolio.GetChildCount() == 0)//si portefeuille n'a pas d'enfant
            {
                int positionNumber = portfolio.GetTreeViewPositionCount();
                valNav = 0;
                valNav = FonctionAdd.GetNAVFund(portfolio, extraction);
                for (int index = 0; index < positionNumber; index++)
                {
                    position = portfolio.GetNthTreeViewPosition(index);
                    if (position.GetInstrumentCount() != 0)//On ne tient compte que des positions ouvertes
                    {
                        instrument = CSMInstrument.GetInstance(position.GetInstrumentCode());
                        if (valNav != 0) val += DataSource_a.GetGearing(position, instrument) * 100 / valNav;
                    }
                }
            }
            else
            {
                bool IsPort = false;

                if (portfolio.GetName() == "Strategy" || portfolio.GetName() == "TSS Master Fund") IsPort = true;
                else
                {
                    if (portfolio.GetLevel() > 1)
                    {
                        CSMPortfolio father_pf = CSMPortfolio.GetCSRPortfolio(portfolio.GetParentCode());
                        if (father_pf.GetName() == "Mandats" || father_pf.GetName() == "Fonds Dedies" ||
                            father_pf.GetName() == "Fonds Immobiliers" || father_pf.GetName() == "Fonds Ouverts" ||
                            father_pf.GetName() == "Fonds Private Debt" || father_pf.GetName() == "Fonds Titrisation") IsPort = true;
                    }
                }
                //Si le portfolio est Strategy
                if (IsPort)//portfolio.GetName() =="Strategy"
                {
                    //On fait la somme de toutes les position situées dans les folders sans enfant.
                    int nb_child = portfolio.GetChildCount();
                    int pos_number = 0;
                    CSMPortfolio child_pf;
                    //On boucle sur tous les folios
                    for (int i = 0; i < nb_child; i++)
                    {
                        // on extrait le folio child rang i.
                        child_pf = portfolio.GetNthChild(i);
                        if (child_pf.IsAPortfolio() && child_pf.GetChildCount() == 0)
                        {
                            pos_number = child_pf.GetTreeViewPositionCount();
                            valNav = 0;
                            valNav = FonctionAdd.GetNAVFund(portfolio, extraction);
                            for (int j = 0; j < pos_number; j++)
                            {
                                position = child_pf.GetNthTreeViewPosition(j);
                                if (position.GetInstrumentCount() != 0)//On ne tient compte que des positions ouvertes
                                {
                                    instrument = CSMInstrument.GetInstance(position.GetInstrumentCode());
                                    if (valNav != 0) val += DataSource_a.GetGearing(position, instrument) * 100 / valNav;
                                }

                            }
                        }

                    }

                }

            }
            return val;

        }
      */
        //Fonction YTM Asset
        public static double GetPondYTM(CSMPortfolio portfolio, DataSourceTreeFindYTValue Datasource)
        {
            double val = 0;
            CSMPosition position;
            CSMInstrument instrument;
            //On boucle sur les folios 
            double asset = 0;
            double fxspot = 0;
            double sumasset = 0;
            double ytm = 0;
            
      
                int positionNumber = portfolio.GetTreeViewPositionCount();
                for (int index = 0; index < positionNumber; index++)
                {
                    position = portfolio.GetNthTreeViewPosition(index);
                    if (position.GetInstrumentCount() != 0)//On ne tient compte que des positions ouvertes
                    {
                        instrument = CSMInstrument.GetInstance(position.GetInstrumentCode());
                        fxspot = CSMMarketData.GetCurrentMarketData().GetForex(position.GetCurrency());
                        asset = position.GetAssetValue() * fxspot * 1000;
                        ytm = Datasource.GetTreeYtmValue(position, instrument);

                        if (asset != 0)
                        {
                            if (ytm < 0) ytm = 0;
                            val += asset * ytm;
                            sumasset += asset;
                        }
            
                    }
                }
                if (sumasset != 0) val = val / sumasset;
    
           
            return val;
        }
        //Fonction Duration Asset
        public static double GetPondDurationCr(CSMPortfolio portfolio, CSMExtraction extraction, DataSourceDurationValue Datasource)
        {
            double val = 0;
            CSMPosition position;
            CSMInstrument instrument;
            //On boucle sur les folios 
            double asset = 0;
            double passet = 0;
            double fxspot = 0;
            double sumasset = 0;

           
                int positionNumber = portfolio.GetTreeViewPositionCount();
                for (int index = 0; index < positionNumber; index++)
                {
                    position = portfolio.GetNthTreeViewPosition(index);
                    if (position.GetInstrumentCount() != 0)//On ne tient compte que des positions ouvertes
                    {
                        instrument = CSMInstrument.GetInstance(position.GetInstrumentCode());
                        fxspot = CSMMarketData.GetCurrentMarketData().GetForex(position.GetCurrency());
                        passet = position.GetAssetValue() * fxspot * 1000;

                        //Différence des cas 

                        //Cas Futures
                        if (instrument.GetInstrumentType() == 'F')
                        {
                            asset = position.GetInstrumentCount() * instrument.GetNotional();
                        }
                        else
                        {
                            asset = passet;
                        }

                        if (asset != 0)
                        {
                            val += asset * Datasource.GetDurationValue(position, instrument);
                            sumasset += asset;
                        }
                    }
                }
                if (sumasset != 0) val = val / sumasset;
         
            return val;
        }
        //Fonction DurationIr Asset
        public static double GetPondDurationIr(CSMPortfolio portfolio, CSMExtraction extraction, DataSourceDurationValueir Datasource)
        {
            double val = 0;
            CSMPosition position;
            CSMInstrument instrument;
            //On boucle sur les folios 
            double asset = 0;
            double passet = 0;
            double fxspot = 0;
            double sumasset = 0;

          
                int positionNumber = portfolio.GetTreeViewPositionCount();
                for (int index = 0; index < positionNumber; index++)
                {
                    position = portfolio.GetNthTreeViewPosition(index);
                    if (position.GetInstrumentCount() != 0)//On ne tient compte que des positions ouvertes
                    {
                        instrument = CSMInstrument.GetInstance(position.GetInstrumentCode());
                       
                            fxspot = CSMMarketData.GetCurrentMarketData().GetForex(position.GetCurrency());
                            passet = position.GetAssetValue() * fxspot * 1000;
                            //Cas Futures
                            if (instrument.GetInstrumentType() == 'F')
                            {
                                asset = position.GetInstrumentCount() * instrument.GetNotional();
                            }
                            else
                            {
                                asset = passet;
                            }
                            if (asset != 0)
                            {
                                if (instrument.GetInstrumentType() == 'O' || instrument.GetInstrumentType() == 'F' || instrument.GetInstrumentType() == 'D')
                                {
                                    val += asset * Datasource.GetDurationValueir(position, instrument);
                                }
                                sumasset += asset;
                            }
                   }
                }
                if (sumasset != 0) val = val / sumasset;
          
            
            return val;
        }

        
        //Fonction retournant le YTM
        public static double GetPondGearingYTM(CSMPortfolio portfolio, CSMExtraction extraction, DataSourceTreeFindYTValueContrib Datasource)
        {
            double val = 0;
            CSMPosition position;
            CSMInstrument instrument;
            //On boucle sur les folios 
            double valNav = 0;
            double asset = 0;
            double fxspot = 0;
            double ytm = 0;
           
                int positionNumber = portfolio.GetTreeViewPositionCount();
                valNav = 0;
                valNav = FonctionAdd.GetNAVFund(portfolio, extraction);
                for (int index = 0; index < positionNumber; index++)
                {
                    position = portfolio.GetNthTreeViewPosition(index);
                    if (position.GetInstrumentCount() != 0)//On ne tient compte que des positions ouvertes
                    {
                        instrument = CSMInstrument.GetInstance(position.GetInstrumentCode());
                        fxspot = CSMMarketData.GetCurrentMarketData().GetForex(position.GetCurrency());
                        asset = position.GetAssetValue() * fxspot * 1000;
                        ytm = Datasource.GetTreeYtmValue(position, instrument);
                        if (valNav != 0) val += asset * ytm / valNav;
                    }
                }
      
          
            return val;

        }

        //Fonction retournant la duration Cr
        public static double GetPondGearingDurationCr(CSMPortfolio portfolio, CSMExtraction extraction, DataSourceDurationValueContrib Datasource)
        {
            double val = 0;

            CSMPosition position;
            CSMInstrument instrument;
            //On boucle sur les folios 
            double valNav = 0;
            double asset = 0;
            double fxspot = 0;

          
                int positionNumber = portfolio.GetTreeViewPositionCount();
                valNav = 0;
                valNav = FonctionAdd.GetNAVFund(portfolio, extraction);
                for (int index = 0; index < positionNumber; index++)
                {
                    position = portfolio.GetNthTreeViewPosition(index);
                    if (position.GetInstrumentCount() != 0)//On ne tient compte que des positions ouvertes
                    {
                        instrument = CSMInstrument.GetInstance(position.GetInstrumentCode());
                        //Cas Futures
                        if (instrument.GetInstrumentType() == 'F')
                        {
                            asset = position.GetInstrumentCount() * instrument.GetNotional();
                        }
                        else
                        {
                            fxspot = CSMMarketData.GetCurrentMarketData().GetForex(position.GetCurrency());
                            asset = position.GetAssetValue() * fxspot * 1000;
                        }
                        if (valNav != 0) val += asset * Datasource.GetDurationValue(position, instrument) / valNav;
                    }
                }
     
            return val;

        }

        //Fonction retournant la duration Ir
        public static double GetPondGearingDurationIr(CSMPortfolio portfolio, CSMExtraction extraction, DataSourceDurationValueirContrib Datasource)
        {
            double val = 0;
            CSMPosition position;
            CSMInstrument instrument;
            //On boucle sur les folios 
            double valNav = 0;
            double asset = 0;
            double fxspot = 0;


            int positionNumber = portfolio.GetTreeViewPositionCount();
            valNav = 0;
            valNav = FonctionAdd.GetNAVFund(portfolio, extraction);
            for (int index = 0; index < positionNumber; index++)
            {
                position = portfolio.GetNthTreeViewPosition(index);
                if (position.GetInstrumentCount() != 0)//On ne tient compte que des positions ouvertes
                {
                    instrument = CSMInstrument.GetInstance(position.GetInstrumentCode());
                    //Cas Futures
                    if (instrument.GetInstrumentType() == 'F')
                    {
                        asset = position.GetInstrumentCount() * instrument.GetNotional();
                    }
                    else
                    {
                        fxspot = CSMMarketData.GetCurrentMarketData().GetForex(position.GetCurrency());
                        asset = position.GetAssetValue() * fxspot * 1000;
                    }
                    if (valNav != 0 && asset != 0) val += asset * Datasource.ComputeTKODurationIr(instrument, VersionClass.Get_ReportingDate(), position) / valNav;
                }
            }


            return val;


        }

    }
}

