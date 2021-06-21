using sophis;

///{{SOPHIS_TOOLKIT_INCLUDE (do not delete this line)

//}}SOPHIS_TOOLKIT_INCLUDE


namespace TKO.Indicator
{
    /// <summary>
    /// Definition of DLL entry point: to register new functionality and closing point
    /// </summary>
    public class MainClass : IMain
    {
        public void EntryPoint()
        {
            //{{SOPHIS_INITIALIZATION (do not delete this line)

            ColumnIndicator.Register();
            //CSMInstrumentEvent.Register(InstrumentEventIndicator.Name, new InstrumentEventIndicator());

            //}}SOPHIS_INITIALIZATION
        }

        public void Close()
        {

        }
    }

}
