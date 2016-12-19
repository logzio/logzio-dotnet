namespace Logzio.DotNet.Core.WebClient
{
	public interface IWebClientFactory
	{
		IWebClient GetWebClient();
	}

	public class WebClientFactory : IWebClientFactory
	{
		public IWebClient GetWebClient()
		{
			return new SystemWebClient();
		}
	}
}