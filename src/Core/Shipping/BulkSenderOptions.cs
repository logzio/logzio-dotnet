using System;

namespace Logzio.DotNet.Core.Shipping
{
    public class BulkSenderOptions
    {
        public string Token { get; set; } = "INVALID TOKEN";
        public string ListenerUrl { get; set; } = "https://listener.logz.io:8071";
        public TimeSpan RetriesInterval { get; set; } = TimeSpan.FromSeconds(2);
        public int RetriesMaxAttempts { get; set; } = 3;
        public string Type { get; set; } = "dotnet";
        public bool Debug { get; set; }
        public bool UseGzip { get; set; } = false;
        public string ProxyAddress { get; set; } = String.Empty;
        public bool EnableJson { get; set; } = false;
    }
}