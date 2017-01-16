using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Logzio.DotNet.Core.InternalLogger;

namespace Logzio.DotNet.Core.Shipping
{
	public interface IShipper
	{
		void Ship(LogzioLoggingEvent logzioLoggingEvent, ShipperOptions options);
		void Flush(ShipperOptions options);
	}

	public class Shipper : IShipper
	{
	    private readonly IBulkSender _bulkSender;
	    private readonly IInternalLogger _internalLogger;

		private readonly ConcurrentQueue<LogzioLoggingEvent> _queue = new ConcurrentQueue<LogzioLoggingEvent>();
		private readonly ConcurrentDictionary<Task, byte> _tasks = new ConcurrentDictionary<Task, byte>();
		private readonly object _fullBufferlocker = new object();
		private readonly object _timedOutBufferlocker = new object();
		private Task _delayTask;

		public Shipper(IBulkSender bulkSender, IInternalLogger internalLogger)
		{
		    _bulkSender = bulkSender;
		    _internalLogger = internalLogger;
		}

		public void Ship(LogzioLoggingEvent logzioLoggingEvent, ShipperOptions options)
		{
			// ReSharper disable once InconsistentlySynchronizedField
			_queue.Enqueue(logzioLoggingEvent);
			if (options.Debug)
				_internalLogger.Log("Added log message. Queue size - [{0}]", _queue.Count);

			SendLogsIfBufferIsFull(options);
			if (_delayTask == null || _delayTask.IsCompleted)
				_delayTask = Task.Delay(options.BufferTimeLimit).ContinueWith(task => SendLogsIfBufferTimedOut(options));
		}

		public void Flush(ShipperOptions options)
		{
			if (options.Debug)
				_internalLogger.Log("Flushing remaining logz.");

			while (!_queue.IsEmpty)
			{
				var logz = new List<LogzioLoggingEvent>();

				

				for (int i = 0; i < options.BufferSize; i++)
				{
					LogzioLoggingEvent log;
					if (!_queue.TryDequeue(out log))
						break;

					logz.Add(log);
				}

				_bulkSender.Send(logz, options.BulkSenderOptions);
			}

			Task.WaitAll(_tasks.Keys.ToArray());
		}

		private void SendLogsIfBufferIsFull(ShipperOptions options)
		{
			if (_queue.Count < options.BufferSize)
				return;

			lock (_fullBufferlocker)
			{
				if (_queue.Count < options.BufferSize)
					return;

				if (options.Debug)
					_internalLogger.Log("Buffer is full. Sending logs.");
				SendLogs(options);
			}
		}

		private void SendLogsIfBufferTimedOut(ShipperOptions options)
		{
			if (_queue.IsEmpty)
				return;

			lock (_timedOutBufferlocker)
			{
				if (_queue.IsEmpty)
					return;

				if (options.Debug)
					_internalLogger.Log("Buffer is timed out. Sending logs.");
				SendLogs(options);
			}
		}

		private void SendLogs(ShipperOptions options)
		{
			var logz = new List<LogzioLoggingEvent>();
			for (var i = 0; i < options.BufferSize; i++)
			{
				LogzioLoggingEvent log;
				if (!_queue.TryDequeue(out log))
					break;

				logz.Add(log);
			}

			var task = _bulkSender.SendAsync(logz, options.BulkSenderOptions);
			_tasks[task] = 0;
			byte b;
			task.ContinueWith((x) => _tasks.TryRemove(task, out b));
		}
	}
}