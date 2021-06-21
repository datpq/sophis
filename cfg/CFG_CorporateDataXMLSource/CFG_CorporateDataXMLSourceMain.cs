using System;

using sophis;
using sophis.utils;
using CFG_CorporateDataXMLSource;

///{{SOPHIS_TOOLKIT_INCLUDE (do not delete this line)
using sophis.reporting;

//}}SOPHIS_TOOLKIT_INCLUDE


namespace CFG_CorporateDataXMLSource
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

            CSMXMLSource.Register("CFG_CorporateDataExport", new CSxCorporateDataExportSource());
            CSMXMLSource.Register("CFG_CorporateDataImport", new CSxCorporateDataImportSource());
            CSMXMLSource.Register("CFG_CorporateDataDeletion", new CSxCorporateDataDeletionSource());
            CSMXMLSource.Register("CFG_SOAServerCulture", new CSxSOAServerCultureSource());

            //}}SOPHIS_INITIALIZATION
        }

        public void Close()
        {
            GC.Collect();
        }
    }

}