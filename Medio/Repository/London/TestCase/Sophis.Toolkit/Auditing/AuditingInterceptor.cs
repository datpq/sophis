// -----------------------------------------------------------------------
//  <copyright file="AuditingInterceptor.cs" company="Sophis Tech (Toolkit)">
//  Copyright (c) Sophis Tech. All rights reserved.
//  </copyright>
//  <author>Marco Cordeiro</author>
//  <created>2013/01/30</created>
// -----------------------------------------------------------------------

namespace Sophis.Toolkit.Auditing
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
#if V421x
    using Castle.Core.Interceptor;
#else
    using Castle.DynamicProxy;
#endif
    using Utils.Log;

    public class AuditingInterceptor : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            using (var log = LogHelper.GetLogger(invocation.TargetType.Name, invocation.Method.Name))
            {
                var stopWatch = new Stopwatch();
                
                var arguments = string.Empty;
                if (invocation.Arguments.Length > 0)
                {
                    var sb = new StringBuilder(invocation.Arguments[0].ToString());
                    invocation.Arguments.Aggregate((x1, x2) => sb.Append(", ").Append(x2));
                    arguments = sb.ToString();
                }
                log.WriteDebug("Calling ({0})", arguments);

                try
                {
                    stopWatch.Start();
                    invocation.Proceed();
                    stopWatch.Stop(); 
                }
                catch (Exception e)
                {
                    log.WriteError(e.Message);
                    log.WriteVerbose(e.StackTrace);
                    throw;
                }
                finally
                {
                    log.WriteDebug("Executed in {0}", stopWatch.Elapsed);
                }
            }
        }
    }
}