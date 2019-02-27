﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Logzio.DotNet.Core.InternalLogger;

namespace Logzio.DotNet.Core.Shipping
{
    public interface IShipper
    {
        void Ship(LogzioLoggingEvent logzioLoggingEvent, ShipperOptions options);
        void Flush(ShipperOptions options);
        void WaitForSendLogsTask();
    }

    public class Shipper : IShipper
    {
        private readonly IBulkSender _bulkSender;
        private readonly IInternalLogger _internalLogger;

        private readonly ConcurrentQueue<LogzioLoggingEvent> _queue = new ConcurrentQueue<LogzioLoggingEvent>();
        private readonly object _sendLogsLocker = new object();
        private Task _delayTask;
        private Task _sendLogsTask;
        private bool _timeoutReached;

        public Shipper(IBulkSender bulkSender, IInternalLogger internalLogger)
        {
            _bulkSender = bulkSender;
            _internalLogger = internalLogger;
        }

        public void Ship(LogzioLoggingEvent logzioLoggingEvent, ShipperOptions options)
        {
            _queue.Enqueue(logzioLoggingEvent);
            if (options.Debug)
                _internalLogger.Log("Logz.io: Added log message. Queue size - [{0}]", _queue.Count);

            SendLogsIfBufferIsFull(options);
            if (_delayTask == null || _delayTask.IsCompleted)
                _delayTask = Task.Delay(options.BufferTimeLimit).ContinueWith(task => SendLogsIfBufferTimedOut(options));
        }

        public void Flush(ShipperOptions options)
        {
            if (options.Debug)
                _internalLogger.Log("Logz.io: Flushing remaining logz.");

            SendLogs(options, true);
            WaitForSendLogsTask();
        }

        private void SendLogsIfBufferIsFull(ShipperOptions options)
        {
            if (_queue.Count < options.BufferSize)
                return;

            lock (_sendLogsLocker)
            {
                if (_queue.Count < options.BufferSize)
                    return;

                if (options.Debug)
                    _internalLogger.Log("Logz.io: Buffer is full. Sending logs.");

                SendLogs(options);
            }
        }

        private void SendLogsIfBufferTimedOut(ShipperOptions options)
        {
            if (_queue.IsEmpty)
                return;

            lock (_sendLogsLocker)
            {
                if (_queue.IsEmpty)
                    return;

                if (options.Debug)
                    _internalLogger.Log("Logz.io: Buffer is timed out. Sending logs.");

                _timeoutReached = true;
                SendLogs(options);
            }
        }

        private void SendLogs(ShipperOptions options, bool flush = false)
        {
            lock (_sendLogsLocker)
            {
                if (_sendLogsTask != null && !_sendLogsTask.IsCompleted)
                    return;

                _sendLogsTask = Task.Run(async () =>
                {
                    do
                    {
                        _timeoutReached = false;
                        var logz = new List<LogzioLoggingEvent>();
                        for (var i = 0; i < options.BufferSize; i++)
                        {
                            LogzioLoggingEvent log;
                            if (!_queue.TryDequeue(out log))
                                break;

                            logz.Add(log);
                        }
                        if (options.Debug)
                            _internalLogger.Log("Logz.io: Sending [{0}] logs ([{1}] in queue)...", logz.Count, _queue.Count);

                        if (logz.Count > 0)
                        {
                            int i = 0;
                            do
                            {
                                try
                                {
                                    using (var response = await _bulkSender.SendAsync(logz, options.BulkSenderOptions).ConfigureAwait(false))
                                    {
                                        if (response.IsSuccessStatusCode)
                                            break;

                                        if (options.Debug)
                                            _internalLogger.Log("Logz.io: Failed: " + response.StatusCode);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _internalLogger.Log(ex, "Logz.io: ERROR");
                                }

                                await Task.Delay(options.BulkSenderOptions.RetriesInterval).ConfigureAwait(false);
                            } while (++i < options.BulkSenderOptions.RetriesMaxAttempts);
                        }

                        if (options.Debug)
                            _internalLogger.Log("Logz.io: Sent logs. [{0}] in queue.", _queue.Count);

                    } while (_queue.Count >= options.BufferSize || ((_timeoutReached || flush) && !_queue.IsEmpty));
                });
            }
        }

        public void WaitForSendLogsTask()
        {
            _sendLogsTask?.Wait();
        }
    }
}