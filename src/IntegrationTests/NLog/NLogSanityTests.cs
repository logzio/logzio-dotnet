using System;
using System.Threading;
using FluentAssertions;
using Logzio.DotNet.Core.Bootstrap;
using Logzio.DotNet.Core.Shipping;
using Logzio.DotNet.IntegrationTests.Listener;
using Logzio.DotNet.NLog;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NUnit.Framework;

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
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, "Logzio", "*");

            LogManager.Configuration = config;

            var logger = LogManager.GetCurrentClassLogger();
            logger.Info("Hello");

            new Bootstraper().Resolve<IShipper>().WaitForSendLogsTask();
            LogManager.Shutdown();

            _dummy.Requests.Should().HaveCount(1);
            _dummy.Requests[0].Should().Match("*Hello*");
        }

        [Test]
        public void SanityWithLayout()
        {
            var config = new LoggingConfiguration();

            var layout = Layout.FromString("'${shortdate}|${level:uppercase=true}|${message}'");
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

            new Bootstraper().Resolve<IShipper>().WaitForSendLogsTask();
            LogManager.Shutdown();

            _dummy.Requests.Should().HaveCount(1);
            _dummy.Requests[0].Should().Match($"*{DateTime.Today:yyyy-MM-dd}|INFO|Hello*");
        }
    }
}
