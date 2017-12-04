﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace JetBlack.Network.Common
{
    public static partial class SocketExtensions
    {
        public static async Task<Socket> AcceptAsync(this Socket socket)
        {
            return await Task<Socket>.Factory.FromAsync(socket.BeginAccept, socket.EndAccept, null);
        }

        public static async Task ConnectAsync(this Socket socket, IPEndPoint endpoint)
        {
            await Task.Factory.FromAsync((callback, state) => socket.BeginConnect(endpoint, callback, state), ias => socket.EndConnect(ias), null);
        }

        public static async Task<int> SendAsync(this Socket socket, byte[] buffer, int offset, int size, SocketFlags flags)
        {
            return await Task<int>.Factory.FromAsync((callback, state) => socket.BeginSend(buffer, offset, size, flags, callback, state), ias => socket.EndSend(ias), null);
        }

        public static async Task<int> SendAsync(this Socket socket, IList<ArraySegment<byte>> buffers, SocketFlags flags)
        {
            return await Task<int>.Factory.FromAsync((callback, state) => socket.BeginSend(buffers, flags, callback, state), ias => socket.EndSend(ias), null);
        }

        public static async Task<int> ReceiveAsync(this Socket socket, byte[] buffer, int offset, int size, SocketFlags socketFlags)
        {
            return await Task<int>.Factory.FromAsync((callback, state) => socket.BeginReceive(buffer, offset, size, socketFlags, callback, state), ias => socket.EndReceive(ias), null);
        }

        public static async Task<int> ReceiveCompletelyAsync(this Socket socket, byte[] buffer, int size, SocketFlags socketFlags, CancellationToken token)
        {
            var received = 0;
            while (received < size)
            {
                token.ThrowIfCancellationRequested();

                var bytes = await socket.ReceiveAsync(buffer, received, size - received, socketFlags);
                if (bytes == 0)
                    return received;
                received += bytes;
            }

            return received;
        }

        public static async Task<int> SendCompletelyAsync(this Socket socket, byte[] buffer, int size, SocketFlags socketFlags, CancellationToken token)
        {
            var sent = 0;
            while (sent < size)
            {
                token.ThrowIfCancellationRequested();

                var bytes = await socket.SendAsync(buffer, sent, size - sent, socketFlags);
                if (bytes == 0)
                    break;
                sent += bytes;
            }
            return sent;
        }

        public static async Task<int> SendCompletelyAsync(this Socket socket, IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, CancellationToken token)
        {
            var size = buffers.Sum(x => x.Count);
            var sent = 0;
            while (sent < size)
            {
                token.ThrowIfCancellationRequested();

                var bytes = await socket.SendAsync(buffers, socketFlags);
                if (bytes == 0)
                    break;
                sent += bytes;
            }
            return sent;
        }
    }
}
