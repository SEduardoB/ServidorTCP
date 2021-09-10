using System;
using System.Collections.Generic;

namespace ServidorTCP_MT
{
    class Program
    {
        static void Main(string[] args)
        {
            var allSockets = new List<ITCPConnection>();
            var server = new TCPServer("127.0.0.1", 8182);
            server.Start(socket =>
            {
                byte[] temp = new byte[0];
                socket.OnOpen = () =>
                {
                    Console.WriteLine("Open! " + socket.IpCliente + ":" + socket.PortCliente);
                    allSockets.Add(socket);
                };
                socket.OnClose = () =>
                {
                    Console.WriteLine("Close! " + socket.IpCliente + ":" + socket.PortCliente);
                    allSockets.Remove(socket);
                };
                socket.OnMessage = message =>
                {
                    Console.WriteLine("MSG! " + socket.IpCliente + ":" + socket.PortCliente);
                    Console.WriteLine(message);
                    //allSockets.ToList().ForEach(s => s.Send("Echo: " + message));
                    socket.Send("Echo: " + message);
                };
                socket.OnError = error =>
                {
                    Console.WriteLine("Error! " + socket.IpCliente + ":" + socket.PortCliente);
                    Console.WriteLine(error);
                    socket.Close();
                };
            });

            var input = Console.ReadLine();
            server.Dispose();
        }
    }
}
