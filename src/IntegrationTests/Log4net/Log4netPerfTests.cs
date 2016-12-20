using System;
using System.Diagnostics;
using log4net;
using log4net.Repository.Hierarchy;
using Logzio.DotNet.Log4net;
using NUnit.Framework;

namespace Logzio.DotNet.IntegrationTests.Log4net
{
	[TestFixture]
	public class Log4netPerfTests
	{
		[Test]
		public void Perf()
		{
			var hierarchy = (Hierarchy)LogManager.GetRepository();
			var logzioAppender = new LogzioAppender();
			logzioAppender.AddToken("DKJiomZjbFyVvssJDmUAWeEOSNnDARWz");
			logzioAppender.AddBufferSize(100);
			logzioAppender.AddDebug(true);
			hierarchy.Root.AddAppender(logzioAppender);
			hierarchy.Configured = true;
			var logger = LogManager.GetLogger(typeof (Log4netSanityTests));

			var stopwatch = Stopwatch.StartNew();

			for (int i = 0; i < 10000; i++)
			{
				logger.Info("A Fish");
			}

			stopwatch.Stop();
			Console.WriteLine("Total time: " + stopwatch.Elapsed);
			LogManager.Shutdown();
		}
	}
}