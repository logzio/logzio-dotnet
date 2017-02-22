using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using log4net.Appender;
using log4net.Core;
using Logzio.DotNet.Core.Bootstrap;
using Logzio.DotNet.Core.InternalLogger;
using Logzio.DotNet.Core.Shipping;

namespace Logzio.DotNet.Log4net
{
    public class LogzioAppender : AppenderSkeleton
    {
        private static readonly string ProcessId = Process.GetCurrentProcess().Id.ToString();

        private readonly IShipper _shipper;
        private readonly IInternalLogger _internalLogger;

        private readonly ShipperOptions _shipperOptions = new ShipperOptions {BulkSenderOptions = {Type = "log4net"}};
        private readonly List<LogzioAppenderCustomField> _customFields = new List<LogzioAppenderCustomField>();
	    private Task _lastTask;

	    public LogzioAppender()
        {
            var bootstraper = new Bootstraper();
            bootstraper.Bootstrap();
            _shipper = bootstraper.Resolve<IShipper>();
            _internalLogger = bootstraper.Resolve<IInternalLogger>();
        }

        public LogzioAppender(IShipper shipper, IInternalLogger internalLogger)
        {
            _shipper = shipper;
            _internalLogger = internalLogger;
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            _lastTask = Task.Run(() => { WriteImpl(loggingEvent); });
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
                    {"exception.Log4Net", loggingEvent.GetExceptionString()},
                    {"processId", ProcessId}
                };

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

                    var key = property.Key.ToString();
                    key = key.Replace(":", "").Replace(".", "").Replace("log4net", "");
                    values[key] = value;
                }

                ExtendValues(loggingEvent, values);

                _shipper.Ship(new LogzioLoggingEvent(values), _shipperOptions);
            }
            catch (Exception ex)
            {
                if (_shipperOptions.Debug)
                    _internalLogger.Log("Couldn't handle log message: " + ex);
            }
        }

        protected virtual void ExtendValues(LoggingEvent loggingEvent, Dictionary<string, object> values)
        {

        }

        protected override void OnClose()
        {
			base.OnClose();
	        _lastTask?.Wait();
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

        public void AddCustomField(LogzioAppenderCustomField customField)
        {
            _customFields.Add(customField);
        }

        public void AddDebug(bool value)
        {
            _shipperOptions.Debug = value;
            _shipperOptions.BulkSenderOptions.Debug = value;
        }
    }
}