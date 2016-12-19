using System.Collections.Generic;

namespace Logzio.DotNet.Core.Shipping
{
	public class LogzioLoggingEvent
	{
		public Dictionary<string,string> LogData { get; set; }

		public LogzioLoggingEvent(Dictionary<string,string> logData)
		{
			LogData = logData;
		}
	}
}