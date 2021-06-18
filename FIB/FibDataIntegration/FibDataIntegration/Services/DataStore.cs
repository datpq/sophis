using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using FibDataIntegration.DataModel;
using Newtonsoft.Json;
using NLog;

namespace FibDataIntegration.Services
{
    public class DataStore : IDataStore
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        //[Obsolete("Do not use this in Production code!!!", true)]
        private static void NEVER_EAT_POISON_Disable_CertificateValidation()
        {
            // Disabling certificate validation can expose you to a man-in-the-middle attack
            // which may allow your encrypted message to be read by an attacker
            // https://stackoverflow.com/a/14907718/740639
            ServicePointManager.ServerCertificateValidationCallback =
                delegate(
                    object s,
                    X509Certificate certificate,
                    X509Chain chain,
                    SslPolicyErrors sslPolicyErrors
                )
                {
                    return true;
                };
        }

        private static HttpClient GetHttpClient()
        {
            NEVER_EAT_POISON_Disable_CertificateValidation();
            return new HttpClient { BaseAddress = new Uri(ConfigurationManager.AppSettings["ApiBackendUrl"]) };
        }

        private static HttpClient GetSecuredHttpClient()
        {
            var client = GetHttpClient();
            client.DefaultRequestHeaders.Add(ConfigurationManager.AppSettings["TokenName"], ConfigurationManager.AppSettings["TokenValue"]);
            Logger.Debug("token={0}", ConfigurationManager.AppSettings["TokenValue"]);
            return client;
        }

        public async Task<IEnumerable<RateCurve>> GetRateCurve(DateTime? date = null)
        {
            var stopWatch = Stopwatch.StartNew();
            IEnumerable<RateCurve> result = null;
            try
            {
                Logger.Debug("GetRateCurve.BEGIN(date={0})", date);

                var client = GetSecuredHttpClient();
                if (!date.HasValue) date = DateTime.Today.AddDays(-1);
                var requestUrl = string.Format("mo/Version1/api/CourbeBDT?dateCourbe={0}", date.Value.ToString("MM/dd/yyyy"));
                Logger.Debug("requestUrl: {0}", requestUrl);
                var response = await client.GetAsync(requestUrl);
                Logger.Debug("StatusCode: {0}", response.StatusCode);

                var jsonString = response.Content.ReadAsStringAsync();
                jsonString.Wait();
                //Logger.Debug("jsonString: {0}", jsonString.Result);

                if (response.IsSuccessStatusCode)
                {
                    result = JsonConvert.DeserializeObject<IEnumerable<RateCurve>>(jsonString.Result);
                }

                return result;
            }
            catch (WebException e)
            {
                Logger.Error(e.ToString());
                throw;
            }
            catch (Exception e) //Unknown error
            {
                Logger.Error(e.ToString());
                return result;
            }
            finally
            {
                Logger.Debug("GetRateCurve.END(ProcessingTime={0})", stopWatch.Elapsed.ToStringStandardFormat());
            }
        }
    }
}
