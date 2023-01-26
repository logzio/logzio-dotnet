using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using log4net.Appender;
using log4net.Core;
using Logzio.DotNet.Core.InternalLogger;
using Logzio.DotNet.Core.Shipping;
using Newtonsoft.Json.Linq;

#if !NETSTANDARD1_3
using OpenTelemetry.Trace;
#endif

namespace Logzio.DotNet.Log4net
{
    public class LogzioAppender : AppenderSkeleton
    {
        private static readonly object ProcessId = Process.GetCurrentProcess().Id;

        private IShipper _shipper;
        private IInternalLogger _internalLogger;

        private readonly ShipperOptions _shipperOptions = new ShipperOptions { BulkSenderOptions = { Type = "log4net" } };
        private readonly List<LogzioAppenderCustomField> _customFields = new List<LogzioAppenderCustomField>();

        public LogzioAppender()
        {
        }

        public LogzioAppender(IShipper shipper, IInternalLogger internalLogger)
            :this()
        {
            _shipper = shipper;
            _internalLogger = internalLogger;
        }

        public override void ActivateOptions()
        {
            if (_shipper == null)
            {
                if (_internalLogger == null)
                    _internalLogger = new InternalLoggerLog4net(_shipperOptions, new Core.InternalLogger.InternalLogger());
                _shipper = new Shipper(new BulkSender(new Core.WebClient.HttpClientHandler(_shipperOptions.BulkSenderOptions.ProxyAddress), _shipperOptions.BulkSenderOptions.JsonKeysCamelCase), _internalLogger);
            }

            base.ActivateOptions();
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            WriteImpl(loggingEvent);
        }

        private void WriteImpl(LoggingEvent loggingEvent)
        {
            try
            {
                var values = new Dictionary<string, object>
                {
                    {"@timestamp", loggingEvent.TimeStamp.ToString("o")},
                    {"logger", loggingEvent.LoggerName},
                    {"domain", loggingEvent.Domain},
                    {"level", loggingEvent.Level.DisplayName},
                    {"thread", loggingEvent.ThreadName},
                    {"message", loggingEvent.RenderedMessage},
                    {"exception", loggingEvent.GetExceptionString()},
                    {"processId", ProcessId}
                };
                if (_shipperOptions.BulkSenderOptions.ParseJsonMessage)
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

#if !NETSTANDARD1_3
                if (_shipperOptions.BulkSenderOptions.AddTraceContext)
                {
                    var traceId = Tracer.CurrentSpan.Context.TraceId;
                    var spanId = Tracer.CurrentSpan.Context.SpanId;

                    values.Add("traceId", traceId.ToString());
                    values.Add("spanId", spanId.ToString());
                }
#endif

                foreach (var customField in _customFields)
                {
                    values[customField.Key] = customField.Value;
                }

                var properties = loggingEvent.GetProperties();
                foreach (DictionaryEntry property in properties)
                {
                    var value = property.Value;
                    if (value == null || ReferenceEquals(value, ""))
                        continue;

                    var key = property.Key?.ToString() ?? string.Empty;
                    key = key.Replace(":", "").Replace(".", "").Replace("log4net", "");
                    if (!string.IsNullOrEmpty(key))
                        values[key] = value;
                }

                ExtendValues(loggingEvent, values);

                _shipper.Ship(new LogzioLoggingEvent(values), _shipperOptions);
            }
            catch (Exception ex)
            {
                _internalLogger?.Log(ex, "Logz.io: Couldn't handle log message");
            }
        }

        public override bool Flush(int millisecondsTimeout)
        {
            try
            {
                _shipper?.Flush(_shipperOptions);
                return true;
            }
            catch (Exception ex)
            {
                _internalLogger?.Log(ex, "Logz.io: Couldn't flush log messages");
                return false;
            }
        }

        protected virtual void ExtendValues(LoggingEvent loggingEvent, Dictionary<string, object> values)
        {

        }

        protected override void OnClose()
        {
            base.OnClose();
            //Shipper can be null if there was an error in the ctor, we don't
            //want to create another exception in the closing
            _shipper?.Flush(_shipperOptions);
        }

        public void AddToken(string value)
        {
            _shipperOptions.BulkSenderOptions.Token = value;
        }

        public void AddType(string value)
        {
            _shipperOptions.BulkSenderOptions.Type = value;
        }

        public void AddListenerUrl(string value)
        {
            _shipperOptions.BulkSenderOptions.ListenerUrl = value?.TrimEnd('/');
        }

        public void AddBufferSize(int bufferSize)
        {
            _shipperOptions.BufferSize = bufferSize;
        }

        public void ParseJsonMessage(bool value)
        {
            _shipperOptions.BulkSenderOptions.ParseJsonMessage = value;
        }
        
        public void JsonKeysCamelCase(bool value)
        {
            _shipperOptions.BulkSenderOptions.JsonKeysCamelCase = value;
        }

        public void AddTraceContext(bool value)
        {
            _shipperOptions.BulkSenderOptions.AddTraceContext = value;
        }

        public void AddBufferTimeout(TimeSpan value)
        {
            _shipperOptions.BufferTimeLimit = value;
        }

        public void AddRetriesMaxAttempts(int value)
        {
            _shipperOptions.BulkSenderOptions.RetriesMaxAttempts = value;
        }

        public void AddRetriesInterval(TimeSpan value)
        {
            _shipperOptions.BulkSenderOptions.RetriesInterval = value;
        }

        public void AddProxyAddress(string proxyAddress)
        {
            _shipperOptions.BulkSenderOptions.ProxyAddress = proxyAddress;
        }

        public void AddCustomField(LogzioAppenderCustomField customField)
        {
            _customFields.Add(customField);
        }

        public void AddDebug(bool value)
        {
            _shipperOptions.Debug = value;
            _shipperOptions.BulkSenderOptions.Debug = value;
        }

        public void AddGzip(bool value)
        {
            _shipperOptions.BulkSenderOptions.UseGzip = value;
        }
    }
}