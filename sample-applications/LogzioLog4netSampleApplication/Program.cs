using log4net;
using log4net.Config;

namespace LogzioLog4netSampleApplication
{
	public class Program
	{
		static void Main()
		{
			XmlConfigurator.Configure();

			var logger = LogManager.GetLogger("GreetingsLogger");
			logger.Info("Greetings, earthling.");
		}
	}
}
