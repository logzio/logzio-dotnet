using System;
using System.Diagnostics;
using Logzio.DotNet.Core.Shipping;
using Logzio.DotNet.IntegrationTests.Listener;
using Logzio.DotNet.NLog;
using NLog;
using NLog.Config;
using NUnit.Framework;
using Shouldly;

namespace Logzio.DotNet.IntegrationTests.NLog
{
    [TestFixture]
    public class NLogPerfTests
    {
        private LogzioListenerDummy _dummy;

        [SetUp]
        public void Setup()
        {
            _dummy = new LogzioListenerDummy();
            _dummy.Start();
        }

        [TearDown]
        public void TearDown()
        {
            _dummy.Stop();
        }

        [Test]
        public void Perf()
        {
            var config = new LoggingConfiguration();

            var logzioTarget = new LogzioTarget
            {
                Token = "123456789",
                ListenerUrl = _dummy.DefaultUrl,
            };
            config.AddTarget("Logzio", logzioTarget);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, "Logzio");
            LogManager.Configuration = config;

            var logger = LogManager.GetCurrentClassLogger();

            logger.Info("A Fish");  // Warm the engine

            var stopwatch = Stopwatch.StartNew();

            const int logsAmount = 1000;
            for (var i = 0; i < logsAmount - 1; i++)
            {
                logger.Info("A Bird");
            }

            stopwatch.Stop();
            Console.WriteLine("Total time: " + stopwatch.Elapsed);
            stopwatch.Elapsed.ShouldBeLessThanOrEqualTo(TimeSpan.FromMilliseconds(120));

            LogManager.Shutdown();  // Flushes and closes

            _dummy.Requests.Count.ShouldBe(logsAmount / 100);
        }
    }
}