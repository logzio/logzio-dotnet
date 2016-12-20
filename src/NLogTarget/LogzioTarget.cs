using System;
using System.Collections.Generic;
using System.Linq;
using Logzio.DotNet.Core.Shipping;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Logzio.DotNet.NLog
{
	public class LogzioTarget : Target
	{
		public IShipper Shipper { get; set; } = new Shipper();
		public ShipperOptions ShipperOptions => Shipper.Options;
		public BulkSenderOptions SendOptions => Shipper.SendOptions;

		[RequiredParameter]
		public string Token { get { return SendOptions.Token; } set { SendOptions.Token = value; } }

		public string Type { get { return SendOptions.Type; } set { SendOptions.Type = value; } }
		public bool IsSecured { get { return SendOptions.IsSecured; } set { SendOptions.IsSecured = value; } }
		public int BufferSize { get { return ShipperOptions.BufferSize; } set { ShipperOptions.BufferSize= value; } }
		public TimeSpan BufferTimeout { get { return ShipperOptions.BufferTimeLimit; } set { ShipperOptions.BufferTimeLimit = value; } }
		public int RetriesMaxAttempts { get { return SendOptions.RetriesMaxAttempts; } set { SendOptions.RetriesMaxAttempts= value; } }
		public TimeSpan RetriesInterval { get { return SendOptions.RetriesInterval; } set { SendOptions.RetriesInterval = value; } }

		protected override void Write(LogEventInfo logEvent)
		{
			var values = new Dictionary<string, string>
			{
				{"@timestamp", logEvent.TimeStamp.ToString("o")},
				{"logger", logEvent.LoggerName},
				{"level", logEvent.Level.Name},
				{"message", logEvent.FormattedMessage},
				{"exception", logEvent.Exception?.ToString()},
				{"sequenceId", logEvent.SequenceID.ToString() }
			};

			foreach (var pair in logEvent.Properties.Where(pair => pair.Key != null))
			{
				values[pair.Key.ToString()] = pair.Value?.ToString();
			}

			ExtendValues(logEvent, values);

			Shipper.Ship(new LogzioLoggingEvent(values));
		}

		protected virtual void ExtendValues(LogEventInfo logEvent, Dictionary<string, string> values)
		{

		}
	}
}