using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Logzio.DotNet.Core.Shipping;
using NSubstitute;
using NUnit.Framework;

namespace Logzio.DotNet.UnitTests.Shipping
{
	[TestFixture]
	public class ShipperTests
	{
		private Shipper _target;
		private IBulkSender _bulkSender;

		[SetUp]
		public void SetUp()
		{
			_target = new Shipper { BulkSender = _bulkSender = Substitute.For<IBulkSender>() };
		}

		[Test]
		public void Ship_BufferSetTo1_LogSent()
		{
			_target.Options.BufferSize = 1;
			_target.Ship(GetLoggingEventWithSomeData());

			_bulkSender.Received().SendAsync(Arg.Is<ICollection<LogzioLoggingEvent>>(x => x.Count == 1));
		}

		[Test]
		public void Ship_BufferLimitReached_OnlySetAmountSent()
		{
			_target.Options.BufferSize = 10;

			for (var i = 0; i < 12; i++)
			{
				_target.Ship(GetLoggingEventWithSomeData());
			}

			_bulkSender.Received().SendAsync(Arg.Is<ICollection<LogzioLoggingEvent>>(x => x.Count == 10));
		}

		[Test]
		public void Ship_BufferLimitReachedMultipleTimes_OnlySetAmountSentInBulks()
		{
			_target.Options.BufferSize = 10;

			for (var i = 0; i < 40; i++)
			{
				_target.Ship(GetLoggingEventWithSomeData());
			}

			var calls = _bulkSender.ReceivedCalls().ToList();
			calls.Count.ShouldBeEquivalentTo(4);
			foreach (var call in calls)
			{
				((ICollection<LogzioLoggingEvent>)call.GetArguments()[0]).Count.ShouldBeEquivalentTo(10);
			}
		}

		[Test]
		public void Ship_BufferLimitNotReached_NothingWasSent()
		{
			_target.Options.BufferSize = 2;

			_target.Ship(GetLoggingEventWithSomeData());

			_bulkSender.DidNotReceiveWithAnyArgs().SendAsync(Arg.Any<ICollection<LogzioLoggingEvent>>());
		}

		[Test]
		public void Ship_BufferTimeoutReached_LogWasSent()
		{
			_target.Options.BufferSize = 10;
			_target.Options.BufferTimeLimit = TimeSpan.FromMilliseconds(20);

			_target.Ship(GetLoggingEventWithSomeData());
			_target.Ship(GetLoggingEventWithSomeData());

			_bulkSender.DidNotReceiveWithAnyArgs().SendAsync(Arg.Any<ICollection<LogzioLoggingEvent>>());

			Thread.Sleep(TimeSpan.FromMilliseconds(30));
			_bulkSender.Received().SendAsync(Arg.Is<ICollection<LogzioLoggingEvent>>(x => x.Count == 2));
		}

		private LogzioLoggingEvent GetLoggingEventWithSomeData()
		{
			return new LogzioLoggingEvent(new Dictionary<string, string> {
				{"message", "hey" },
				{"@timestamp", "2016-01-01T01:01:01Z" },
			});
		}
	}
}