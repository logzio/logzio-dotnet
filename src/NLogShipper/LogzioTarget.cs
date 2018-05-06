﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Logzio.Community.Core.Bootstrap;
using Logzio.Community.Core.InternalLogger;
using Logzio.Community.Core.Shipping;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Logzio.Community.NLog
{
	[Target("Logzio")]
	public class LogzioTarget : Target
	{
	    private readonly IShipper _shipper;
	    private readonly IInternalLogger _internalLogger;

	    private readonly ShipperOptions _shipperOptions = new ShipperOptions { BulkSenderOptions = { Type = "nlog" }};
		private Task _lastTask;

	    [RequiredParameter]
		public string Token { get => _shipperOptions.BulkSenderOptions.Token; set => _shipperOptions.BulkSenderOptions.Token = value; }

		public string LogzioType { get => _shipperOptions.BulkSenderOptions.Type; set => _shipperOptions.BulkSenderOptions.Type = value; }
		public string ListenerUrl { get => _shipperOptions.BulkSenderOptions.ListenerUrl; set => _shipperOptions.BulkSenderOptions.ListenerUrl = value?.TrimEnd('/'); }
		public int BufferSize { get => _shipperOptions.BufferSize; set => _shipperOptions.BufferSize = value; }
		public TimeSpan BufferTimeout { get => _shipperOptions.BufferTimeLimit; set => _shipperOptions.BufferTimeLimit = value; }
		public int RetriesMaxAttempts { get => _shipperOptions.BulkSenderOptions.RetriesMaxAttempts; set => _shipperOptions.BulkSenderOptions.RetriesMaxAttempts = value; }
		public TimeSpan RetriesInterval { get => _shipperOptions.BulkSenderOptions.RetriesInterval; set => _shipperOptions.BulkSenderOptions.RetriesInterval = value; }
		public bool Debug { get => _shipperOptions.BulkSenderOptions.Debug; set => _shipperOptions.BulkSenderOptions.Debug = _shipperOptions.Debug = value; }

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
			_lastTask = Task.Run(() => { WriteImpl(logEvent); });
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
					values[pair.Key.ToString()] = pair.Value;
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
			_lastTask?.Wait();
			//Shipper can be null if there was an error in the ctor, we don't
			//want to create another exception in the closing
			_shipper?.Flush(_shipperOptions);
		}

		protected virtual void ExtendValues(LogEventInfo logEvent, Dictionary<string, object> values)
		{

		}
	}
}