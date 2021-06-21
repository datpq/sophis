using System;
using System.Linq;
using System.Collections.Generic;
using sophis.scenario;
using sophis.instrument;
using TkoPortfolioColumn.DbRequester;
using System.Globalization;
using sophis.utils;
using System.Xml;

namespace TKOSphTikehauScenario
{
    public class TkoSynchronizeNextCallDate : CSMScenario
    {
        public override eMProcessingType GetProcessingType()
        {
            return eMProcessingType.M_pScenario;
        }

        public static DateTime mydate(int i)
        {
            DateTime d = new DateTime(1904, 01, 01, 0, 0, 0);
            System.TimeSpan spa = new TimeSpan(i, 0, 0, 0);
            DateTime nd = d.Add(spa);
            return nd;
        }

        public static int stendate(DateTime d)
        {
            DateTime refdate = new DateTime(1904, 01, 01);
            TimeSpan diff = d - refdate;
            return diff.Days;
        }

        public override bool AlwaysEnabled()
        {
            return true;
        }

        public override void Run()
        {
            try
            {
                CSMLog.Write("TKO-SphTikehauScenario", "TkoSynchronizeNextCallDate", CSMLog.eMVerbosity.M_info, ">>> START SYNCHRO GUI");

                //Retrieve instrument of type '0' with American clause.
                int sysdatemoinsun = stendate(DateTime.UtcNow) ;
                var intrumentlistWithNextCallDate = DbrNextCallDate.RetrieveNextCallDate();
                foreach (var instr in intrumentlistWithNextCallDate)
                {
                   var instrument =  CSMInstrument.GetInstance(instr.sicovam);
                   if (instrument.GetType_API() == 'O')
                   {
                        CSMBond Bond = instrument;
                        if (Bond == null)
                            return;

                       System.Collections.ArrayList array = new System.Collections.ArrayList();
                       Bond.GetClauseInformation(array);

                       List<int> listofstartdate = new List<int>() ;
                       List<int> listofenddate = new List<int>() ;
                       bool isAmerican =  false ;
                       foreach(var eltclause in array)
                       {
                           var currentclause = (SSMClause)eltclause;
                           listofstartdate.Add(currentclause.start_date) ;
                           listofenddate.Add(currentclause.end_date);
                           if (currentclause.comment.StringValue == "AMERICAN")
                           {
                              isAmerican = true;
                           }
                       }

                       if(isAmerican)
                       {
                           listofenddate.Sort();
                           listofstartdate.Sort();

                           var bloombergdatetime = DateTime.ParseExact(instr.NXT_CALL_DT_BBG, "yyyy/MM/dd", CultureInfo.InvariantCulture);
                           var numbloombergdatetime  =  TkoSynchronizeNextCallDate.stendate(bloombergdatetime);
                          
                           foreach (var endate in listofenddate)
                           {
                               if (numbloombergdatetime <= endate)
                               {
                                   int i = 0;
                                   foreach (var clause in array)
                                   {   
                                       var currentclause = (SSMClause)clause;
                                       var enddate = endate;
                                       if (currentclause.end_date == enddate && (short)eMDerivativeClauseType.M_dcCall == currentclause.type)
                                       {
                                           var startdatecall = mydate(currentclause.start_date);
                                           
                                           if (currentclause.start_date <= numbloombergdatetime)
                                           {
                                                //Add bloom start date for this new clause.
                                                var tmpstartdatetochange = currentclause.start_date;
                                                currentclause.start_date = numbloombergdatetime;
                                                Bond.SetNthClauseOfThisType("Call", i, currentclause);
                                                var ret = DbrNextCallDate.UpdateClause( instr.sicovam, mydate(currentclause.start_date), mydate(currentclause.end_date), currentclause.order_id, currentclause.comment, mydate(tmpstartdatetochange), mydate(currentclause.end_date));
                                                //update in db.
                                                if (ret >= 1)
                                                {
                                                    //we have to desactivate all clause where begin Date is between sysdate - 1 and numbloombergdatetime
                                                    int j = 0;
                                                    foreach (var clausetocheck in array)
                                                    {
                                                        var currentclausetocheck = (SSMClause)clausetocheck;
                                                        if (j != i && (short)eMDerivativeClauseType.M_dcCall == currentclausetocheck.type)
                                                        {
                                                            //var currentclausetocheck = (SSMClause)clause;
                                                            if (currentclausetocheck.end_date  >= sysdatemoinsun && currentclausetocheck.end_date <= numbloombergdatetime)
                                                            {
                                                                var tmpstartdatetocheck = currentclausetocheck.start_date;
                                                                var tmpenddatetocheck = currentclausetocheck.end_date;
                                                                currentclausetocheck.start_date = sysdatemoinsun;
                                                                currentclausetocheck.end_date = sysdatemoinsun;
                                                                //currentclausetocheck.comment = "DESACTIVATED";
                                                                Bond.SetNthClauseOfThisType("Call", j, currentclausetocheck);
                                                                var ret1 = DbrNextCallDate.UpdateClause(instr.sicovam, mydate(currentclausetocheck.start_date), mydate(currentclausetocheck.end_date), currentclausetocheck.order_id, currentclausetocheck.comment, mydate(tmpstartdatetocheck), mydate(tmpenddatetocheck));
                                                                if (ret1 >=1)
                                                                {
                                                                    var msg1 = String.Format("DESACTIVATED SICOVAM[{0}] => OLD CLAUSE [{1} <=> {2}]", instr.sicovam, String.Format("{0:yyyy-MM-dd}", mydate(tmpstartdatetocheck)), String.Format("{0:yyyy-MM-dd}", mydate(tmpenddatetocheck)));
                                                                    var msg2 = String.Format("DESACTIVATED SICOVAM[{0}] =>  NEW CLAUSE  [{1} <=> {2}]", instr.sicovam, String.Format("{0:yyyy-MM-dd}", mydate(currentclausetocheck.start_date)), String.Format("{0:yyyy-MM-dd}", mydate(currentclausetocheck.end_date)));

                                                                    CSMLog.Write("TKO-SphTikehauScenario", "TkoSynchronizeNextCallDate", CSMLog.eMVerbosity.M_info, msg1);
                                                                    CSMLog.Write("TKO-SphTikehauScenario", "TkoSynchronizeNextCallDate", CSMLog.eMVerbosity.M_info, msg2);
                                                                }
                                                            }
                                                        }
                                                        j++;
                                                    }
                                                    var msg3 = String.Format("UPDATE SICOVAM[{0}] => OLD CLAUSE[{1} <=> {2}]", instr.sicovam, String.Format("{0:yyyy-MM-dd}", mydate(tmpstartdatetochange)), String.Format("{0:yyyy-MM-dd}", mydate(currentclause.end_date)));
                                                    var msg4 = String.Format("UPDATE SICOVAM[{0}] => NEW CLAUSE  [{1} <=> {2}]", instr.sicovam, String.Format("{0:yyyy-MM-dd}", mydate(currentclause.start_date)), String.Format("{0:yyyy-MM-dd}", mydate(currentclause.end_date)));
                                                    CSMLog.Write("TKO-SphTikehauScenario", "TkoSynchronizeNextCallDate", CSMLog.eMVerbosity.M_info, msg3);
                                                    CSMLog.Write("TKO-SphTikehauScenario", "TkoSynchronizeNextCallDate", CSMLog.eMVerbosity.M_info, msg4);
                                                }
                                               
                                            }
                                           break;
                                       }
                                       i++;
                                   }
                               }
                           }
                       }
                       //Bond.Save(NSREnums.eMParameterModificationType.M_pmModification);
                   }
                }

                CSMLog.Write("TKO-SphTikehauScenario", "TkoSynchronizeNextCallDate", CSMLog.eMVerbosity.M_info, "<<< END SYNCHRO GUI");
            }
            catch (Exception ex)
            {
                CSMApi.Log("Toolkit Error[TKOSphScenario => Fonction TKOSphScenario:CSMScenario.Run()]" + ex.Message);
            }
        }
    }
}
