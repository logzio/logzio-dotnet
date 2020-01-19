using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Core.HttpClient
{
    class Proxy : IWebProxy
    {
        private Uri _proxy;

        public Proxy(Uri proxy)
        {
            this._proxy = proxy;
        }

        public ICredentials Credentials { get; set; }

        public Uri GetProxy(Uri destination)
        {
            return _proxy;
        }

        public bool IsBypassed(Uri host)
        {
            return false;
        }
    }
}
