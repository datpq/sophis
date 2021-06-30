using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
using sophis.utils;
using NLog;

namespace Eff
{
    namespace Utils
    {
        public static class EmcLog
        {
            private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
            private static bool IsFirstMsg = true;

            private static void Log(CSMLog.eMVerbosity verb, string message, params object[] args)
            {
                string formatedMsg = string.Format(message, args);
                StackTrace stackTrace = new StackTrace();

                var methodBase = stackTrace.GetFrame(2).GetMethod();
                var assembly = Assembly.GetCallingAssembly();
                var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                var assemblyName = assembly.GetName();
                var callSiteClass = string.Format("{0}({1}) {2}", assemblyName.Name, fileVersionInfo.FileVersion /* assemblyName.Version */, methodBase.DeclaringType);
                CSMLog.Write(callSiteClass, methodBase.Name, verb, formatedMsg);
                var loggerMsg = string.Format("{0}::{1}() : {2}", callSiteClass, methodBase.Name, formatedMsg); //same format as Sophis log
                if (IsFirstMsg)
                {
                    IsFirstMsg = false;
                    Logger.Info("NEW SOPHIS INSTANCE STARTING HERE...");
                }
                switch (verb)
                {
                    case CSMLog.eMVerbosity.M_error:
                        Logger.Error(loggerMsg);
                        break;
                    case CSMLog.eMVerbosity.M_warning:
                        Logger.Warn(loggerMsg);
                        break;
                    case CSMLog.eMVerbosity.M_info:
                        Logger.Info(loggerMsg);
                        break;
                    default:
                        Logger.Debug(loggerMsg);
                        break;
                }
            }

            public static bool IsDebugEnabled()
            {
                return CSMLog.IsLogWorthIt(CSMLog.eMVerbosity.M_debug);
            }

            public static void Error(string message, params object[] args)
            {
                Log(CSMLog.eMVerbosity.M_error, message, args);
            }

            public static void Warning(string message, params object[] args)
            {
                Log(CSMLog.eMVerbosity.M_warning, message, args);
            }

            public static void Info(string message, params object[] args)
            {
                Log(CSMLog.eMVerbosity.M_info, message, args);
            }

            public static void Verbose(string message, params object[] args)
            {
                Log(CSMLog.eMVerbosity.M_verbose, message, args);
            }

            public static void Debug(string message, params object[] args)
            {
                Log(CSMLog.eMVerbosity.M_debug, message, args);
            }
        }
    }
}
