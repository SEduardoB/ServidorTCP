using System;
using System.Net;
using System.Net.Sockets;

namespace ServidorTCP_MT
{
    interface ITCPServer : IDisposable
    {
        void Start(Action<ITCPConnection> config);
    }

    class TCPServer : ITCPServer
    {
        private Action<ITCPConnection> _config;
        
        private readonly IPAddress IpServer;

        public ISocketWrapper ListenerSocket { get; set; }

        private readonly int Port;

        public string Location { get; private set; }
        
        public TCPServer(string ip, int puerto)
        {
            try
            {
                IpServer = IPAddress.Parse(ip);
                Location = ip;
                Port = puerto;

                var socket = new Socket(IpServer.AddressFamily, SocketType.Stream, ProtocolType.IP);
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
                socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
                ListenerSocket = new SocketWrapper(socket);
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                Console.WriteLine("Por favor verifica la dirección ip, el puerto o tus adapatadores de red.");
            }
        }

        public void Dispose()
        {
            ListenerSocket.Dispose();
        }

        private void ListenForClients()
        {
            ListenerSocket.Accept(OnClientConnect, e =>
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine("Error en la escucha de clientes.");
                ListenerSocket.Dispose();
            });
        }

        private void OnClientConnect(ISocketWrapper clientSocket)
        {
            if (clientSocket == null) return;

            Console.WriteLine(String.Format("Client connected from {0}:{1}", clientSocket.RemoteIpAddress, clientSocket.RemotePort.ToString()));
            ListenForClients();

            TCPConnection connection = null;

            connection = new TCPConnection(
                clientSocket,
                _config);

            connection.StartReceiving();
        }

        public void Start(Action<ITCPConnection> config)
        {
            var ipLocal = new IPEndPoint(IpServer, Port);
            ListenerSocket.Bind(ipLocal);
            ListenerSocket.Listen(1000);
            Console.WriteLine(string.Format("Server started at {0} (actual port {1})", Location, Port));

            ListenForClients();

            _config = config;
        }
    }
}
