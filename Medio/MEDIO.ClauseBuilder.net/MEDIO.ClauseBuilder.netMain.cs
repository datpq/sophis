using System;

using sophis;
using sophis.utils;
using MEDIO.ClauseBuilder.net;
using MEDIO.ClauseBuilder.net.Data;
using MEDIO.ClauseBuilder.net.GUI;
using sophis.finance;

///{{SOPHIS_TOOLKIT_INCLUDE (do not delete this line)

//}}SOPHIS_TOOLKIT_INCLUDE


namespace MEDIO.ClauseBuilder.net
{
    /// <summary>
    /// Definition of DLL entry point, registrations, and closing point
    /// </summary>
    public class MainClass : IMain
    {
        public void EntryPoint()
        {
            //{{SOPHIS_INITIALIZATION (do not delete this line)

            // TO DO; Perform registrations

            Sophis.ClauseBuilder.ClauseBuilderExoticMaskToolkit.Instance.Register<Sophis.ClauseBuilder.ClauseBuilderExoticMask<
                CSxClauseBuilderAutocallData, CSxClauseBuilderAutocallWizard>>("Medio Autocall");
            Sophis.ClauseBuilder.ClauseBuilderMaskToolkitUI.Instance.Register<CSxClauseBuilderAutocallGUI>("Medio Autocall");
            
            //}}SOPHIS_INITIALIZATIONs
        }

        public void Close()
        {
            GC.Collect();
        }
    }

}