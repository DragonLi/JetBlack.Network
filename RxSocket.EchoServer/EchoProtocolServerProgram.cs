using System;
using System.Collections.Generic;

namespace RxSocket.EchoServer
{
    internal class EchoProtocolServerProgram
    {
        public static void Main(string[] args)
        {
            /*var endpoint = ProgramArgs.Parse(args, new[] { "127.0.0.1:9211" }).EndPoint;

            var cts = new CancellationTokenSource();

            endpoint.ToListenerObservable(10)
                .ObserveOn(TaskPoolScheduler.Default)
                .Subscribe(
                    client =>
                        client.ToClientObservable(1024, SocketFlags.None)
                            .Subscribe(client.ToClientObserver(1024, SocketFlags.None), cts.Token),
                    error => Console.WriteLine("Error: " + error.Message),
                    () => Console.WriteLine("OnCompleted"),
                    cts.Token);

            Console.WriteLine("Press <ENTER> to quit");
            Console.ReadLine();

            cts.Cancel();*/
        }
    }
}