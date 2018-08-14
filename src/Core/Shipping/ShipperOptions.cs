using System;

namespace Logzio.DotNet.Core.Shipping
{
    public class ShipperOptions
    {
        public ShipperOptions()
        {
            BufferSize = 100;
            BufferTimeLimit = TimeSpan.FromSeconds(5);
            BulkSenderOptions = new BulkSenderOptions();
        }

        public int BufferSize { get; set; }
        public TimeSpan BufferTimeLimit { get; set; }
        public bool Debug { get; set; }
        public BulkSenderOptions BulkSenderOptions { get; set; }
    }
}