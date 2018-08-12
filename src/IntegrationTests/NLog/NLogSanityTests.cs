using System;
using Logzio.DotNet.Core.Bootstrap;
using Logzio.DotNet.Core.Shipping;
using Logzio.DotNet.IntegrationTests.Listener;
using Logzio.DotNet.NLog;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NUnit.Framework;
using Shouldly;
using TargetPropertyWithContext = NLog.Targets.TargetPropertyWithContext;

namespace Logzio.DotNet.IntegrationTests.NLog
{
    [TestFixture]
    public class NLogSanityTests
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
        public void Sanity()
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
            logger.Info("Hello");

            LogManager.Shutdown();  // Flushes and closes

            _dummy.Requests.Count.ShouldBe(1);
            _dummy.Requests[0].ShouldContain("Hello");
        }

        [Test]
        public void SanityWithLayout()
        {
            var config = new LoggingConfiguration();
            var layout = Layout.FromString("${level:uppercase=true}|${message}");
            var logzioTarget = new LogzioTarget
            {
                Token = "DKJiomZjbFyVvssJDmUAWeEOSNnDARWz",
                ListenerUrl = LogzioListenerDummy.DefaultUrl,
                Layout = layout
            };
            config.AddTarget("Logzio", logzioTarget);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, "Logzio", "*");
            LogManager.Configuration = config;

            var logger = LogManager.GetCurrentClassLogger();
            logger.Info("Hello");

            LogManager.Shutdown();  // Flushes and closes

            _dummy.Requests.Count.ShouldBe(1);
            _dummy.Requests[0].ShouldContain("INFO|Hello");
        }

        [Test]
        public void SanityWithContextProperty()
        {
            var config = new LoggingConfiguration();
            var logzioTarget = new LogzioTarget
            {
                Token = "DKJiomZjbFyVvssJDmUAWeEOSNnDARWz",
                ListenerUrl = LogzioListenerDummy.DefaultUrl,
            };
            logzioTarget.ContextProperties.Add(new TargetPropertyWithContext { Name = "threadid", Layout = "${threadid}" });
            config.AddTarget("Logzio", logzioTarget);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, "Logzio", "*");
            LogManager.Configuration = config;

            var logger = LogManager.GetCurrentClassLogger();
            logger.Info("Hello");

            LogManager.Shutdown();  // Flushes and closes

            _dummy.Requests.Count.ShouldBe(1);
            _dummy.Requests[0].ShouldContain("threadid");
        }
    }
}
