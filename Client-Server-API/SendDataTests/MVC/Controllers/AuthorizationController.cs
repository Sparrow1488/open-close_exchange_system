﻿using ExchangeServer.Protocols.Responders;
using ExchangeSystem.Requests.Packages;
using ExchangeSystem.Requests.Packages.Default;
using ExchangeSystem.SecurityData;
using System;
using System.Net.Sockets;

namespace ExchangeServer.MVC.Controllers
{
    public class AuthorizationController : Controller
    {
        public AuthorizationController(TcpClient client) : base(client)
        {
            _client = client;
        }
        private TcpClient _client;
        public override RequestTypes RequestType => RequestTypes.Authorization;
        protected override Responder Responder { get; set; }
        protected override IResponderSelector ResponderSelector { get; set; }


        public override void ProcessRequest(IPackage package, EncryptTypes encryptType)
        {
            throw new NotImplementedException();
        }
    }
}
