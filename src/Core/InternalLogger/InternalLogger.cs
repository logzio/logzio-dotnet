using System;
using System.Diagnostics;
using System.IO;

namespace Logzio.DotNet.Core.InternalLogger
{
    public interface IInternalLogger
    {
        void Log(Exception ex, string message, params object[] args);
    }

    public class InternalLogger : IInternalLogger
    {
        private readonly object _writerLocker = new object();
        private readonly string _logFile;

        public InternalLogger(string logFile)
        {
            _logFile = String.IsNullOrEmpty(logFile) ? Path.Combine(Directory.GetCurrentDirectory(), "debug.txt") : logFile;
        }
        
        public void Log(Exception ex, string message, params object[] args)
        {
            var formattedMessage = string.Concat(DateTime.Now.ToString("HH:mm:ss.fff"), " - ", args?.Length > 0 ? string.Format(message, args) : message);
            if (ex != null)
                formattedMessage += " - " + ex.ToString();

            if (ex != null)
            {
#if !NETSTANDARD1_3
                Trace.TraceError(formattedMessage);
#endif
                Console.Error.WriteLine(formattedMessage);
            }
            else
            {
#if !NETSTANDARD1_3
                Trace.WriteLine(formattedMessage);
#endif
                Console.WriteLine(formattedMessage);
            }

            lock (_writerLocker)
            {
                using (StreamWriter writer = File.AppendText(_logFile))
                {
                    writer.WriteLine(formattedMessage);   
                }
            }
        }
    }

    internal static class InternalLoggerExtensions
    {
        public static void Log(this IInternalLogger logger, string message)
        {
#if !NET45
            logger.Log(null, message, Array.Empty<object>());
#else
            logger.Log(null, message);
#endif
        }

        public static void Log(this IInternalLogger logger, string message, params object[] args)
        {
            logger.Log(null, message, args);
        }
    }
}