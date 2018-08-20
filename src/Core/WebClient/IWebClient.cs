using System;

namespace Logzio.DotNet.Core.WebClient
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