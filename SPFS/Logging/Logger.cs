
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web;
using System.Configuration;
using log4net.Repository;
using log4net;
using log4net.Appender;
using System.IO;
using SPFS.Logging;

namespace SPFS.Logging
{
    /// <summary>
    /// To Log Exception in DB.
    /// 
    /// </summary>
    public class Logger : ILogger
    {
        private static readonly ILog _Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region ILogger Members

        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="loggingLevel">The logging level.</param>
        public void Log(string message, LoggingLevel loggingLevel)
        {
            Log(message, loggingLevel, null, null, null, null, null, null, null);
        }

        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="loggingLevel">The logging level.</param>
        /// <param name="user">The user.</param>
        public void Log(string message, LoggingLevel loggingLevel, string user)
        {
            Log(message, loggingLevel, user, null, null, null, null, null, null);
        }

        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="loggingLevel">The logging level.</param>
        /// <param name="ex">The ex.</param>
        public void Log(string message, LoggingLevel loggingLevel, Exception ex)
        {
            Log(message, loggingLevel, ex, null, null, null, null, null, null, null);
        }

        /// <summary>
        /// Logs the specified exception message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="loggingLevel">The logging level.</param>
        /// <param name="ex">The exception.</param>
        /// <param name="user">The user.</param>
        public void Log(string message, LoggingLevel loggingLevel, Exception ex, string user)
        {
            Log(message, loggingLevel, ex, user, null, null, null, null, null, null);
        }

        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="loggingLevel">The logging level.</param>
        /// <param name="user">The user.</param>
        /// <param name="agent">The agent.</param>
        /// <param name="address">The address.</param>
        /// <param name="url">The URL.</param>
        /// <param name="area">The area.</param>
        /// <param name="controller">The controller.</param>
        /// <param name="action">The action.</param>
        public void Log(string message, LoggingLevel loggingLevel, string user, string agent, string address, string url, string area, string controller, string action)
        {
            Log(message, loggingLevel, null, user, agent, address, url, area, controller, action);
        }
        public void Log(string message, LoggingLevel loggingLevel, Exception ex, string user, string agent, string address, string url, string area, string controller, string action)
        {
            log4net.Config.XmlConfigurator.Configure();
            log4net.LogicalThreadContext.Properties["user"] = user;
            log4net.LogicalThreadContext.Properties["agent"] = agent;
            log4net.LogicalThreadContext.Properties["address"] = address;
            log4net.LogicalThreadContext.Properties["url"] = url;
            log4net.LogicalThreadContext.Properties["area"] = area;
            log4net.LogicalThreadContext.Properties["controller"] = controller;
            log4net.LogicalThreadContext.Properties["action"] = action;

            if (message.Length > 4000) message = message.Substring(0, 4000);

            switch (loggingLevel)
            {
                case LoggingLevel.All:
                    if (_Logger.Logger.IsEnabledFor(log4net.Core.Level.All)) _Logger.Debug(message);
                    break;
                case LoggingLevel.Debug:
                    _Logger.Debug(message);
                    break;
                case LoggingLevel.Info:
                    _Logger.Info(message);
                    break;
                case LoggingLevel.Warn:
                    _Logger.Warn(message);
                    break;
                case LoggingLevel.Error:
                    _Logger.Error(message, ex);
                    break;
                case LoggingLevel.Fatal:
                    _Logger.Fatal(message);
                    break;
                case LoggingLevel.Off:
                    break;
                default:
                    break;
            }

            FlushBuffers();

        }

        #endregion

        private void FlushBuffers()
        {
            ILoggerRepository rep = LogManager.GetRepository();
            foreach (IAppender appender in rep.GetAppenders())
            {
                var buffered = appender as BufferingAppenderSkeleton;
                if (buffered != null)
                {
                    buffered.Flush();
                }
            }
        }
    }
}


