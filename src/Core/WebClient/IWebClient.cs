using System;

namespace Logzio.Community.Core.WebClient
{
	public interface IWebClient : IDisposable
	{
		string UploadString(string address, string data);
	}

	[System.ComponentModel.DesignerCategory("Code")]
	public class SystemWebClient : System.Net.WebClient, IWebClient
	{

	}

}