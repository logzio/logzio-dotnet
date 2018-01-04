using System;
using System.Diagnostics;
using Logzio.DotNet.Core.Bootstrap;
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
                Token = "DKJiomZjbFyVvssJDmUAWeEOSNnDARWz",
                ListenerUrl = LogzioListenerDummy.DefaultUrl
            };
            config.AddTarget("Logzio", logzioTarget);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, "Logzio");
            LogManager.Configuration = config;

            var logger = LogManager.GetCurrentClassLogger();

            var stopwatch = Stopwatch.StartNew();

            const int logsAmount = 1000;
            for (var i = 0; i < logsAmount; i++)
            {
                logger.Info("A Bird");
            }

            stopwatch.Stop();
            Console.WriteLine("Total time: " + stopwatch.Elapsed);

            stopwatch.Elapsed.ShouldBeLessThanOrEqualTo(TimeSpan.FromMilliseconds(100));

            new Bootstraper().Resolve<IShipper>().WaitForSendLogsTask();
            LogManager.Shutdown();

            _dummy.Requests.Count.ShouldBeSameAs(Math.Ceiling((decimal)(logsAmount / 100)));
        }
    }
}
