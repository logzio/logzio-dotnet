using Logzio.Community.Core.Bootstrap;
using Logzio.Community.Core.Shipping;
using Logzio.Community.IntegrationTests.Listener;
using Logzio.Community.NLog;
using NLog;
using NLog.Config;
using NUnit.Framework;
using Shouldly;

namespace Logzio.Community.IntegrationTests.NLog
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
                Token = "iWnDeXJFJtuEPPcgWRDpkCdkBksbrUAO",
                ListenerUrl = LogzioListenerDummy.DefaultUrl
            };
            config.AddTarget("Logzio", logzioTarget);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, "Logzio");

            LogManager.Configuration = config;

            var logger = LogManager.GetCurrentClassLogger();
            logger.Info("Hello");

            new Bootstraper().Resolve<IShipper>().WaitForSendLogsTask();
            LogManager.Shutdown();

            _dummy.Requests.Count.ShouldBe(1);
            _dummy.Requests[0].ShouldContain("Hello");
        }
    }
}
