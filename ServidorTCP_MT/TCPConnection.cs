using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ServidorTCP_MT
{
    interface ITCPConnection
    {
        Action<byte[]> OnBinary { get; set; }
        Action OnClose { get; set; }
        Action<Exception> OnError { get; set; }
        Action<string> OnMessage { get; set; }
        Action OnOpen { get; set; }
        bool IsAvailable { get; }
        int PortCliente { get; set; }
        string IpCliente { get; set; }
        Task Send(string message);
        void Close();
    }

    class TCPConnection : ITCPConnection
    {
        public readonly Action<ITCPConnection> _initialize;
        public Action<byte[]> OnBinary { get; set; }
        public Action OnClose { get; set; }
        public Action<Exception> OnError { get; set; }
        public Action<string> OnMessage { get; set; }
        public Action OnOpen { get; set; }

        private bool _closed;
        private bool _closing;

        private const int ReadSize = 1024 * 4;

        public int PortCliente { get; set; }

        public ISocketWrapper Socket { get; set; }

        public string IpCliente { get; set; }

        public TCPConnection(ISocketWrapper socket, Action<ITCPConnection> initialize)
        {
            Socket = socket;
            OnOpen = () => { };
            OnClose = () => { };
            OnMessage = x => { };
            OnBinary = x => { };
            OnError = x => { };
            _initialize = initialize;
        }

        public void Close()
        {
            CloseSocket();
        }

        private void CloseSocket()
        {
            _closing = true;
            OnClose();
            _closed = true;
            Socket.Close();
            Socket.Dispose();
            _closing = false;
        }

        private void HandleReadError(Exception e)
        {
            OnError(e);
        }

        public bool IsAvailable
        {
            get { return !_closing && !_closed && Socket.Connected; }
        }

        private void Read(List<byte> data, byte[] buffer)
        {
            if (!IsAvailable)
                return;

            Socket.Receive(buffer, r =>
            {
                if (r <= 0)
                {
                    Console.WriteLine("0 bytes read. Closing.");
                    CloseSocket();
                    return;
                }
                
                var encoding = new UTF8Encoding(false, true);
                OnMessage(Encoding.ASCII.GetString(buffer, 0, r));

                Read(data, buffer);
            },
            HandleReadError);
        }

        public Task Send(string message)
        {
            byte[] to_send = Encoding.UTF8.GetBytes(message);
            return SendBytes(to_send);
        }
        
        private Task SendBytes(byte[] bytes, Action callback = null)
        {
            return Socket.Send(bytes, () =>
            {
                //FleckLog.Debug("Sent " + bytes.Length + " bytes");
                if (callback != null)
                    callback();
            },
                              e =>
                              {
                                  if (e is IOException)
                                      Console.WriteLine("Failed to send. Disconnecting.", e);
                                  else
                                      Console.WriteLine("Failed to send. Disconnecting.", e);
                                  CloseSocket();
                              });
        }

        public void StartReceiving()
        {
            IpCliente = Socket.RemoteIpAddress;
            PortCliente = Socket.RemotePort;
            _initialize(this);
            OnOpen();

            var data = new List<byte>(ReadSize);
            var buffer = new byte[ReadSize];
            Read(data, buffer);
        }
    }
}
