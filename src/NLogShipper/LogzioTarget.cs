using System;
using System.Collections.Generic;
using Logzio.DotNet.Core.InternalLogger;
using Logzio.DotNet.Core.Shipping;
using Newtonsoft.Json.Linq;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Targets;

#if !NETSTANDARD1_3
using OpenTelemetry.Trace;
#endif

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
        public string DebugLogFile { get { return _shipperOptions.BulkSenderOptions.DebugLogFile; } set { _shipperOptions.BulkSenderOptions.DebugLogFile = _shipperOptions.DebugLogFile = value; } }
        public bool UseGzip { get { return _shipperOptions.BulkSenderOptions.UseGzip; } set { _shipperOptions.BulkSenderOptions.UseGzip = value; } }
        public string ProxyAddress { get { return _shipperOptions.BulkSenderOptions.ProxyAddress; } set { _shipperOptions.BulkSenderOptions.ProxyAddress = value; } }

        public bool ParseJsonMessage { get { return _shipperOptions.BulkSenderOptions.ParseJsonMessage; } set { _shipperOptions.BulkSenderOptions.ParseJsonMessage = value; } }
        public bool JsonKeysCamelCase { get { return _shipperOptions.BulkSenderOptions.JsonKeysCamelCase; } set { _shipperOptions.BulkSenderOptions.JsonKeysCamelCase = value; } }
        public bool AddTraceContext { get { return _shipperOptions.BulkSenderOptions.AddTraceContext; } set { _shipperOptions.BulkSenderOptions.AddTraceContext = value; } }

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
            if (_shipper == null)
            {
                if (_internalLogger == null)
                    _internalLogger = new InternalLoggerNLog(_shipperOptions, new Core.InternalLogger.InternalLogger(_shipperOptions.DebugLogFile));
                _shipper = new Shipper(new BulkSender(new Core.WebClient.HttpClientHandler(ProxyAddress), _shipperOptions.BulkSenderOptions.JsonKeysCamelCase), _internalLogger);
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
                    {"exception", logEvent.Exception?.ToString()},
                    {"message", _usingDefaultLayout ? logEvent.FormattedMessage : RenderLogEvent(Layout, logEvent)},
                    {"sequenceId", logEvent.SequenceID.ToString()}
                };
                if (ParseJsonMessage)
                {
                    try
                    {
                        foreach (var keyValuePair in JObject.Parse(values["message"].ToString()))
                        {
                            values.Add(keyValuePair.Key, keyValuePair.Value);
                        }
                        values.Remove("message");
                    }
                    catch (Exception e)
                    {
                    }
                }
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

                        if (values.ContainsKey(key))
                        {
                            string newKey = string.Concat(key, "_1");
                            int i = 1;
                            while (values.ContainsKey(newKey))
                            {
                                i++;
                                newKey = string.Concat(key, "_", i.ToString("d"));
                            }
                            key = newKey;
                        }
                        values[key] = property.Value;
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
                
#if !NETSTANDARD1_3
                if (AddTraceContext)
                {
                    var traceId = Tracer.CurrentSpan.Context.TraceId;
                    var spanId = Tracer.CurrentSpan.Context.SpanId;

                    values.Add("traceId", traceId.ToString());
                    values.Add("spanId", spanId.ToString());
                }
#endif

                ExtendValues(logEvent, values);

                _shipper.Ship(new LogzioLoggingEvent(values), _shipperOptions);
            }
            catch (Exception ex)
            {
                _internalLogger?.Log(ex, "Logz.io: Couldn't handle log message");
                throw;  // Notify the NLog engine about this error
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
                _internalLogger?.Log(ex, "Logz.io: Couldn't handle log message");
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