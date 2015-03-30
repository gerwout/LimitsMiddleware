namespace LimitsMiddleware
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using LimitsMiddleware.LibOwin;

    public class RequestContext
    {
        private readonly IOwinRequest _request;

        internal RequestContext(IOwinRequest request)
        {
            _request = request;
        }

        public string Method
        {
            get { return _request.Method; }
        }

        public Uri Uri
        {
            get { return _request.Uri; }
        }

        public IDictionary<string, string[]> Headers
        {
            get { return _request.Headers; }
        }

        public string Host
        {
            get { return _request.Host.ToString(); }
        }

        public string LocalIpAddress
        {
            get { return _request.LocalIpAddress; }
        }

        public int? LocalPort
        {
            get { return _request.LocalPort; }
        }

        public string RemoteIpAddress
        {
            get { return _request.RemoteIpAddress; }
        }

        public int? RemotePort
        {
            get { return _request.RemotePort; }
        }

        public ClaimsPrincipal User
        {
            get { return _request.User; }
        }
    }
}