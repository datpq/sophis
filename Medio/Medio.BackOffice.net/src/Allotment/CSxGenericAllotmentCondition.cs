using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using MEDIO.CORE.Tools;
using sophis.backoffice_kernel;
using sophis.instrument;
using sophis.utils;
using sophis.value;

namespace MEDIO.BackOffice.net.src.Allotment
{

    public class CSxGenericAllotmentCondition : CSMAllotmentCondition
    {
        private readonly CSxAllotConditionModel _model;
        private static List<CSxAllotConditionModel> _Models = new List<CSxAllotConditionModel>();
        private static Dictionary<int, string> _LegalInfoDictionary = new Dictionary<int, string>();

        public CSxGenericAllotmentCondition(CSxAllotConditionModel model)
        {
            _model = model;
        }

        /// <summary>
        /// Check the instrument:
        /// return true if it has a market
        /// return false otherwise 
        /// </summary>
        /// <param name="instrument"></param>
        /// <returns></returns>
        public override bool is_matched(CSMInstrument instrument)
        {
            bool res = false;

                using (var log = new CSMLog())
                {

                    try
                    {
                        // Check if alltoment already set...

                        //int allotment = instrument.GetAllotment();
                        log.Write(CSMLog.eMVerbosity.M_debug, "in mode:"+m_Mode.ToString());

                       // log.Write(CSMLog.eMVerbosity.M_debug, "Instrument has allotment : " + instrument.GetAllotment().ToString());


                        log.Begin(GetType().Name, MethodBase.GetCurrentMethod().Name);

                        log.Write(CSMLog.eMVerbosity.M_debug, "Instrument type = " + _model.InstrumentType);
                        switch (_model.InstrumentType)
                        {
                            case "Z":
                                {
                                    return CheckFund(instrument, _model.InstrumentProperty);
                                }
                            case "": // more cases
                                break;
                            default:
                                break;
                        }
                        log.End();
                    }
                    catch (Exception E)
                    {
                        log.Write(CSMLog.eMVerbosity.M_error, "Exception Caught: " + E.Message);
                    }
            }

            return res;
        }

        #region business cases
        private bool CheckFund(CSMInstrument instrument, string instProp)
        {
            using (var log = new CSMLog())
            {
                log.Begin(GetType().Name, MethodBase.GetCurrentMethod().Name);
                
                log.Write(CSMLog.eMVerbosity.M_debug, "Instrument prop = " + instProp);    
                switch (instProp)
                {
                    case "Legal Form":
                    {
                        CSMAmFundBase fund = CSMAmFundBase.GetFund(instrument);
                        if (fund == null) return false;
                        log.Write(CSMLog.eMVerbosity.M_debug, String.Format("Instrument {0} is a fund", instrument.GetCode()));
                        return CheckFundLegalForm(fund, _model.GetConditionValueList());
                    }
                    case "":
                    {
                        // More cases
                    }
                        break;
                    default :
                    {
                        // not handled
                        return false;
                    }
                }
            }
            return false;
        }

        private bool CheckFundLegalForm(CSMAmFundBase fund, List<string> includedLegalFroms)
        {
            bool res = false;
            using (var log = new CSMLog())
            {
                log.Begin(GetType().Name, MethodBase.GetCurrentMethod().Name);
                var legalForm = fund.GetLegalForm();
                fund.SetAllotment(0);

                if (_LegalInfoDictionary.ContainsKey(legalForm))
                {
                    var legalName = _LegalInfoDictionary[legalForm];
                    if (includedLegalFroms.Contains(legalName))
                    {
                        log.Write(CSMLog.eMVerbosity.M_debug, String.Format("Legal name {0} is defined in the toolkit parameter. Return true for criteria {1}", legalName,_model.ConditionName));
                        return true;
                    }
                    else
                    {
                        log.Write(CSMLog.eMVerbosity.M_debug, String.Format("Legal name {0} is not defined in the toolkit parameter. Return false.", legalName));
                        return false;
                    }
                }
                log.End();
            }
            return res;
        }
        #endregion

        #region static
        public static void InitConditions()
        {
            string sql = "select * from MEDIO_TKT_ALLOTMENT_CONDS";
            var dataset = CSxDBHelper.GetDataSet(sql);
            _Models = TransfroModels(dataset);
            LoadFundLegalInfo();
        }

        private static List<CSxAllotConditionModel> TransfroModels(DataSet ds)
        {
            var res = ds.Tables[0].AsEnumerable().Select
            (dataRow => new CSxAllotConditionModel
            {
                ConditionName = dataRow.Field<string>("COND_NAME"),
                InstrumentType = dataRow.Field<string>("INSTR_TYPE"),
                InstrumentProperty = dataRow.Field<string>("INSTR_PROP"),
                ConditionValues = dataRow.Field<string>("COND_VALUES"),
            }).ToList();
            return res;
        }

        private static void LoadFundLegalInfo()
        {
            string sql = "select id, name from FUND_LEGAL_FORM";
            _LegalInfoDictionary = CSxDBHelper.GetDictionary<int, string>(sql);
        }

        public static List<CSxAllotConditionModel> GetModels()
        {
            return _Models;
        }
        #endregion
    }
}
