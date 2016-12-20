using System;

namespace Logzio.DotNet.Core.Shipping
{
	public class BulkSenderOptions
	{
		public string Token { get; set; } = "INVALID TOKEN";
		public bool IsSecured { get; set; } = true;
		public TimeSpan RetriesInterval { get; set; } = TimeSpan.FromSeconds(2);
		public int RetriesMaxAttempts { get; set; } = 3;
		public string Type { get; set; } = "dotnet";
	}
}