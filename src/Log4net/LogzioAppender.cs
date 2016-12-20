using System;
using System.Collections.Generic;
using log4net.Appender;
using log4net.Core;
using Logzio.DotNet.Core.InternalLogger;
using Logzio.DotNet.Core.Shipping;

namespace Logzio.DotNet.Log4net
{
	public class LogzioAppender : AppenderSkeleton
	{
		public IShipper Shipper { get; set; } = new Shipper();
		public IInternalLogger InternalLogger { get; set; } = new InternalLogger();

		private readonly List<LogzioAppenderCustomField> _customFields = new List<LogzioAppenderCustomField>();

		public LogzioAppender()
		{
			Shipper.SendOptions.Type = "log4net";
		}

		protected override void Append(LoggingEvent loggingEvent)
		{
			try
			{
				var values = new Dictionary<string, string>
				{
					{"@timestamp", loggingEvent.TimeStamp.ToString("o")},
					{"logger", loggingEvent.LoggerName},
					{"domain", loggingEvent.Domain},
					{"level", loggingEvent.Level.DisplayName},
					{"thread", loggingEvent.ThreadName},
					{"message", loggingEvent.RenderedMessage},
					{"exception", loggingEvent.GetExceptionString()},
					{"user", loggingEvent.UserName}
				};

				foreach (var customField in _customFields)
				{
					values[customField.Key] = customField.Value;
				}

				ExtendValues(loggingEvent, values);

				Shipper.Ship(new LogzioLoggingEvent(values));
			}
			catch (Exception ex)
			{
				if (Shipper.Options.Debug)
					InternalLogger.Log("Couldn't handle log message: " + ex);
			}
		}

		protected virtual void ExtendValues(LoggingEvent loggingEvent, Dictionary<string, string> values)
		{
			
		}

		public void AddToken(string value)
		{
			Shipper.SendOptions.Token = value;
		}

		public void AddType(string value)
		{
			Shipper.SendOptions.Type = value;
		}

		public void AddIsSecured(bool value)
		{
			Shipper.SendOptions.IsSecured = value;
		}

		public void AddBufferSize(int bufferSize)
		{
			Shipper.Options.BufferSize = bufferSize;
		}

		public void AddBufferTimeout(TimeSpan value)
		{
			Shipper.Options.BufferTimeLimit = value;
		}

		public void AddRetriesMaxAttempts(int value)
		{
			Shipper.SendOptions.RetriesMaxAttempts = value;
		}

		public void AddRetriesInterval(TimeSpan value)
		{
			Shipper.SendOptions.RetriesInterval = value;
		}

		public void AddCustomField(LogzioAppenderCustomField customField)
		{
			_customFields.Add(customField);			
		}

		public void AddDebug(bool value)
		{
			Shipper.Options.Debug = value;
			Shipper.SendOptions.Debug = value;
		}
	}
}