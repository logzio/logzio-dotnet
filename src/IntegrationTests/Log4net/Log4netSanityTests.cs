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
		public void Sanity()
		{
			var hierarchy = (Hierarchy)LogManager.GetRepository();
			hierarchy.Root.AddAppender(new LogzioAppender());
			hierarchy.Configured = true;
			
			var logger = LogManager.GetLogger(typeof (Log4netSanityTests));
			logger.Info("Just a random log line");
		}
	}
}