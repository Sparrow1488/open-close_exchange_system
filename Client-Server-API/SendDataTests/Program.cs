﻿using ExchangeServer.MVC.Controllers;
using ExchangeServer.MVC.Routers;
using ExchangeServer.Protocols;
using ExchangeServer.Protocols.Receivers;
using ExchangeSystem.Requests.Packages.Default;
using System;
using System.Linq;
using System.Reflection;

namespace SendDataTests
{
    class Program
    {
        static void Main(string[] args)
        {
            ClientReceiver receiver = new ClientReceiver("127.0.0.1", 80);
            receiver.StartReceive();
            var client = receiver.AcceptClient();
            Console.WriteLine("Client was connected");

            Router router = new Router();
            var requestPackage = router.IssueRequest(client) as Package;
            Console.WriteLine("Received package has '{0}' request type", requestPackage.RequestType);

            ControllerSelector controllerSelector = new ControllerSelector();
            Controller controller = controllerSelector.SelectController(requestPackage);

            Console.WriteLine(requestPackage.RequestObject);
        }
     }
}
