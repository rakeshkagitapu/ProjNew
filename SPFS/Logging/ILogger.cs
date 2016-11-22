// Author               Date             Comments.  
// ----------------------------------------------------------------------
// Rakesh               10/12/2016        Logger to log information
// ----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SPFS.Logging
{
    /// <summary>
    /// Enumeration that is used to determine the level of logging and is set in configuration.
    /// </summary>
    public enum LoggingLevel
        {
            All,
            Debug,
            Info,
            Warn,
            Error,
            Fatal,
            Off
        }

    /// <summary>
    /// ILogger interface that defines the logging methods used in the application.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="loggingLevel">The logging level.</param>
        void Log(string message, LoggingLevel loggingLevel);

        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="loggingLevel">The logging level.</param>
        /// <param name="user">The user.</param>
        void Log(string message, LoggingLevel loggingLevel, string user);

        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="loggingLevel">The logging level.</param>
        /// <param name="ex">The ex.</param>
        void Log(string message, LoggingLevel loggingLevel, Exception ex);

        /// <summary>
        /// Logs the specified exception message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="loggingLevel">The logging level.</param>
        /// <param name="ex">The exception.</param>
        /// <param name="user">The user.</param>
        void Log(string message, LoggingLevel loggingLevel, Exception ex, string user);

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
        void Log(string message, LoggingLevel loggingLevel, string user, string agent, string address, string url, string area, string controller, string action);

        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="loggingLevel">The logging level.</param>
        /// <param name="ex">The ex.</param>
        /// <param name="user">The user.</param>
        /// <param name="agent">The agent.</param>
        /// <param name="address">The address.</param>
        /// <param name="url">The URL.</param>
        /// <param name="area">The area.</param>
        /// <param name="controller">The controller.</param>
        /// <param name="action">The action.</param>
        void Log(string message, LoggingLevel loggingLevel, Exception ex, string user, string agent, string address, string url, string area, string controller, string action);
    }
}
