using System;
using System.Collections.Generic;
using System.Linq;
using Logzio.DotNet.Core.Bootstrap;
using Logzio.DotNet.Core.InternalLogger;
using Logzio.DotNet.Core.Shipping;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Targets;

namespace Logzio.DotNet.NLog
{
    [Target("Logzio")]
    public class LogzioTarget : TargetWithContext
    {
        private IShipper _shipper;
        private IInternalLogger _internalLogger;

        private readonly ShipperOptions _shipperOptions = new ShipperOptions { BulkSenderOptions = { Type = "nlog" } };

        [RequiredParameter]
        public string Token { get { return _shipperOptions.BulkSenderOptions.Token; } set { _shipperOptions.BulkSenderOptions.Token = value; } }

        public string LogzioType { get { return _shipperOptions.BulkSenderOptions.Type; } set { _shipperOptions.BulkSenderOptions.Type = value; } }
        public string ListenerUrl { get { return _shipperOptions.BulkSenderOptions.ListenerUrl; } set { _shipperOptions.BulkSenderOptions.ListenerUrl = value?.TrimEnd('/'); } }
        public int BufferSize { get { return _shipperOptions.BufferSize; } set { _shipperOptions.BufferSize = value; } }
        public TimeSpan BufferTimeout { get { return _shipperOptions.BufferTimeLimit; } set { _shipperOptions.BufferTimeLimit = value; } }
        public int RetriesMaxAttempts { get { return _shipperOptions.BulkSenderOptions.RetriesMaxAttempts; } set { _shipperOptions.BulkSenderOptions.RetriesMaxAttempts = value; } }
        public TimeSpan RetriesInterval { get { return _shipperOptions.BulkSenderOptions.RetriesInterval; } set { _shipperOptions.BulkSenderOptions.RetriesInterval = value; } }
        public bool Debug { get { return _shipperOptions.BulkSenderOptions.Debug; } set { _shipperOptions.BulkSenderOptions.Debug = _shipperOptions.Debug = value; } }

        /// <summary>
        /// Configuration of additional properties to include with each LogEvent (Ex. ${logger}, ${machinename}, ${threadid} etc.)
        /// </summary>
        public override IList<TargetPropertyWithContext> ContextProperties { get; } = new List<TargetPropertyWithContext>();

        private readonly string DefaultLayout;
        private bool _usingDefaultLayout;

        public LogzioTarget()
        {
            IncludeEventProperties = true;
            OptimizeBufferReuse = true;
            DefaultLayout = Layout?.ToString();
        }

        public LogzioTarget(IShipper shipper, IInternalLogger internalLogger)
            : this()
        {
            _shipper = shipper;
            _internalLogger = internalLogger;
        }

        protected override void InitializeTarget()
        {
            if (_shipper == null && _internalLogger == null)
            {
                try
                {
                    var bootstraper = new Bootstraper();
                    bootstraper.Bootstrap();
                    _shipper = bootstraper.Resolve<IShipper>();
                    _internalLogger = bootstraper.Resolve<IInternalLogger>();
                }
                catch (Exception ex)
                {
                    // TinyIOC does not always work, fallback to manual resolve
                    _internalLogger = new Core.InternalLogger.InternalLogger();
                    _internalLogger.Log("Couldn't resolve dependencies: " + ex);
                    _shipper = new Shipper(new BulkSender(new Core.WebClient.WebClientFactory(), _internalLogger), _internalLogger);
                }
            }
            _usingDefaultLayout = Layout?.ToString() == DefaultLayout;
            base.InitializeTarget();
        }

        protected override void Write(LogEventInfo logEvent)
        {
            try
            {
                var values = new Dictionary<string, object>
                {
                    {"@timestamp", logEvent.TimeStamp.ToString("o")},
                    {"logger", logEvent.LoggerName},
                    {"level", logEvent.Level.Name},
                    {"message", _usingDefaultLayout ? logEvent.FormattedMessage : RenderLogEvent(Layout, logEvent)},
                    {"exception", logEvent.Exception?.ToString()},
                    {"sequenceId", logEvent.SequenceID.ToString()}
                };

                if (ShouldIncludeProperties(logEvent))
                {
                    var properties = GetAllProperties(logEvent);
                    foreach (var property in properties)
                    {
                        string key = property.Key;
                        if (string.IsNullOrEmpty(key))
                            continue;

                        if (key.IndexOf('.') != -1)
                            key = key.Replace('.', '_');
                        if (key.IndexOf(':') != -1)
                            key = key.Replace(':', '_');

                        if (!values.ContainsKey(key))
                        {
                            values[key] = property.Value;
                        }
                    }
                }
                else if (ContextProperties.Count > 0)
                {
                    for (int i = 0; i < ContextProperties.Count; ++i)
                    {
                        var property = ContextProperties[i];
                        var propertyValue = RenderLogEvent(property.Layout, logEvent);
                        if (property.IncludeEmptyValue || !string.IsNullOrEmpty(propertyValue))
                        {
                            values[property.Name] = propertyValue;
                        }
                    }
                }

                ExtendValues(logEvent, values);

                _shipper.Ship(new LogzioLoggingEvent(values), _shipperOptions);
            }
            catch (Exception ex)
            {
                if (Debug)
                    _internalLogger?.Log("Couldn't handle log message: " + ex);
            }
        }

        protected override void FlushAsync(AsyncContinuation asyncContinuation)
        {
            try
            {
                _shipper?.Flush(_shipperOptions);
                asyncContinuation(null);
            }
            catch (Exception ex)
            {
                asyncContinuation(ex);
                if (Debug)
                    _internalLogger?.Log("Couldn't handle log message: " + ex);
            }
        }

        protected override void CloseTarget()
        {
            base.CloseTarget();
            //Shipper can be null if there was an error in the ctor, we don't
            //want to create another exception in the closing
            _shipper?.Flush(_shipperOptions);
        }

        protected virtual void ExtendValues(LogEventInfo logEvent, Dictionary<string, object> values)
        {

        }
    }
}