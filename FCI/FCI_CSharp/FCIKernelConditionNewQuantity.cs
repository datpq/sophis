/*
** Includes
*/
using System;
using sophis.utils;
using sophis.backoffice_kernel;
using sophis.portfolio;
using sophis.misc;

namespace FCI_CSharp
{
    /// <summary>
    /// <para>Interface to create a new condition to select a workflow or a rule in a workflow.</para>
    /// <para>Only available with back office kernel.</para>
    /// </summary>
    /// <remarks>
    ///	When creating or modifying a transaction, the kernel engine will first select a workflow browsing the
    ///	workflow selector; when the criteria matchs, it plays the user conditions. A new condition can be added
    ///	by implementing this interface. Once the workflow has been selected, then the kernel engine selects a rule
    ///	browsing the workflow rules; when the criteria matchs, it plays the same user conditions.
    /// </remarks>
    public class FCIKernelConditionNewQuantity : CSMKernelCondition
    {
        public const int eNewQuantity = 12379 - 100;

        /// <summary>
        /// <para>Used by the framework while selecting the rule from Workflow Definition rules set.</para>
		/// <para>
        /// Method is called for Condition1, Condition2, Condition3 columns. Logical 'AND' is used
        /// to make decision if to select the matching rule - found by framework.
        /// </para>
        /// </summary>
        /// <param name="tr">is the reference to the transaction associated with the processed deal;
        /// it is the final (resp. initial) state for a deal created or modified (resp. canceled).
        /// </param>
        /// <param name="sel">is a structure giving some information about the instrument created; As the instrument may be
		/// not yet created, the structure gives some data coming from the future instrument created; if the instrument is created,
		/// the structure gives the data from the instrument.
        /// </param>
        /// <returns>is the boolean and is calculated by the client code. The result has to be TRUE to make the rule selected.</returns>
		public override bool GetCondition(CSMTransaction tr, SSMKernelInstrumentSelector sel)
        {
            bool result = false;
            var logger = new CSMLog();
            logger.Begin("FCIKernelConditionNewQuantity", "GetCondition");
            try
            {
                var newQuantityThresholdStr = string.Empty;
                CSMConfigurationFile.getEntryValue("FCI", "TKT_THRESHOLD", ref newQuantityThresholdStr, "0");
                var newQuantityThreshold = double.Parse(newQuantityThresholdStr);
                logger.Write(CSMLog.eMVerbosity.M_debug, $"refcon={tr.GetTransactionCode()}, newQuantityThreshold={newQuantityThreshold}");

                tr.LoadGeneralElement(eNewQuantity, out double newQuantity);

                logger.Write(CSMLog.eMVerbosity.M_debug, $"newQuantity={newQuantity}");
                result = newQuantity < newQuantityThreshold;
            }
            catch (Exception e)
            {
                logger.Write(CSMLog.eMVerbosity.M_error, "error: " + e.Message);
                logger.Write(CSMLog.eMVerbosity.M_error, e.StackTrace);
            }
            finally
            {
                logger.Write(CSMLog.eMVerbosity.M_debug, $"result={result}");
                logger.End();
            }

            return result;
        }
    }
}
