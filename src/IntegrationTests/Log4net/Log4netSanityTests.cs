using System.Reflection;
using log4net;
using log4net.Repository.Hierarchy;
using Logzio.DotNet.Core.Shipping;
using Logzio.DotNet.IntegrationTests.Listener;
using Logzio.DotNet.Log4net;
using NUnit.Framework;
using Shouldly;

namespace Logzio.DotNet.IntegrationTests.Log4net
{
    [TestFixture]
    public class Log4netSanityTests
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
            logzioAppender.AddToken("123456789");
            logzioAppender.AddListenerUrl(_dummy.DefaultUrl);
            logzioAppender.ActivateOptions();
            hierarchy.Root.AddAppender(logzioAppender);
            hierarchy.Configured = true;
            var logger = LogManager.GetLogger(typeof(Log4netSanityTests));

            logger.Info("Just a random log line");

            LogManager.Shutdown();  // Flushes and closes

            _dummy.Requests.Count.ShouldBe(1);
            _dummy.Requests[0].ShouldContain("Just a random log line");
        }
    }
}
