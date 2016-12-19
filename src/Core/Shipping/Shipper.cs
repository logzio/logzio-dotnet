using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Logzio.DotNet.Core.Shipping
{
	public class Shipper
	{
		private readonly BulkSender _bulkSender;

		public ShipperOptions Options { get; set; }
		public BulkSenderOptions SendOptions { get; set; }

		private readonly ConcurrentQueue<LogzioLoggingEvent> _queue;
		private readonly object _fullBufferlocker;
		private readonly object _timedOutBufferlocker;
		private Task _delayTask;

		public Shipper()
		{
			Options = new ShipperOptions();
			SendOptions = new BulkSenderOptions();
			_bulkSender = new BulkSender(SendOptions);
			_queue = new ConcurrentQueue<LogzioLoggingEvent>();
			_fullBufferlocker = new object();
			_timedOutBufferlocker = new object();
		}

		public void Log(LogzioLoggingEvent logzioLoggingEvent)
		{
			_queue.Enqueue(logzioLoggingEvent);

			SendLogsIfBufferIsFull();
			if (_delayTask == null || _delayTask.IsCompleted)
				_delayTask = Task.Delay(Options.BufferTimeLimit).ContinueWith(task => SendLogsIfBufferTimedOut());
		}

		private void SendLogsIfBufferIsFull()
		{
			if (_queue.Count < Options.BufferSize)
				return;

			lock (_fullBufferlocker)
			{
				if (_queue.Count < Options.BufferSize)
					return;

				SendLogs();
			}
		}

		private void SendLogsIfBufferTimedOut()
		{
			if (_queue.IsEmpty)
				return;

			lock (_timedOutBufferlocker)
			{
				if (_queue.IsEmpty)
					return;

				SendLogs();
			}
		}

		private void SendLogs()
		{
			var logz = new List<LogzioLoggingEvent>();
			for (var i = 0; i < Options.BufferSize; i++)
			{
				LogzioLoggingEvent log;
				if (!_queue.TryDequeue(out log))
					break;

				logz.Add(log);
			}

			_bulkSender.SendAsync(logz);
		}
	}
}