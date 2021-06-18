using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FibDataIntegration.DataModel;

namespace FibDataIntegration.Services
{
    public interface IDataStore
    {
        Task<IEnumerable<RateCurve>> GetRateCurve(DateTime? date = null);
    }
}
