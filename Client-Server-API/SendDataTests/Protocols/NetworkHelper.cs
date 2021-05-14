﻿using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeServer.Protocols
{
    public class NetworkHelper
    {
        public Encoding Encoding { get; } = Encoding.UTF8;
        public async Task<byte[]> ReadDataAsync(NetworkStream stream, int bufferSize)
        {
            byte[] receivedBuffer = new byte[bufferSize];
            do
            {
                await stream.ReadAsync(receivedBuffer, 0, receivedBuffer.Length);
            }
            while (stream.DataAvailable);
            stream.Flush();
            return receivedBuffer;
        }
        public async Task WriteDataAsync(NetworkStream stream, byte[] buffer)
        {
            stream.Flush();
            do
            {
                await stream.WriteAsync(buffer, 0, buffer.Length);
            }
            while (stream.DataAvailable);
        }
    }
}
