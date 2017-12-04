using System;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.ServiceModel.Channels;
using System.Threading;
using JetBlack.Network.Common;

namespace JetBlack.Network.RxSocketProtocol
{
    public static class FrameProtocolExtensions
    {
        public static ISubject<DisposableValue<ArraySegment<byte>>, DisposableValue<ArraySegment<byte>>> ToFrameClientSubject(this Socket socket, SocketFlags socketFlags, BufferManager bufferManager,IFrameDecoder decoder, CancellationToken token)
        {
            return Subject.Create(socket.ToFrameClientObserver(socketFlags, token), socket.ToFrameClientObservable(socketFlags, bufferManager,decoder));
        }
        
        public static IObservable<DisposableValue<ArraySegment<byte>>> ToFrameClientObservable(this Socket socket, SocketFlags socketFlags, BufferManager bufferManager,IFrameDecoder decoder)
        {
            return Observable.Create<DisposableValue<ArraySegment<byte>>>(async (observer, token) =>
            {
                try
                {
                    var leftoverCount = 0;
                    byte[] leftoverBuf = null;
                    while (!token.IsCancellationRequested)
                    {
                        byte[] bufferArray;
                        var bufferStart = 0;
                        if (leftoverBuf != null)
                        {
                            bufferArray = leftoverBuf;
                            bufferStart = leftoverCount;
                            leftoverBuf = null;
                            leftoverCount = 0;
                        }
                        else
                        {
                            bufferArray = bufferManager.TakeBuffer(decoder.BufferSize);
                            bufferStart = 0;

                        }
                        var pair = await socket.ReceiveCompletelyAsync(bufferArray,bufferStart, decoder, socketFlags, token);
                        var receiveLen = pair.Item1;
                        leftoverCount = pair.Item2;
                        if (receiveLen == 0)//no data received, and leftoverCount should be zero
                        {
                            //keep last received data
                            leftoverBuf = bufferArray;
                            leftoverCount = bufferStart;
                            continue;
                        }
                        if (receiveLen == -1)//overflow
                        {
                            //reclaim buffer array
                            bufferManager.ReturnBuffer(bufferArray);
                            break;
                        }
                        //copy leftover
                        if (leftoverCount > 0)
                        {
                            leftoverBuf = bufferManager.TakeBuffer(decoder.BufferSize);
                            Buffer.BlockCopy(bufferArray, receiveLen, leftoverBuf, 0, leftoverCount);
                        }

                        var buffer=new ArraySegment<byte>(bufferArray,0,receiveLen);
                        observer.OnNext(
                            new DisposableValue<ArraySegment<byte>>(buffer,
                                Disposable.Create(() => bufferManager.ReturnBuffer(bufferArray))));
                    }

                    observer.OnCompleted();

                    socket.Close();
                }
                catch (Exception error)
                {
                    observer.OnError(error);
                }
            });
        }

        public static IObserver<DisposableValue<ArraySegment<byte>>> ToFrameClientObserver(this Socket socket, SocketFlags socketFlags, CancellationToken token)
        {
            return Observer.Create<DisposableValue<ArraySegment<byte>>>(async disposableBuffer =>
            {
                //var headerBuffer = BitConverter.GetBytes(disposableBuffer.Value.Count);
                await socket.SendCompletelyAsync(
                    new[]
                    {
                        new ArraySegment<byte>(headerBuffer, 0, headerBuffer.Length),
                        new ArraySegment<byte>(disposableBuffer.Value.Array, 0, disposableBuffer.Value.Count)
                    },
                    SocketFlags.None,
                    token);
            });
        }
    }
}