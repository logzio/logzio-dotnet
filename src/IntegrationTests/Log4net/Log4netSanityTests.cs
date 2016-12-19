using System;
using System.Threading;
using log4net;
using log4net.Repository.Hierarchy;
using Logzio.DotNet.Log4net;
using NUnit.Framework;

namespace Logzio.DotNet.IntegrationTests.Log4net
{
	[TestFixture]
	public class Log4netSanityTests
	{
		[Test]
		public void  Sanity()
		{
			var hierarchy = (Hierarchy)LogManager.GetRepository();
			var logzioAppender = new LogzioAppender();
			logzioAppender.AddToken("DKJiomZjbFyVvssJDmUAWeEOSNnDARWz");
			hierarchy.Root.AddAppender(logzioAppender);
			hierarchy.Configured = true;
			var logger = LogManager.GetLogger(typeof (Log4netSanityTests));

			logger.Info("Just a random log line");
			Thread.Sleep(TimeSpan.FromDays(1));
		}
	}
}