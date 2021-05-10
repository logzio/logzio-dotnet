using System;
using Logzio.DotNet.IntegrationTests.Listener;
using Logzio.DotNet.NLog;
using Newtonsoft.Json.Linq;
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
                Token = "123456789",
                ListenerUrl = _dummy.DefaultUrl,
            };
            config.AddTarget("Logzio", logzioTarget);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, "Logzio");
            LogManager.Configuration = config;

            var logger = LogManager.GetCurrentClassLogger();
            logger.Info("Hello");

            LogManager.Shutdown();  // Flushes and closes

            _dummy.Requests.Count.ShouldBe(1);
            _dummy.Requests[0].Body.ShouldContain("Hello");
        }

        [Test]
        public void SanityCompressed()
        {
            var config = new LoggingConfiguration();

            var logzioTarget = new LogzioTarget
            {
                Token = "123456789",
                ListenerUrl = _dummy.DefaultUrl,
                UseGzip = true,
            };
            config.AddTarget("Logzio", logzioTarget);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, "Logzio");
            LogManager.Configuration = config;

            var logger = LogManager.GetCurrentClassLogger();
            logger.Info("Hello");

            LogManager.Shutdown();  // Flushes and closes

            _dummy.Requests.Count.ShouldBe(1);
            _dummy.Requests[0].Body.ShouldContain("Hello");
            _dummy.Requests[0].Headers["Content-Encoding"].ShouldBe("gzip");
        }

        [Test]
        public void SanityWithLayout()
        {
            var config = new LoggingConfiguration();
            var layout = Layout.FromString("${level:uppercase=true}|${message}");
            var logzioTarget = new LogzioTarget
            {
                Token = "132456789",
                ListenerUrl = _dummy.DefaultUrl,
                Layout = layout
            };
            config.AddTarget("Logzio", logzioTarget);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, "Logzio", "*");
            LogManager.Configuration = config;

            var logger = LogManager.GetCurrentClassLogger();
            logger.Info("Hello");

            LogManager.Shutdown();  // Flushes and closes

            _dummy.Requests.Count.ShouldBe(1);
            _dummy.Requests[0].Body.ShouldContain("INFO|Hello");
        }

        [Test]
        public void SanityWithContextProperty()
        {
            var config = new LoggingConfiguration();
            var logzioTarget = new LogzioTarget
            {
                Token = "123456789",
                ListenerUrl = _dummy.DefaultUrl,
            };
            logzioTarget.ContextProperties.Add(new TargetPropertyWithContext { Name = "threadid", Layout = "${threadid}" });
            config.AddTarget("Logzio", logzioTarget);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, "Logzio", "*");
            LogManager.Configuration = config;

            var logger = LogManager.GetCurrentClassLogger();
            logger.Info("Hello");

            LogManager.Shutdown();  // Flushes and closes

            _dummy.Requests.Count.ShouldBe(1);
            _dummy.Requests[0].Body.ShouldContain("threadid");
        }

        [Test]
        public void SanityWithDuplicateProperty()
        {
            var config = new LoggingConfiguration();
            var logzioTarget = new LogzioTarget
            {
                Token = "123456789",
                ListenerUrl = _dummy.DefaultUrl,
            };
            config.AddTarget("Logzio", logzioTarget);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, "Logzio", "*");
            LogManager.Configuration = config;

            var logger = LogManager.GetCurrentClassLogger();
            logger.Info("Received {sequenceId}", 42);

            LogManager.Shutdown();  // Flushes and closes

            _dummy.Requests.Count.ShouldBe(1);
            _dummy.Requests[0].Body.ShouldContain("sequenceId");
            _dummy.Requests[0].Body.ShouldContain("sequenceId_1");
        }
        
        [Test]
        public void SanityJson()
        {
            var config = new LoggingConfiguration();
            var logzioTarget = new LogzioTarget
            {
                Token = "123456789",
                ListenerUrl = _dummy.DefaultUrl,
                EnableJson = true
            };
            config.AddTarget("Logzio", logzioTarget);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, "Logzio", "*");
            LogManager.Configuration = config;

            var logger = LogManager.GetCurrentClassLogger();

            logger.Info("{ \"key1\" : \"val1\", \"key2\" : { \"key3\" : \"val3\"} }");
            LogManager.Shutdown();  // Flushes and closes
            JObject body = JObject.Parse(_dummy.Requests[0].Body);
            try
            {
                //Should append key-value pairs to log and parse them as Json
                body["key1"].ShouldBe("val1");
                body["key2"]["key3"].ShouldBe("val3");
            }
            catch (NullReferenceException e)
            {
                Assert.Fail("Failed to parse log as Json.");
            }
        }

        [Test]
        public void SanityInvalidJsonAsString()
        {
            var config = new LoggingConfiguration();
            var logzioTarget = new LogzioTarget
            {
                Token = "123456789",
                ListenerUrl = _dummy.DefaultUrl,
                EnableJson = true 
            };
            config.AddTarget("Logzio", logzioTarget);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, "Logzio", "*");
            LogManager.Configuration = config;

            var logger = LogManager.GetCurrentClassLogger();

            logger.Info("{ Invalid json }");
            LogManager.Shutdown();  // Flushes and closes
            JObject body = JObject.Parse(_dummy.Requests[0].Body);
            
            //Should leave log as a string under 'message' field
            body["message"].ShouldBe("{ Invalid json }");
        }
    }
}
