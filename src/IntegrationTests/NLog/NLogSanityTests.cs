using Logzio.DotNet.NLog;
using NLog;
using NLog.Config;
using NUnit.Framework;

namespace Logzio.DotNet.IntegrationTests.NLog
{
	[TestFixture]
	public class NLogSanityTests
	{
		[Test]
		public void Sanity()
		{
			var config = new LoggingConfiguration();

			var logzioTarget = new LogzioTarget
			{
				Token = "DKJiomZjbFyVvssJDmUAWeEOSNnDARWz",
				Debug = true,
				BufferSize = 1,
			};
			config.AddTarget("Logzio", logzioTarget);
			config.AddRule(LogLevel.Debug, LogLevel.Fatal, "Logzio", "*");
			LogManager.Configuration = config;

			var logger = LogManager.GetCurrentClassLogger();
			logger.Info("Hello");
		}
		 
	}
}