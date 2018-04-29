﻿using System.Collections.Generic;
using Logzio.DotNet.Core.Bootstrap;
using Logzio.DotNet.Core.Shipping;
using Logzio.DotNet.IntegrationTests.Listener;
using NUnit.Framework;
using Shouldly;

namespace Logzio.DotNet.IntegrationTests.Shipper
{
    [TestFixture]
    public class ShipperThreadHandlingTests
    {
        private LogzioListenerDummy _dummy;
        private IShipper _shipper;

        [SetUp]
        public void Setup()
        {
            _dummy = new LogzioListenerDummy();
            _dummy.Start();

            var bootstraper = new Bootstraper();
            bootstraper.Bootstrap();
            _shipper = bootstraper.Resolve<IShipper>();
        }

        [TearDown]
        public void TearDown()
        {
            _dummy.Stop();
        }

        [Test]
        public void Ship_SendLogsInDelay_CreatesNewThreadCorrectly()
        {
            var options = new ShipperOptions
            {
                BufferSize = 1,
                BulkSenderOptions = { ListenerUrl = LogzioListenerDummy.DefaultUrl }
            };

            _shipper.Ship(GetLogging(), options);
            _shipper.Flush(options);

            _shipper.Ship(GetLogging(), options);
            _shipper.Flush(options);

            _dummy.Requests.Count.ShouldBe(2);
        }

        private static LogzioLoggingEvent GetLogging()
        {
            return new LogzioLoggingEvent(new Dictionary<string, object>());
        }
    }
}
