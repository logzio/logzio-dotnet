using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Logzio.DotNet.Core.Shipping
{
	public class Shipper
	{
		private readonly BulkSender _bulkSender = new BulkSender();

		private readonly ShipperOptions _options;
		private readonly ConcurrentQueue<LogEvent> _queue;
		private readonly DateTime _lastShippingTime;
		private readonly object _locker;

		public Shipper(ShipperOptions options)
		{
			_options = options;
			_queue = new ConcurrentQueue<LogEvent>();
			_lastShippingTime = DateTime.Now;
			_locker = new object();
		}

		public void Log(LogEvent logEvent)
		{
			_queue.Enqueue(logEvent);

			if (!ShouldSendLogs())
				return;

			lock (_locker)
			{
				if (!ShouldSendLogs())
					return;

				var logz = new List<LogEvent>();
				for (var i = 0; i < _options.BufferSize; i++)
				{
					LogEvent log;
					if (!_queue.TryDequeue(out log))
						break;

					logz.Add(log);
				}

				_bulkSender.SendAsync(logz);		
			}
		}

		public bool ShouldSendLogs()
		{
			return _queue.Count >= _options.BufferSize || DateTime.Now - _lastShippingTime > _options.BufferTimeLimit;
		}
	}
}