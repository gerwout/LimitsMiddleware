namespace LimitsMiddleware
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using LimitsMiddleware.LibOwin;

    /// <summary>
    /// Context of the request.
    /// </summary>
    public class RequestContext
    {
        private readonly IOwinRequest _request;

        internal RequestContext(IOwinRequest request)
        {
            _request = request;
        }

        /// <summary>
        /// The Http Method
        /// </summary>
        public string Method
        {
            get { return _request.Method; }
        }

        /// <summary>
        /// The <see cref="Uri"/> of the request.
        /// </summary>
        public Uri Uri
        {
            get { return _request.Uri; }
        }

        /// <summary>
        /// The request headers.
        /// </summary>
        public IDictionary<string, string[]> Headers
        {
            get { return _request.Headers; }
        }

        /// <summary>
        /// The Host header. May include the port.
        /// </summary>
        public string Host
        {
            get { return _request.Host.ToString(); }
        }

        /// <summary>
        /// The local IP Address the request was received on.
        /// </summary>
        public string LocalIpAddress
        {
            get { return _request.LocalIpAddress; }
        }

        /// <summary>
        /// The port the request was received on.
        /// </summary>
        public int? LocalPort
        {
            get { return _request.LocalPort; }
        }

        /// <summary>
        /// The IP Address of the remote client.
        /// </summary>
        public string RemoteIpAddress
        {
            get { return _request.RemoteIpAddress; }
        }

        /// <summary>
        /// The port of the remote client.
        /// </summary>
        public int? RemotePort
        {
            get { return _request.RemotePort; }
        }

        /// <summary>
        /// The owin.RequestUser (or gets server.User for non-standard implementations).
        /// </summary>
        public ClaimsPrincipal User
        {
            get { return _request.User; }
        }
    }
}