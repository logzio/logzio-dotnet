
namespace IntegrationTests.Listener
{
    public class HttpRequest
    {
        public HttpRequest() { }
        public HttpRequest(string body, System.Collections.Specialized.NameValueCollection headers)
        {
            Body = body;
            Headers = headers;
        }

        public string Body { get; set; }
        public System.Collections.Specialized.NameValueCollection Headers { get; set; }
    }
}
