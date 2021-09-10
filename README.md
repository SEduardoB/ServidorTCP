# ServidorTCP

Servidor TCP para múltiples conexiones implementado en C#.
Este servidor está inspirado (copy😸) en código y funciones de un servidor WS de los desarrolladores de [Fleck Websockets](https://github.com/statianzo/Fleck).
Se le realizaron adecuaciones para su funcionalidad como servidor TCP.

## Ejemplo de uso.
```c#
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
```
