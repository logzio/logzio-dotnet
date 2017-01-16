using System;
using System.Threading;
using FluentAssertions;
using log4net;
using log4net.Repository.Hierarchy;
using Logzio.DotNet.IntegrationTests.Listener;
using Logzio.DotNet.Log4net;
using NUnit.Framework;

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
        public void  Sanity()
        {
            var hierarchy = (Hierarchy)LogManager.GetRepository();
            var logzioAppender = new LogzioAppender();
            logzioAppender.AddToken("DKJiomZjbFyVvssJDmUAWeEOSNnDARWz");
            logzioAppender.AddListenerUrl(LogzioListenerDummy.DefaultUrl);
            hierarchy.Root.AddAppender(logzioAppender);
            hierarchy.Configured = true;
            var logger = LogManager.GetLogger(typeof (Log4netSanityTests));

            logger.Info("Just a random log line");
            Thread.Sleep(100);
            logzioAppender.Close();
            LogManager.Shutdown();

            _dummy.Requests.Should().HaveCount(1);
            _dummy.Requests[0].Should().Match("*Just a random log line*");
        }
    }
}