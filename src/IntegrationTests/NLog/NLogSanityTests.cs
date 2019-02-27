﻿using Logzio.DotNet.IntegrationTests.Listener;
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
            _dummy.Requests[0].ShouldContain("Hello");
        }

        [Test]
        public void SanityCompressed()
        {
            var config = new LoggingConfiguration();

            var logzioTarget = new LogzioTarget
            {
                Token = "123456789",
                ListenerUrl = _dummy.DefaultUrl,
                UseCompression = true,
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
            _dummy.Requests[0].ShouldContain("INFO|Hello");
        }

        [Test]
        public void SanityWithContextProperty()
        {
            var config = new LoggingConfiguration();
            var logzioTarget = new LogzioTarget
            {
                Token = "DKJiomZjbFyVvssJDmUAWeEOSNnDARWz",
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
            _dummy.Requests[0].ShouldContain("threadid");
        }

        [Test]
        public void SanityWithDuplicateProperty()
        {
            var config = new LoggingConfiguration();
            var logzioTarget = new LogzioTarget
            {
                Token = "DKJiomZjbFyVvssJDmUAWeEOSNnDARWz",
                ListenerUrl = _dummy.DefaultUrl,
            };
            config.AddTarget("Logzio", logzioTarget);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, "Logzio", "*");
            LogManager.Configuration = config;

            var logger = LogManager.GetCurrentClassLogger();
            logger.Info("Received {sequenceId}", 42);

            LogManager.Shutdown();  // Flushes and closes

            _dummy.Requests.Count.ShouldBe(1);
            _dummy.Requests[0].ShouldContain("sequenceId");
            _dummy.Requests[0].ShouldContain("sequenceId_1");
        }
    }
}
