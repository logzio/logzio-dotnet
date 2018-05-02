using System.Reflection;
using log4net;
using log4net.Repository.Hierarchy;
using Logzio.Community.Core.Bootstrap;
using Logzio.Community.Core.Shipping;
using Logzio.Community.IntegrationTests.Listener;
using Logzio.Community.Log4Net;
using NUnit.Framework;
using Shouldly;

namespace Logzio.Community.IntegrationTests.Log4net
{
    [TestFixture]
    public class Log4NetSanityTests
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
            var hierarchy = (Hierarchy)LogManager.GetRepository(Assembly.GetCallingAssembly());
            var logzioAppender = new LogzioAppender();
            logzioAppender.AddToken("iWnDeXJFJtuEPPcgWRDpkCdkBksbrUAO");
            logzioAppender.AddListenerUrl(LogzioListenerDummy.DefaultUrl);
            hierarchy.Root.AddAppender(logzioAppender);
            hierarchy.Configured = true;
            var logger = LogManager.GetLogger(typeof(Log4NetSanityTests));

            logger.Info("Just a random log line");

            new Bootstraper().Resolve<IShipper>().WaitForSendLogsTask();
            logzioAppender.Close();
            LogManager.Shutdown();

            _dummy.Requests.Count.ShouldBe(1);
            _dummy.Requests[0].ShouldContain("Just a random log line");
        }
    }
}
