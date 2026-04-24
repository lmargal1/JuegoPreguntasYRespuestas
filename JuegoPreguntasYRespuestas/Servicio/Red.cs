using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuegoPreguntasYRespuestas.Servicio
{
    internal class Red
    {

    }
}

/*
 Cliente:
using System.Net;
using System.Net.Sockets;
using System.Text;

//CONFIGURAR EL PUNTO DE CONEXIÓN
string ipServidor = "10.103.151.106";
IPAddress ipAddress = IPAddress.Parse(ipServidor);
IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);

// 2. CREAR EL SOCKET CLIENTE
// Se inicializa con la familia de direcciones, tipo de socket y protocolo TCP
using Socket client = new Socket(
    ipAddress.AddressFamily,
    SocketType.Stream,
    ProtocolType.Tcp);

try
{
    // 3. CONECTARSE AL SERVIDOR
    // El cliente usa ConnectAsync para iniciar la comunicación
    await client.ConnectAsync(remoteEP);
    Console.WriteLine($"¡Conectado exitosamente al servidor en {ipServidor}!");

    // 4. ENVIAR UN MENSAJE
    // El mensaje debe terminar con <|EOM|> para que tu servidor sepa que terminó
    string mensaje = "Ola de maaaar. Mensaje enviado desde el cliente. <|EOM|>";
    byte[] messageBytes = Encoding.UTF8.GetBytes(mensaje);
    await client.SendAsync(messageBytes, SocketFlags.None);
    Console.WriteLine("Mensaje enviado. Esperando respuesta del servidor...");

    // 5. RECIBIR LA CONFIRMACIÓN (ACK)
    // El cliente debe esperar la respuesta del servidor antes de cerrar
    byte[] buffer = new byte[1024];
    int bytesRecibidos = await client.ReceiveAsync(buffer, SocketFlags.None);
    string respuesta = Encoding.UTF8.GetString(buffer, 0, bytesRecibidos);

    Console.WriteLine($"Respuesta del servidor: {respuesta}");

    // 6. CIERRE SEGURO
    // Se usa Shutdown para finalizar operaciones de envío y recepción
    client.Shutdown(SocketShutdown.Both);
    Console.WriteLine("Conexión cerrada.");
}
catch (Exception e)
{
    Console.WriteLine($"No se pudo conectar: {e.Message}");
}

Console.WriteLine("\nPresiona cualquier tecla para salir...");
Console.ReadKey();
*/



/*
 Servidor:
using System.Net;
using System.Net.Sockets;
using System.Text;

// CONFIGURAR IP MANUALMENTE (AQUÍ PONES LA IP REAL DEL SERVIDOR)
string ipServidor = "10.103.151.106";
IPAddress ipAddress = IPAddress.Parse(ipServidor);

int puerto = 11000;
IPEndPoint localEndPoint = new IPEndPoint(ipAddress, puerto);

// CREAR SOCKET
using Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

// Vincular
listener.Bind(localEndPoint);

// Escuchar
listener.Listen(100);

Console.WriteLine($"SERVIDOR MULTICLIENTE INICIADO EN: {ipAddress}");
Console.WriteLine($"PUERTO: {puerto}");
Console.WriteLine("\nEsperando a que los clientes se conecten...");

while (true)
{
    Socket handler = await listener.AcceptAsync();
    _ = Task.Run(() => AtenderCliente(handler));
}

async Task AtenderCliente(Socket handler)
{
    Console.WriteLine($"\n[NUEVA CONEXIÓN] Cliente conectado desde: {handler.RemoteEndPoint}");

    try
    {
        while (true)
        {
            var buffer = new byte[1024];
            var received = await handler.ReceiveAsync(buffer, SocketFlags.None);
            var response = Encoding.UTF8.GetString(buffer, 0, received);

            if (response.Contains("<|EOM|>"))
            {
                Console.WriteLine($"[MENSAJE] {handler.RemoteEndPoint} dice: {response.Replace("<|EOM|>", "")}");

                var ack = Encoding.UTF8.GetBytes("<|ACK|>");
                await handler.SendAsync(ack, SocketFlags.None);
                break;
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error con un cliente: {ex.Message}");
    }
    finally
    {
        handler.Shutdown(SocketShutdown.Both);
        handler.Close();
        Console.WriteLine($"[DESCONECTADO] Cliente de {handler.RemoteEndPoint} se ha ido.");
    }
}
 
*/
