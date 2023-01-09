using System;
using sophis.utils;
using MEDIO.MEDIO_CUSTOM_PARAM;

namespace MEDIO.TransactionAction.Source
{
    public class CSxRMAGenericTradeCheck : sophis.portfolio.CSMTransactionAction
    {
        private const string COMMENT_SEPA = "#*#";
        public override void VoteForCreation(sophis.portfolio.CSMTransaction transaction, int event_id)
        {
            using (CSMLog Log = new CSMLog())
            {
                try
                {
                    Log.Write(CSMLog.eMVerbosity.M_debug, $"CSxRMAGenericTradeCheck.BEGIN(TradeId={transaction.getInternalCode()}, Operator={transaction.GetOperator()}, Comment={transaction.GetComment()})");
                    bool isRMAOperator = false;
                    foreach (var operatorName in CSxToolkitCustomParameter.Instance.RMAGenericTradeOperators.Split(','))
                    {
                        if (CSMUserRights.ConvNameToIdent(operatorName) == transaction.GetOperator())
                        {
                            isRMAOperator = true;
                            break;
                        }
                    }
                    if (!isRMAOperator) return;
                    string transComments = transaction.GetComment();
                    if (!string.IsNullOrEmpty(transComments) && transComments.IndexOf(COMMENT_SEPA) >= 0)
                    {
                        Log.Write(CSMLog.eMVerbosity.M_debug, $"Processing comments: {transComments}");
                        var extraFields = transComments.Split(new string[] {COMMENT_SEPA}, StringSplitOptions.None);
                        bool modifDone = false;
                        for(int i=1; i<extraFields.Length; i++)
                        {
                            var fieldName = extraFields[i].Split('=')[0];
                            var fieldVal = extraFields[i].Split('=')[1];
                            if (fieldName == "NOSTRO_CASH_ID")
                            {
                                Log.Write(CSMLog.eMVerbosity.M_debug, $"Setting SetNostroCashId & SetNostroPhysicalId: {fieldVal}");
                                transaction.SetNostroPhysicalId(int.Parse(fieldVal));
                                transaction.SetNostroCashId(int.Parse(fieldVal));
                                modifDone = true;
                            } else
                            {
                                Log.Write(CSMLog.eMVerbosity.M_error, $"Unknown field found: {fieldName}");
                            }
                        }
                        if (modifDone)
                        {
                            Log.Write(CSMLog.eMVerbosity.M_debug, $"Setting SetComment: {extraFields[0]}");
                            transaction.SetComment(extraFields[0]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(CSMLog.eMVerbosity.M_error, $"Exception Occured: {ex.Message}");
                }
                finally
                {
                    Log.Write(CSMLog.eMVerbosity.M_debug, "CSxRMAGenericTradeCheck.END");
                }
            }
        }
    }
}
