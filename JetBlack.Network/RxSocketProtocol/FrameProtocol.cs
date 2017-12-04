using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using JetBlack.Network.RxSocketProtocol;

namespace JetBlack.Network.RxSocketProtocol
{
    public interface IFrameDecoder
    {
        int BufferSize { get; }
    }
    
    public interface IFrameEncoder
    {
        
    }
}

namespace JetBlack.Network.Common
{
    public static partial class SocketExtensions
    {
        public static async Task<ValueTuple<int,int>> ReceiveCompletelyAsync(this Socket socket, byte[] buffer,int bufferStart, IFrameDecoder decoder,
            SocketFlags socketFlags, CancellationToken token)
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

    }
}