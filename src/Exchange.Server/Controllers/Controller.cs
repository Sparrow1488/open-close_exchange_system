﻿using Exchange.Server.Exceptions.NetworkExceptions;
using Exchange.Server.Primitives;
using Exchange.Server.Protocols;
using Exchange.System.Entities;
using Exchange.System.Enums;
using Exchange.System.Packages;
using ExchangeSystem.Helpers;
using ExchangeSystem.Packages;
using System;
using System.Threading.Tasks;

namespace Exchange.Server.Controllers
{
    public abstract class Controller
    {
        internal Controller() { }

        public Response Response { get; private set; }
        public RequestContext Context { get; private set; }

        public async Task ProcessRequestAsync(RequestContext context)
        {
            Context = context;
            Response = ExecuteRequestMethod<Response>();
            await SendResponseAsync();
        }

        private T ExecuteRequestMethod<T>()
            where T : Response
        {
            Response response;
            try
            {
                string requestMethodName = Context.Request.Query;
                var method = GetType().GetMethod(requestMethodName);
                response = (T)method.Invoke(this, new object[0]);
                // TODO : сделать асинхронную реализацию
            }
            catch (Exception ex)
            {
                response = HandleException(ex);
            }
            return (T)response ?? default;
        }

        private async Task SendResponseAsync()
        {
            Ex.ThrowIfTrue<ConnectionException>(() => !Context.Client.Connected, "Client was not connected!");
            var protocol = new NewDefaultProtocol(Context.Client);
            await protocol.SendResponseAsync(Response);
        }

        private Response HandleException(Exception ex)
        {
            ResponseReport report;
            if (ex?.InnerException is InvalidCastException)
            {
                report = new ResponseReport(ResponseStatus.Invalid.Message, ResponseStatus.Invalid);
            }
            else
            {
                report = new ResponseReport(ex?.InnerException?.Message, ResponseStatus.Bad);
            }
            return new Response<EmptyEntity>(report, new EmptyEntity());
        }
    }
}
