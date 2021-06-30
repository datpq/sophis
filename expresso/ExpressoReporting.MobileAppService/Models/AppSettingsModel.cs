using ExpressoReporting.DataModel;
using System.Collections.Generic;

namespace ExpressoReporting.MobileAppService.Models
{
    public class AppSettingsModel
    {
        public string SophisWorkingDirectory { get; set; }
        public string SophisCommandLine { get; set; }
        public string SophisConfigFile { get; set; }
        public string ConnectionString { get; set; }
        public int CacheDefaultTimeout { get; set; }
        public int SophisExecWaitingTime { get; set; }
        public IList<User> Users { get; set; } = new List<User>();
    }
}
