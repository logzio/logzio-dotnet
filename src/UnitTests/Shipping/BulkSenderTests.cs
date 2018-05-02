using System.Collections.Generic;
using Logzio.Community.Core.InternalLogger;
using Logzio.Community.Core.Shipping;
using Logzio.Community.Core.WebClient;
using NSubstitute;
using NUnit.Framework;
using Shouldly;

namespace Logzio.Community.UnitTests.Shipping
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
            _target.SendAsync(new[] { GetLoggingEventWithSomeData(), GetLoggingEventWithSomeData() }, new BulkSenderOptions()).Wait();

            _webClientFactory.Received().GetWebClient();
            _webClient.Received().UploadString(Arg.Any<string>(), Arg.Any<string>());
        }

        [Test]
        public void Send_Logs_LogsAreFormatted()
        {
            _target.Send(new[] { GetLoggingEventWithSomeData(), GetLoggingEventWithSomeData(), GetLoggingEventWithSomeData() },
                new BulkSenderOptions());

            _webClient.Received()
                .UploadString(Arg.Any<string>(), Arg.Is<string>(x => x.Contains("\"message\":\"hey\"")));
        }

        [Test]
        public void Send_LogWithNumericField_LogsAreFormatted()
        {
            var log = GetLoggingEventWithSomeData();
            log.LogData["id"] = 300;

            _target.Send(new[] { log }, new BulkSenderOptions());

            _webClient.Received()
                .UploadString(Arg.Any<string>(), Arg.Is<string>(x => x.Contains("\"id\":300")));
        }

        [Test]
        public void Send_LogWithObjectField_LogsAreFormatted()
        {
            var log = GetLoggingEventWithSomeData();
            log.LogData["dummy"] = new DummyLogObject
            {
                SomeId = 42,
                SomeString = "The Answer"
            };

            _target.Send(new[] { log }, new BulkSenderOptions());

            _webClient.Received()
                .UploadString(Arg.Any<string>(), Arg.Is<string>(x => x.Contains("\"dummy\":{\"someId\":42,\"someString\":\"The Answer\"}")));
        }

        [Test]
        public void Send_EmptyLogsList_ShouldntSendAnything()
        {
            _target.Send(new List<LogzioLoggingEvent>(), new BulkSenderOptions());

            _webClient.ReceivedCalls().ShouldBeEmpty();
        }

        private LogzioLoggingEvent GetLoggingEventWithSomeData()
        {
            return new LogzioLoggingEvent(new Dictionary<string, object>
            {
                { "message", "hey" },
                { "@timestamp", "2016-01-01T01:01:01Z" },
            });
        }
    }

    public class DummyLogObject
    {
        public int SomeId { get; set; }
        public string SomeString { get; set; }
    }
}
