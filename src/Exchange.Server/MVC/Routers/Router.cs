﻿using Exchange.Server.MVC.Exceptions.NetworkExceptions;
using Exchange.Server.Protocols;
using Exchange.Server.Protocols.Selectors;
using Exchange.System.Requests.Packages;
using Exchange.System.Requests.Packages.Default;
using Exchange.System.Protection;
using Newtonsoft.Json;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Exchange.Server.MVC.Routers
{
    public class Router : IRouter
    {
        private TcpClient _client;
        private IProtocolSelector _selector;
        private IProtocol _selectedProtocol;
        private Package _receivedPackage;
        private EncryptType _encryptType = EncryptType.None;
        private NetworkChannel _networkChannel = new NetworkChannel();

        /// <summary>
        /// Получает запрос от подключенного пользователя, выбирая необходимый протокол и декодера
        /// </summary>
        /// <param name="client">Подключенный клиент</param>
        /// <returns>Пакет-запрос пользователя типа Package</returns>
        public async Task<IPackage> IssueRequestAsync(TcpClient client)
        {
            if (!client.Connected)
                throw new ConnectionException();
            if (client == null)
                throw new NullReferenceException($"Переданный клиент '{nameof(client)}' не может быть равен null") ;

            _client = client;
            var stream = _client.GetStream();

            string _requestInfoJson = await _networkChannel.ReadAsync(stream);
            var _requestInfo = (RequestInformator)JsonConvert.DeserializeObject(_requestInfoJson, typeof(RequestInformator), new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
            });
            _encryptType = _requestInfo.EncryptType;

            _selectedProtocol = LookForProtocol(_requestInfo.EncryptType);
            _receivedPackage = await _selectedProtocol.ReceivePackageAsync(client) as Package;
            _encryptType = _selectedProtocol.GetProtocolEncryptType();

            return _receivedPackage;
        }
        /// <summary>
        /// Используйте этот метод после метода "IssueRequest()".
        /// </summary>
        public EncryptType GetPackageEncryptType()
        {
            return _encryptType;
        }
        private IProtocol LookForProtocol(EncryptType encryptType)
        {
            _selector = new ProtocolSelector();
            return _selector.SelectProtocol(encryptType);
        }
        
    }
}
