using System.Text;
using System.Collections;
using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;

using sophisTools;
using sophis.gui;
using sophis.value;
using sophis.instrument;
using sophis.market_data;
using sophis.utils;
using sophis.static_data;
using sophis.portfolio;

using TkoPortfolioColumn.DataCache;
using TkoPortfolioColumn.DbRequester;
using System.Configuration;

//@DPH
using Eff.UpgradeUtilities;

namespace TkoPortfolioColumn
{
    public static class GlobalRiskAMFExtentionMethods
    {
        private static readonly CSMHistoricalData Historicalprice = sophis.static_data.CSMHistoricalData.GetInstance();
 
        public static double TkoRouteAmfConsolidation(this CSMInstrument instrument, InputProvider input)
        {
            switch (input.Column)
            {
                case "TKO Gross Commitment":
                    return TkoComputeGlobalRisk(instrument, input);
                case "TKO Leverage":
                    return TkoComputeGlobalRiskLevrage(instrument, input);
                case "TKO Exposure":
                    return TkoComputeAmfExposure(instrument, input);
                default:
                    throw new NotImplementedException("Error AMF Consolidation no method Found to consolidate please check configuration table TIKEHAU_PORTFOLIO_COLUMN");
            }
        }

        #region TKO Levrage
        public static double TkoComputeGlobalRiskLevrage(this CSMInstrument instrument, InputProvider input)
        {

            //@SB
            input.TmpPortfolioColName = "Allotment";
            string allotFlag = Helper.TkoGetValuefromSophisString(input);

            //@SB

            input.TmpPortfolioColName = "Instrument type";
            string instrType = Helper.TkoGetValuefromSophisString(input);

            var nullInstrumentsTypes = DbrTikehau_Config.GetTikehauConfigFromName("INSTRUMENTTYPES-NULL-GLOBAL-RISK-AMF").Where(p => p.VALUE == instrType).FirstOrDefault();

            input.InstrumentReference = input.Instrument.GetReference();
            input.InstrumentType = instrType;

            //if (!input.AllOtherFieldInfos.ContainsKey("AmfForceDelta"))
            //    //input.AllOtherFieldInfos.Add("AmfForceDelta", "NONE");
            //else
            //    //input.AllOtherFieldInfos["AmfForceDelta"] = "NONE";

            input.NumberOfSecurities = input.Position.GetInstrumentCount();
            input.ContractSize = input.Instrument.GetQuotity();
            input.Notional = input.Instrument.GetNotional();
            input.Nominal = input.NumberOfSecurities * input.Notional;

            double ret = double.NaN;

            //@SB

            if (instrument.GetAllotment() == TKOAllotment.AllotmentData.GetIRFuturesCTAllotmentID() ||
                instrument.GetAllotment() == TKOAllotment.AllotmentData.GetIRFuturesAllotmentID() ||
                instrument.GetAllotment() == TKOAllotment.AllotmentData.GetIROptionsCTAllotmentID() ||
                instrument.GetAllotment() == TKOAllotment.AllotmentData.GetIROptionsFuturesAllotmentID() ||
                instrument.GetAllotment() == TKOAllotment.AllotmentData.GetOptionsCDSAllotmentID() ||
                instrument.GetAllotment() == TKOAllotment.AllotmentData.GetIRSwapsAllotmentID() ||
                instrument.GetAllotment() == TKOAllotment.AllotmentData.GetSwaptionsAllotmentID()
                )
            {
                // abs (Notional)
                ret = Math.Abs(input.Nominal);
                input.IndicatorValue = ret;
                return ret;
            }

            //@SB

            if (input.NumberOfSecurities == 0.0)
            {
                ret = 0;
                input.IndicatorValue = ret;
                return 0;
            }
            if (nullInstrumentsTypes != null)
            {
                ret = 0;
                input.IndicatorValue = ret;
                return ret;
            }

            //@SB

            else if (allotFlag.Equals("IR Futures CT", StringComparison.InvariantCultureIgnoreCase) && (input.NumberOfSecurities != 0))
            {

                ret = 0;
                ret = Math.Abs(input.Nominal);
                return ret;


            }
            //else if (allotFlag.Equals("IR Options CT", StringComparison.InvariantCultureIgnoreCase) && (input.NumberOfSecurities != 0))
            //{

                //    input.UnderLyingLast = CSMInstrument.GetLast(input.Instrument.GetUnderlying_API(0));


                //        ret = 0;
            //        ret = input.Nominal * input.UnderLyingLast;
            //        return ret;                   

                //}

            //@SB

            else if (instrType.Equals("Listed Options")
                    || instrType.Equals("Stock Derivatives"))
            {
                CSMOption option = input.Instrument;
                int underlyingcode = input.Instrument.GetUnderlying_API(0);
                input.UnderLyingLast = CSMInstrument.GetLast(underlyingcode);
                input.Strike = option.GetStrike(Historicalprice);

                ret = Math.Abs(input.UnderLyingLast * input.NumberOfSecurities * input.ContractSize);
                input.IndicatorValue = ret;

                //CSMInstrument underlyingInstrument = CSMInstrument.GetInstance(underlyingcode);
                //if (underlyingInstrument != null)
                //    //input.AllOtherFieldInfos.Add("UnderLyingReference", underlyingInstrument.GetReference().StringValue);

                return ret;
            }
            else if (instrType.Equals("Forex"))
            {
                ret = 0;
                input.IndicatorValue = ret;
                return ret;
            }
            else if (instrType.Equals("Exchange Rate Options"))
            {
                int sicovam = input.InstrumentCode;
                input.Delta = input.Instrument.GetDelta(ref sicovam);

                input.Nominal = input.Position.GetInstrumentCount() * input.Instrument.GetNotional();
                input.NumberOfSecurities = input.Position.GetInstrumentCount();
                input.ContractSize = input.Instrument.GetQuotity();
                ret = Math.Abs(input.Nominal);

                input.IndicatorValue = ret;
                return ret;
            }
            else if (instrType.Equals("Forward Forex"))
            {
                CSMAmPortfolio folio = CSMAmPortfolio.GetCSRPortfolio(input.PortFolioCode, input.Extraction);
                CSMAmFund fund = CSMAmFund.GetFundFromFolio(folio);

                Int32 fundCurrencyCode;
                if (fund != null)
                {
                    fundCurrencyCode = fund.GetCurrencyCode();
                }
                else
                {
                    //By default we take EUR currency.
                    CMString cur = new CMString("EUR");
                    fundCurrencyCode = CSMCurrency.StringToCurrency(cur);
                }
                CSMCurrency CurrencyToExclude = CSMCurrency.CreateInstance(fundCurrencyCode);

                CSMForexFuture forexFuture = input.Instrument;

                var reference = forexFuture.GetReference();

                var quantityCurrencyCode = forexFuture.GetExpiryCurrency();
                var amountCurrencyCode = forexFuture.GetCurrencyCode();

                CSMCurrency receivedDevise = CSMCurrency.CreateInstance(quantityCurrencyCode);
                CSMCurrency paidDevise = CSMCurrency.CreateInstance(amountCurrencyCode);

                SSMFxPair currencyPair = new SSMFxPair(quantityCurrencyCode, amountCurrencyCode);
                int valueDays = currencyPair.fNbdVal;
                DateTime reportinDateShiftted = Helper.mydate(input.ReportingDate + valueDays);

                CSMForexSpot forexspot = input.Instrument;
                CSMTransactionVector transactionvector = new CSMTransactionVector();
                input.Position.GetTransactions(transactionvector);
                if (transactionvector.Count > 0)
                {
                    var amount = 0.0;
                    var quantity = 0.0;
                    foreach (CSMTransaction deal in transactionvector)
                    {
                        int date = deal.GetSettlementDate();
                        DateTime settlementDate = Helper.mydate(date);
                        //to change:

                        //if (settlementDate > reportinDateShiftted && TradeDate <=Helper.mydate(input.ReportingDate)) !!!!!!

                        if (settlementDate > reportinDateShiftted)
                        {
                            // the quantity is the amount in ccy1 (for ccy1/ccy2)
                            if (fundCurrencyCode != quantityCurrencyCode)
                            {
                                quantity += deal.GetQuantity();
                            }
                            if (fundCurrencyCode != amountCurrencyCode)
                            {
                                amount += deal.GetNetAmount();
                            }
                        }
                    }

                    int positionCurrencyCode = input.Position.GetCurrency();
                    double fxSpotquantity = CSMMarketData.GetCurrentMarketData().GetForex(quantityCurrencyCode, positionCurrencyCode);
                    double fxspotamount = CSMMarketData.GetCurrentMarketData().GetForex(amountCurrencyCode, positionCurrencyCode);

                    ret = Math.Abs(quantity * fxSpotquantity) + Math.Abs(amount * fxspotamount);

                    //input.AllOtherFieldInfos.Add("ForexSpotQuantity", fxSpotquantity.ToString());
                    //input.AllOtherFieldInfos.Add("ForexSpotAmount", fxspotamount.ToString());
                    //input.AllOtherFieldInfos.Add("SumAmount", amount.ToString());
                    //input.AllOtherFieldInfos.Add("SumQuantity", quantity.ToString());

                    CMString quantityCurrency = new CMString();
                    CSMCurrency.CurrencyToString(quantityCurrencyCode, quantityCurrency);

                    CMString amountCurrency = new CMString();
                    CSMCurrency.CurrencyToString(amountCurrencyCode, amountCurrency);

                    CMString positionCurrency = new CMString();
                    CSMCurrency.CurrencyToString(positionCurrencyCode, positionCurrency);

                    CMString fundCurrency = new CMString();
                    CurrencyToExclude.GetName(fundCurrency);

                    //input.AllOtherFieldInfos.Add("FundCurrency", fundCurrency.StringValue);
                    //input.AllOtherFieldInfos.Add("PositionCurrency", positionCurrency.StringValue);
                    //input.AllOtherFieldInfos.Add("AmountCurrency", amountCurrency.StringValue);
                    //input.AllOtherFieldInfos.Add("QuantityCurrency", quantityCurrency.StringValue);
                    //input.AllOtherFieldInfos.Add("ValueDays", valueDays.ToString());
                    input.IndicatorValue = ret;
                    return ret;
                }
                else
                {
                    throw new Exception("NOT COMPUTED");
                }
            }
            else if (instrType.Equals("Exchange Rate Futures"))
            {
                input.NumberOfSecurities = input.Position.GetInstrumentCount();
                input.ContractSize = input.Instrument.GetQuotity();

                ret = Math.Abs(input.NumberOfSecurities * input.ContractSize);
                input.IndicatorValue = ret;
                return ret;
            }
            else if (instrType.Equals("Index Futures"))
            {
                int underlyingcode = input.Instrument.GetUnderlying_API(0);
                input.UnderLyingLast = CSMInstrument.GetLast(underlyingcode);

                ret = Math.Abs(input.NumberOfSecurities * input.ContractSize * input.UnderLyingLast);
                input.IndicatorValue = ret;

                CSMInstrument underlyingInstrument = CSMInstrument.GetInstance(underlyingcode);
                //if (underlyingInstrument != null)
                //    //input.AllOtherFieldInfos.Add("UnderLyingReference", underlyingInstrument.GetReference().StringValue);
                return ret;
            }
            else if (instrType.Equals("Interest Rate Futures"))
            {
                //@DPH
                //int sicovam = input.InstrumentCode;
                //input.Delta = input.Instrument.GetDelta(ref sicovam);
                //if (input.Delta == 0) input.Delta = 1;
                //input.Nominal = input.Position.GetInstrumentCount() * input.Instrument.GetNotional();
                //CSMInstrument underlying = input.Instrument.GetUnderlyingInstrument();
                //input.UnderLyingLast = CSMInstrument.GetLast(underlying.GetCode());

                //ret = Math.Abs(input.Nominal * input.UnderLyingLast / 100 * input.Delta);
                //input.IndicatorValue = ret;
                //return ret;
                sophis.finance.CSMNotionalFuture nominalFuture = instrument;
                if (nominalFuture != null)
                {
                    int sicovam = nominalFuture.GetCheapest();
                    CSMBond bond = CSMInstrument.GetInstance(sicovam);
                    if (bond != null)
                    {
                        SSMCalcul concordanceFactor = new SSMCalcul();
                        nominalFuture.GetConcordanceFactor(CSMMarketData.GetCurrentMarketData(), sicovam, concordanceFactor);

                        input.ContractSize = nominalFuture.GetQuotity();
                        input.NumberOfSecurities = input.Position.GetInstrumentCount();
                        ret = Math.Abs(bond.GetValueInPrice() * input.NumberOfSecurities * input.ContractSize);
                        input.IndicatorValue = ret;

                        input.UnderLyingLast = bond.GetValueInPrice();
                        //input.AllOtherFieldInfos.Add("UnderLyingReference", bond.GetReference().StringValue);

                        return ret;
                    }
                    else
                    {
                        throw new Exception("NOT COMPUTED");
                    }
                }
                else
                {
                    throw new Exception("NOT COMPUTED");
                }
            }
            else if (instrType.Equals("Credit Default Swaps"))
            {
                CSMSwap cds = instrument;

                var leg0 = cds.GetLeg(0);
                CSMIndexedLeg indexLeg = leg0;
                CSMLeg Leg = leg0;

                int legCode = leg0.GetUnderlyingCode();
                var cdsunderlying = CSMInstrument.GetInstance(legCode);

                //if (cdsunderlying != null)
                //    //input.AllOtherFieldInfos.Add("UnderLyingReference", cdsunderlying.GetReference().StringValue);

                input.NumberOfSecurities = input.Position.GetInstrumentCount();
                input.Nominal = input.Position.GetInstrumentCount() * input.Instrument.GetNotional();

                //input.AllOtherFieldInfos.Add("UnderlyingType", cdsunderlying.GetType_API().ToString());
                if ((cdsunderlying.GetType_API() == 'H' || cdsunderlying.GetType_API() == 'O'))
                {
                    ret = Math.Abs(input.Nominal);
                    input.IndicatorValue = ret;
                }
                else if ((cdsunderlying.GetType_API() == 'I'))
                {
                    ret = Math.Abs(input.Nominal);
                    input.IndicatorValue = ret;
                }
                else
                {
                    throw new Exception("NOT COMPUTED");
                }
                input.ContractSize = input.Instrument.GetQuotity();


                // to do:
                // input.NumberOfSecurities = input.Position.GetInstrumentCount();
                // Math.Abs(input.NumberOfSecurities);


                return ret;
            }
            else if (instrType.Equals("Convertibles and Indexed"))
            {
                CSMOption option = instrument;
                input.ConvertionRatio = option.GetConversionRatioInPrice();
                var reference = option.GetReference();

                input.NumberOfSecurities = input.Position.GetInstrumentCount();
                input.ContractSize = input.Instrument.GetQuotity();

                int underlyingCode = input.Instrument.GetUnderlying_API(0);
                input.UnderLyingLast = CSMInstrument.GetLast(underlyingCode);

                CSMInstrument underlyingInstrument = CSMInstrument.GetInstance(underlyingCode);
                if (option != null)
                {
                    ret = Math.Abs(input.NumberOfSecurities * input.UnderLyingLast * input.ConvertionRatio);
                    input.IndicatorValue = ret;

                    //if (underlyingInstrument != null)
                    //    //input.AllOtherFieldInfos.Add("UnderLyingReference", underlyingInstrument.GetReference().StringValue);
                    return ret;
                }
                else
                {
                    throw new Exception("NOT COMPUTED");
                }
            }
            else if (instrType.Equals("Interest Rate Swaps"))
            {
                input.NumberOfSecurities = input.Position.GetInstrumentCount();
                // input.ContractSize = input.Instrument.GetQuotity();
                input.Notional = input.Instrument.GetNotional();
                input.Nominal = input.NumberOfSecurities * input.Notional;


                // to do:
                // input.NumberOfSecurities = input.Position.GetInstrumentCount();
                // Math.Abs(input.NumberOfSecurities);
                ret = Math.Abs(input.Nominal);
                input.IndicatorValue = ret;
                return ret;
            }
            else if (instrType.Equals("Total Return Swaps"))
            {
                input.NumberOfSecurities = input.Position.GetInstrumentCount();
                //input.ContractSize = input.Instrument.GetQuotity();
                input.Notional = input.Instrument.GetNotional();
                input.Nominal = input.NumberOfSecurities * input.Notional;
                ret = Math.Abs(input.Nominal);
                input.IndicatorValue = ret;

                // to do:
                // input.NumberOfSecurities = input.Position.GetInstrumentCount();
                // Math.Abs(input.NumberOfSecurities);

                return ret;
            }
            else if (instrType.Equals("Interest Rate Derivatives"))
            {
                int sicovam = input.InstrumentCode;
                input.InstrumentCode = input.Instrument.GetUnderlying_API(0);
                string underlyingType = Helper.TkoGetValuefromSophisString(input);
                input.InstrumentCode = sicovam;

                if (underlyingType.Equals("Interest Rate Futures"))
                {
                    input.UnderLyingLast = CSMInstrument.GetLast(input.Instrument.GetUnderlying_API(0));
                    input.NumberOfSecurities = input.Position.GetInstrumentCount();
                    input.ContractSize = input.Instrument.GetQuotity();

                    ret = Math.Abs(input.UnderLyingLast * input.NumberOfSecurities * input.ContractSize);
                    input.IndicatorValue = ret;

                    //CSMInstrument underlyingInstrument = CSMInstrument.GetInstance(input.Instrument.GetUnderlying_API(0));
                    //if (underlyingInstrument != null)
                    //    //input.AllOtherFieldInfos.Add("UnderLyingReference", underlyingInstrument.GetReference().StringValue);

                    return ret;
                }
                else
                {
                    throw new Exception("NOT COMPUTED");
                }
            }
            else if (instrType.Equals("Loans on Stock"))
            {
                input.NumberOfSecurities = input.Position.GetInstrumentCount();
                input.Notional = input.Instrument.GetNotional();
                input.Nominal = input.NumberOfSecurities * input.Notional;
                if (input.NumberOfSecurities > 0)
                {
                    input.IndicatorValue = 0;
                    ret = 0;
                }
                else
                {
                    input.IndicatorValue = Math.Abs(input.Nominal);
                    ret = Math.Abs(input.Nominal);
                }
                return ret;
            }
            return ret;
        }

        #endregion

        #region Tko Exposure

        public static double TkoComputeAmfExposure(this CSMInstrument instrument, InputProvider input)
        {

            //@SB

            input.TmpPortfolioColName = "Allotment";
            string allotFlag = Helper.TkoGetValuefromSophisString(input);

            //@SB

            input.TmpPortfolioColName = "Instrument type";
            string instrType = Helper.TkoGetValuefromSophisString(input);

            var nullInstrumentsTypes = DbrTikehau_Config.GetTikehauConfigFromName("INSTRUMENTTYPES-ASSET-VALUE-RISK-AMF").Where(p => p.VALUE == instrType).FirstOrDefault();
            var amfVolatilityonfig = DbrTikehau_Config.GetTikehauConfigFromName("AMF-CHECK-VOLATILITY").FirstOrDefault().VALUE;

            input.InstrumentReference = input.Instrument.GetReference();
            input.InstrumentType = instrType;

            input.NumberOfSecurities = input.Position.GetInstrumentCount();
            input.ContractSize = input.Instrument.GetQuotity();
            input.Notional = input.Instrument.GetNotional();
            input.Nominal = input.NumberOfSecurities * input.Notional;

            //if (!input.AllOtherFieldInfos.ContainsKey("AmfForceDelta"))
            //    input.AllOtherFieldInfos.Add("AmfForceDelta", "NONE");
            //else
            //    input.AllOtherFieldInfos["AmfForceDelta"] = "NONE";


            double ret = double.NaN;

            //@SB

            // Tikehau Specific
            if (instrument.GetAllotment() == TKOAllotment.AllotmentData.GetIRFuturesCTAllotmentID() ||
                instrument.GetAllotment() == TKOAllotment.AllotmentData.GetCDSAllotmentID() ||
                instrument.GetAllotment() == TKOAllotment.AllotmentData.GetOTCStockDerivativesAllotmentID() ||
                instrument.GetAllotment() == TKOAllotment.AllotmentData.GetIRSwapsAllotmentID() ||
                instrument.GetAllotment() == TKOAllotment.AllotmentData.GetOTCIRDerivativesAllotmentID()
                )
            {
                // Notional
                ret = input.Nominal;
                input.IndicatorValue = ret;
                return ret;
            }
            else if (instrument.GetAllotment() == TKOAllotment.AllotmentData.GetIRFuturesAllotmentID())
            {
                // Notional * price of cheapest
                sophis.finance.CSMNotionalFuture fut = sophis.finance.CSMNotionalFuture.GetInstance(instrument.GetCode());
                if (fut != null) //should always be true
                {
                    SSMCalcul data = new SSMCalcul();
                    int bondCode = fut.GetCheapest();
                    CSMBond bond = CSMBond.GetInstance(bondCode);
                    ret = bond.GetValueInPrice() * input.Nominal / 100.0;
                    input.IndicatorValue = ret;
                    return ret;
                }
            }
            else if (instrument.GetAllotment() == TKOAllotment.AllotmentData.GetIROptionsCTAllotmentID() ||
                instrument.GetAllotment() == TKOAllotment.AllotmentData.GetListedOptionsAllotmentID())
            {
                // number of securities * contract size * underlying last
                CSMOption opt = CSMOption.GetInstance(instrument.GetCode());
                if (opt != null) //should always be true
                {
                    int underlyingCode = opt.GetUnderlying_API(0);

                    input.UnderLyingLast = CSMInstrument.GetLast(underlyingCode);

                    ret = input.UnderLyingLast * input.NumberOfSecurities * input.ContractSize;
                    input.IndicatorValue = ret;
                    return ret;

                }
            }
            else if (instrument.GetAllotment() == TKOAllotment.AllotmentData.GetTRSAllotmentID())
            {
                // Nominal * Underlying Last / Underlying price at start date
                CSMSwap trs = instrument;

                var creditleg = trs.GetLeg(0);
                CSMIndexedLeg indexLeg = creditleg;
                CSMLeg Leg = creditleg;

                int legCode = creditleg.GetUnderlyingCode();
                var leg = CSMInstrument.GetInstance(legCode);

                double underlyingInitialPrice = 0;
                if (indexLeg != null)
                {
                    legCode = indexLeg.GetIndex();
                    underlyingInitialPrice = indexLeg.GetInitialIndexValue();
                    leg = CSMInstrument.GetInstance(legCode);

                    //input.AllOtherFieldInfos.Add("PaymentLeg", leg.GetReference().ToString());
                }
                else if (Leg != null)
                {
                    if (legCode == 0)
                    {
                        creditleg = trs.GetLeg(1);
                        legCode = creditleg.GetUnderlyingCode();
                    }
                    underlyingInitialPrice = Historicalprice.GetFixing(legCode, 2, input.Position.GetCSRInstrument().GetStartDate(), false, true);
                    if (underlyingInitialPrice == -10000000.0)
                        underlyingInitialPrice = 0.0;

                    leg = CSMInstrument.GetInstance(legCode);
                    //input.AllOtherFieldInfos.Add("PaymentLeg", leg.GetReference().ToString());
                }

                if (underlyingInitialPrice != 0.0)
                    ret = input.Nominal * input.UnderLyingLast / underlyingInitialPrice;
                else
                    ret = 0.0;
                input.IndicatorValue = ret;
                return ret;
            }
            //else if (instrument.GetAllotment() == TKOAllotment.AllotmentData.GetOptionsCDSAllotmentID())
            //{
            //    // delta * Notional
            //    CSMOption opt = CSMOption.GetInstance(instrument.GetCode());
            //    if (opt != null) //should always be true
            //    {
            //        int underlyingCode = 0;
            //        double delta = opt.GetDelta(ref underlyingCode);
            //        ret = delta * input.Nominal;
            //        input.IndicatorValue = ret;
            //        return ret;
            //    }
            //}
            //else if (instrument.GetAllotment() == TKOAllotment.AllotmentData.GetSwapsAllotmentID())
            //{
            //    // Notional
            //    ret = input.Nominal;
            //    input.IndicatorValue = ret;
            //    return ret;
            //}
            //else if (instrument.GetAllotment() == TKOAllotment.AllotmentData.GetSwaptionsAllotmentID())
            //{
            //    // delta * Notional
            //    CSMOption opt = instrument;
            //    if (opt != null) //should always be true
            //    {
            //        int underlyingCode = 0;
            //        double delta = opt.GetDelta(ref underlyingCode);
            //        ret = delta * input.Nominal;
            //        input.IndicatorValue = ret;
            //        return ret;
            //    }
            //}

            //@SB

            if (input.NumberOfSecurities == 0.0)
            {
                ret = 0;
                input.IndicatorValue = ret;
                return 0;
            }

            //@SB

            else if (allotFlag.Equals("IR Futures CT", StringComparison.InvariantCultureIgnoreCase) && (input.NumberOfSecurities != 0))
            {
                input.TmpPortfolioColName = "PV01";
                var coff = Helper.TkoGetValuefromSophisDouble(input) * (-1) * input.NumberOfSecurities * 100 / input.Nominal;
                if (coff < 1)
                {
                    ret = 0;
                    ret = input.Nominal * coff;
                    return ret;
                }
                else
                {
                    ret = 0;
                    ret = input.Nominal;
                    return ret;
                }

            }

            else if (nullInstrumentsTypes != null)//@SB
            {
                var quotity = 1000;
                ret = input.Position.GetAssetValue() * quotity;

                //input.AllOtherFieldInfos.Add("Hard Coded Quotity", quotity.ToString());
                //input.AllOtherFieldInfos.Add("Sophis Instrument Quotity",  input.Instrument.GetQuotity().ToString());
                //input.AllOtherFieldInfos.Add("AssetValue", ret.ToString());
                input.IndicatorValue = ret;
                return ret;
            }
            else if (instrType.Equals("Listed Options")
                    || instrType.Equals("Stock Derivatives"))
            {
                CSMOption option = input.Instrument;
                int underlyingcode = input.Instrument.GetUnderlying_API(0);
                input.UnderLyingLast = CSMInstrument.GetLast(underlyingcode);
                input.Strike = option.GetStrike(Historicalprice);

                int sicovam = input.InstrumentCode;
                input.Delta = option.GetDelta(ref sicovam);

                input.Volatility = 0;
                if (amfVolatilityonfig == "Y")
                {
                    //override delta.
                    input.Volatility = option.GetVolatility(CSMMarketData.GetCurrentMarketData());
                    if (input.Volatility == 0)
                    {
                        input.Delta = 1;
                        input.AllOtherFieldInfos["AmfForceDelta"] = "FORCE";
                    }
                }

                ret = input.UnderLyingLast * input.NumberOfSecurities * input.ContractSize * input.Delta;
                input.IndicatorValue = ret;

                //CSMInstrument underlyingInstrument = CSMInstrument.GetInstance(underlyingcode);
                //if (underlyingInstrument != null)
                //    input.AllOtherFieldInfos.Add("UnderLyingReference", underlyingInstrument.GetReference().StringValue);

                return ret;
            }
            else if (instrType.Equals("Forex"))
            {
                ret = 0;
                input.IndicatorValue = ret;
                return ret;
            }
            else if (instrType.Equals("Exchange Rate Options"))
            {
                //ret = 0;
                CSMOption option = input.Instrument;
                int sicovam = input.InstrumentCode;
                input.Delta = option.GetDelta(ref sicovam);
                ret = -1 * input.Nominal * input.Delta;
                input.IndicatorValue = ret;

                return ret;
            }
            else if (instrType.Equals("Forward Forex"))
            {
                //ret = 0;
                ret = -1 * input.NumberOfSecurities;
                input.IndicatorValue = ret;
                return ret;
            }
            else if (instrType.Equals("Exchange Rate Futures"))
            {
                //ret = 0;
                ret = -1 * input.ContractSize * input.NumberOfSecurities;
                input.IndicatorValue = ret;
                return ret;
            }
            else if (instrType.Equals("Index Futures"))
            {
                int underlyingcode = input.Instrument.GetUnderlying_API(0);
                input.UnderLyingLast = CSMInstrument.GetLast(underlyingcode);

                ret = input.NumberOfSecurities * input.ContractSize * input.UnderLyingLast;
                input.IndicatorValue = ret;

                //CSMInstrument underlyingInstrument = CSMInstrument.GetInstance(underlyingcode);
                //if (underlyingInstrument != null)
                //    //input.AllOtherFieldInfos.Add("UnderLyingReference", underlyingInstrument.GetReference().StringValue);
                return ret;
            }
            else if (instrType.Equals("Interest Rate Futures"))
            {
                //to delete
                sophis.finance.CSMNotionalFuture nominalFuture = instrument;
                if (nominalFuture != null)
                {
                    int sicovam = nominalFuture.GetCheapest();
                    CSMBond bond = CSMInstrument.GetInstance(sicovam);
                    if (bond != null)
                    {
                        SSMCalcul concordanceFactor = new SSMCalcul();
                        nominalFuture.GetConcordanceFactor(CSMMarketData.GetCurrentMarketData(), sicovam, concordanceFactor);

                        input.ContractSize = nominalFuture.GetQuotity();
                        input.NumberOfSecurities = input.Position.GetInstrumentCount();

                        //Correctif 11-19-2015 => / 100
                        ret = bond.GetValueInPrice() / 100 * input.NumberOfSecurities * input.ContractSize;
                        input.IndicatorValue = ret;

                        input.UnderLyingLast = bond.GetValueInPrice();
                        //input.AllOtherFieldInfos.Add("UnderLyingReference", bond.GetReference().StringValue);

                        return ret;
                    }
                    else
                    {
                        // ?????
                        throw new Exception("NOT COMPUTED");
                    }
                }
                else
                {
                    throw new Exception("NOT COMPUTED");
                }
            }
            else if (instrType.Equals("Credit Default Swaps"))
            {
                CSMSwap cds = instrument;

                var leg0 = cds.GetLeg(0);
                CSMIndexedLeg indexLeg = leg0;
                CSMLeg Leg = leg0;

                int legCode = leg0.GetUnderlyingCode();
                var cdsunderlying = CSMInstrument.GetInstance(legCode);

                //if (cdsunderlying != null)
                //    //input.AllOtherFieldInfos.Add("UnderLyingReference", cdsunderlying.GetReference().StringValue);

                input.NumberOfSecurities = input.Position.GetInstrumentCount();
                input.Nominal = input.Position.GetInstrumentCount() * input.Instrument.GetNotional();


                string UnderlyingType = cdsunderlying.GetType_API().ToString();
                //input.AllOtherFieldInfos.Add("UnderlyingType", UnderlyingType);
                input.UnderLyingLast = CSMInstrument.GetLast(legCode);

                if ((UnderlyingType[0] == 'H' || UnderlyingType[0] == 'O'))
                {
                    if (input.UnderLyingLast == -10000000.0)
                    {
                        ret = input.Nominal;
                        input.IndicatorValue = ret;
                    }
                    else if (input.NumberOfSecurities < 0)
                    {
                        //ret = Math.Max(input.Nominal, input.Nominal * input.UnderLyingLast / 100 );

                        ret = Math.Min(input.Nominal, input.Nominal * input.UnderLyingLast / 100);
                    }
                    else
                    {
                        ret = input.Nominal * input.UnderLyingLast / 100;
                        input.IndicatorValue = ret;
                    }
                }
                else if ((UnderlyingType[0] == 'I'))
                {
                    ret = input.Nominal;
                    input.IndicatorValue = ret;
                }
                else
                {
                    throw new Exception("NOT COMPUTED");
                }
                input.ContractSize = input.Instrument.GetQuotity();
                return ret;
            }
            else if (instrType.Equals("Convertibles and Indexed"))
            {
                CSMOption option = instrument;
                input.NumberOfSecurities = input.Position.GetInstrumentCount();
                input.ContractSize = input.Instrument.GetQuotity();
                input.ConvertionRatio = option.GetConversionRatioInPrice();

                int underlyingCode = input.Instrument.GetUnderlying_API(0);
                input.UnderLyingLast = CSMInstrument.GetLast(underlyingCode);

                int sicovam = input.InstrumentCode;
                input.Delta = option.GetDelta(ref sicovam);

                CSMInstrument underlyingInstrument = CSMInstrument.GetInstance(underlyingCode);
                input.Volatility = 0;
                if (option != null)
                {
                    input.Volatility = option.GetVolatility(CSMMarketData.GetCurrentMarketData());
                    if (amfVolatilityonfig == "Y")
                    {
                        //override delta.
                        if (input.Volatility == 0)
                        {
                            input.Delta = 1;
                            //input.AllOtherFieldInfos["AmfForceDelta"] = "FORCE";
                        }
                    }

                    ret = input.NumberOfSecurities * input.UnderLyingLast * input.ConvertionRatio * input.Delta;
                    input.IndicatorValue = ret;

                    //if (underlyingInstrument != null)
                    //    //input.AllOtherFieldInfos.Add("UnderLyingReference",underlyingInstrument.GetReference().StringValue);
                    return ret;
                }
                else
                {
                    throw new Exception("NOT COMPUTED");
                }
            }
            else if (instrType.Equals("Interest Rate Swaps"))
            {
                input.NumberOfSecurities = input.Position.GetInstrumentCount();
                input.ContractSize = input.Instrument.GetQuotity();
                input.Notional = input.Instrument.GetNotional();
                input.Nominal = input.NumberOfSecurities * input.Notional;
                ret = input.Nominal;
                input.IndicatorValue = ret;
                return ret;
            }
            else if (instrType.Equals("Total Return Swaps"))
            {
                CSMSwap cds = instrument;

                var creditleg = cds.GetLeg(0);
                CSMIndexedLeg indexLeg = creditleg;
                CSMLeg Leg = creditleg;

                int legCode = creditleg.GetUnderlyingCode();
                var leg = CSMInstrument.GetInstance(legCode);

                double underlyingInitialPrice = 0;
                if (indexLeg != null)
                {
                    legCode = indexLeg.GetIndex();
                    underlyingInitialPrice = indexLeg.GetInitialIndexValue();
                    leg = CSMInstrument.GetInstance(legCode);

                    //input.AllOtherFieldInfos.Add("PaymentLeg", leg.GetReference().ToString());
                }
                else if (Leg != null)
                {
                    if (legCode == 0)
                    {
                        creditleg = cds.GetLeg(1);
                        legCode = creditleg.GetUnderlyingCode();
                    }
                    underlyingInitialPrice = Historicalprice.GetFixing(legCode, 2, input.Position.GetCSRInstrument().GetStartDate(), false, true);
                    if (underlyingInitialPrice == -10000000.0)
                        underlyingInitialPrice = 0.0;

                    leg = CSMInstrument.GetInstance(legCode);
                    //input.AllOtherFieldInfos.Add("PaymentLeg", leg.GetReference().ToString());
                }

                if (!underlyingInitialPrice.Equals(0))
                {
                    input.NumberOfSecurities = input.Position.GetInstrumentCount();
                    input.ContractSize = input.Instrument.GetQuotity();
                    input.Nominal = input.NumberOfSecurities * input.Instrument.GetNotional();
                    input.UnderLyingLast = CSMInstrument.GetLast(legCode);
                    ret = input.Nominal * input.UnderLyingLast / underlyingInitialPrice;
                    input.IndicatorValue = ret;

                    //input.AllOtherFieldInfos.Add("UnderlyingInitialPrice", underlyingInitialPrice.ToString());
                    return ret;
                }
                else
                {
                    ret = input.Nominal;
                    input.IndicatorValue = ret;
                }
            }
            else if (instrType.Equals("Interest Rate Derivatives"))
            {
                int sicovam = input.InstrumentCode;
                input.InstrumentCode = input.Instrument.GetUnderlying_API(0);
                string underlyingType = Helper.TkoGetValuefromSophisString(input);
                input.InstrumentCode = sicovam;

                input.Delta = input.Instrument.GetDelta(ref sicovam);
                //input.AllOtherFieldInfos.Add("UnderlyingType", underlyingType);

                if (underlyingType.Equals("Interest Rate Futures"))
                {
                    input.UnderLyingLast = CSMInstrument.GetLast(input.Instrument.GetUnderlying_API(0));
                    input.NumberOfSecurities = input.Position.GetInstrumentCount();
                    input.ContractSize = input.Instrument.GetQuotity();

                    if (amfVolatilityonfig == "Y")
                    {
                        CSMOption option = input.Instrument;
                        input.Volatility = option.GetVolatility(CSMMarketData.GetCurrentMarketData());
                        if (input.Volatility == 0)
                        {
                            input.Delta = 1;
                            //input.AllOtherFieldInfos["AmfForceDelta"] = "FORCE";
                        }
                    }

                    //Correctif 11-19-2015 => / 100
                    ret = input.UnderLyingLast / 100 * input.NumberOfSecurities * input.ContractSize * input.Delta;
                    input.IndicatorValue = ret;

                    //CSMInstrument underlyingInstrument = CSMInstrument.GetInstance(input.Instrument.GetUnderlying_API(0));
                    //if (underlyingInstrument != null)
                    //    //input.AllOtherFieldInfos.Add("UnderLyingReference", underlyingInstrument.GetReference().StringValue);

                    return ret;
                }
                else
                {
                    throw new Exception("NOT COMPUTED");
                }
            }
            else if (instrType.Equals("Loans on Stock"))
            {
                input.NumberOfSecurities = input.Position.GetInstrumentCount();
                input.Notional = input.Instrument.GetNotional();
                input.Nominal = input.NumberOfSecurities * input.Notional;
                input.UnderLyingLast = CSMInstrument.GetLast(input.Instrument.GetUnderlying_API(0));
                if (input.NumberOfSecurities > 0)
                {
                    input.IndicatorValue = 0;
                    ret = 0;
                }
                else
                {
                    ret = input.NumberOfSecurities * input.ContractSize * input.UnderLyingLast;
                    input.IndicatorValue = ret;
                }
                return ret;
            }
            return ret;
        }
        #endregion

        #region Global Risk AMF

        public static double TkoComputeGlobalRisk(this CSMInstrument instrument, InputProvider input)
        {

            //@SB

            input.TmpPortfolioColName = "Allotment";
            string allotFlag = Helper.TkoGetValuefromSophisString(input);

            //@SB

            input.TmpPortfolioColName = "Instrument type";
            string instrType = Helper.TkoGetValuefromSophisString(input);

            var nullInstrumentsTypes = DbrTikehau_Config.GetTikehauConfigFromName("INSTRUMENTTYPES-NULL-GLOBAL-RISK-AMF").Where(p => p.VALUE == instrType).FirstOrDefault();
            var amfVolatilityonfig = DbrTikehau_Config.GetTikehauConfigFromName("AMF-CHECK-VOLATILITY").FirstOrDefault().VALUE;

            input.InstrumentReference = input.Instrument.GetReference();
            input.InstrumentType = instrType;

            input.MarketDataDate = String.Format("{0:dd/MM/yyyy}", Helper.mydate(CSMMarketData.GetCurrentMarketData().GetDate()));
            //if (!input.AllOtherFieldInfos.ContainsKey("AmfForceDelta"))
            //    //input.AllOtherFieldInfos.Add("AmfForceDelta", "NONE");
            //else
            //    //input.AllOtherFieldInfos["AmfForceDelta"] = "NONE";

            input.NumberOfSecurities = input.Position.GetInstrumentCount();
            input.ContractSize = input.Instrument.GetQuotity();
            input.Notional = input.Instrument.GetNotional();
            input.Nominal = input.NumberOfSecurities * input.Notional;

            double ret = double.NaN;

            //@SB

            // Tikehau Specific
            if (instrument.GetAllotment() == TKOAllotment.AllotmentData.GetIRFuturesCTAllotmentID())
            {
                // abs (Notional)
                ret = Math.Abs(input.Nominal);
                input.IndicatorValue = ret;
                return ret;
            }
            else if (instrument.GetAllotment() == TKOAllotment.AllotmentData.GetIRFuturesAllotmentID())
            {
                // Notional * price of cheapest
                sophis.finance.CSMNotionalFuture fut = sophis.finance.CSMNotionalFuture.GetInstance(instrument.GetCode());
                if (fut != null) //should always be true
                {
                    SSMCalcul data = new SSMCalcul();
                    int bondCode = fut.GetCheapest();
                    CSMBond bond = CSMBond.GetInstance(bondCode);
                    ret = Math.Abs(bond.GetValueInPrice() * input.Nominal / 100.0);
                    input.IndicatorValue = ret;
                    return ret;
                }
            }
            else if (instrument.GetAllotment() == TKOAllotment.AllotmentData.GetIROptionsCTAllotmentID())
            {
                // delta * Notional
                CSMOption opt = CSMOption.GetInstance(instrument.GetCode());
                if (opt != null) //should always be true
                {
                    int underlyingCode = 0;
                    double delta = opt.GetDelta(ref underlyingCode);
                    ret = Math.Abs(delta * input.Nominal);
                    input.IndicatorValue = ret;
                    return ret;
                }
            }
            else if (instrument.GetAllotment() == TKOAllotment.AllotmentData.GetIROptionsFuturesAllotmentID())
            {
                // delta * Notional
                CSMOption opt = CSMOption.GetInstance(instrument.GetCode());
                if (opt != null) //should always be true
                {
                    int underlyingCode = 0;
                    double delta = opt.GetDelta(ref underlyingCode);
                    ret = Math.Abs(delta * input.Nominal);
                    input.IndicatorValue = ret;
                    return ret;
                }
            }
            else if (instrument.GetAllotment() == TKOAllotment.AllotmentData.GetOptionsCDSAllotmentID())
            {
                // delta * Notional
                CSMOption opt = CSMOption.GetInstance(instrument.GetCode());
                if (opt != null) //should always be true
                {
                    int underlyingCode = 0;
                    double delta = opt.GetDelta(ref underlyingCode);
                    ret = Math.Abs(delta * input.Nominal);
                    input.IndicatorValue = ret;
                    return ret;
                }
            }
            else if (instrument.GetAllotment() == TKOAllotment.AllotmentData.GetIRSwapsAllotmentID())
            {
                // Notional
                ret = Math.Abs(input.Nominal);
                input.IndicatorValue = ret;
                return ret;
            }
            else if (instrument.GetAllotment() == TKOAllotment.AllotmentData.GetSwaptionsAllotmentID())
            {
                // delta * Notional
                CSMOption opt = CSMOption.GetInstance(instrument.GetCode());
                if (opt != null) //should always be true
                {
                    int underlyingCode = 0;
                    double delta = opt.GetDelta(ref underlyingCode);
                    ret = Math.Abs(delta * input.Nominal);
                    input.IndicatorValue = ret;
                    return ret;
                }
            }

            //@SB

            if (input.NumberOfSecurities == 0.0)
            {
                ret = 0;
                input.IndicatorValue = ret;
                return ret;
            }
            if (nullInstrumentsTypes != null)
            {
                ret = 0;
                input.IndicatorValue = ret;
                return ret;
            }

            //@SB

            else if (allotFlag.Equals("IR Futures CT", StringComparison.InvariantCultureIgnoreCase) && (input.NumberOfSecurities != 0))
            {
                input.TmpPortfolioColName = "PV01";
                return Helper.TkoGetValuefromSophisDouble(input);

                //input.TmpPortfolioColName = "PV01";
                //var coff = Helper.TkoGetValuefromSophisDouble(input) *(-1)* input.NumberOfSecurities * 100 / input.Nominal;
                //if (coff < 1)
                //{
                //    ret = 0;
                //    ret = input.Nominal * coff;
                //    return Math.Abs(ret);
                //}
                //else
                //{
                //    ret = 0;
                //    ret = input.Nominal;
                //    return Math.Abs(ret);
                //}

            }
            //else if (allotFlag.Equals("IR Options CT", StringComparison.InvariantCultureIgnoreCase) && (input.NumberOfSecurities != 0))
            //{
            //    input.TmpPortfolioColName = "PV01";
            //    var coff = Helper.TkoGetValuefromSophisDouble(input) * (-1) * input.NumberOfSecurities * 100 / input.Nominal;
            //    input.UnderLyingLast = CSMInstrument.GetLast(input.Instrument.GetUnderlying_API(0));
            //    int sicovam = input.InstrumentCode;
            //    input.Delta = input.Instrument.GetDelta(ref sicovam);
            //    if (coff < 1)
            //    {
            //        ret = 0;
            //        ret = input.Nominal * coff * input.UnderLyingLast * input.Delta;
            //        return ret;
            //    }
            //    else
            //    {
            //        ret = 0;
            //        ret = input.Nominal * input.UnderLyingLast * input.Delta;
            //        return ret;
            //    }

                //}

            //@SB

            else if (instrType.Equals("Listed Options")
                    || instrType.Equals("Stock Derivatives"))
            {
                CSMOption option = input.Instrument;
                int sicovam = input.InstrumentCode;
                input.Delta = input.Instrument.GetDelta(ref sicovam);
                if (input.Delta == 0) input.Delta = 1;

                input.NumberOfSecurities = input.Position.GetInstrumentCount();
                input.ContractSize = input.Instrument.GetQuotity();

                CSMInstrument underlying = input.Instrument.GetUnderlyingInstrument();
                input.UnderLyingLast = CSMInstrument.GetLast(underlying.GetCode());

                input.Volatility = 0;
                if (amfVolatilityonfig == "Y")
                {
                    //override delta.
                    input.Volatility = option.GetVolatility(CSMMarketData.GetCurrentMarketData());
                    if (input.Volatility == 0)
                    {
                        input.Delta = 1;
                        //input.AllOtherFieldInfos["AmfForceDelta"] = "FORCE";
                    }
                }

                ret = Math.Abs(input.Delta * input.UnderLyingLast * input.NumberOfSecurities * input.ContractSize);
                input.IndicatorValue = ret;

                //if (underlying != null)
                //input.AllOtherFieldInfos.Add("UnderLyingReference", underlying.GetReference().StringValue);

                return ret;

            }
            else if (instrType.Equals("Forex"))
            {
                ret = 0;
                input.IndicatorValue = ret;
                return ret;
            }
            else if (instrType.Equals("Exchange Rate Options"))
            {
                int sicovam = input.InstrumentCode;
                input.Delta = input.Instrument.GetDelta(ref sicovam);

                CSMOption option = input.Instrument;
                //_ToCheck_ Delta Cash
                //double deltaCash = input.Position.GetDeltaCash();
                input.Volatility = 0;
                if (amfVolatilityonfig == "Y")
                {
                    //override delta.
                    input.Volatility = option.GetVolatility(CSMMarketData.GetCurrentMarketData());
                    if (input.Volatility == 0)
                    {
                        input.Delta = 1;
                        //input.AllOtherFieldInfos["AmfForceDelta"] = "FORCE";
                    }
                }

                // to check : if we test directly delta === 0 to see whether the result will be different or not ==> OK

                input.Nominal = input.Position.GetInstrumentCount() * input.Instrument.GetNotional();
                input.NumberOfSecurities = input.Position.GetInstrumentCount();
                input.ContractSize = input.Instrument.GetQuotity();
                ret = Math.Abs(input.Nominal * input.Delta);

                input.IndicatorValue = ret;
                return ret;
            }
            else if (instrType.Equals("Forward Forex"))
            {
                CSMAmPortfolio folio = CSMAmPortfolio.GetCSRPortfolio(input.PortFolioCode, input.Extraction);
                CSMAmFund fund = CSMAmFund.GetFundFromFolio(folio);
                Int32 fundCurrencyCode;
                if (fund != null)
                {
                    fundCurrencyCode = fund.GetCurrencyCode();
                }
                else
                {
                    //By default we take EUR currency.
                    CMString cur = new CMString("EUR");
                    fundCurrencyCode = CSMCurrency.StringToCurrency(cur);
                }
                CSMCurrency CurrencyToExclude = CSMCurrency.CreateInstance(fundCurrencyCode);

                CSMForexFuture forexFuture = input.Instrument;

                var reference = forexFuture.GetReference();

                var quantityCurrencyCode = forexFuture.GetExpiryCurrency();
                var amountCurrencyCode = forexFuture.GetCurrencyCode();

                CSMCurrency receivedDevise = CSMCurrency.CreateInstance(quantityCurrencyCode);
                CSMCurrency paidDevise = CSMCurrency.CreateInstance(amountCurrencyCode);

                SSMFxPair currencyPair = new SSMFxPair(quantityCurrencyCode, amountCurrencyCode);
                int valueDays = currencyPair.fNbdVal;
                DateTime reportinDateShiftted = Helper.mydate(input.ReportingDate + valueDays);

                CSMForexSpot forexspot = input.Instrument;
                CSMTransactionVector transactionvector = new CSMTransactionVector();
                input.Position.GetTransactions(transactionvector);
                if (transactionvector.Count > 0)
                {
                    var amount = 0.0;
                    var quantity = 0.0;
                    foreach (CSMTransaction deal in transactionvector)
                    {
                        int date = deal.GetSettlementDate();
                        DateTime settlementDate = Helper.mydate(date);

                        //to change:

                        //if (settlementDate > reportinDateShiftted && TradeDate <=Helper.mydate(input.ReportingDate)) !!!!!!
                        if (settlementDate > reportinDateShiftted)
                        {
                            // the quantity is the amount in ccy1 (for ccy1/ccy2)
                            if (fundCurrencyCode != quantityCurrencyCode)
                            {
                                quantity += deal.GetQuantity();
                            }
                            if (fundCurrencyCode != amountCurrencyCode)
                            {
                                amount += deal.GetNetAmount();
                            }
                        }
                    }

                    int positionCurrencyCode = input.Position.GetCurrency();
                    double fxSpotquantity = CSMMarketData.GetCurrentMarketData().GetForex(quantityCurrencyCode, positionCurrencyCode);
                    double fxspotamount = CSMMarketData.GetCurrentMarketData().GetForex(amountCurrencyCode, positionCurrencyCode);

                    ret = Math.Abs(quantity * fxSpotquantity) + Math.Abs(amount * fxspotamount);

                    //input.AllOtherFieldInfos.Add("ForexSpotQuantity", fxSpotquantity.ToString());
                    //input.AllOtherFieldInfos.Add("ForexSpotAmount", fxspotamount.ToString());
                    //input.AllOtherFieldInfos.Add("SumAmount", amount.ToString());
                    //input.AllOtherFieldInfos.Add("SumQuantity", quantity.ToString());

                    CMString quantityCurrency = new CMString();
                    CSMCurrency.CurrencyToString(quantityCurrencyCode, quantityCurrency);

                    CMString amountCurrency = new CMString();
                    CSMCurrency.CurrencyToString(amountCurrencyCode, amountCurrency);

                    CMString positionCurrency = new CMString();
                    CSMCurrency.CurrencyToString(positionCurrencyCode, positionCurrency);

                    CMString fundCurrency = new CMString();
                    CurrencyToExclude.GetName(fundCurrency);

                    //input.AllOtherFieldInfos.Add("FundCurrency", fundCurrency.StringValue);
                    //input.AllOtherFieldInfos.Add("PositionCurrency", positionCurrency.StringValue);
                    //input.AllOtherFieldInfos.Add("AmountCurrency", amountCurrency.StringValue);
                    //input.AllOtherFieldInfos.Add("QuantityCurrency", quantityCurrency.StringValue);
                    //input.AllOtherFieldInfos.Add("ValueDays", valueDays.ToString());
                    input.IndicatorValue = ret;
                    return ret;
                }
                else
                {
                    throw new Exception("NOT COMPUTED");
                }
            }
            else if (instrType.Equals("Exchange Rate Futures"))
            {
                double nbSecurities = input.Position.GetInstrumentCount();
                double contractsize = input.Instrument.GetQuotity();

                ret = Math.Abs(nbSecurities * contractsize);
                input.IndicatorValue = ret;
                input.NumberOfSecurities = nbSecurities;
                input.ContractSize = contractsize;
                return ret;
            }
            else if (instrType.Equals("Index Futures"))
            {
                input.NumberOfSecurities = input.Position.GetInstrumentCount();
                input.ContractSize = input.Instrument.GetQuotity();
                int underlyingcode = input.Instrument.GetUnderlying_API(0);
                input.UnderLyingLast = CSMInstrument.GetLast(underlyingcode);

                ret = Math.Abs(input.NumberOfSecurities * input.ContractSize * input.UnderLyingLast);
                input.IndicatorValue = ret;

                //CSMInstrument underlying= CSMInstrument.GetInstance(input.Instrument.GetUnderlying_API(0));
                //if (underlying != null)
                //input.AllOtherFieldInfos.Add("UnderLyingReference", underlying.GetReference().StringValue);
                return ret;
            }
            else if (instrType.Equals("Interest Rate Futures"))
            {
                ////@DPH
                //int sicovam = input.InstrumentCode;
                //input.Delta = input.Instrument.GetDelta(ref sicovam);
                //if (input.Delta == 0) input.Delta = 1;
                //input.Nominal = input.Position.GetInstrumentCount() * input.Instrument.GetNotional();
                //CSMInstrument underlying = input.Instrument.GetUnderlyingInstrument();
                //input.UnderLyingLast = CSMInstrument.GetLast(underlying.GetCode());

                //ret = input.Nominal * input.UnderLyingLast / 100 * input.Delta;
                //input.IndicatorValue = ret;
                //return ret;
                //second
                sophis.finance.CSMNotionalFuture nominalFuture = instrument;
                if (nominalFuture != null)
                {
                    int sicovam = nominalFuture.GetCheapest();
                    CSMBond bond = CSMInstrument.GetInstance(sicovam);
                    if (bond != null)
                    {
                        SSMCalcul concordanceFactor = new SSMCalcul();
                        nominalFuture.GetConcordanceFactor(CSMMarketData.GetCurrentMarketData(), sicovam, concordanceFactor);
                        //int maturity = nominalFuture.GetFutureMaturityForConcordanceFactor(nominalFuture.GetExpiry(), bond.GetMaturity(), eMNotionalFutureRoundingType.M_nfrRoundingToYear);

                        input.ContractSize = nominalFuture.GetQuotity();
                        input.NumberOfSecurities = input.Position.GetInstrumentCount();
                        input.UnderLyingLast = bond.GetValueInPrice();

                        //to check: if UnderLyingLast does not exist to understand!!!!!

                        //by default we take 100 as value :-xle 
                        // that means if underlyinglast does not exist =>  input.UnderLyingLast = 100   !!! 2018-04-30

                        //Correctif 11-19-2015 => / 100
                        ret = Math.Abs(input.UnderLyingLast / 100 * input.NumberOfSecurities * input.ContractSize);
                        input.IndicatorValue = ret;

                        return ret;
                    }
                    else
                    {

                // to do:
                /*

                get underlying name, (interest rate) Ex: Euro 3Months => 3/ 12
                if interest rate >= 1 year
                    factor  = 1
                else
                    factor  = X/12   (X is the months number) 

                ret = Math.Abs(factor * input.NumberOfSecurities * input.ContractSize);
                input.IndicatorValue = ret;

                return ret;
                */
                        throw new Exception("NOT COMPUTED");
                    }
                }
                else
                {
                    throw new Exception("NOT COMPUTED");
                }
            }
            else if (instrType.Equals("Credit Default Swaps"))
            {
                CSMSwap cds = instrument;

                var leg0 = cds.GetLeg(0);
                CSMIndexedLeg indexLeg = leg0;
                CSMLeg Leg = leg0;

                int legCode = leg0.GetUnderlyingCode();
                var cdsunderlying = CSMInstrument.GetInstance(legCode);

                input.NumberOfSecurities = input.Position.GetInstrumentCount();
                input.Nominal = input.Position.GetInstrumentCount() * input.Instrument.GetNotional();
                input.UnderLyingLast = CSMInstrument.GetLast(legCode);
                if (input.UnderLyingLast == -10000000.0)
                    input.UnderLyingLast = 0.0;

                double underlyingLastInPercent = input.UnderLyingLast / 100;

                if ((cdsunderlying.GetType_API() == 'H' || cdsunderlying.GetType_API() == 'O'))
                {
                    if (cdsunderlying.GetType_API() == 'O')
                    {
                        if (input.NumberOfSecurities < 0)
                        {
                            ret = Math.Max(Math.Abs(input.Nominal), Math.Abs(input.Nominal * underlyingLastInPercent));
                            input.IndicatorValue = ret;
                        }
                        else
                        {
                            ret = Math.Abs(input.Nominal * underlyingLastInPercent);
                            input.IndicatorValue = ret;
                        }
                    }
                    else
                    {
                        ret = Math.Abs(input.Nominal);
                        input.IndicatorValue = ret;
                    }

                }
                else if ((cdsunderlying.GetType_API() == 'I'))
                {
                    ret = Math.Abs(input.Nominal);
                    input.IndicatorValue = ret;
                }
                else
                {
                    throw new Exception("NOT COMPUTED");
                }
                input.ContractSize = input.Instrument.GetQuotity();
                return ret;
            }
            else if (instrType.Equals("Convertibles and Indexed"))
            {
                CSMOption option = instrument;
                input.ConvertionRatio = option.GetConversionRatioInPrice();
                input.InstrumentReference = option.GetReference();

                int sicovam = input.InstrumentCode;
                input.Delta = input.Instrument.GetDelta(ref sicovam);

                input.NumberOfSecurities = input.Position.GetInstrumentCount();
                input.ContractSize = input.Instrument.GetQuotity();

                int underlyingCode = input.Instrument.GetUnderlying_API(0);
                input.UnderLyingLast = CSMInstrument.GetLast(underlyingCode);

                CSMInstrument underlyingInstrument = CSMInstrument.GetInstance(underlyingCode);

                if (option != null)
                {
                    double volatility = 0;
                    if (amfVolatilityonfig == "Y")
                    {
                        //override delta.
                        volatility = option.GetVolatility(CSMMarketData.GetCurrentMarketData());
                        if (volatility == 0)
                        {
                            input.Volatility = volatility;
                            input.Delta = 1;
                            //input.AllOtherFieldInfos["AmfForceDelta"] = "FORCE";
                        }
                    }

                    ret = Math.Abs(input.NumberOfSecurities * input.UnderLyingLast * input.Delta * input.ConvertionRatio);
                    //to fix: 
                    //ret = Math.Abs(input.NumberOfSecurities * input.UnderLyingLast * input.Delta * input.ConvertionRatio * input.ContractSize);
                    // input.ContractSize should be included in the calculation

                    input.IndicatorValue = ret;

                    input.ContractSize = input.Instrument.GetQuotity();

                    //if (underlyingInstrument != null)
                    //input.AllOtherFieldInfos.Add("UnderLyingReference", underlyingInstrument.GetReference().StringValue);
                    return ret;
                }
                else
                {
                    throw new Exception("NOT COMPUTED");
                }
            }
            else if (instrType.Equals("Interest Rate Swaps"))
            {
                input.NumberOfSecurities = input.Position.GetInstrumentCount();
                input.ContractSize = input.Instrument.GetQuotity();
                input.Nominal = input.Position.GetInstrumentCount() * input.Instrument.GetNotional();

                ret = Math.Abs(input.Nominal);
                //to fix: 
                //ret = Math.Abs(input.NumberOfSecurities);
                // 


                input.IndicatorValue = ret;
                return ret;
            }
            else if (instrType.Equals("Total Return Swaps"))
            {
                CSMSwap cds = instrument;

                var creditleg = cds.GetLeg(0);
                CSMIndexedLeg indexLeg = creditleg;
                CSMLeg Leg = creditleg;

                int legCode = creditleg.GetUnderlyingCode();
                var leg = CSMInstrument.GetInstance(legCode);

                double underlyingInitialPrice = 0;
                if (indexLeg != null)
                {
                    legCode = indexLeg.GetIndex();
                    underlyingInitialPrice = indexLeg.GetInitialIndexValue();
                    leg = CSMInstrument.GetInstance(legCode);

                    //input.AllOtherFieldInfos.Add("PaymentLeg", leg.GetReference().ToString());
                }
                else if (Leg != null)
                {
                    if (legCode == 0)
                    {
                        creditleg = cds.GetLeg(1);
                        legCode = creditleg.GetUnderlyingCode();
                    }
                    underlyingInitialPrice = Historicalprice.GetFixing(legCode, 2, input.Position.GetCSRInstrument().GetStartDate(), false, true);
                    if (underlyingInitialPrice == -10000000.0)
                        underlyingInitialPrice = 0.0;

                    leg = CSMInstrument.GetInstance(legCode);
                    //input.AllOtherFieldInfos.Add("PaymentLeg", leg.GetReference().ToString());
                }

                if (!underlyingInitialPrice.Equals(0))
                {
                    input.NumberOfSecurities = input.Position.GetInstrumentCount();
                    input.ContractSize = input.Instrument.GetQuotity();
                    input.Nominal = input.NumberOfSecurities * input.Instrument.GetNotional();
                    input.UnderLyingLast = CSMInstrument.GetLast(legCode);
                    ret = Math.Abs(input.Nominal * input.UnderLyingLast / underlyingInitialPrice);
                    input.IndicatorValue = ret;


                    //input.AllOtherFieldInfos.Add("UnderlyingInitialPrice", underlyingInitialPrice.ToString());                            

                    return ret;
                }
                else
                {
                    throw new Exception("NOT COMPUTED");
                }
            }
            else if (instrType.Equals("Interest Rate Derivatives", StringComparison.InvariantCultureIgnoreCase))
            {

                // BG:   initial Position
                int sicovam = input.InstrumentCode;
                input.InstrumentCode = input.Instrument.GetUnderlying_API(0);
                string underlyingType = Helper.TkoGetValuefromSophisString(input);
                input.InstrumentCode = sicovam;

                // to change:
                // there is no need to test whether underlying type is "Interest Rate Futures", we should calculate in all case!!!
                if (underlyingType.Equals("Interest Rate Futures", StringComparison.InvariantCultureIgnoreCase))
                {
                    // BG:   underlying of the initial position, called as Jerry
                    input.Delta = input.Instrument.GetDelta(ref sicovam);

                    input.UnderLyingLast = CSMInstrument.GetLast(input.Instrument.GetUnderlying_API(0));
                    input.NumberOfSecurities = input.Position.GetInstrumentCount();
                    input.ContractSize = input.Instrument.GetQuotity();

                    if (amfVolatilityonfig == "Y")
                    {
                        CSMOption option = input.Instrument;
                        input.Volatility = option.GetVolatility(CSMMarketData.GetCurrentMarketData());
                        if (input.Volatility == 0)
                        {
                            input.Delta = 1;
                            //input.AllOtherFieldInfos["AmfForceDelta"] = "FORCE";
                        }
                    }

                    // BG:  the underlying of the underlying of the initial position, called as Tom
                    // Retrieve Tom's name (interest rate name): Ex: Euro 3Months => 3/ 12
                    /*
                     * to do:
                    if interest rate >= 1 year
                        factor = 1
                    else
                        factor = X / 12(X is the months number)
                    */


                    ret = Math.Abs(input.Delta * input.NumberOfSecurities * (input.UnderLyingLast / 100) * input.ContractSize);
                    // to do:
                    //ret = Math.Abs(input.Delta * input.NumberOfSecurities * (input.UnderLyingLast/100) * input.ContractSize * factor)

                    input.IndicatorValue = ret;

                    input.Delta = input.Delta;

                    //CSMInstrument underlyingInstrument = CSMInstrument.GetInstance(input.Instrument.GetUnderlying_API(0));
                    //if (underlyingInstrument != null)
                    //input.AllOtherFieldInfos.Add("UnderLyingReference", underlyingInstrument.GetReference().StringValue);
                    return ret;

                }
                else
                {
                    throw new Exception("NOT COMPUTED");
                }
            }
            else if (instrType.Equals("Loans on Stock"))
            {
                input.NumberOfSecurities = input.Position.GetInstrumentCount();
                input.ContractSize = input.Instrument.GetQuotity();
                input.UnderLyingLast = CSMInstrument.GetLast(input.Instrument.GetUnderlying_API(0));

                if (input.NumberOfSecurities < 0)
                {
                    //repo
                    ret = Math.Abs(input.NumberOfSecurities * input.ContractSize * input.UnderLyingLast);
                    input.IndicatorValue = ret;
                }
                else
                {
                    //reverse repo
                    ret = 0;
                    input.IndicatorValue = ret;
                }

                //CSMInstrument underlyingInstrument = CSMInstrument.GetInstance(input.Instrument.GetUnderlying_API(0));
                //if (underlyingInstrument != null)
                //    //input.AllOtherFieldInfos.Add("UnderLyingReference", underlyingInstrument.GetReference().StringValue);
                return ret;
            }
            else
            {
                throw new Exception("NOT COMPUTED");
            }
        }
        #endregion 
    }
}
