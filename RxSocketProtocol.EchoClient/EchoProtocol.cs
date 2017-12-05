using System;
using System.Collections.Generic;
using System.Net.Sockets;
using JetBlack.Network.RxSocketProtocol;

namespace RxSocketProtocol.EchoClient
{
    public class EchoProtocolDecoder : AbsSimpleDecoder
    {
        private const int HeaderSize = sizeof(int);
        public override SocketFlags ReceivedFlags => SocketFlags.None;

        public override int BufferSize => 2 << 8;

        public override int LookupSize(object state)
        {
            if (!(state is LengthBytesState lengthState)) throw new ArgumentException("incorrect state object");
            if (lengthState.Length == -1)
                return HeaderSize; //lookup length header
            else
                return HeaderSize + lengthState.Length - lengthState.Received;
        }

        public override object InitState()
        {
            return new LengthBytesState();
        }

        public override (bool, int) CheckFinished(object state, byte[] buffer, int received)
        {
            if (!(state is LengthBytesState lengthState)) throw new ArgumentException("incorrect state object");

            if (lengthState.Length == -1)
            {
                if (received != HeaderSize) return (true, 0); //no header received, stop this frame
                //lookup length header
                lengthState.Length = BitConverter.ToInt32(buffer, 0);
                lengthState.Received = received;
                return (false, 0);
            }
            else
            {
                if (lengthState.Length + HeaderSize < received)
                {
                    lengthState.Received = received; //data not complete,continue
                    return (false, 0);
                }
                else
                {
                    lengthState.Received =
                        lengthState.Length + HeaderSize; //data completed,count the leftover,should be zero
                    return (true, received - lengthState.Length - HeaderSize);
                }
            }
        }

        protected override ArraySegment<byte> BuildFrameImpl(object state, byte[] bufferArray, int receiveLen,
            int leftoverCount)
        {
            //skip header
            return receiveLen <= HeaderSize
                ? new ArraySegment<byte>(bufferArray, 0, 0)
                : new ArraySegment<byte>(bufferArray, HeaderSize, receiveLen - leftoverCount - HeaderSize);
        }

        protected override void ResetState(object state)
        {
            if (!(state is LengthBytesState lengthState)) throw new ArgumentException("incorrect state object");
            lengthState.Length = -1;
            lengthState.Received = 0;
        }

        private class LengthBytesState
        {
            public int Length = -1;
            public int Received;
        }
    }

    public class EchoProtocolEncoder : ISimpleFrameEncoder
    {
        public IList<ArraySegment<byte>> EncoderSendFrame(ArraySegment<byte> data)
        {
            if (data.Array == null) throw new ArgumentException("ArraySegment contains no data");
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