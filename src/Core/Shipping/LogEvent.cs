using System.Collections.Generic;

namespace Logzio.DotNet.Core.Shipping
{
	public class LogEvent
	{
		public Dictionary<string,string> LogData { get; set; }

		public LogEvent(Dictionary<string,string> logData)
		{
			LogData = logData;
		}
	}
}