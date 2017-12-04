using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using JetBlack.Network.RxSocketProtocol;

namespace JetBlack.Network.RxSocketProtocol
{
    public interface ISimpleFrameDecoder
    {
        int BufferSize { get; }
        int LookupSize(object state);
        SocketFlags ReceivedFlags { get; }
        (bool, int) CheckFinished(object state, byte[] buffer, int startIdx, int received);
        bool CheckDropFrame(object state, byte[] bufferArray, int leftoverCount);
        object InitState();
    }

    public interface ISimpleFrameEncoder
    {
        IList<ArraySegment<byte>> EncoderSendFrame(ArraySegment<byte> data);
        SocketFlags SendFlags { get; }
    }
}

namespace JetBlack.Network.Common
{
    public static partial class SocketExtensions
    {
        /// <summary>
        /// return received counts and leftover counts start at index of received count
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="state">decoder state</param>
        /// <param name="buffer"></param>
        /// <param name="startIdx">received data start to fill at this index</param>
        /// <param name="decoder"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<(int, int)> ReceiveCompletelyAsync(this Socket socket, object state,
            byte[] buffer, int startIdx, ISimpleFrameDecoder decoder, CancellationToken token)
        {
            var received = 0;
            var leftover = 0;
            var lookupZise = decoder.LookupSize(state);
            var stopReceive = false;
            var socketFlags = decoder.ReceivedFlags;
            while (!stopReceive)
            {
                token.ThrowIfCancellationRequested();

                var bytes = await socket.ReceiveAsync(buffer, startIdx + received, lookupZise, socketFlags);
                if (bytes == 0)
                {
                    return (received, leftover);
                }
                received += bytes;
                (stopReceive, leftover) = decoder.CheckFinished(state, buffer, startIdx, received);
            }

            return (received, leftover);
        }
    }
}