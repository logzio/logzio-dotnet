using NLog;
using NLog.Fluent;

namespace LogzioNLogSampleApplication
{
	public class Program
	{
		static void Main(string[] args)
		{
			var logger = LogManager.GetCurrentClassLogger();

			logger.Info()
				.Message("Hello to you, from NLog with properties")
				.Property("userId", "1237123")
				.Property("sunlocation", "justabovehorizon")
				.Write();
		}
	}
}
