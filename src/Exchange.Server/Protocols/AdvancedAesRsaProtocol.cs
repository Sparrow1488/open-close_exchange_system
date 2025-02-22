﻿using Encryptors;
using Exchange.System.Enums;
using Exchange.System.Helpers;
using Exchange.System.Packages;
using Exchange.System.Protection;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Exchange.Server.Protocols
{
    public class AdvancedAesRsaProtocol : NetworkProtocol<Request, Response>, IAsyncDisposable
    {
        /*
         * 
         * 1. Клиент подключается
            2. Клиент уведомляет сервер об используемом протоколе
	            3. Сервер отправляет клиенту публичный RSA ключ
            4. Клиент получает публичный ключ сервера
            6. Клиент шифрует AES ключ и отправляет его серверу
	            7. Сервер получает ключ и дешифрует его своим приватным RSA
            8. Клиент отправляет серверу зашифрованный в AES запрос
	            9. Сервер принимает и дешифрует ключи
            10. Клиент отправляет зашифрованный в AES запрос
	            10. Сервер отбрабатывает запрос
	            11. Сервер шифрует ответ AES ключом и отправляет
            12. Клиент получает и дешифрует ответ от сервера
         */

        public AdvancedAesRsaProtocol(TcpClient tcpClient) : base(tcpClient) { }

        public override ProtectionType Protection => ProtectionType.AesRsa;
        private readonly AesEncryptor _aesEncryptor = CreateAesEncryptor();
        private readonly RsaEncryptor _rsaEncryptor = CreateRsaEncryptor();
        private NetworkChannel _channel = new NetworkChannel();
        private AesKeysStringify _aesKeysStringify;
        private byte[] _encryptedResponseData = default;

        public override async Task<Request> AcceptRequestAsync()
        {
            await SendPublicKeyAsync();
            await GetClientAesKeysAsync();
            DecryptClientAesKeys();
            await GetEncryptedRequestAsync();
            return Request;
        }

        public override async Task SendResponseAsync(Response response)
        {
            Response = response;
            EncryptResponse();
            await SendEncryptedResponseAsync();
        }

        public ValueTask DisposeAsync() =>
            throw new NotImplementedException();

        private static AesEncryptor CreateAesEncryptor()
        {
            var aes = new AesEncryptor();
            aes.GenerateKeysBag();
            return aes;
        }

        private static RsaEncryptor CreateRsaEncryptor()
        {
            var rsa = new RsaEncryptor();
            rsa.GenerateKeysBag();
            return rsa;
        }

        private async Task SendPublicKeyAsync()
        {
            var publicKeyInBase64 = Convert.ToBase64String(_rsaEncryptor.GetPublicKey());
            var publicKeyData = _channel.Encoding.GetBytes(publicKeyInBase64);
            await _channel.WriteAsync(TcpClient.GetStream(), publicKeyData);
        }

        private async Task GetClientAesKeysAsync()
        {
            var stringyAesKeys = await _channel.ReadAsync(TcpClient.GetStream());
            _aesKeysStringify = JsonConvert.DeserializeObject<AesKeysStringify>(stringyAesKeys, JsonSettings);
        }

        private void DecryptClientAesKeys()
        {
            var encAesKeys = _aesKeysStringify.FromBase64();
            var decryptedKey = _rsaEncryptor.Decrypt(encAesKeys.Key);
            var decryptedIV = _rsaEncryptor.Decrypt(encAesKeys.IV);
            _aesEncryptor.UseKeys(new AesKeysBag(decryptedKey, decryptedIV));
        }

        private async Task GetEncryptedRequestAsync()
        {
            var encryptedRequest = (await _channel.ReadDataAsync(TcpClient.GetStream()));
            var decryptedRequest = _aesEncryptor.Decrypt(encryptedRequest);
            var jsonRequest = _channel.Encoding.GetString(decryptedRequest);
            Request = JsonConvert.DeserializeObject<Request>(jsonRequest, JsonSettings);
        }

        private void EncryptResponse()
        {
            var responseStringify = JsonConvert.SerializeObject(Response, JsonSettings);
            var encodedJsonResponse = _channel.Encoding.GetBytes(responseStringify);
            _encryptedResponseData = _aesEncryptor.Encrypt(encodedJsonResponse);
        }

        private async Task SendEncryptedResponseAsync() =>
            await _channel.WriteAsync(TcpClient.GetStream(), _encryptedResponseData);
    }
}
