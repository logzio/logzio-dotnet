﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace Logzio.DotNet.IntegrationTests.Listener
{
    public class LogzioListenerDummy
    {
        public const string DefaultUrl = "http://127.0.0.1:7621/";

        public IList<string> Requests { get; set; }

        private bool _isActive;
        private HttpListener _httpListener;

        public void Start(string url = DefaultUrl)
        {
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add(url);
            _httpListener.Start();
            _isActive = true;
            Requests = new List<string>();
            _httpListener.BeginGetContext(OnContext, null);
        }

        private void OnContext(IAsyncResult ar)
        {
            if (!_isActive)
                return;

            var context = _httpListener.EndGetContext(ar);

            _httpListener.BeginGetContext(OnContext, null);

            var request = context.Request;
            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                Requests.Add(reader.ReadToEnd());
            }

            context.Response.StatusCode = 200;
            context.Response.Close();
        }

        public void Stop()
        {
            _isActive = false;
            _httpListener.Close();
            _httpListener.Abort();

            // TODO: remove sleep ater bug is fixed (milestone of dotnet 2.1.0)
            // Disposing Socket then rebinding fails with SocketError.AddressAlreadyInUse on Unix
            // https://github.com/dotnet/corefx/issues/25016     
            Thread.Sleep(TimeSpan.FromMilliseconds(100)); 
        }
    }
}