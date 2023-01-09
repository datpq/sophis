using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Reflection;
using System.Threading;
using System.Configuration;

namespace RBCInstrumentsPrice
{
    public class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void Main(string[] args)
        {

            try
            {
                DataModel.LoadFileMappings();
                DataModel.LoadFileAllotments();
                CSxDBHelper.Initialize();

                LookupController.LoadInstrumentsInScopeFromFusion();
                FileWatcher watcher = new FileWatcher();
                watcher.watch();

            }
            catch (Exception exc)
            {
                log.Error(exc.ToString());
            }

        }
    }
}
