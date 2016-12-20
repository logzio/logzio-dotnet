using System;
using System.Diagnostics;
using System.Threading;
using Logzio.DotNet.NLog;
using NLog;
using NLog.Config;
using NUnit.Framework;

namespace Logzio.DotNet.IntegrationTests.NLog
{
	[TestFixture]
	public class NLogPerfTests
	{
		[Test]
		public void Perf()
		{
			var config = new LoggingConfiguration();

			var logzioTarget = new LogzioTarget
			{
				Token = "DKJiomZjbFyVvssJDmUAWeEOSNnDARWz",
				Debug = true
			};
			config.AddTarget("Logzio", logzioTarget);
			config.AddRule(LogLevel.Debug, LogLevel.Fatal, "Logzio", "*");
			LogManager.Configuration = config;

			var logger = LogManager.GetCurrentClassLogger();

			var stopwatch = Stopwatch.StartNew();

			for (int i = 0; i < 10000; i++)
			{
				logger.Info("A Bird");
			}

			stopwatch.Stop();
			Console.WriteLine("Total time: " + stopwatch.Elapsed);
			LogManager.Shutdown();
		}
	}
}