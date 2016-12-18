using System;

namespace Logzio.DotNet.Core.Shipping
{
	public class BulkSenderOptions
	{
		public string Token { get; set; } = "Guest";
		public bool IsSecured { get; set; } = false;
		public TimeSpan RetriesInterval { get; set; } = TimeSpan.FromSeconds(2);
		public int RetriesMaxAttempts { get; set; } = 3;
		public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(1);
		public string Type { get; set; } = "dotnet";
	}
}