using System;
using Logzio.DotNet.Core.InternalLogger;
using Logzio.DotNet.Core.Shipping;

namespace Logzio.DotNet.Log4net
{
    internal class InternalLoggerLog4net : IInternalLogger
    {
        private readonly ShipperOptions _shipperOptions;
        private readonly IInternalLogger _internalLogger;

        public InternalLoggerLog4net(ShipperOptions shipperOptions, IInternalLogger internalLogger)
        {
            _shipperOptions = shipperOptions;
            _internalLogger = internalLogger;
        }

        public void Log(Exception ex, string message, params object[] args)
        {
            if (ex != null)
                log4net.Util.LogLog.Debug(typeof(LogzioAppender), args?.Length > 0 ? string.Format(message, args) : message, ex);

            if (_shipperOptions.Debug)
                _internalLogger?.Log(ex, message, args);
        }
    }
}
