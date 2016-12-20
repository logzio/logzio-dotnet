using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Logzio.DotNet.Core.InternalLogger;

namespace Logzio.DotNet.Core.Shipping
{
	public interface IShipper
	{
		ShipperOptions Options { get; set; }
		BulkSenderOptions SendOptions { get; set; }
		void Ship(LogzioLoggingEvent logzioLoggingEvent);
	}

	public class Shipper : IShipper
	{
		public IBulkSender BulkSender { get; set; }
		public IInternalLogger InternalLogger = new InternalLogger.InternalLogger();

		public ShipperOptions Options { get; set; } = new ShipperOptions();
		public BulkSenderOptions SendOptions { get; set; } = new BulkSenderOptions();

		private readonly ConcurrentQueue<LogzioLoggingEvent> _queue = new ConcurrentQueue<LogzioLoggingEvent>();
		private readonly object _fullBufferlocker = new object();
		private readonly object _timedOutBufferlocker = new object();
		private Task _delayTask;

		public Shipper()
		{
			BulkSender = new BulkSender(SendOptions);
		}

		public void Ship(LogzioLoggingEvent logzioLoggingEvent)
		{
			// ReSharper disable once InconsistentlySynchronizedField
			_queue.Enqueue(logzioLoggingEvent);
			if (Options.Debug)
				InternalLogger.Log("Added log message. Queue size - [{0}]", _queue.Count);

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
				
				InternalLogger.Log("Buffer is full. Sending logs.");
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

				InternalLogger.Log("Buffer is timed out. Sending logs.");
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

			BulkSender.SendAsync(logz);
		}
	}
}