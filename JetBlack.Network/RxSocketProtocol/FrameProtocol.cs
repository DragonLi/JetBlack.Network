using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using JetBlack.Network.RxSocketProtocol;

namespace JetBlack.Network.RxSocketProtocol
{
    //TODO multi frame protocol support
    public interface ISimpleFrameDecoder
    {
        SocketFlags ReceivedFlags { get; }

        int BufferSize { get; }

        object InitState();

        int LookupSize(object state);

        /// <summary>
        /// check whether a frame is received completely, and whether next frame's data is received
        /// </summary>
        /// <param name="state"></param>
        /// <param name="buffer"></param>
        /// <param name="startIdx"></param>
        /// <param name="received"></param>
        /// <returns>flag a frame is complete, and leftover data counts for the next frame</returns>
        (bool, int) CheckFinished(object state, byte[] buffer, int startIdx, int received);

        ArraySegment<byte> BuildFrame(object state,byte[] bufferArray, int startInd, int receiveLen,int leftoverCount);

        DropFrameStrategyEnum CheckDropFrame(object state, byte[] bufferArray, int leftoverCount);
    }

    public enum DropFrameStrategyEnum
    {
        DropAndClose,
        KeepAndContinue,
        DropAndRestart,
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
            var stopReceive = false;
            var socketFlags = decoder.ReceivedFlags;
            
            //check left over buffer
            if (startIdx > 0)
                (stopReceive, leftover) = decoder.CheckFinished(state, buffer, 0, startIdx);

            while (!stopReceive)
            {
                token.ThrowIfCancellationRequested();

                if (startIdx + received >= buffer.Length) //overflow checking
                    return (-1, 0);

                var lookupZise = decoder.LookupSize(state);
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