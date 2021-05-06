using System;
using System.Reflection;
using log4net;
using log4net.Repository.Hierarchy;
using Logzio.DotNet.IntegrationTests.Listener;
using Logzio.DotNet.Log4net;
using Newtonsoft.Json.Linq;
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
            _dummy.Requests[0].Body.ShouldContain("Just a random log line");
        }

        [Test]
        public void SanityCompressed()
        {
            var hierarchy = (Hierarchy)LogManager.GetRepository(Assembly.GetCallingAssembly());
            var logzioAppender = new LogzioAppender();
            logzioAppender.AddToken("123456789");
            logzioAppender.AddListenerUrl(_dummy.DefaultUrl);
            logzioAppender.AddGzip(true);
            logzioAppender.ActivateOptions();
            hierarchy.Root.AddAppender(logzioAppender);
            hierarchy.Configured = true;
            var logger = LogManager.GetLogger(typeof(Log4netSanityTests));

            logger.Info("Just a random log line");

            LogManager.Shutdown();  // Flushes and closes

            _dummy.Requests.Count.ShouldBe(1);
            _dummy.Requests[0].Body.ShouldContain("Just a random log line");
            _dummy.Requests[0].Headers["Content-Encoding"].ShouldBe("gzip");
        }
        
        [Test]
        public void SanityJson()
        {
            var hierarchy = (Hierarchy)LogManager.GetRepository(Assembly.GetCallingAssembly());
            var logzioAppender = new LogzioAppender();
            logzioAppender.AddToken("123456789");
            logzioAppender.AddListenerUrl(_dummy.DefaultUrl);
            logzioAppender.AddFormat("Json");
            logzioAppender.ActivateOptions();
            hierarchy.Root.AddAppender(logzioAppender);
            hierarchy.Configured = true;
            var logger = LogManager.GetLogger(typeof(Log4netSanityTests));
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
            var hierarchy = (Hierarchy)LogManager.GetRepository(Assembly.GetCallingAssembly());
            var logzioAppender = new LogzioAppender();
            logzioAppender.AddToken("123456789");
            logzioAppender.AddListenerUrl(_dummy.DefaultUrl);
            logzioAppender.AddFormat("Json");
            logzioAppender.ActivateOptions();
            hierarchy.Root.AddAppender(logzioAppender);
            hierarchy.Configured = true;
            var logger = LogManager.GetLogger(typeof(Log4netSanityTests));
            logger.Info("{ Invalid json }");
            LogManager.Shutdown();  // Flushes and closes
            JObject body = JObject.Parse(_dummy.Requests[0].Body);
            
            //Should leave log as a string under 'message' field
            body["message"].ShouldBe("{ Invalid json }");
        }
    }
}

