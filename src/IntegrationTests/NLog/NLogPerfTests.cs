using System;
using System.Diagnostics;
using System.Threading;
using FluentAssertions;
using Logzio.DotNet.IntegrationTests.Listener;
using Logzio.DotNet.NLog;
using NLog;
using NLog.Config;
using NUnit.Framework;

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
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, "Logzio", "*");
            LogManager.Configuration = config;

            var logger = LogManager.GetCurrentClassLogger();

            var stopwatch = Stopwatch.StartNew();

            var logsAmount = 1000;
            for (var i = 0; i < logsAmount; i++)
            {
                logger.Info("A Bird");
            }

            stopwatch.Stop();
            Console.WriteLine("Total time: " + stopwatch.Elapsed);

            stopwatch.Elapsed.Should().BeLessOrEqualTo(TimeSpan.FromMilliseconds(40));

            Thread.Sleep(logsAmount); //Make sure the logs are added to the queue before we flush everything

            LogManager.Shutdown();

            _dummy.Requests.Count.ShouldBeEquivalentTo(Math.Ceiling((decimal) (logsAmount / 100)));
        }
    }
}