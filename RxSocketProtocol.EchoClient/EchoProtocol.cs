using System;
using System.Collections.Generic;
using System.Net.Sockets;
using JetBlack.Network.RxSocketProtocol;

namespace RxSocketProtocol.EchoClient
{
    public class EchoProtocolDecoder : ISimpleFrameDecoder
    {
        private const int HeaderSize = sizeof(int);
        public SocketFlags ReceivedFlags => SocketFlags.None;
        
        public int BufferSize => 2 << 8;
        
        public int LookupSize(object state)
        {
            if (!(state is LengthBytesState lengthState)) throw new ArgumentException("incorrect state object");
            if (lengthState.length == -1)
                return HeaderSize; //lookup length header
            else
                return HeaderSize + lengthState.length - lengthState.received;
        }

        public object InitState()
        {
            return new LengthBytesState();
        }

        public (bool, int) CheckFinished(object state, byte[] buffer, int startIdx, int received)
        {
            if (!(state is LengthBytesState lengthState)) throw new ArgumentException("incorrect state object");

            if (lengthState.length == -1)
            {
                if (received != HeaderSize) return (true, 0);//no header received, stop this frame
                //lookup length header
                lengthState.length = BitConverter.ToInt32(buffer, 0);
                lengthState.received = received;
                return (false, 0);
            }
            else
            {
                if (lengthState.length+HeaderSize < received)
                {
                    lengthState.received = received;//data not complete,continue
                    return (false, 0);
                }
                else
                {
                    lengthState.received = lengthState.length+HeaderSize;//data completed,count the leftover,should be zero
                    return (true, received - lengthState.length-HeaderSize);
                }
            }
        }

        public DropFrameStrategyEnum CheckDropFrame(object state, byte[] bufferArray, int leftoverCount)
        {
            return DropFrameStrategyEnum.DropAndClose;
        }

        public ArraySegment<byte> BuildFrame(object state,byte[] bufferArray, int startInd, int receiveLen,int leftoverCount)
        {
            if (!(state is LengthBytesState lengthState)) throw new ArgumentException("incorrect state object");
            //reset state
            lengthState.length = -1;
            lengthState.received = 0;
            //skip header
            return receiveLen <= HeaderSize ? 
                new ArraySegment<byte>(bufferArray, startInd, 0) : 
                new ArraySegment<byte>(bufferArray,startInd+HeaderSize,receiveLen-HeaderSize);
        }

        private class LengthBytesState
        {
            public int length = -1;
            public int received = 0;
        }
    }
    
    public class EchoProtocolEncoder : ISimpleFrameEncoder
    {
        public IList<ArraySegment<byte>> EncoderSendFrame(ArraySegment<byte> data)
        {
            var headerBuffer = BitConverter.GetBytes(data.Count);
            return new[]
            {
                new ArraySegment<byte>(headerBuffer, 0, headerBuffer.Length),
                new ArraySegment<byte>(data.Array, 0, data.Count)
            };
        }

        public SocketFlags SendFlags => SocketFlags.None;
    }
}