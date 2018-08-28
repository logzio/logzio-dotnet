using System;
using System.Diagnostics;
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
    public class Log4netPerfTests
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
        public void MeasurePerfForLog4net()
        {
            const int bufferSize = 100;
            var logzioAppender = SetupAppender(bufferSize);
            log4net.Util.LogLog.InternalDebugging = true;
            var logger = LogManager.GetLogger(typeof(Log4netSanityTests));

            logger.Info("A Fish");  // Warm the engine

            var stopwatch = Stopwatch.StartNew();

            const int logsAmount = 1000;
            for (var i = 0; i < logsAmount - 1; i++)
            {
                logger.Info("A Fish");
            }

            stopwatch.Stop();
            Console.WriteLine("Total time: " + stopwatch.Elapsed);
            stopwatch.Elapsed.ShouldBeLessThanOrEqualTo(TimeSpan.FromMilliseconds(120));

            LogManager.Shutdown();  // Flushes and closes

            _dummy.Requests.Count.ShouldBe(logsAmount / bufferSize);
        }

        private LogzioAppender SetupAppender(int bufferSize)
        {
            var hierarchy = (Hierarchy)LogManager.GetRepository(Assembly.GetCallingAssembly());
            var logzioAppender = new LogzioAppender();
            logzioAppender.AddToken("DKJiomZjbFyVvssJDmUAWeEOSNnDARWz");
            logzioAppender.AddListenerUrl(_dummy.DefaultUrl);
            logzioAppender.AddBufferSize(bufferSize);
            logzioAppender.ActivateOptions();
            hierarchy.Root.AddAppender(logzioAppender);
            hierarchy.Configured = true;
            return logzioAppender;
        }
    }
}
