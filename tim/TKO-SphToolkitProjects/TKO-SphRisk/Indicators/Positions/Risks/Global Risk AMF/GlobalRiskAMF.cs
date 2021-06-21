using System.Text;
using System.Collections;
using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;

using sophisTools;
using sophis.gui;
using sophis.instrument;
using sophis.market_data;
using sophis.utils;
using sophis.static_data;
using sophis.portfolio;

using TkoPortfolioColumn.DataCache;
using TkoPortfolioColumn.DbRequester;
using System.Configuration;
using sophis.value;

//@DPH
using Eff.UpgradeUtilities;

namespace TkoPortfolioColumn
{
    public static class GlobalRiskAMFExtentionMethods
    {
        private static readonly CSMHistoricalData Historicalprice = sophis.static_data.CSMHistoricalData.GetInstance();

        public static double TkoRouteAmfConsolidation(this CSMInstrument instrument, InputProvider input)
        {
            switch (input.Methods)
            {
                case "TkoGlobalRiskAmfPortFolioConsolidation":
                    return TkoComputeGlobalRisk(instrument, input);
                case "TkoGlobalRiskAmfPortFolioConsolidationLevrage":
                    return TkoComputeGlobalRiskLevrage(instrument, input);
                case "TkoGlobalRiskAmfPortFolioConsolidationExposure":
                    return TkoComputeAmfExposure(instrument, input);
                default:
                    throw new NotImplementedException("Error AMF Consolidation no method Found to consolidate please check configuration table TIKEHAU_PORTFOLIO_COLUMN");
            }
        }

        #region TKO Levrage
        public static double TkoComputeGlobalRiskLevrage(this CSMInstrument instrument, InputProvider input)
        {
            if (UpgradeExtensions.IsDebugEnabled())
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(input={0})", input.ToString());
            }


            //@SB

            input.TmpPortfolioColName = "Allotment";
            string allotFlag = Helper.TkoGetValuefromSophisString(input);

            //@SB

            input.TmpPortfolioColName = "Instrument type";
            string instrType = Helper.TkoGetValuefromSophisString(input);
            if (UpgradeExtensions.IsDebugEnabled())
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "instrType={0}", instrType);
            }

            var nullInstrumentsTypes = DbrTikehau_Config.GetTikehauConfigFromName("INSTRUMENTTYPES-NULL-GLOBAL-RISK-AMF").Where(p => p.VALUE == instrType).FirstOrDefault();

            input.InstrumentReference = input.Instrument.GetReference();
            input.InstrumentType = instrType;

            input.NumberOfSecurities = input.Position.GetInstrumentCount();
            input.ContractSize = input.Instrument.GetQuotity();
            input.Notional = input.Instrument.GetNotional();
            input.Nominal = input.NumberOfSecurities * input.Notional;

            double ret = double.NaN;

            //@SB

            if (instrument.GetAllotment() == TKOAllotment.AllotmentData.GetIRFuturesCTAllotmentID() ||
                instrument.GetAllotment() == TKOAllotment.AllotmentData.GetCDSAllotmentID() ||
                instrument.GetAllotment() == TKOAllotment.AllotmentData.GetTRSAllotmentID() ||
                instrument.GetAllotment() == TKOAllotment.AllotmentData.GetOTCStockDerivativesAllotmentID() ||
                instrument.GetAllotment() == TKOAllotment.AllotmentData.GetIRSwapsAllotmentID() ||
                instrument.GetAllotment() == TKOAllotment.AllotmentData.GetOTCIRDerivativesAllotmentID()
                )
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
                    ret = Math.Abs(bond.GetValueInPrice() * input.Nominal) / 100.0;
                    input.IndicatorValue = ret;
                    return ret;
                }
            }
            else if (instrument.GetAllotment() == TKOAllotment.AllotmentData.GetListedOptionsAllotmentID() ||
                instrument.GetAllotment() == TKOAllotment.AllotmentData.GetIROptionsCTAllotmentID())
            {
                CSMOption opt = CSMOption.GetInstance(instrument.GetCode());
                if (opt != null) //should always be true
                {
                    int underlyingCode = opt.GetUnderlying_API(0);

                    input.UnderLyingLast = CSMInstrument.GetLast(underlyingCode);

                    ret = Math.Abs(input.NumberOfSecurities * input.ContractSize * input.UnderLyingLast);
                    input.IndicatorValue = ret;
                    return ret;
                }
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
                input.TmpPortfolioColName = input.Column;
                input.Column = "TKO STRATEGY";
                DbrPerfAttribMapping.SetColumnConfig(input);

                input.Column = input.TmpPortfolioColName;
                var tkostrategy = IsFundFees(input.Position, input);

                if (tkostrategy)
                {
                    ret = 0;
                    input.IndicatorValue = ret;
                    return ret;
                }
                else
                {
                    CSMAmPortfolio folio = CSMAmPortfolio.GetCSRPortfolio(input.PortFolioCode);
                    Int32 fundCurrencyCode;
                    if (folio != null)
                    {
                        CSMAmFund fund = CSMAmFund.GetFundFromFolio(folio);
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

                        CMString quantityCurrency = new CMString();
                        CSMCurrency.CurrencyToString(quantityCurrencyCode, quantityCurrency);

                        CMString amountCurrency = new CMString();
                        CSMCurrency.CurrencyToString(amountCurrencyCode, amountCurrency);

                        CMString positionCurrency = new CMString();
                        CSMCurrency.CurrencyToString(positionCurrencyCode, positionCurrency);

                        CMString fundCurrency = new CMString();
                        CurrencyToExclude.GetName(fundCurrency);

                        input.IndicatorValue = ret;
                        return ret;
                    }
                    else
                    {
                        throw new Exception("NOT COMPUTED");
                    }
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
                return ret;
            }
            else if (instrType.Equals("Interest Rate Futures"))
            {
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

                input.NumberOfSecurities = input.Position.GetInstrumentCount();
                input.Nominal = input.Position.GetInstrumentCount() * input.Instrument.GetNotional();
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
                return ret;
            }
            else if (instrType.Equals("Convertibles and Indexed"))
            {
                CSMOption option = instrument;
                input.ConvertionRatio = option.GetConversionRatioInPrice();

                input.NumberOfSecurities = input.Position.GetInstrumentCount();
                input.ContractSize = input.Instrument.GetQuotity();

                int underlyingCode = input.Instrument.GetUnderlying_API(0);
                input.UnderLyingLast = CSMInstrument.GetLast(underlyingCode);

                if (option != null)
                {
                    ret = Math.Abs(input.NumberOfSecurities * input.UnderLyingLast * input.ConvertionRatio);
                    input.IndicatorValue = ret;
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
                ret = Math.Abs(input.Nominal);
                input.IndicatorValue = ret;
                return ret;
            }
            else if (instrType.Equals("Total Return Swaps"))
            {
                input.NumberOfSecurities = input.Position.GetInstrumentCount();
                input.ContractSize = input.Instrument.GetQuotity();
                input.Notional = input.Instrument.GetNotional();
                input.Nominal = input.NumberOfSecurities * input.Notional;
                ret = Math.Abs(input.Nominal);
                input.IndicatorValue = ret;
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
            if (UpgradeExtensions.IsDebugEnabled())
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(input={0})", input.ToString());
            }

            //@SB

            input.TmpPortfolioColName = "Allotment";
            string allotFlag = Helper.TkoGetValuefromSophisString(input);

            //@SB

            input.TmpPortfolioColName = "Instrument type";
            string instrType = Helper.TkoGetValuefromSophisString(input);
            if (UpgradeExtensions.IsDebugEnabled())
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "instrType={0}", instrType);
            }

            var nullInstrumentsTypes = DbrTikehau_Config.GetTikehauConfigFromName("INSTRUMENTTYPES-ASSET-VALUE-RISK-AMF").Where(p => p.VALUE == instrType).FirstOrDefault();
            var amfVolatilityonfig = DbrTikehau_Config.GetTikehauConfigFromName("AMF-CHECK-VOLATILITY").FirstOrDefault().VALUE;

            input.InstrumentReference = input.Instrument.GetReference();
            input.InstrumentType = instrType;

            input.NumberOfSecurities = input.Position.GetInstrumentCount();
            input.ContractSize = input.Instrument.GetQuotity();
            input.Notional = input.Instrument.GetNotional();
            input.Nominal = input.NumberOfSecurities * input.Notional;

            double ret = double.NaN;

            //@SB

            // Tikehau Specific
            if (instrument.GetAllotment() == TKOAllotment.AllotmentData.GetIRFuturesCTAllotmentID() ||
            instrument.GetAllotment() == TKOAllotment.AllotmentData.GetCDSAllotmentID() ||
            instrument.GetAllotment() == TKOAllotment.AllotmentData.GetIRSwapsAllotmentID()
            )
            {
                // Notional
                ret = input.Nominal;
                input.IndicatorValue = ret;
                return ret;
            }
            else if (instrument.GetAllotment() == TKOAllotment.AllotmentData.GetOTCStockDerivativesAllotmentID() ||
                instrument.GetAllotment() == TKOAllotment.AllotmentData.GetOTCIRDerivativesAllotmentID())
            {
                // Notional
                int sicovam = input.InstrumentCode;
                input.Delta = instrument.GetDelta(ref sicovam);
                ret = input.Nominal * input.Delta;
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
                    int sicovam = input.InstrumentCode;
                    input.Delta = instrument.GetDelta(ref sicovam);
                    int underlyingCode = opt.GetUnderlying_API(0);

                    input.UnderLyingLast = CSMInstrument.GetLast(underlyingCode);

                    ret = input.UnderLyingLast * input.NumberOfSecurities * input.ContractSize * input.Delta;
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

            //@SB

            if (input.NumberOfSecurities == 0.0)
            {
                ret = 0;
                input.IndicatorValue = ret;
                return 0;
            }
            if (nullInstrumentsTypes != null)
            {
                var quotity = 1000;
                ret = input.Position.GetAssetValue() * quotity;
                input.IndicatorValue = ret;
                return ret;
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

            //@SB

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

                //ret = input.Nominal * input.UnderLyingLast / 100 * input.Delta;
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
                        input.UnderLyingLast = bond.GetValueInPrice();

                        //Correctif 11-19-2015 => / 100
                        //ret = bond.GetValueInPrice() /100  * input.NumberOfSecurities * input.ContractSize;

                        //Correctif 01-05-2016 => / 100
                        ret = input.UnderLyingLast / 100 * input.Nominal;

                        input.IndicatorValue = ret;
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

                input.NumberOfSecurities = input.Position.GetInstrumentCount();
                input.Nominal = input.Position.GetInstrumentCount() * input.Instrument.GetNotional();


                string UnderlyingType = cdsunderlying.GetType_API().ToString();
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
                        }
                    }

                    ret = input.NumberOfSecurities * input.UnderLyingLast * input.ConvertionRatio * input.Delta;
                    input.IndicatorValue = ret;

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
                }

                if (!underlyingInitialPrice.Equals(0))
                {
                    input.NumberOfSecurities = input.Position.GetInstrumentCount();
                    input.ContractSize = input.Instrument.GetQuotity();
                    input.Nominal = input.NumberOfSecurities * input.Instrument.GetNotional();
                    input.UnderLyingLast = CSMInstrument.GetLast(legCode);
                    ret = input.Nominal * input.UnderLyingLast / underlyingInitialPrice;
                    input.IndicatorValue = ret;
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
                        }
                    }

                    //Correctif 11-19-2015 => / 100
                    ret = input.UnderLyingLast / 100 * input.NumberOfSecurities * input.ContractSize * input.Delta;
                    input.IndicatorValue = ret;
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

        public static bool IsFundFees(this CSMPosition position, InputProvider input)
        {
            var positionfoliocode = 0;
            CSMTransactionVector transactionvector = new CSMTransactionVector();
            var Folio = input.Position.GetPortfolio();
            if (Folio != null)
                positionfoliocode = Folio.GetCode();
            else
            {
                input.Position.GetTransactions(transactionvector);
                if (transactionvector.Count > 0)
                {
                    foreach (CSMTransaction deal in transactionvector)
                    {
                        positionfoliocode = deal.GetFolioCode();
                    }
                }
            }

            input.StringIndicatorValue = "";
            input.TmpPortfolioColName = input.Column;
            input.Column = "TKO STRATEGY";
            DbrPerfAttribMapping.SetColumnConfig(input);

            input.Column = input.TmpPortfolioColName;
            var listOfConfig = input.PerfAttribMappingConfigDic;
            DbRequester.DbrPerfAttribMapping.TIKEHAU_PERFATTRIB_MAPPING value = null;
            if (listOfConfig.TryGetValue(positionfoliocode, out value))
            {

                if (value.PORTFOLIOMAPPINGNAME == "FUND FEES")
                    return true;
                else
                    return false;
                value = null;
            }
            else
            {
                using (CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(positionfoliocode))
                {
                    var cpt = listOfConfig.Count;
                    int j = 0;
                    var parentCode = 0;
                    CSMPortfolio portfolio2;
                    while (value == null && j < cpt)
                    {
                        if (portfolio != null)
                        {
                            parentCode = portfolio.GetParentCode();
                            portfolio2 = CSMPortfolio.GetCSRPortfolio(parentCode);
                            if (listOfConfig.TryGetValue(parentCode, out value))
                            {
                                if (value.PORTFOLIOMAPPINGNAME == "FUND FEES")
                                    return true;
                                else
                                    return false;
                            }
                            j++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            return false;
        }

        public static double TkoComputeGlobalRisk(this CSMInstrument instrument, InputProvider input)
        {
            if (UpgradeExtensions.IsDebugEnabled())
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(input={0})", input.ToString());
            }

            //@SB

            input.TmpPortfolioColName = "Allotment";
            string allotFlag = Helper.TkoGetValuefromSophisString(input);

            //@SB

            input.TmpPortfolioColName = "Instrument type";
            string instrType = Helper.TkoGetValuefromSophisString(input);
            if (UpgradeExtensions.IsDebugEnabled())
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "instrType={0}", instrType);
            }

            var nullInstrumentsTypes = DbrTikehau_Config.GetTikehauConfigFromName("INSTRUMENTTYPES-NULL-GLOBAL-RISK-AMF").Where(p => p.VALUE == instrType).FirstOrDefault();
            var amfVolatilityonfig = DbrTikehau_Config.GetTikehauConfigFromName("AMF-CHECK-VOLATILITY").FirstOrDefault().VALUE;

            input.InstrumentReference = input.Instrument.GetReference();
            input.InstrumentType = instrType;

            input.MarketDataDate = String.Format("{0:dd/MM/yyyy}", Helper.mydate(CSMMarketData.GetCurrentMarketData().GetDate()));

            input.NumberOfSecurities = input.Position.GetInstrumentCount();
            input.ContractSize = input.Instrument.GetQuotity();
            input.Notional = input.Instrument.GetNotional();
            input.Nominal = input.NumberOfSecurities * input.Notional;

            double ret = double.NaN;

            //@SB

            // Tikehau Specific
            if (instrument.GetAllotment() == TKOAllotment.AllotmentData.GetIRFuturesCTAllotmentID() ||
                instrument.GetAllotment() == TKOAllotment.AllotmentData.GetIRSwapsAllotmentID())
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
                    ret = Math.Abs(bond.GetValueInPrice() * input.Nominal) / 100.0;
                    input.IndicatorValue = ret;
                    return ret;
                }
            }
            else if (instrument.GetAllotment() == TKOAllotment.AllotmentData.GetIROptionsCTAllotmentID() ||
                instrument.GetAllotment() == TKOAllotment.AllotmentData.GetListedOptionsAllotmentID())
            {
                // Abs(Number of securities * Contract size * Underlying last * delta)

                CSMOption opt = CSMOption.GetInstance(instrument.GetCode());
                if (opt != null) //should always be true
                {
                    int underlyingCode = 0;
                    double delta = opt.GetDelta(ref underlyingCode);

                    underlyingCode = opt.GetUnderlying_API(0);

                    input.UnderLyingLast = CSMInstrument.GetLast(underlyingCode);

                    ret = Math.Abs(input.NumberOfSecurities * input.ContractSize * delta * input.UnderLyingLast);
                    input.IndicatorValue = ret;
                    return ret;
                }
            }
            //else if (instrument.GetAllotment() == TKOAllotment.AllotmentData.GetOptionsCDSAllotmentID())
            //{
            //    //"If nominal <0 then Abs(Max(market value of the underlying reference asset, nominal))
            //    // If nominal>0 then Abs(market value of the underlying reference asset)"

            //    CSMOption opt = CSMOption.GetInstance(instrument.GetCode());
            //    int underlyingCode = opt.GetUnderlying_API(0);
            //    CSMPortfolioColumn col = CSMPortfolioColumn.GetCSRPortfolioColumn("Market Value");
            //    double Svalue = 0.0 ;
            //    if(col != null)
            //    {
            //        SSMCellValue cvalue = new SSMCellValue();
            //        SSMCellStyle cstyle = new SSMCellStyle();

            //        cstyle.kind = NSREnums.eMDataType.M_dDouble;
            //        cstyle.@decimal = 3;
            //        col.GetPositionCell(input.Position,0,input.PortFolioCode,null,underlyingCode,instrument.GetCode(),ref cvalue,cstyle,true);
            //        Svalue = cvalue.doubleValue;
            //    }
            //    if(input.Nominal > 0 )
            //    {
            //        ret = Math.Abs(Math.Max(Svalue,input.Nominal));
            //        input.IndicatorValue = ret;
            //        return ret;
            //    }
            //    else
            //    {
            //        ret = Math.Abs(Svalue);
            //        input.IndicatorValue = ret;
            //        return ret;
            //    }

            //}
            //else if (instrument.GetAllotment() == TKOAllotment.AllotmentData.GetTRSAllotmentID())
            //{
            //    // Max(Abs(Underlying Asset Value), Abs(Nominal))
            //    CSMSwap swap = CSMSwap.GetInstance(instrument.GetCode());
            //    int underlyingCode = swap.GetUnderlying_API(0);
            //    CSMPortfolioColumn col = CSMPortfolioColumn.GetCSRPortfolioColumn("Market Value");
            //    double Svalue = 0.0;
            //    if (col != null)
            //    {
            //        SSMCellValue cvalue = new SSMCellValue();
            //        SSMCellStyle cstyle = new SSMCellStyle();

            //        cstyle.kind = NSREnums.eMDataType.M_dDouble;
            //        cstyle.@decimal = 3;
            //        col.GetPositionCell(input.Position, 0, input.PortFolioCode, null, underlyingCode, instrument.GetCode(), ref cvalue, cstyle, true);
            //        Svalue = cvalue.doubleValue;
            //    }

            //    ret = Math.Max(Math.Abs(Svalue),Math.Abs(input.Nominal));
            //    input.IndicatorValue = ret;
            //    return ret;

            //}
            else if (instrument.GetAllotment() == TKOAllotment.AllotmentData.GetOTCIRDerivativesAllotmentID() ||
                instrument.GetAllotment() == TKOAllotment.AllotmentData.GetOTCStockDerivativesAllotmentID())
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
                var coff = Helper.TkoGetValuefromSophisDouble(input) * (-1) * input.NumberOfSecurities * 100 / input.Nominal;
                if (coff < 1)
                {
                    ret = 0;
                    ret = input.Nominal * coff;
                    return Math.Abs(ret);
                }
                else
                {
                    ret = 0;
                    ret = input.Nominal;
                    return Math.Abs(ret);
                }

            }

            //@SB

            else if (instrType.Equals("Listed Options")
                    || instrType.Equals("Stock Derivatives"))
            {
                //_ToCheck_ Delta Cash
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
                    }
                }

                ret = Math.Abs(input.Delta * input.UnderLyingLast * input.NumberOfSecurities * input.ContractSize);
                input.IndicatorValue = ret;

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
                    }
                }

                input.Nominal = input.Position.GetInstrumentCount() * input.Instrument.GetNotional();
                input.NumberOfSecurities = input.Position.GetInstrumentCount();
                input.ContractSize = input.Instrument.GetQuotity();
                ret = Math.Abs(input.Nominal * input.Delta);

                input.IndicatorValue = ret;
                return ret;
            }
            else if (instrType.Equals("Forward Forex"))
            {
                input.TmpPortfolioColName = input.Column;
                input.Column = "TKO STRATEGY";
                DbrPerfAttribMapping.SetColumnConfig(input);

                input.Column = input.TmpPortfolioColName;
                var tkostrategy = IsFundFees(input.Position, input);
                if (tkostrategy)
                {
                    ret = 0;
                    input.IndicatorValue = ret;
                    return ret;
                }
                else
                {
                    CSMAmPortfolio folio = CSMAmPortfolio.GetCSRPortfolio(input.PortFolioCode, input.Extraction);
                    Int32 fundCurrencyCode;
                    if (folio != null)
                    {
                        CSMAmFund fund = CSMAmFund.GetFundFromFolio(folio);
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
                        CMString quantityCurrency = new CMString();
                        CSMCurrency.CurrencyToString(quantityCurrencyCode, quantityCurrency);

                        CMString amountCurrency = new CMString();
                        CSMCurrency.CurrencyToString(amountCurrencyCode, amountCurrency);

                        CMString positionCurrency = new CMString();
                        CSMCurrency.CurrencyToString(positionCurrencyCode, positionCurrency);

                        CMString fundCurrency = new CMString();
                        CurrencyToExclude.GetName(fundCurrency);
                        input.IndicatorValue = ret;
                        return ret;
                    }
                    else
                    {
                        throw new Exception("NOT COMPUTED");
                    }
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

                return ret;
            }
            else if (instrType.Equals("Interest Rate Futures"))
            {
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
                        //Correctif 11-19-2015 => / 100
                        //ret = Math.Abs(input.UnderLyingLast / 100 * input.NumberOfSecurities * input.ContractSize);

                        //Correctif 01-05-2016 => / 100
                        ret = Math.Abs(input.UnderLyingLast / 100 * input.Nominal);

                        input.IndicatorValue = ret;

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
                input.Nominal = input.Position.GetInstrumentCount() * input.Instrument.GetNotional();
                input.TmpPortfolioColName = "Asset value";
                var assetvalue = Helper.TkoGetValuefromSophisDouble(input);

                CSMSwap cds = instrument;
                var accruedamount = (cds.GetAccruedCoupon_API() / 100) * input.NumberOfSecurities;

                var prix = (-(assetvalue - accruedamount) / input.Nominal) + 1;
                if (input.Nominal >= 0)
                {
                    ret = Math.Abs(input.Nominal * prix);
                }
                else
                {
                    //@DPH
                    ret = Math.Max(Math.Abs(input.Nominal), Math.Abs((double)input.Nominal * (double)prix));
                }
                input.IndicatorValue = ret;
                return ret;


                //Desactivate due to #9284712
                //CSMSwap cds = instrument;
                //var leg0 = cds.GetLeg(0);
                //CSMIndexedLeg indexLeg = leg0;
                //CSMLeg Leg = leg0;

                //int legCode = leg0.GetUnderlyingCode();
                //var cdsunderlying = CSMInstrument.GetInstance(legCode);

                //input.NumberOfSecurities = input.Position.GetInstrumentCount();
                //input.Nominal = input.Position.GetInstrumentCount() * input.Instrument.GetNotional();
                //input.UnderLyingLast = CSMInstrument.GetLast(legCode);
                //if (input.UnderLyingLast == -10000000.0)
                //    input.UnderLyingLast = 0.0;

                //double underlyingLastInPercent = input.UnderLyingLast / 100;

                //if ((cdsunderlying.GetType_API() == 'H' || cdsunderlying.GetType_API() == 'O'))
                //{
                //    if (cdsunderlying.GetType_API() == 'O')
                //    {
                //        if (input.NumberOfSecurities < 0)
                //        {
                //            ret = Math.Max(Math.Abs(input.Nominal), Math.Abs(input.Nominal * underlyingLastInPercent));
                //            input.IndicatorValue = ret;
                //        }
                //        else
                //        {
                //            ret = Math.Abs(input.Nominal * underlyingLastInPercent);
                //            input.IndicatorValue = ret;
                //        }
                //    }
                //    else
                //    {
                //        ret = Math.Abs(input.Nominal);
                //        input.IndicatorValue = ret;
                //    }

                //}
                //else if ((cdsunderlying.GetType_API() == 'I'))
                //{
                //    ret = Math.Abs(input.Nominal);
                //    input.IndicatorValue = ret;
                //}
                //else
                //{
                //    throw new Exception("NOT COMPUTED");
                //}
                //input.ContractSize = input.Instrument.GetQuotity();
                //return ret;
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
                        }
                    }

                    ret = Math.Abs(input.NumberOfSecurities * input.UnderLyingLast * input.Delta * input.ConvertionRatio);
                    input.IndicatorValue = ret;

                    input.ContractSize = input.Instrument.GetQuotity();

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
                }

                if (!underlyingInitialPrice.Equals(0))
                {
                    input.NumberOfSecurities = input.Position.GetInstrumentCount();
                    input.ContractSize = input.Instrument.GetQuotity();
                    input.Nominal = input.NumberOfSecurities * input.Instrument.GetNotional();
                    input.UnderLyingLast = CSMInstrument.GetLast(legCode);
                    ret = Math.Abs(input.Nominal * input.UnderLyingLast / underlyingInitialPrice);
                    input.IndicatorValue = ret;

                    return ret;
                }
                else
                {
                    throw new Exception("NOT COMPUTED");
                }
            }
            else if (instrType.Equals("Interest Rate Derivatives"))
            {
                int sicovam = input.InstrumentCode;
                input.InstrumentCode = input.Instrument.GetUnderlying_API(0);
                string underlyingType = Helper.TkoGetValuefromSophisString(input);
                input.InstrumentCode = sicovam;

                if (underlyingType.Equals("Interest Rate Futures"))
                {
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
                        }
                    }

                    ret = Math.Abs(input.Delta * input.NumberOfSecurities * (input.UnderLyingLast / 100) * input.ContractSize);
                    input.IndicatorValue = ret;
                    input.Delta = input.Delta;

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
                return ret;
            }
            else
            {
                throw new Exception("NOT COMPUTED");
            }
        }
        #endregion

        #region ForexForward

        public static double TkoForexForwardPaymentLeg(this CSMInstrument instrument, InputProvider input)
        {
            input.TmpPortfolioColName = "Instrument type";
            string instrType = Helper.TkoGetValuefromSophisString(input);

            input.InstrumentReference = input.Instrument.GetReference();
            input.InstrumentType = instrType;

            input.MarketDataDate = String.Format("{0:dd/MM/yyyy}", Helper.mydate(CSMMarketData.GetCurrentMarketData().GetDate()));

            input.NumberOfSecurities = input.Position.GetInstrumentCount();
            input.ContractSize = input.Instrument.GetQuotity();
            input.Notional = input.Instrument.GetNotional();
            input.Nominal = input.NumberOfSecurities * input.Notional;

            double ret = 0.0;
            input.IndicatorValue = ret;
            if (instrType.Equals("Forward Forex"))
            {
                CSMAmPortfolio folio = CSMAmPortfolio.GetCSRPortfolio(input.PortFolioCode);
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
                        if (settlementDate > reportinDateShiftted)
                        {
                            // the quantity is the amount in ccy1 (for ccy1/ccy2)
                            //if (fundCurrencyCode != quantityCurrencyCode)
                            //{
                            quantity += deal.GetQuantity();
                            //}
                            //if (fundCurrencyCode != amountCurrencyCode)
                            //{
                            amount += deal.GetNetAmount();
                            //}
                        }
                    }
                    int positionCurrencyCode = input.Position.GetCurrency();

                    ret = amount;
                    input.IndicatorValue = ret;
                    return ret;
                }
            }
            return ret;
        }

        public static double TkoForexForwardReceivingLeg(this CSMInstrument instrument, InputProvider input)
        {
            input.TmpPortfolioColName = "Instrument type";
            string instrType = Helper.TkoGetValuefromSophisString(input);

            input.InstrumentReference = input.Instrument.GetReference();
            input.InstrumentType = instrType;

            input.MarketDataDate = String.Format("{0:dd/MM/yyyy}", Helper.mydate(CSMMarketData.GetCurrentMarketData().GetDate()));

            input.NumberOfSecurities = input.Position.GetInstrumentCount();
            input.ContractSize = input.Instrument.GetQuotity();
            input.Notional = input.Instrument.GetNotional();
            input.Nominal = input.NumberOfSecurities * input.Notional;

            double ret = 0.0;
            input.IndicatorValue = ret;
            if (instrType.Equals("Forward Forex"))
            {
                CSMAmPortfolio folio = CSMAmPortfolio.GetCSRPortfolio(input.PortFolioCode);
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
                        if (settlementDate > reportinDateShiftted)
                        {
                            //// the quantity is the amount in ccy1 (for ccy1/ccy2)
                            //if (fundCurrencyCode != quantityCurrencyCode)
                            //{
                            quantity += deal.GetQuantity();
                            //}
                            //if (fundCurrencyCode != amountCurrencyCode)
                            //{
                            amount += deal.GetNetAmount();
                            //}
                        }
                    }
                    int positionCurrencyCode = input.Position.GetCurrency();
                    ret = quantity;

                    input.IndicatorValue = ret;
                    return ret;
                }
            }
            return ret;
        }

        public static double TkoForexForwardReceivedAndPaymentLegFxSpot(this CSMInstrument instrument, InputProvider input)
        {
            input.TmpPortfolioColName = "Instrument type";
            string instrType = Helper.TkoGetValuefromSophisString(input);

            input.InstrumentReference = input.Instrument.GetReference();
            input.InstrumentType = instrType;

            input.MarketDataDate = String.Format("{0:dd/MM/yyyy}", Helper.mydate(CSMMarketData.GetCurrentMarketData().GetDate()));

            input.NumberOfSecurities = input.Position.GetInstrumentCount();
            input.ContractSize = input.Instrument.GetQuotity();
            input.Notional = input.Instrument.GetNotional();
            input.Nominal = input.NumberOfSecurities * input.Notional;

            double ret = 0.0;
            input.IndicatorValue = ret;
            if (instrType.Equals("Forward Forex"))
            {
                CSMForexFuture forexFuture = input.Instrument;
                var quantityCurrencyCode = forexFuture.GetExpiryCurrency();
                var amountCurrencyCode = forexFuture.GetCurrencyCode();


                double fxSpotquantity = CSMMarketData.GetCurrentMarketData().GetForex(quantityCurrencyCode, amountCurrencyCode);
                ret = fxSpotquantity;
                input.IndicatorValue = ret;
                return ret;
            }
            return ret;
        }

        public static double TkoForexForwardReceivedLegCurrency(this CSMInstrument instrument, InputProvider input)
        {
            input.TmpPortfolioColName = "Instrument type";
            string instrType = Helper.TkoGetValuefromSophisString(input);

            input.InstrumentReference = input.Instrument.GetReference();
            input.InstrumentType = instrType;

            input.MarketDataDate = String.Format("{0:dd/MM/yyyy}", Helper.mydate(CSMMarketData.GetCurrentMarketData().GetDate()));

            input.NumberOfSecurities = input.Position.GetInstrumentCount();
            input.ContractSize = input.Instrument.GetQuotity();
            input.Notional = input.Instrument.GetNotional();
            input.Nominal = input.NumberOfSecurities * input.Notional;

            double ret = double.NaN;
            input.IndicatorValue = ret;
            if (instrType.Equals("Forward Forex"))
            {
                CSMForexFuture forexFuture = input.Instrument;
                var quantityCurrencyCode = forexFuture.GetExpiryCurrency();
                var amountCurrencyCode = forexFuture.GetCurrencyCode();

                CSMCurrency receivedDevise = CSMCurrency.CreateInstance(quantityCurrencyCode);
                CSMCurrency paidDevise = CSMCurrency.CreateInstance(amountCurrencyCode);

                CMString dev1 = new CMString();
                CSMCurrency.CurrencyToString(quantityCurrencyCode, dev1);

                input.StringIndicatorValue = " " + dev1.StringValue;
                return ret;
            }
            return ret;
        }


        public static double TkoForexForwardPaymentLegCurrency(this CSMInstrument instrument, InputProvider input)
        {
            input.TmpPortfolioColName = "Instrument type";
            string instrType = Helper.TkoGetValuefromSophisString(input);

            input.InstrumentReference = input.Instrument.GetReference();
            input.InstrumentType = instrType;

            input.MarketDataDate = String.Format("{0:dd/MM/yyyy}", Helper.mydate(CSMMarketData.GetCurrentMarketData().GetDate()));

            input.NumberOfSecurities = input.Position.GetInstrumentCount();
            input.ContractSize = input.Instrument.GetQuotity();
            input.Notional = input.Instrument.GetNotional();
            input.Nominal = input.NumberOfSecurities * input.Notional;

            double ret = double.NaN;
            input.IndicatorValue = ret;
            if (instrType.Equals("Forward Forex"))
            {
                CSMForexFuture forexFuture = input.Instrument;
                var quantityCurrencyCode = forexFuture.GetExpiryCurrency();
                var amountCurrencyCode = forexFuture.GetCurrencyCode();

                CSMCurrency receivedDevise = CSMCurrency.CreateInstance(quantityCurrencyCode);
                CSMCurrency paidDevise = CSMCurrency.CreateInstance(amountCurrencyCode);

                CMString dev2 = new CMString();
                CSMCurrency.CurrencyToString(amountCurrencyCode, dev2);

                input.StringIndicatorValue = " " + dev2.StringValue;
                return ret;
            }
            return ret;
        }
        #endregion
    }
}
