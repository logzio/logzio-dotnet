using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Logzio.DotNet.Core.Bootstrap;
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
	    private readonly IShipper _shipper;
	    private readonly IInternalLogger _internalLogger;

	    private readonly ShipperOptions _shipperOptions = new ShipperOptions { BulkSenderOptions = { Type = "nlog" }};

	    [RequiredParameter]
		public string Token { get { return _shipperOptions.BulkSenderOptions.Token; } set { _shipperOptions.BulkSenderOptions.Token = value; } }

		public string LogzioType { get { return _shipperOptions.BulkSenderOptions.Type; } set { _shipperOptions.BulkSenderOptions.Type = value; } }
		public string ListenerUrl { get { return _shipperOptions.BulkSenderOptions.ListenerUrl; } set { _shipperOptions.BulkSenderOptions.ListenerUrl = value?.TrimEnd('/'); } }
		public int BufferSize { get { return _shipperOptions.BufferSize; } set { _shipperOptions.BufferSize = value; } }
		public TimeSpan BufferTimeout { get { return _shipperOptions.BufferTimeLimit; } set { _shipperOptions.BufferTimeLimit = value; } }
		public int RetriesMaxAttempts { get { return _shipperOptions.BulkSenderOptions.RetriesMaxAttempts; } set { _shipperOptions.BulkSenderOptions.RetriesMaxAttempts = value; } }
		public TimeSpan RetriesInterval { get { return _shipperOptions.BulkSenderOptions.RetriesInterval; } set { _shipperOptions.BulkSenderOptions.RetriesInterval = value; } }
		public bool Debug { get { return _shipperOptions.BulkSenderOptions.Debug; } set { _shipperOptions.BulkSenderOptions.Debug = _shipperOptions.Debug = value; } }

		public LogzioTarget()
		{
		    var bootstraper = new Bootstraper();
		    bootstraper.Bootstrap();
		    _shipper = bootstraper.Resolve<IShipper>();
		    _internalLogger = bootstraper.Resolve<IInternalLogger>();
		}

	    public LogzioTarget(IShipper shipper, IInternalLogger internalLogger)
	    {
	        _shipper = shipper;
	        _internalLogger = internalLogger;
	    }

	    protected override void Write(LogEventInfo logEvent)
		{
			Task.Run(() => { WriteImpl(logEvent); });
		}

		private void WriteImpl(LogEventInfo logEvent)
		{
			try
			{
				var values = new Dictionary<string, object>
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

				_shipper.Ship(new LogzioLoggingEvent(values), _shipperOptions);
			}
			catch (Exception ex)
			{
				if (Debug)
					_internalLogger.Log("Couldn't handle log message: " + ex);
			}
		}

		protected override void CloseTarget()
		{
			base.CloseTarget();
			_shipper.Flush(_shipperOptions);
		}

		protected virtual void ExtendValues(LogEventInfo logEvent, Dictionary<string, object> values)
		{

		}
	}
}