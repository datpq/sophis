using sophis.instrument;
using sophis.utils;

namespace TKO.Indicator
{
    public class InstrumentEventIndicator : CSMInstrumentEvent
    {
        public const string Name = "InstrumentEventIndicator";

        public override void HasBeenModified(int instrument_id)
        {
            using (CSMLog logger = new CSMLog())
            {
                logger.Write(CSMLog.eMVerbosity.M_info, string.Format("HasBeenModified.BEGIN(instrument_id={0})", instrument_id));
                logger.Write(CSMLog.eMVerbosity.M_info, "HasBeenModified.END");
            }
        }
    }
}
