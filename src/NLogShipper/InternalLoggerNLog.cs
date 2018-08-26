using System;
using Logzio.DotNet.Core.InternalLogger;
using Logzio.DotNet.Core.Shipping;
using InternalLogger = NLog.Common.InternalLogger;

namespace Logzio.DotNet.NLog
{
    internal class InternalLoggerNLog : IInternalLogger
    {
        private readonly ShipperOptions _shipperOptions;
        private readonly IInternalLogger _internalLogger;

        public InternalLoggerNLog(ShipperOptions shipperOptions, IInternalLogger internalLogger)
        {
            _shipperOptions = shipperOptions;
            _internalLogger = internalLogger;
        }

        public void Log(Exception ex, string message, params object[] args)
        {
            if (ex != null)
                InternalLogger.Warn(ex, message, args);
            else
                InternalLogger.Trace(message, args);

            if (_shipperOptions.Debug)
                _internalLogger?.Log(ex, message, args);
         }
    }
}
