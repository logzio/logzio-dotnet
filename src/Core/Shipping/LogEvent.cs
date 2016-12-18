namespace Logzio.DotNet.Core.Shipping
{
	public class LogEvent
	{
		public object LogData { get; set; }

		public LogEvent(object logData)
		{
			LogData = logData;
		}
	}
}