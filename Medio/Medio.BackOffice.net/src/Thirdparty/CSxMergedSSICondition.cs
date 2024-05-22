using Oracle.DataAccess.Client;
using sophis.backoffice_cash;
using sophis.DAL;
using sophis.instrument;
using sophis.portfolio;
using sophis.static_data;
using sophis.utils;
using Sophis.DataAccess;
using Sophis.OMS.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MEDIO.BackOffice.net.src.Thirdparty
{
    public class CSxMergedSSICondition : CSMSettlementRulesCondition
    {
        public override bool checkCondition(CSMTransaction trans, CSMInstrument instr, int linePriority, int lineID, int thirdPartyID)
        {
            return Check(instr, thirdPartyID, linePriority);
        }

        private bool Check(CSMInstrument instr, int thirdPartyID, int linePriority)
        {
            bool res = false;
            using (var log = new CSMLog())
            {
                log.Begin(GetType().Name, MethodBase.GetCurrentMethod().Name);
                if (instr == null)
                {
                    log.Write(CSMLog.eMVerbosity.M_error, "Unable to find the instrument.Returning false.");
                    return res;
                }

                char instrType = instr.GetType_API();

                if (instrType != 'O')
                {
                    log.Write(CSMLog.eMVerbosity.M_error, "Instrument is not a bond.Returning false.");
                    return res;
                }

                bool marketFlag = true;
                bool typeFlag = true;
                bool countryFlag = true;

                OracleParameter parameter1 = new OracleParameter(":THIRDPARTY_ID", thirdPartyID);
                OracleParameter parameter2 = new OracleParameter(":SSI_ID", linePriority);
                List<OracleParameter> parameters = new List<OracleParameter>() { parameter1, parameter2 };
                string sSQL = "SELECT COUNTRYCODE, FI_TYPE , FI_EXCH_CODE FROM TIERSSETTLEMENT WHERE CODE = :THIRDPARTY_ID AND NUMERO =:SSI_ID";
                string ssiCountry = "";
                string ssiType = "";
                string ssiMarket = "";

                try
                {
                    using (OracleCommand cmd = Sophis.DataAccess.DBContext.Connection.CreateCommand())
                    {
                        cmd.CommandText = sSQL;
                        if (!parameters.IsNullOrEmpty())
                        {
                            cmd.BindByName = true;
                            cmd.Parameters.Clear();
                            foreach (var parameter in parameters)
                            {
                                cmd.Parameters.Add(parameter);
                            }
                        }

                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ssiCountry = reader[0] != DBNull.Value ? Convert.ToString(reader[0]) : "";
                                ssiType = reader[1] != DBNull.Value ? Convert.ToString(reader[1]) : "";
                                ssiMarket = reader[2] != DBNull.Value ? Convert.ToString(reader[2]) : "";
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    log.Write(CSMLog.eMVerbosity.M_error, "Exception Occured: " + ex.Message);
                }

                int sicovam = instr.GetCode();

                if (ssiMarket != "")
                {
                    using (SSMComplexReference complexRef = new SSMComplexReference())
                    {
                        complexRef.type = "EXCH_CODE";
                        complexRef.value = "";
                        log.Write(CSMLog.eMVerbosity.M_debug, "Getting reference EXCH_CODE value for instrument " + sicovam);

                        CSMInstrument.GetClientReference(sicovam, complexRef);

                        log.Write(CSMLog.eMVerbosity.M_debug, " EXCH_CODE value for instrument " + sicovam + " is: " + complexRef.value);
                        log.Write(CSMLog.eMVerbosity.M_debug, "Comparing EXCH_CODE value " + complexRef.value + " with SSI configured value " + ssiMarket);
                        if (complexRef.value == ssiMarket)
                        {
                            log.Write(CSMLog.eMVerbosity.M_debug, "Matching market values.Market Flag = True");
                        }
                        else
                        {
                            log.Write(CSMLog.eMVerbosity.M_debug, " Market values do not match. Market Flag = False");
                            marketFlag = false;
                        }
                    }
                }
                else
                {
                    log.Write(CSMLog.eMVerbosity.M_debug, "SSI Market not set. Ignoring market check.");
                }


                if (ssiType != "")
                {
                    string bondRef = instr.GetReference().StringValue;
                    string bondRefAdj = bondRef.Substring(0, 2);

                    log.Write(CSMLog.eMVerbosity.M_debug, " Reference value for instrument " + sicovam + " is: " + bondRef);
                    log.Write(CSMLog.eMVerbosity.M_debug, "Comparing first substring value " + bondRefAdj + " with SSI configured value " + ssiType);

                    if (bondRefAdj == ssiType)
                    {
                        log.Write(CSMLog.eMVerbosity.M_debug, "Matching type values.Type Flag = True ");
                    }
                    else
                    {
                        log.Write(CSMLog.eMVerbosity.M_debug, " Type values do not match. Type Flag = False");
                        typeFlag = false;
                    }

                }
                else
                {
                    log.Write(CSMLog.eMVerbosity.M_debug, "SSI Type not set. Ignoring type check.");
                }

                if (ssiCountry != "")
                {
                    int sectorID = CSMSector.GetSectorIDFromName("Country of Incorporation");
                    if (sectorID != 0)
                    {
                        log.Write(CSMLog.eMVerbosity.M_debug, "Getting 'Country of Incorporation' sector value for instrument " + sicovam);

                        using (CSMSectorData sector = instr.GetSector(sectorID))
                        {
                            if (sector != null)
                            {
                                CMString sectorCode = "";
                                sectorCode = sector.GetCode();
                                log.Write(CSMLog.eMVerbosity.M_debug, " 'Country of Incorporation' sector code for instrument " + sicovam + " is: " + sectorCode);

                                log.Write(CSMLog.eMVerbosity.M_debug, "Comparing instrument sector " + sectorCode + " with SSI configured value " + ssiCountry);
                                if (sectorCode == ssiCountry)
                                {
                                    log.Write(CSMLog.eMVerbosity.M_debug, "Matching country values.Country Flag = True ");
                                }
                                else
                                {
                                    log.Write(CSMLog.eMVerbosity.M_debug, " Country values do not match. Country Flag = False");
                                    countryFlag = false;
                                }

                            }
                        }
                    }
                }
                else
                {
                    log.Write(CSMLog.eMVerbosity.M_debug, "SSI Country not set. Ignoring country check.");
                }

                res = marketFlag & typeFlag & countryFlag;

                log.Write(CSMLog.eMVerbosity.M_debug, "Market Flag = " + marketFlag.ToString() + " Type Flag = " + typeFlag.ToString() + " Country Flag = " + countryFlag.ToString());
                log.Write(CSMLog.eMVerbosity.M_debug, "Final result for toolkit condition is (MarketFlag AND TypeFlag AND CountryFlag): " + res.ToString());

                log.End();
                return res;
            }
        }
    }
}
