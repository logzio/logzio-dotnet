using log4net.Appender;
using log4net.Core;
using Logzio.DotNet.Core.Shipping;

namespace Logzio.DotNet.Log4net
{
	public class LogzioAppender : AppenderSkeleton
	{
		private readonly Shipper _shipper; 
		protected override void Append(LoggingEvent loggingEvent)
		{
			_shipper.Log(new LogEvent(new
			{
				loggingEvent.TimeStamp,
				Logger = loggingEvent.LoggerName,
				loggingEvent.Domain,
				loggingEvent.Level,
				Thread = loggingEvent.ThreadName,
				Message = loggingEvent.RenderedMessage,
				Exception = loggingEvent.GetExceptionString(),
				User = loggingEvent.UserName
			}));
		}
	}
}