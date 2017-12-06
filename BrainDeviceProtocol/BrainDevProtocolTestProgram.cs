using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using JetBlack.Examples.Common;
using JetBlack.Network.Common;
using JetBlack.Network.RxSocket;
using JetBlack.Network.RxSocketProtocol;

namespace BrainDeviceProtocol
{
    internal static class BrainDevProtocolTestProgram
    {
        public static void Main(string[] args)
        {
            var endpoint = ProgramArgs.Parse(args, new[] {"127.0.0.1:9211"}).EndPoint;

            var cts = new CancellationTokenSource();
            var bufferManager = BufferManager.CreateBufferManager(2 << 16, 1024);
            var encoder = new ClientFrameEncoder(0xA0, 0XC0);
            var decoder = new ClientFrameDecoder(0xA0, 0XC0);

            endpoint.ToConnectObservable()
                .ObserveOn(TaskPoolScheduler.Default)
                .Subscribe(socket =>
                    {
                        var frameClientSubject =
                            socket.ToFrameClientSubject(encoder, decoder, bufferManager, cts.Token);

                        var observerDisposable =
                            frameClientSubject
                                .ObserveOn(TaskPoolScheduler.Default)
                                .Subscribe(
                                    managedBuffer =>
                                    {
                                        var segment = managedBuffer.Value;
                                        if (!ReceivedDataProcessor.Instance.Process(segment) && segment.Array != null)
                                            Console.WriteLine(
                                                "Echo: " + Encoding.UTF8.GetString(segment.Array, segment.Offset,
                                                    segment.Count));
                                        managedBuffer.Dispose();
                                    },
                                    error =>
                                    {
                                        Console.WriteLine("Error: " + error.Message);
                                        cts.Cancel();
                                    },
                                    () =>
                                    {
                                        Console.WriteLine("OnCompleted: Frame Protocol Receiver");
                                        cts.Cancel();
                                    });

                        var cmdSender = new DevCommandSender(frameClientSubject, bufferManager);

                        Console.In.ToLineObservable("exit")
                            .Subscribe(
                                line =>
                                {
                                    if (string.IsNullOrEmpty(line)) return;
                                    if (line == "start")
                                    {
                                        cmdSender.Start();
                                        return;
                                    }
                                    if (line == "stop")
                                    {
                                        cmdSender.Stop();
                                        return;
                                    }
                                    var writeBuffer = Encoding.UTF8.GetBytes(line);
                                    frameClientSubject.OnNext(
                                        DisposableValue.Create(new ArraySegment<byte>(writeBuffer), Disposable.Empty));
                                },
                                error =>
                                {
                                    Console.WriteLine("Error: " + error.Message);
                                    cts.Cancel();
                                },
                                () =>
                                {
                                    Console.WriteLine("OnCompleted: LineReader");
                                    cts.Cancel();
                                });

                        cts.Token.WaitHandle.WaitOne();
                        observerDisposable.Dispose();
                    },
                    error =>
                    {
                        Console.WriteLine("Failed to connect: " + error.Message);
                        cts.Cancel();
                    },
                    cts.Token);

            cts.Token.WaitHandle.WaitOne();
        }
    }
}