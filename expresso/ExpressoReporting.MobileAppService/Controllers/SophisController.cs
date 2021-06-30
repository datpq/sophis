using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using ExpressoReporting.DataModel;
using ExpressoReporting.MobileAppService.Models;
using ExpressoReporting.MobileAppService.Services;
using ISReportingService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NLog;
using Oracle.ManagedDataAccess.Client;
using Message = ExpressoReporting.DataModel.Message;
using Parameter = ExpressoReporting.DataModel.Parameter;
using Report = ExpressoReporting.DataModel.Report;
using User = ExpressoReporting.DataModel.User;

namespace ExpressoReporting.MobileAppService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SophisController : ControllerBase
    {
        private readonly IOptions<AppSettingsModel> _appSettings;
        private readonly ICacheService _cacheService;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        protected readonly string CacheNameSpace;

        private const string ReportTransformQuery = @"
SELECT RT.STEP, RT.TYPE_NAME, RTP.TRANSFORM_PARAM_INDEX, RTP.PARAM_VALUE
FROM REPORT_XMLTRANSFORM_PARAM RTP
    JOIN REPORT_XMLTRANSFORM RT ON RT.TEMPLATE_NAME = RTP.TEMPLATE_NAME AND RTP.TRANSFORM_INDEX = RT.TRANSFORM_INDEX
WHERE RTP.TEMPLATE_NAME = '{0}'
ORDER BY RT.STEP, RTP.TRANSFORM_PARAM_INDEX";

        //private const string ConfigFilePrefix = "file://";

        public SophisController(IOptions<AppSettingsModel> appSettings, ICacheService cacheService)
        {
            _appSettings = appSettings;
            _cacheService = cacheService;
            CacheNameSpace = GetType().FullName;
        }

        [HttpPost]
        [Route("Login")]
        public async Task<ActionResult<User>> Login([FromBody] User user)
        {
            var stopWatch = Stopwatch.StartNew();
            ActionResult<User> result = null;
            try
            {
                Logger.Debug($"BEGIN(userName={user.Name})");

                var reportingServiceClient = new ReportingServiceClient();
                reportingServiceClient.ClientCredentials.UserName.UserName = user.Name;
                reportingServiceClient.ClientCredentials.UserName.Password = user.Password;
                //reportingServiceClient.getReportsListAsync(new GetReportsList()).ContinueWith(task =>
                //{

                //});
                var loginResult = (await reportingServiceClient.loginAsync(new Login
                {
                    user = user.Name,
                    password = user.Password
                })).loginOutput;

                Logger.Debug($"error.code={loginResult.error.code}, error.reason={loginResult.error.reason})");
                if (loginResult.error.code != ErrorCode.SUCCESS)
                {
                    Logger.Warn($"User login failed. code={loginResult.error.code}, reason={loginResult.error.reason}");
                    return BadRequest(new Message { Code = MessageCode.UserLoginFailed, Msg = loginResult.error.reason });
                }

                var reportListResult = await reportingServiceClient.getReportsListAsync(new GetReportsList
                {
                    token = loginResult.token,
                    reportsListRequest = new ReportsListRequest
                    {

                    }
                });
                Logger.Debug($"reportListResult={JsonConvert.SerializeObject(reportListResult)}");

                var token = loginResult.token.securityId;
                Logger.Debug($"token={token})");
                var resultUser = new User
                {
                    Name = user.Name,
                    Token = token
                };
                result = resultUser;
                _appSettings.Value.Users.Where(x => x.Name.Equals(user.Name,
                    StringComparison.OrdinalIgnoreCase)).ToList().ForEach(x => _appSettings.Value.Users.Remove(x));
                _appSettings.Value.Users.Add(resultUser);
                Logger.Debug($"Users count: {_appSettings.Value.Users.Count}");
                //reportingServiceClient.loginAsync(new Login
                //{
                //    user = @"Alhambra\Dat",
                //    password = "Dat"
                //}).ContinueWith(task =>
                //{
                //    var token = task.Result?.loginOutput.token;
                //});
                return result;
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                return BadRequest("Unknown error");
            }
            finally
            {
                Logger.Debug($"END(ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("GetReports/{useCache}")]
        public ActionResult<IEnumerable<Report>> GetReports([FromHeader] string user, [FromHeader] string token, bool useCache = true)
        {
            var stopWatch = Stopwatch.StartNew();
            ActionResult<IEnumerable<Report>> result = null;
            try
            {
                Logger.Debug($"BEGIN(user={user}, token={token}, useCache={useCache})");

                #region Data Validation

                if (string.IsNullOrEmpty(token))
                {
                    Logger.Warn(Message.MsgMissingToken.Msg);
                    return BadRequest(Message.MsgMissingToken);
                }

                if (string.IsNullOrEmpty(user))
                {
                    Logger.Warn(Message.MsgUserNotFound.Msg);
                    return BadRequest(Message.MsgUserNotFound);
                }

                if (!_appSettings.Value.Users.Any(x => x.Name.Equals(user, StringComparison.OrdinalIgnoreCase)))
                {
                    Logger.Warn(Message.MsgUserNotFound.Msg);
                    return BadRequest(Message.MsgUserNotFound);
                }

                if (!_appSettings.Value.Users.Any(x => x.Name.Equals(
                    user, StringComparison.OrdinalIgnoreCase) && x.Token == token))
                {
                    Logger.Warn(Message.MsgWrongToken.Msg);
                    return BadRequest(Message.MsgWrongToken);
                }

                #endregion

                var cachePrefix = $"{CacheNameSpace}.{MethodBase.GetCurrentMethod().Name}";
                var cacheTimeout = _cacheService.GetCacheTimeout(cachePrefix);
                var cacheKey = $"{cachePrefix}.";

                Logger.Debug($"cacheKey={cacheKey}, cacheTimeout={cacheTimeout}");

                if (useCache)
                {
                    result = _cacheService.Get(cacheKey) as ActionResult<IEnumerable<Report>>;
                    if (result != null)
                    {
                        Logger.Debug("Cache found. Return value in cache.");
                        return result;
                    }
                }

                var arrResult = new List<Report>();
                using (var conn = new OracleConnection(_appSettings.Value.ConnectionString))
                {
                    conn.Open();
                    using (var command = new OracleCommand(@"
SELECT R.NAME REPORT_NAME, RP.NAME PARAM_NAME, RP.TYPE PARAM_TYPE,
    R.*, RP.* FROM REPORT_TEMPLATE R
    LEFT JOIN REPORT_PARAMETER RP ON RP.REPORT_TEMPLATE_NAME = R.NAME
WHERE R.NAME IN (SELECT TEMPLATE_NAME FROM REPORT_XMLTRANSFORM)
ORDER BY R.NAME, RP.ID", conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            var lastReportName = string.Empty;
                            Report currentReport = null;
                            while (true)
                            {
                                if (!reader.Read()) break;
                                var currentReportName = reader.GetString(0);
                                if (currentReportName != lastReportName) //new report found
                                {
                                    currentReport = new Report
                                    {
                                        Name = currentReportName,
                                        Parameters = new List<Parameter>()
                                    };
                                    arrResult.Add(currentReport);
                                    lastReportName = currentReportName;
                                }

                                if (reader.IsDBNull(1)) continue;
                                var paramName = reader.GetString(1);
                                if (!Enum.TryParse<ParameterType>(reader.GetString(2), out var paramType))
                                {
                                    paramType = ParameterType.Unknown;
                                }

                                (currentReport.Parameters as List<Parameter>).Add(new Parameter
                                {
                                    //Report = currentReport,
                                    Name = paramName,
                                    Type = paramType
                                });
                            }
                        }
                    }
                }

                result = arrResult;

                _cacheService.Set(cacheKey, result, DateTimeOffset.Now.AddMinutes(cacheTimeout));
                return result;
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                return BadRequest("Unknown error");
            }
            finally
            {
                Logger.Debug(
                    $"END(count={result?.Value.Count()}, ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        [HttpPost]
        [Route("GenerateReport")]
        public IActionResult GenerateReport([FromHeader] string user, [FromHeader] string token, [FromBody] Report report)
        {
            var stopWatch = Stopwatch.StartNew();
            IActionResult result = null;
            try
            {
                Logger.Debug($"BEGIN(reportName={report.Name})");

                #region Data Validation

                if (string.IsNullOrEmpty(token))
                {
                    Logger.Warn(Message.MsgMissingToken.Msg);
                    return BadRequest(Message.MsgMissingToken);
                }

                if (string.IsNullOrEmpty(user))
                {
                    Logger.Warn(Message.MsgUserNotFound.Msg);
                    return BadRequest(Message.MsgUserNotFound);
                }

                if (!_appSettings.Value.Users.Any(x => x.Name.Equals(user, StringComparison.OrdinalIgnoreCase)))
                {
                    Logger.Warn(Message.MsgUserNotFound.Msg);
                    return BadRequest(Message.MsgUserNotFound);
                }

                if (!_appSettings.Value.Users.Any(x => x.Name.Equals(
                    user, StringComparison.OrdinalIgnoreCase) && x.Token == token))
                {
                    Logger.Warn(Message.MsgWrongToken.Msg);
                    return BadRequest(Message.MsgWrongToken);
                }

                #endregion

                foreach (var param in report.Parameters)
                {
                    Logger.Debug($"{param.Name}={param.Value}");
                }

                var runDir = $"Runs{Path.DirectorySeparatorChar}{DateTime.Now.ToString("yyyy-MM-dd-HHmmss")}-{Request.HttpContext.TraceIdentifier.Replace(":", "_")}";
                var runDirFullPath = $"{_appSettings.Value.SophisWorkingDirectory}{Path.DirectorySeparatorChar}{runDir}";
                Logger.Debug($"runDirFullPath={runDirFullPath}");
                if (!Directory.Exists(runDirFullPath))
                {
                    Directory.CreateDirectory(runDirFullPath);
                }
                var configTemplateFile = $"{_appSettings.Value.SophisWorkingDirectory}{Path.DirectorySeparatorChar}{_appSettings.Value.SophisConfigFile}";
                var configFile = $"{runDirFullPath}{Path.DirectorySeparatorChar}{_appSettings.Value.SophisConfigFile}";
                System.IO.File.Copy(configTemplateFile, configFile);

                //modify config file
                var xmlConfidDoc = XDocument.Load(configFile);
                var reportsElem = xmlConfidDoc.Descendants("Reports").SingleOrDefault();
                var elemsToRemove = reportsElem.Elements("add").ToList();
                elemsToRemove.ForEach(x => x.Remove());

                reportsElem.Add(new XElement("add", new XAttribute("Id", "1"), new XAttribute("Name", report.Name)));
                reportsElem = reportsElem.Element("add");
                reportsElem.Add(new XElement("Parameters"));
                reportsElem = reportsElem.Element("Parameters");
                reportsElem.Add(new XElement("clear"));
                report.Parameters.ToList().ForEach(x =>
                {
                    reportsElem.Add(new XElement("add", new XAttribute("Name", x.Name), new XAttribute("Value", x.Value)));
                });

                xmlConfidDoc.Save(configFile);

                //run command
                var commandLine = string.Format(_appSettings.Value.SophisCommandLine, $"{runDir}{Path.DirectorySeparatorChar}{_appSettings.Value.SophisConfigFile}");
                Logger.Debug($"commandLine={commandLine}");

                var pInfo = new ProcessStartInfo
                {
                    FileName = commandLine.Substring(0, commandLine.IndexOf(" ")),
                    Arguments = commandLine.Substring(commandLine.IndexOf(" ")),
                    WorkingDirectory = _appSettings.Value.SophisWorkingDirectory
                };
                var process = Process.Start(pInfo);
                process.WaitForExit(_appSettings.Value.SophisExecWaitingTime);
                Logger.Debug("Process finished.");

                //var reportTransforms = GetReportTransformsInfo(report.Name);
                //reportTransforms.Where(x => x.TransformType == ReportTransform.TransformTypeExternalCommand).ToList().ForEach(x =>
                //{
                //    pInfo = new ProcessStartInfo
                //    {
                //        FileName = x.Params.Single(y => y.Index == 1).ParamVal,
                //        Arguments = string.Join(' ', x.Params.Where(y => y.Index >= 2 && y.Index <=9
                //        && !string.IsNullOrEmpty(y.ParamVal)).Select(y => y.ParamVal)),
                //        WorkingDirectory = _appSettings.Value.SophisWorkingDirectory
                //    };
                //    Logger.Debug($"FileName={pInfo.FileName}, Arguments={pInfo.Arguments}");
                //    process = Process.Start(pInfo);
                //    process.WaitForExit(_appSettings.Value.SophisExecWaitingTime);
                //    Logger.Debug("Process finished.");
                //});

                var outputFilePath = GetLastGeneratedReportPath(report.Name);

                if (outputFilePath.Length > 0 && System.IO.File.Exists(outputFilePath))
                {
                    var backupFilePath = $"{runDirFullPath}{Path.DirectorySeparatorChar}{Path.GetFileName(outputFilePath)}";
                    Logger.Debug($"outputFilePath={outputFilePath}, backupFilePath={backupFilePath}");
                    System.IO.File.Copy(outputFilePath, backupFilePath);
                    result = new PhysicalFileResult(outputFilePath, "application/pdf");
                }

                //determine the config file name
                //var sophisDir = Path.GetDirectoryName(_appSettings.Value.SophisCommandLine);
                //var xmlDoc = XDocument.Load($"{sophisDir}{Path.DirectorySeparatorChar}Bootstrap.config");
                //var configUri = xmlDoc.Descendants("BootstrapProgramConfiguration").Descendants("ProgramBootstrap")
                //    .Attributes("ProgramConfigurationUri").FirstOrDefault().Value;
                //string configFileName = "risk.config";
                //if (configUri.StartsWith(ConfigFilePrefix, StringComparison.OrdinalIgnoreCase))
                //{
                //    configFileName = configUri.Substring(ConfigFilePrefix.Length);
                //}
                //configFileName = $"{_appSettings.Value.SophisWorkingDirectory}{Path.DirectorySeparatorChar}{configFileName}";
                //Logger.Debug($"configFileName = {configFileName}");
                //if (!System.IO.File.Exists(configFileName)) return result;
                //var xmlConfigDoc = XDocument.Load(configFileName);

                return result;
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                return BadRequest("Unknown error");
            }
            finally
            {
                Logger.Debug($"END(ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        private List<ReportTransform> GetReportTransformsInfo(string reportName)
        {
            var result = new List<ReportTransform>();
            using (var conn = new OracleConnection(_appSettings.Value.ConnectionString))
            {
                conn.Open();
                var sqlQuery = string.Format(ReportTransformQuery, reportName.Replace("'", "''"));
                Logger.Debug($"sqlQuery={sqlQuery}");
                using (var command = new OracleCommand(sqlQuery, conn))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        int lastStep = 0;
                        ReportTransform reportTransform = null;
                        while (reader.Read())
                        {
                            var currentStep = reader.GetInt32(0);
                            if (currentStep != lastStep)
                            {
                                reportTransform = new ReportTransform
                                {
                                    Step = reader.GetInt32(0),
                                    TransformType = reader.GetString(1)
                                };
                                result.Add(reportTransform);
                                lastStep = currentStep;
                                Logger.Debug($"New step found: Step={currentStep}, TransformType={reportTransform.TransformType}");
                            }
                            var reportTransformParam = new ReportTransformParam
                            {
                                Index = reader.GetInt32(2),
                                ParamVal = reader.IsDBNull(3) ? null : reader.GetString(3)
                            };
                            reportTransform.Params.Add(reportTransformParam);
                            if (reportTransformParam.ParamVal != null)
                            {
                                Logger.Debug($"New param found: Index={reportTransformParam.Index}, ParamVal={reportTransformParam.ParamVal}");
                            }
                        }
                    }
                }
            }
            return result;
        }

        private string GetLastGeneratedReportPath(string reportName)
        {
            var outputFilePath = string.Empty;
            var reportTransforms = GetReportTransformsInfo(reportName);
            reportTransforms.ForEach(x =>
            {
                if (x.TransformType.Equals(ReportTransform.TransformTypeCrystalReport, StringComparison.OrdinalIgnoreCase))
                {
                    outputFilePath = x.Params.Single(y => y.Index == 2).ParamVal;
                    Logger.Debug($"{x.TransformType}, outputFilePath={outputFilePath}");
                } else if (x.TransformType.Equals(ReportTransform.TransformTypeExternalCommand, StringComparison.OrdinalIgnoreCase))
                {
                    if (x.Params.Any(y => y.Index == 0 && y.ParamVal.Equals("Excel2Pdf", StringComparison.OrdinalIgnoreCase)))
                    {
                        outputFilePath = x.Params.Single(y => y.Index == 4).ParamVal;
                        Logger.Debug($"{x.TransformType}, Excel2Pdf, outputFilePath={outputFilePath}");
                    }
                }
            });

            if (outputFilePath.Length > 0)
            {
                outputFilePath = outputFilePath.Replace("%WORKING_DIRECTORY%",
                        _appSettings.Value.SophisWorkingDirectory, StringComparison.OrdinalIgnoreCase);
            }
            return outputFilePath;
        }

        [HttpGet]
        [Route("GetLastGeneratedReport/{reportName}")]
        public IActionResult GetLastGeneratedReport([FromHeader] string user, [FromHeader] string token, [FromRoute] string reportName)
        {
            var stopWatch = Stopwatch.StartNew();
            IActionResult result = null;
            try
            {
                Logger.Debug($"BEGIN(reportName={reportName})");

                #region Data Validation

                if (string.IsNullOrEmpty(token))
                {
                    Logger.Warn(Message.MsgMissingToken.Msg);
                    return BadRequest(Message.MsgMissingToken);
                }

                if (string.IsNullOrEmpty(user))
                {
                    Logger.Warn(Message.MsgUserNotFound.Msg);
                    return BadRequest(Message.MsgUserNotFound);
                }

                if (!_appSettings.Value.Users.Any(x => x.Name.Equals(user, StringComparison.OrdinalIgnoreCase)))
                {
                    Logger.Warn(Message.MsgUserNotFound.Msg);
                    return BadRequest(Message.MsgUserNotFound);
                }

                if (!_appSettings.Value.Users.Any(x => x.Name.Equals(
                    user, StringComparison.OrdinalIgnoreCase) && x.Token == token))
                {
                    Logger.Warn(Message.MsgWrongToken.Msg);
                    return BadRequest(Message.MsgWrongToken);
                }

                #endregion

                var outputFilePath = GetLastGeneratedReportPath(reportName);

                if (outputFilePath.Length > 0 && System.IO.File.Exists(outputFilePath))
                {
                    result = new PhysicalFileResult(outputFilePath, "application/pdf");
                }

                return result;
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                return BadRequest("Unknown error");
            }
            finally
            {
                Logger.Debug($"END(ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }
    }
}