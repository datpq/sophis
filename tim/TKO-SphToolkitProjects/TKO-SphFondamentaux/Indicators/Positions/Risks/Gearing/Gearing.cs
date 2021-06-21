using System.Text;
using sophis.portfolio;
using System.Collections;
using sophis.instrument;
using sophis.market_data;
using sophis.utils;
using TkoPortfolioColumn.DataCache;
using sophis.static_data;
using TkoPortfolioColumn.DbRequester;
using System.ComponentModel;
using System;

namespace TkoPortfolioColumn
{
    public static class GearingExtentionMethods
    {
        #region Gearing

        public static double TkoComputeGearingWithoutAbs(this CSMInstrument instrument, InputProvider input, CSMPortfolio portfolio)
        {
            double gearing = 0;
            try
            {
                //DPH
                //var position = input.PortFolio.GetTreeViewPosition(input.PositionIdentifier);
                var position = portfolio.GetTreeViewPosition(input.PositionIdentifier);
                if (position == null) return 0;
                double fxspot = CSMMarketData.GetCurrentMarketData().GetForex(position.GetCurrency());
                double assetvalue;
                double nominal;
                int PositionMvtident = position.GetIdentifier();
                int reportingdate = CSMMarketData.GetCurrentMarketData().GetDate();

                switch (instrument.GetType_API())
                {
                    case 'O'://Obligation
                        assetvalue = position.GetAssetValue() * 1000 * fxspot;
                        gearing = assetvalue;
                        break;

                    case 'S'://Swap
                        assetvalue = position.GetAssetValue() * 1000 * fxspot;
                        nominal = position.GetInstrumentCount() * instrument.GetNotional();
                        gearing = Math.Abs(nominal) + assetvalue;

                        break;

                    case 'P'://Repo
                        assetvalue = position.GetAssetValue() * 1000 * fxspot;
                        gearing = assetvalue;
                        break;

                    case 'A'://Actions
                        assetvalue = position.GetAssetValue() * 1000 * fxspot;
                        gearing = assetvalue;
                        break;

                    case 'Z'://Fund
                        assetvalue = position.GetAssetValue() * 1000 * fxspot;
                        gearing = assetvalue;
                        break;

                    case 'F'://Futures
                        input.TmpPortfolioColName = "Instrument type";
                        sophis.finance.CSMNotionalFuture nominalFuture = instrument;
                        if (nominalFuture != null)
                        {
                            var sicovam = nominalFuture.GetCheapest();
                            SSMCalcul concordanceFactor = new SSMCalcul();

                            nominalFuture.GetConcordanceFactor(CSMMarketData.GetCurrentMarketData(), sicovam, concordanceFactor);
                            CSMBond bond = CSMInstrument.GetInstance(sicovam);

                            //TKO Gearing Nominal Futures = Price of CTD * Contract Size * Number of Securities
                            gearing = Math.Abs(bond.GetValueInPrice() * nominalFuture.GetQuotity() * position.GetInstrumentCount() * fxspot);

                            break;
                        }
                        else
                        {
                            if (Helper.TkoGetValuefromSophisString(input) == "Index Futures")
                            {
                                assetvalue = position.GetDeltaCash() * fxspot;
                                gearing = assetvalue;
                                break;
                            }
                            else
                            {
                                assetvalue = position.GetInstrumentCount() * instrument.GetNotional();
                                gearing = assetvalue;
                                break;
                            }
                        }
                    case 'C'://Cash,fees
                        gearing = 0; //aucune exposition cash, on pourrait faire le CDS de la banque !!!
                        break;

                    case 'E'://Forex
                        gearing = 0; //à revoir
                        break;

                    case 'T'://Billets de treso
                        assetvalue = position.GetAssetValue() * 1000 * fxspot;
                        gearing = assetvalue;
                        break;

                    case 'D'://Options et convertibles
                        CMString otype = "";
                        instrument.GetModelName(otype);
                        string otypes = otype.GetString();

                        input.TmpPortfolioColName = "Allotment";
                        var config = DbrTikehau_Config.GetTikehauConfigFromName("GEARING-OPTIONS-ALLOTMENTS");
                        bool ret = false;
                        foreach (var elt in config)
                        {
                            if (elt.VALUE == Helper.TkoGetValuefromSophisString(input))
                                ret = true;
                        }
                        if (ret)
                        {
                            gearing = position.GetDeltaCash() * fxspot;
                        }
                        else
                        {
                            if (otypes.Equals("CB Model")) //Petite exception pour les convert traitées comme du FI
                            {
                                assetvalue = position.GetAssetValue() * 1000 * fxspot;
                                gearing = assetvalue;
                            }
                            else//option
                            {
                                assetvalue = position.GetAssetValue() * 1000 * fxspot;
                                gearing = assetvalue;
                            }
                        }
                        break;

                    default:
                        break;
                }
                input.IndicatorValue = gearing;
                var ident = position.GetIdentifier();
                return input.IndicatorValue;
            }
            catch (Exception)
            {
                CSMLog.Write("ExtensionMethods", "TkoComputeGearing", CSMLog.eMVerbosity.M_warning, "gearing cannot be computed for position " + input.PositionIdentifier);
                return 0;
            }
        }

        public static double TkoComputeGearing(this CSMInstrument instrument, InputProvider input, CSMPortfolio portfolio)
        {
            double gearing = 0;
            try
            {
                //DPH
                //var position = input.PortFolio.GetTreeViewPosition(input.PositionIdentifier);
                var position = portfolio.GetTreeViewPosition(input.PositionIdentifier);
                if (position == null) return 0;
                double fxspot = CSMMarketData.GetCurrentMarketData().GetForex(position.GetCurrency());
                double assetvalue;
                double nominal;
                int PositionMvtident = position.GetIdentifier();
                int reportingdate = CSMMarketData.GetCurrentMarketData().GetDate();

                switch (instrument.GetType_API())
                {
                    case 'O'://Obligation
                        assetvalue = position.GetAssetValue() * 1000 * fxspot;
                        gearing = assetvalue;
                        break;

                    case 'S'://Swap
                        assetvalue = position.GetAssetValue() * 1000 * fxspot;
                        nominal = position.GetInstrumentCount() * instrument.GetNotional();
                        gearing = Math.Abs(nominal) + assetvalue;

                        break;

                    case 'P'://Repo
                        assetvalue = position.GetAssetValue() * 1000 * fxspot;
                        gearing = assetvalue;
                        break;

                    case 'A'://Actions
                        assetvalue = position.GetAssetValue() * 1000 * fxspot;
                        gearing = assetvalue;
                        break;

                    case 'Z'://Fund
                        assetvalue = position.GetAssetValue() * 1000 * fxspot;
                        gearing = assetvalue;
                        break;

                    case 'F'://Futures
                        input.TmpPortfolioColName = "Instrument type";
                        sophis.finance.CSMNotionalFuture nominalFuture = instrument;
                        if (nominalFuture != null)
                        {
                            var sicovam = nominalFuture.GetCheapest();
                            SSMCalcul concordanceFactor = new SSMCalcul();

                            nominalFuture.GetConcordanceFactor(CSMMarketData.GetCurrentMarketData(), sicovam, concordanceFactor);
                            CSMBond bond = CSMInstrument.GetInstance(sicovam);

                            //TKO Gearing Nominal Futures = Price of CTD * Contract Size * Number of Securities
                            gearing = bond.GetValueInPrice() * nominalFuture.GetQuotity() * position.GetInstrumentCount() * fxspot;

                            break;
                        }
                        else
                        {
                            if (Helper.TkoGetValuefromSophisString(input) == "Index Futures")
                            {
                                assetvalue = position.GetDeltaCash() * fxspot;
                                gearing = assetvalue;
                                break;
                            }
                            else
                            {
                                assetvalue = position.GetInstrumentCount() * instrument.GetNotional();
                                gearing = assetvalue;
                                break;
                            }
                        }
                    case 'C'://Cash,fees
                        gearing = 0; //aucune exposition cash, on pourrait faire le CDS de la banque !!!
                        break;

                    case 'E'://Forex
                        gearing = 0; //à revoir
                        break;

                    case 'T'://Billets de treso
                        assetvalue = position.GetAssetValue() * 1000 * fxspot;
                        gearing = assetvalue;
                        break;

                    case 'D'://Options et convertibles
                        CMString otype = "";
                        instrument.GetModelName(otype);
                        string otypes = otype.GetString();

                        input.TmpPortfolioColName = "Allotment";
                        var config = DbrTikehau_Config.GetTikehauConfigFromName("GEARING-OPTIONS-ALLOTMENTS");
                        bool ret = false;
                        foreach (var elt in config)
                        {
                            if (elt.VALUE == Helper.TkoGetValuefromSophisString(input))
                                ret = true;
                        }
                        if (ret)
                        {
                            gearing = Math.Abs(position.GetDeltaCash() * fxspot);
                        }
                        else
                        {
                            if (otypes.Equals("CB Model")) //Petite exception pour les convert traitées comme du FI
                            {
                                assetvalue = position.GetAssetValue() * 1000 * fxspot;
                                gearing = assetvalue;
                            }
                            else//option
                            {
                                assetvalue = position.GetAssetValue() * 1000 * fxspot;
                                gearing = assetvalue;
                            }
                        }
                        break;

                    default:
                        break;
                }
                input.IndicatorValue = gearing;
                var ident = position.GetIdentifier();
                return input.IndicatorValue;
            }
            catch (Exception)
            {
                CSMLog.Write("ExtensionMethods", "TkoComputeGearing", CSMLog.eMVerbosity.M_warning, "gearing cannot be computed for position " + input.PositionIdentifier);
                return 0;
            }
        }
        #endregion
    }
}