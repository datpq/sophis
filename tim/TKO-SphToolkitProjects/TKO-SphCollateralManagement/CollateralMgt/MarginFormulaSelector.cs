using System;
using sophis;
using sophis.utils;
using sophis.portfolio;
using sophis.tools;
using sophis.collateral;
using sophis.instrument;
using sophis.scenario;
using System.Collections;


namespace TKO_SophisCollateralManagement
{
    public class TkoCollateralLimitCalculation : CSMScenario
    {
        public override bool AlwaysEnabled()
        {
            return true;
        }

        public override void Run()
        {
            //try
            //{


            // CSMRankingCategoryNAME



            //var sortedList = sophis.finance.CSMRankingRoundingType.GetAllPrototypes();

            //sophis.finance.CSMRankingRoundingType type;
            //foreach (var elt in sortedList.Keys)
            //{
            //    var str = elt;
            //    sortedList.TryGetValue(str, out type);
            //}
            //   var lbavector = DbRequester.Dbrbo_pe_agreement_main.LoadLBA();
            //   sophis.collateral.CSMCollateralLimitReportingAPI api = new CSMCollateralLimitReportingAPI();

            //   CMString lineTypeName = new CMString();
            //   CMString AgreementName = new CMString();
            //   CSMCollateralLimitResultColumn column = new CSMCollateralLimitResultColumn();

            //   CSMCollateralLimitResultColumn columnelt = column;
            //   foreach(var elt in  lbavector)
            //   {
            //       api.ComputeAll(elt.CTPY_ID, elt.ENTITY_ID, elt.PERIMETER_ID, 0);
            //       CSMCollateralResult limitresult = api.GetResult();
            //       ArrayList arrayList = limitresult.GetChildren();
            //       foreach (var child in arrayList)
            //       {
            //           CSMCollateralResult collatresult = child as CSMCollateralResult;
            //           if (collatresult != null)
            //           {
            //               var agreement = collatresult.GetLBA();
            //               sophis.utils.CMString contractname = agreement.GetContractName();
            //               collatresult.GetLineTypeName(lineTypeName);
            //               int lineType = collatresult.GetLineType();
            //               AgreementName = CSMLBAgreement.GetAgreementName(elt.CTPY_ID, elt.ENTITY_ID, elt.PERIMETER_ID);
            //               //var fee = agreement.GetLongMargin(20);
            //               var a = agreement.GetMarginCallFolio();
            //               //var b = agreement.GetShortMargin(0);
            //               var c = agreement.GetSufficiencyLimit();
            //               var d = agreement.GetThresholdRatingAgency();
            //               var e = agreement.GetThresholdRatingSeniority();
            //               var f = agreement.GetThresholdType();
            //               var g = agreement.GetTimeSufficiencyLimit();
            //               var h = agreement.GetTriPartyMarginCall();
            //               var i = agreement.GetAccruedInterestIncluded();
            //               var j = agreement.GetAgreementPropertiesLinesNumber();
            //               //var k = agreement.GetAgreementPropertiesName(j);

            //               var l = agreement.GetAgreementPropertiesValue(j);
            //               var m = agreement.GetAutomaticFeeMark();
            //               var n = agreement.GetCashSufficiencyLimit();
            //               var o = agreement.GetCcyCashRemuneration(0);
            //               var p = agreement.GetCFDBuySellSide();
            //               var q = agreement.GetCFDFreeCashExpThreshold();
            //               var qq = agreement.GetCollateralCallCcy();



            //               SSMCellValue value = new SSMCellValue();
            //               SSMCellStyle style = new SSMCellStyle();

            //               columnelt.GetCell(collatresult, ref value, style);
            //               var collateralMasterResult = collatresult.GetMaster();
            //               double ret = double.NaN;
            //               columnelt.GetCell(collateralMasterResult, ref value, style);
            //               var masteragrrement = collateralMasterResult.GetLBA();

            //               int retttt = agreement.GetMarginCallFolio();
            //               if (style.kind == NSREnums.eMDataType.M_dDouble)
            //               {
            //                   ret = value.doubleValue;
            //               }
            //               else if (style.kind == NSREnums.eMDataType.M_dLong)
            //               {
            //                   ret = value.integerValue;
            //               }
            //               else if (style.kind == NSREnums.eMDataType.M_dNullTerminatedString)
            //               {
            //                   ret = 0;
            //               }
            //               else if (style.kind == NSREnums.eMDataType.M_dDate)
            //               {
            //                   ret = value.integerValue;
            //               }
            //               else
            //               {
            //                   ret = double.NaN;
            //               }

            //               ArrayList arrayList2 = collatresult.GetChildren();
            //               double ret2 = double.NaN;
            //               foreach (var child2 in arrayList2)
            //               {
            //                   CSMCollateralResult collatresult2 = child2 as CSMCollateralResult;
            //                   if (collatresult2 != null)
            //                   {
            //                       SSMCellValue value2 = new SSMCellValue();
            //                       SSMCellStyle style2 = new SSMCellStyle();
            //                       column.GetCell(collatresult, ref value2, style2);

            //                       if (style.kind == NSREnums.eMDataType.M_dDouble)
            //                       {
            //                           ret2 = value.doubleValue;
            //                       }
            //                       else if (style.kind == NSREnums.eMDataType.M_dLong)
            //                       {
            //                           ret2 = value.integerValue;
            //                       }
            //                       else if (style.kind == NSREnums.eMDataType.M_dNullTerminatedString)
            //                       {
            //                           ret2 = 0;
            //                       }
            //                       else if (style.kind == NSREnums.eMDataType.M_dDate)
            //                       {
            //                           ret2 = value.integerValue;
            //                       }
            //                       else
            //                       {
            //                           ret2 = double.NaN;
            //                       }
            //                   }
            //               }
            //           }
            //       }
            //   }
            //}
            //catch (Exception ex)
            //{
            //    CSMLog.Write("TkoCollateralLimitCalculation", " Run", CSMLog.eMVerbosity.M_error, "Error while in Scenario TkoCollateralLimitCalculation");
            //}
            //finally
            //{
            //}
        }
    }

    public class TkoCollateralIndicator : CSMCollateralIndicator
    {
        //@DPH
        //public override double GetCollateralIndicator(CSMPosition pos, CSMLBAgreement lba)
        //{
        //    double unsettledBalance = pos.GetUnsettledBalance() * 1000;
        //    return unsettledBalance;
        //}

        //public override double GetCollateralIndicator(CSMPosition pos, CSMLBAgreement lba, CSMCollateralIndicatorForex collateralForex)
        //{
        //    double unsettledBalance = pos.GetUnsettledBalance() * 1000;
        //    return unsettledBalance;
        //}

        //public override double GetCollateralIndicator(CSMPosition pos, CSMLBAgreement lba, CSMCollateralIndicatorForex collateralForex, MSCollateralIndicatorDetails detailedResults)
        //{
        //    double unsettledBalance = pos.GetUnsettledBalance() * 1000;
        //    return unsettledBalance;
        //}

        public override double GetCollateralIndicator(CSMPosition pos, CSMLBAgreement lba, CSMCollateralIndicatorForex collateralForex, MSCollateralIndicatorDetails detailedResults, int date)
        {
            double unsettledBalance = pos.GetUnsettledBalance() * 1000;
            return unsettledBalance;
        }
    }
}