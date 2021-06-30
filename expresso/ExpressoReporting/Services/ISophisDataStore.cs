using System.Collections.Generic;
using System.Threading.Tasks;
using ExpressoReporting.DataModel;

namespace ExpressoReporting.Services
{
    public interface ISophisDataStore
    {
        Task<User> Login(User user);
        Task<IEnumerable<Report>> GetReportsAsync(bool useCache = true);
        Task<string> GenerateReport(Report report);
        Task<string> GetLastGeneratedReport(string reportName);
    }
}
