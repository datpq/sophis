using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Oracle.DataAccess.Client;
using QuickFix;
using sophis.instrument;
using sophis.log;
using sophis.utils;
using Sophis.DataAccess;
using QuickFix.Fields;
//using sophis.quickfix;
using RichMarketAdapter.interfaces;
using RichMarketAdapter.ticket;
using sophis.configuration;


namespace Mediolanum_RMA_FILTER.Filters
{
    // This class handles the input Fix Message to retrieve specific user fields (1909) from client in Allocation data groups (Tag 78 - NoAllocs).
    // FixPlugin only handle outgoing messages, RMA IFilter handles the inpui one.
    // Set up similar as plugin in FIX Gateway > Section RMA > Filters (collection).
    
   public class CSxInputFilter : IFilter
    {
       public bool filter(IMessageWrapper message)
       {
           using (Logger log = new Logger(this, this.GetType().Name))
           {
              // bool retval = false;

             try
             {

               // Geting the quickFixMessage out of the RMA IMessage.
               ITicketMessage ticket = (ITicketMessage)message.Message;
               if(ticket!=null)
               {
                    Message quickFixMessage = (Message)ticket.TransientData;
                    {
                        if (quickFixMessage.IsSetField(Tags.ClOrdID))
                        {

                            int orderID = Convert.ToInt32(quickFixMessage.GetField(Tags.ClOrdID));

                            //IOrder order = OrderManagerConnector.Instance.GetOrderManager().GetOrderById(orderID);
                            
                            
                            if (quickFixMessage.IsSetField(Tags.NoAllocs))
                            {

                                int nbAllocs = Convert.ToInt32(quickFixMessage.GetField(Tags.NoAllocs));

                                for (int i = 1; i < nbAllocs + 1; i++)
                                {
                                    string accountname = "";
                                    string execReference = "";

                                    Group group = new Group(Tags.NoAllocs, Tags.AllocAccount);
                                    quickFixMessage.GetGroup(i, group);

                                    if (group.IsSetField(Tags.AllocAccount))
                                    {
                                       //TODO include the tag 79 logic here...?
                                        accountname = group.GetField(Tags.AllocAccount);
                                        log.log(Severity.debug, " Got Account Folio : " + accountname);

                                        // Checking Tag as FX All external reference from FXALL
                                        if (group.IsSetField(1909))
                                        {
                                           
                                            execReference = group.GetField(1909);
                                            log.log(Severity.debug, " Tag 1909 set to Value = " + execReference);

                                            //TSave in DB...
                                            if ((orderID > 0) && !string.IsNullOrEmpty(accountname) && !string.IsNullOrEmpty(execReference))
                                            {
                                                if (SaveToDatabase(orderID, accountname, execReference) == false)
                                                {
                                                    log.log(Severity.error, "Issue While saving data to database.");
                                                }
                                            }
                                            else
                                            {
                                                log.log(Severity.error, " Issue with reference set Order ID = " + orderID.ToString() + ", Account name = " + accountname + ", External Reference =" + execReference);
                                            }
                                        }
                                        else
                                        {
                                            log.log(Severity.debug, " Tag 1909 is not set ");
                                        }
                                    }
                                    else
                                    {
                                        log.log(Severity.debug, "Tag Alloc Account not Set.");
                                    }
                                }
                            }
                        }
                        else
                        {
                            log.log(Severity.debug, "Order Id is not Set.");
                        }
                        
                   }
               }
             }
            catch (Exception ex)
            {
                log.log(Severity.error, "Exception Occured during Execution Processing : " + ex.Message);
            }

            return false;
           }

       }

       bool SaveToDatabase(int orderId, string accountName, string extReference)
       {
           using (Logger log = new Logger(this, this.GetType().Name))
           {
               bool retval = true;
               int entity = 0;

               try
               {

                   //string sql = "select entiry from order_defparam_selector where trading_account = :accountName AND EXTERNAL_SYSTEM='FXALL'";
                   //OracleParameter parameter = new OracleParameter(":accountName", accountName);

                   using (var cmd = new OracleCommand())
                   {
                       if (DBContext.Connection == null)
                       {

                            // create a new connection  
                           string connectionString = sophis.configuration.CommonConfigurationGroup.Current.RisqueDatabaseSection.ConnectionString ;
                               
                           OracleConnection myConnection = new OracleConnection(connectionString);
                           // this retrieves the 1st private static field of type OracleConnection  
                           // currently there's only one of such fields  
                         FieldInfo sharedConnectionField = typeof(Sophis.DataAccess.DBContext).GetFields(BindingFlags.NonPublic | BindingFlags.Static).FirstOrDefault(f => f.FieldType == typeof(OracleConnection));
                           // set the value  
                           // 1st argument is NULL because this is a static field  
                         sharedConnectionField.SetValue(null, myConnection);
                           // open the connection  
                           Sophis.DataAccess.DBContext.Connection.Open();
                       }

                       cmd.Connection = DBContext.Connection;

                       cmd.CommandText = "select entity from order_defparam_selector where trading_account ='" + accountName + "' AND EXTERNAL_SYSTEM='FXALL'";
                       //  cmd.Parameters.Add(parameter);
                       entity = cmd.ExecuteScalar() == DBNull.Value ? 0 : Convert.ToInt32(cmd.ExecuteScalar());

                       if (entity > 0)
                       {
                           log.log(Severity.debug, "Deleting any existing entry");
                           cmd.CommandText = "delete MEDIO_FXALL_TEMP_EXTRNREF where PLACEMENT_ID = " + orderId.ToString() + " AND ENTITY=" + entity.ToString();
                           log.log(Severity.debug, "Using Query : " + cmd.CommandText.ToString());
                           cmd.ExecuteNonQuery();
                           log.log(Severity.debug, "Inserting entry");
                           cmd.CommandText = "insert into MEDIO_FXALL_TEMP_EXTRNREF(PLACEMENT_ID,ENTITY,EXTERNAL_REFERENCE) Values(" + orderId + "," + entity + ",'" + extReference + "')";
                           log.log(Severity.debug, "Using Query : " + cmd.CommandText.ToString());
                           cmd.ExecuteNonQuery();
                           // Should Auto Commit

                       }
                       else
                       {
                           log.log(Severity.error, "No valid entity found");
                           retval = false;
                       }
                   }
               }
               catch (Exception ex)
               {
                   log.log(Severity.error, " Exception caught : " + ex.Message);
                   retval = false;
               }
               return retval;
           }
       }

   }
}

