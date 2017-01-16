using System.Collections.Generic;
using Logzio.DotNet.Core.InternalLogger;
using Logzio.DotNet.Core.Shipping;
using Logzio.DotNet.Core.WebClient;
using NSubstitute;
using NUnit.Framework;

namespace Logzio.DotNet.UnitTests.Shipping
{
	[TestFixture]
	public class BulkSenderTests
	{
		private BulkSender _target;
		private IWebClientFactory _webClientFactory;
		private IWebClient _webClient;

		[SetUp]
		public void SetUp()
		{
		    _webClientFactory = Substitute.For<IWebClientFactory>();
		    _target = new BulkSender(_webClientFactory, Substitute.For<IInternalLogger>());
			_webClient = Substitute.For<IWebClient>();
			_webClientFactory.GetWebClient().Returns(x => _webClient);
		}

		[Test]
		public void SendAsync_Logs_LogsAreSent()
		{
			_target.SendAsync(new[] {GetLoggingEventWithSomeData(), GetLoggingEventWithSomeData()}, new BulkSenderOptions()).Wait();

			_webClientFactory.Received().GetWebClient();
			_webClient.Received().UploadString(Arg.Any<string>(), Arg.Any<string>());
		}

		private LogzioLoggingEvent GetLoggingEventWithSomeData()
		{
			return new LogzioLoggingEvent(new Dictionary<string, object> {
				{"message", "hey" },
				{"@timestamp", "2016-01-01T01:01:01Z" },
			});
		}
	}
}