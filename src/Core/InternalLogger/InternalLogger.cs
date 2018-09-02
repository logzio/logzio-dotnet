using System;
using System.Diagnostics;

namespace Logzio.DotNet.Core.InternalLogger
{
    public interface IInternalLogger
    {
        void Log(Exception ex, string message, params object[] args);
    }

    public class InternalLogger : IInternalLogger
    {
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