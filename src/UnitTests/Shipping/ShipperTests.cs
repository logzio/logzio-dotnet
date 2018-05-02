using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Logzio.Community.Core.InternalLogger;
using Logzio.Community.Core.Shipping;
using NSubstitute;
using NUnit.Framework;
using Shouldly;

namespace Logzio.Community.UnitTests.Shipping
{
    [TestFixture]
    public class ShipperTests
    {
        private Shipper _target;
        private IBulkSender _bulkSender;

        [SetUp]
        public void SetUp()
        {
            _bulkSender = Substitute.For<IBulkSender>();
            _target = new Shipper(_bulkSender, Substitute.For<IInternalLogger>());
        }

        [Test]
        public void Ship_BufferSetTo1_LogSent()
        {
            _target.Ship(GetLoggingEventWithSomeData(), new ShipperOptions { BufferSize = 1 });
            _target.WaitForSendLogsTask();

            _bulkSender.Received().Send(Arg.Is<ICollection<LogzioLoggingEvent>>(x => x.Count == 1), Arg.Any<BulkSenderOptions>());
        }

        [Test]
        public void Ship_BufferLimitReached_OnlySetAmountSent()
        {
            for (var i = 0; i < 12; i++)
            {
                _target.Ship(GetLoggingEventWithSomeData(), new ShipperOptions { BufferSize = 10 });
            }

            _target.WaitForSendLogsTask();

            _bulkSender.Received().Send(Arg.Is<ICollection<LogzioLoggingEvent>>(x => x.Count == 10), Arg.Any<BulkSenderOptions>());
        }

        [Test]
        public void Ship_BufferLimitReachedMultipleTimes_OnlySetAmountSentInBulks()
        {
            for (var i = 0; i < 40; i++)
            {
                _target.Ship(GetLoggingEventWithSomeData(), new ShipperOptions { BufferSize = 10 });
            }

            _target.WaitForSendLogsTask();

            var calls = _bulkSender.ReceivedCalls().ToList();
            calls.Count.ShouldBe(4);
            foreach (var call in calls)
            {
                ((ICollection<LogzioLoggingEvent>)call.GetArguments()[0]).Count.ShouldBe(10);
            }
        }

        [Test]
        public void Ship_BufferLimitNotReached_NothingWasSent()
        {
            _target.Ship(GetLoggingEventWithSomeData(), new ShipperOptions { BufferSize = 2 });
            _target.WaitForSendLogsTask();

            _bulkSender.DidNotReceiveWithAnyArgs().Send(Arg.Any<ICollection<LogzioLoggingEvent>>(), Arg.Any<BulkSenderOptions>());
        }

        [Test]
        public void Ship_BufferTimeoutReached_LogWasSent()
        {
            var options = new ShipperOptions
            {
                BufferSize = 10,
                BufferTimeLimit = TimeSpan.FromMilliseconds(20)
            };

            _target.Ship(GetLoggingEventWithSomeData(), options);
            _target.Ship(GetLoggingEventWithSomeData(), options);
            _target.WaitForSendLogsTask();

            _bulkSender.DidNotReceiveWithAnyArgs().Send(Arg.Any<ICollection<LogzioLoggingEvent>>(), Arg.Any<BulkSenderOptions>());

            Thread.Sleep(TimeSpan.FromMilliseconds(30)); //wait for the actual timeout // Measured timing based instability. Hard to reproduce.
            _bulkSender.Received().Send(Arg.Is<ICollection<LogzioLoggingEvent>>(x => x.Count == 2), Arg.Any<BulkSenderOptions>());
        }

        [Test]
        public void Flush_FlushingTwice_DoesntSendEmpty()
        {
            var options = new ShipperOptions
            {
                BufferSize = 1
            };

            const int count = 5;
            for (var i = 0; i < count; i++)
            {
                _target.Ship(GetLoggingEventWithSomeData(), options);
            }

            _target.Flush(options);
            _target.Flush(options);
            _target.WaitForSendLogsTask();

            _bulkSender.ReceivedCalls().Count().ShouldBe(count);
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
}
