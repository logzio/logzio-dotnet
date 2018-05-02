using System;

namespace Logzio.Community.Core.Shipping
{
	public class BulkSenderOptions
	{
		public string Token { get; set; } = "INVALID TOKEN";
		public string ListenerUrl { get; set; } = "https://listener.logz.io:8071";
		public TimeSpan RetriesInterval { get; set; } = TimeSpan.FromSeconds(2);
		public int RetriesMaxAttempts { get; set; } = 3;
		public string Type { get; set; } = "dotnet";
		public bool Debug { get; set; }
	}
}