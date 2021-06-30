using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ExpressoReporting.DataModel;
using Newtonsoft.Json;
using PCLAppConfig;
using PCLStorage;
using Plugin.Connectivity;
using Xamarin.Forms;

namespace ExpressoReporting.Services
{
    public class SophisDataStore : ISophisDataStore
    {
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();
        private const string DefaultPdfFilename = "expresso.pdf";

        private static HttpClient GetHttpClient()
        {
            return new HttpClient { BaseAddress = new Uri($"{ConfigurationManager.AppSettings["AzureBackendUrl"]}/") };
        }

        private static HttpClient GetSecuredHttpClient()
        {
            var client = GetHttpClient();
            client.DefaultRequestHeaders.Add("user", App.User.Name);
            client.DefaultRequestHeaders.Add("token", App.User.Token);
            Logger.Debug($"user={App.User.Name}, token={App.User.Token}");
            return client;
        }

        private static void Validate()
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                throw new Exception("No Internet");
            }
        }

        public async Task<User> Login(User user)
        {
            var stopWatch = Stopwatch.StartNew();
            User result = null;
            try
            {
                Logger.Debug($"Login.BEGIN(username={user.Name})");

                Validate();

                var client = GetHttpClient();
                var requestUrl = "api/Sophis/Login";
                Logger.Debug($"requestUrl: {requestUrl}");

                var serializedUser = JsonConvert.SerializeObject(user);
                var response = await client.PostAsync(requestUrl, new StringContent(serializedUser, Encoding.UTF8, "application/json"));
                Logger.Debug($"StatusCode: {response.StatusCode}");
                if (response.IsSuccessStatusCode)
                {
                    result = await response.Content.ReadAsAsync<User>();
                }
                else
                {
                    var msg = await response.Content.ReadAsAsync<Message>();
                    Logger.Error($"Error: {msg}");
                }

                return result;
            }
            catch (Exception e) //Unknown error
            {
                Logger.Error(e.ToString());
                return result;
            }
            finally
            {
                Logger.Debug($"Login.END(result={result}, ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        public async Task<IEnumerable<Report>> GetReportsAsync(bool useCache = true)
        {
            var stopWatch = Stopwatch.StartNew();
            IEnumerable<Report> result = null;
            try
            {
                Logger.Debug($"GetReportsAsync.BEGIN(useCache={useCache})");

                Validate();

                var client = GetSecuredHttpClient();
                var requestUrl = $"api/Sophis/GetReports/{useCache}";
                Logger.Debug($"requestUrl: {requestUrl}");
                var response = await client.GetAsync(requestUrl);
                Logger.Debug($"StatusCode: {response.StatusCode}");
                var jsonString = response.Content.ReadAsStringAsync();

                jsonString.Wait();
                //Logger.Debug($"jsonString: {jsonString.Result}");

                if (response.IsSuccessStatusCode)
                {
                    result = JsonConvert.DeserializeObject<IEnumerable<Report>>(jsonString.Result);
                }
                else
                {
                    var msg = await response.Content.ReadAsAsync<Message>();
                    Logger.Error($"Error: {msg}");
                }

                return result;
            }
            catch (Exception e) //Unknown error
            {
                Logger.Error(e.ToString());
                return result;
            }
            finally
            {
                Logger.Debug($"GetReportsAsync.END({JsonConvert.SerializeObject(result)}, ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        public async Task<string> GenerateReport(Report report)
        {
            var stopWatch = Stopwatch.StartNew();
            string result = null;
            try
            {
                Logger.Debug($"GenerateReport.BEGIN(report={report.Name})");
                foreach(var param in report.Parameters)
                {
                    Logger.Debug($"{param.Name}={param.Value}");
                }

                Validate();

                var client = GetSecuredHttpClient();
                var requestUrl = "api/Sophis/GenerateReport";
                Logger.Debug($"requestUrl: {requestUrl}");
                var serializedReport = JsonConvert.SerializeObject(report);
                var response = await client.PostAsync(requestUrl, new StringContent(serializedReport, Encoding.UTF8, "application/json"));
                Logger.Debug($"StatusCode: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var inputStream = await response.Content.ReadAsStreamAsync();
                    var file = await FileSystem.Current.LocalStorage.CreateFileAsync(DefaultPdfFilename, CreationCollisionOption.ReplaceExisting);
                    using (var stream = await file.OpenAsync(FileAccess.ReadAndWrite))
                    {
                        while (inputStream.Position < inputStream.Length)
                        {
                            stream.WriteByte((byte)inputStream.ReadByte());
                        }
                    }

                    result = file.Path;
                }
                else
                {
                    var msg = await response.Content.ReadAsAsync<Message>();
                    Logger.Error($"Error: {msg}");
                }

                return result;
            }
            catch (Exception e) //Unknown error
            {
                Logger.Error(e.ToString());
                return result;
            }
            finally
            {
                Logger.Debug($"GenerateReport.END(result={result}, ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        public async Task<string> GetLastGeneratedReport(string reportName)
        {
            var stopWatch = Stopwatch.StartNew();
            string result = null;
            try
            {
                Logger.Debug($"GetLastGeneratedReport.BEGIN(reportName={reportName})");

                Validate();

                var client = GetSecuredHttpClient();
                var requestUrl = $"api/Sophis/GetLastGeneratedReport/{reportName}";
                Logger.Debug($"requestUrl: {requestUrl}");
                var response = await client.GetAsync(requestUrl);
                Logger.Debug($"StatusCode: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var inputStream = await response.Content.ReadAsStreamAsync();
                    var file = await FileSystem.Current.LocalStorage.CreateFileAsync(DefaultPdfFilename, CreationCollisionOption.ReplaceExisting);
                    using (var stream = await file.OpenAsync(FileAccess.ReadAndWrite))
                    {
                        while (inputStream.Position < inputStream.Length)
                        {
                            stream.WriteByte((byte)inputStream.ReadByte());
                        }
                    }

                    Logger.Debug($"inputStream.Length={inputStream.Length}, inputStream.Position={inputStream.Position}");
                    result = file.Path;
                }
                else
                {
                    var msg = await response.Content.ReadAsAsync<Message>();
                    Logger.Error($"Error: {msg}");
                }

                return result;
            }
            catch (Exception e) //Unknown error
            {
                Logger.Error(e.ToString());
                return result;
            }
            finally
            {
                Logger.Debug($"GetLastGeneratedReport.END(result={result}, ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }
    }
}
