
using sophis;
using sophis.utils;
using System;
using System.Threading;
///{{SOPHIS_TOOLKIT_INCLUDE (do not delete this line)

//}}SOPHIS_TOOLKIT_INCLUDE


namespace CFG_Utilities
{
    /// <summary>
    /// Definition of DLL entry point: to register new functionality and closing point
    /// </summary>
    public class MainClass : IMain
    {
        private CSMLog logger = new CSMLog();

        public void EntryPoint()
        {
            //{{SOPHIS_INITIALIZATION (do not delete this line)

            // TO DO; Perform registrations

            //}}SOPHIS_INITIALIZATION

            new Thread(() =>
            {
                try
                {
                    logger.Begin("MainClass", "Thread");
                    Thread.CurrentThread.IsBackground = true;
                    IntPtr srDlg = IntPtr.Zero;
                    while (true)
                    {
                        Thread.Sleep(200);
                        TaskFixingSrDialog.Run();
                    }
                }
                catch (Exception e)
                {
                    logger.Write(CSMLog.eMVerbosity.M_error, "error: " + e.Message);
                    logger.Write(CSMLog.eMVerbosity.M_error, e.StackTrace);
                }
                finally
                {
                    logger.End();
                }
            }).Start();
        }

        public void Close()
        {
        }
    }

}
