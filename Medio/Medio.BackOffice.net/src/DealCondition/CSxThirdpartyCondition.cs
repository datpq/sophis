using System;
using System.Collections.Generic;
using System.Linq;
using MEDIO.BackOffice.net.src.Thirdparty;
using MEDIO.CORE.Tools;
using MEDIO.MEDIO_CUSTOM_PARAM;
using Oracle.DataAccess.Client;
using sophis.backoffice_kernel;
using sophis.misc;
using sophis.portfolio;

namespace MEDIO.BackOffice.net.src.DealCondition
{
    public class CSxAgreement
    {
        public string Agreement;
        public List<CSxThirdpartyAllotment> ThirdpartyAllotment;
    }

    public class CSxThirdpartyAllotment
    {
        public int ThirdpartyCode;
        public int AllotmentCode;
    }

    /// <summary>
    /// Interface to create a new condition to select a workflow or a rule in a workflow.
    ///	Only available with back office kernel.
    ///	When creating or modifying a transaction, the kernel engine will first select a workflow browsing the
    ///	workflows selector; when the criteria matchs, it plays the user conditions. A new condition can be added
    ///	by implementing this interface. Once the workflow has been selected, then the kernel engine selects a rule
    ///	browsing the worflow rule; when the criteria matchs, it plays the same user conditions.
    /// </summary>
    public class CSxThirdpartyCondition : sophis.backoffice_kernel.CSMKernelCondition
    {
        private static string _ThirdpartyAgreement;
        private static string _ThirdpartyAgreementName = CSxToolkitCustomParameter.Instance.BO_THIRDPARTY_AGREEMENT;
        private static string _MAMLUploadName = CSxToolkitCustomParameter.Instance.BO_EVENT_MAML_UPLOAD;
        private static string _DelegateUploadName = CSxToolkitCustomParameter.Instance.BO_EVENT_DELEGATE_UPLOAD;
        private int _MAMLUploadID;
        private int _delegateUploadID;

        public CSxThirdpartyCondition(string agreement)
        {
            _ThirdpartyAgreement = agreement;
            _MAMLUploadID = CSMKernelEvent.GetIdByName(_MAMLUploadName);
            _delegateUploadID = CSMKernelEvent.GetIdByName(_DelegateUploadName);
        }

        public static List<string> GetAgreementNames()
        {
            var res = new List<CSxAgreement>();
            string sql = "select unique(agreement) from tiersagreement where law = :law";
            var parameter = new OracleParameter("law", _ThirdpartyAgreementName);
            var parameters = new List<OracleParameter>() { parameter };
            return CSxDBHelper.GetMultiRecords(sql, parameters).ConvertAll(x=>x.ToString());
        }

        public static List<CSxThirdpartyAllotment> GetThirdpartyAllotment(string agreement)
        {
            string sql = "select code, allotment from tiersagreement where agreement = :agreement";
            List<CSxThirdpartyAllotment> res = new List<CSxThirdpartyAllotment>();
            var parameter = new OracleParameter(":agreement", agreement);
            var parameters = new List<OracleParameter>() { parameter };
            foreach (var onePair in CSxDBHelper.GetMultiRecords<int, int>(sql, parameters))
            {
                var toAdd = new CSxThirdpartyAllotment();
                toAdd.ThirdpartyCode = onePair.Key;
                toAdd.AllotmentCode = onePair.Value;
                res.Add(toAdd);
            }
            return res;
        }

        /// <summary>
        /// Pure virtual method.
        /// Used by the framework while selecting the rule from Workflow Definition rules set.
        /// Method is called for Condition1, Condition2, Condition3 columns. Logical 'AND' is used
        /// to make decision if to select the matching rule - found by framework.
        /// The result has to be TRUE to make the rule selected.
        /// </summary>
        /// <param name="tr">is the reference to the transaction associated with the processed deal;
        /// it is the final (resp. initial) state for a deal created or modified (resp. canceled).</param>
        /// <param name="sel">is a structure giving some information about the instrument created; As the instrument may be
        /// not yet created, the structure gives some data coming from the future instrument created; if the instrument is created,
        /// the structure gives the data from the instrument.</param>
        /// <returns>is the boolean and is calculated by the client code.</returns>
        public override bool GetCondition(sophis.portfolio.CSMTransaction tr, sophis.backoffice_kernel.SSMKernelInstrumentSelector sel)
        {
            using(var inst = tr.GetInstrument())
            {
                if (inst != null && tr != null)
                {
                    int kEventId = tr.GetKEventID();
                    if (kEventId == _MAMLUploadID || kEventId == _delegateUploadID)
                    {
                        return true;
                    }
                    foreach (var thirdpartyAllotment in CSxThirdpartyAction.GetThirdpartyAllotment(_ThirdpartyAgreement))
                    {
                        if (inst.GetAllotment() == thirdpartyAllotment.AllotmentCode && tr.GetCounterparty() == thirdpartyAllotment.ThirdpartyCode)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

    }
}
