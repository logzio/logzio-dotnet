using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Logzio.DotNet.Core.InternalLogger;
using Logzio.DotNet.Core.Shipping;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Logzio.DotNet.NLog
{
	[Target("Logzio")]
	public class LogzioTarget : Target
	{
		public IShipper Shipper { get; set; } = new Shipper();
		public IInternalLogger InternalLogger { get; set; } = new InternalLogger();
		public ShipperOptions ShipperOptions => Shipper.Options;
		public BulkSenderOptions SendOptions => Shipper.SendOptions;

		[RequiredParameter]
		public string Token { get { return SendOptions.Token; } set { SendOptions.Token = value; } }

		public string Type { get { return SendOptions.Type; } set { SendOptions.Type = value; } }
		public bool IsSecured { get { return SendOptions.IsSecured; } set { SendOptions.IsSecured = value; } }
		public int BufferSize { get { return ShipperOptions.BufferSize; } set { ShipperOptions.BufferSize = value; } }
		public TimeSpan BufferTimeout { get { return ShipperOptions.BufferTimeLimit; } set { ShipperOptions.BufferTimeLimit = value; } }
		public int RetriesMaxAttempts { get { return SendOptions.RetriesMaxAttempts; } set { SendOptions.RetriesMaxAttempts = value; } }
		public TimeSpan RetriesInterval { get { return SendOptions.RetriesInterval; } set { SendOptions.RetriesInterval = value; } }
		public bool Debug { get { return SendOptions.Debug; } set { SendOptions.Debug = ShipperOptions.Debug = value; } }

		public LogzioTarget()
		{
			Type = "nlog";
		}

		protected override void Write(LogEventInfo logEvent)
		{
			Task.Run(() => { WriteImpl(logEvent); });
		}

		private void WriteImpl(LogEventInfo logEvent)
		{
			try
			{
				var values = new Dictionary<string, string>
				{
					{"@timestamp", logEvent.TimeStamp.ToString("o")},
					{"logger", logEvent.LoggerName},
					{"level", logEvent.Level.Name},
					{"message", logEvent.FormattedMessage},
					{"exception", logEvent.Exception?.ToString()},
					{"sequenceId", logEvent.SequenceID.ToString()}
				};

				foreach (var pair in logEvent.Properties.Where(pair => pair.Key != null))
				{
					values[pair.Key.ToString()] = pair.Value?.ToString();
				}
		
				foreach (var pair in LogManager.Configuration.Variables.Where(pair => pair.Key != null))
				{
					values[pair.Key] = pair.Value?.OriginalText;
				}

				ExtendValues(logEvent, values);

				Shipper.Ship(new LogzioLoggingEvent(values));
			}
			catch (Exception ex)
			{
				if (Debug)
					InternalLogger.Log("Couldn't handle log message: " + ex);
			}
		}

		protected override void CloseTarget()
		{
			base.CloseTarget();
			Shipper.Flush();
		}

		protected virtual void ExtendValues(LogEventInfo logEvent, Dictionary<string, string> values)
		{

		}
	}
}