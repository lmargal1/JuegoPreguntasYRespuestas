using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace JuegoPreguntasYRespuestas.Servicio
{
    internal class Red
    {
        //Obtenerla ip local del servidor
        public static string ObtenerIPLocal()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();
            }
            throw new InvalidOperationException("No se encontró una dirección IPv4 en esta máquina.");
        }

        //Servidor: empieza a escuchar clientes
        public static async Task IniciarServidorAsync(int puerto = 11000)
        {
            IPEndPoint localEP = new IPEndPoint(IPAddress.Any, puerto);

            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                listener.Bind(localEP);
                listener.Listen(10);
                Console.WriteLine($"[SERVIDOR] Escuchando en {localEP} ...");

                while (true)
                {
                    Socket handler = await Task.Factory.FromAsync(
                        listener.BeginAccept,
                        listener.EndAccept,
                        null);

                    Console.WriteLine($"[CONECTADO] Cliente {handler.RemoteEndPoint}");
                    _ = AtenderClienteAsync(handler); //Atender en paralelo
                }
            }
            finally
            {
                listener.Close();
            }
        }

        //Servidor: atiende a un cliente específico
        private static async Task AtenderClienteAsync(Socket handler)
        {
            try
            {
                byte[] buffer = new byte[1024];
                while (true)
                {
                    int recibidos = await Task.Factory.FromAsync(
                        (cb, st) => handler.BeginReceive(
                            buffer, 0, buffer.Length, SocketFlags.None, cb, st),
                        handler.EndReceive,
                        null);

                    if (recibidos == 0)
                        break;

                    string mensaje = Encoding.UTF8.GetString(buffer, 0, recibidos);
                    Console.WriteLine($"[RECIBIDO] {mensaje}");

                    if (mensaje.IndexOf("<|EOM|>", StringComparison.Ordinal) > -1)
                    {
                        byte[] ack = Encoding.UTF8.GetBytes("<|ACK|>");
                        await Task.Factory.FromAsync(
                            (cb, st) => handler.BeginSend(
                                ack, 0, ack.Length, SocketFlags.None, cb, st),
                            handler.EndSend,
                            null);
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
                try { handler.Shutdown(SocketShutdown.Both); } catch { }
                handler.Close();
                Console.WriteLine("[DESCONECTADO] Cliente se ha ido.");
            }
        }

        //Cliente: envía mensaje al servidor
        public static async Task<string> EnviarMensajeAsync(
            string ipServidor, int puerto, string mensaje)
        {
            IPAddress ipAddress = IPAddress.Parse(ipServidor);
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, puerto);

            Socket client = new Socket(AddressFamily.InterNetwork,
                                       SocketType.Stream,
                                       ProtocolType.Tcp);
            try
            {
                // Conectar
                await Task.Factory.FromAsync(
                    (cb, st) => client.BeginConnect(remoteEP, cb, st),
                    client.EndConnect,
                    null);

                // Enviar
                byte[] datos = Encoding.UTF8.GetBytes(mensaje + "<|EOM|>");
                await Task.Factory.FromAsync(
                    (cb, st) => client.BeginSend(
                        datos, 0, datos.Length, SocketFlags.None, cb, st),
                    client.EndSend,
                    null);

                // Recibir respuesta (ACK)
                byte[] buffer = new byte[1024];
                int recibidos = await Task.Factory.FromAsync(
                    (cb, st) => client.BeginReceive(
                        buffer, 0, buffer.Length, SocketFlags.None, cb, st),
                    client.EndReceive,
                    null);

                return Encoding.UTF8.GetString(buffer, 0, recibidos);
            }
            finally
            {
                try { client.Shutdown(SocketShutdown.Both); } catch { }
                client.Close();
            }
        }
    }
}


//ESTO ES PARA PROGRAMA DE CONSOLA DE PRUEBA, NO ES PARTE DEL SERVIDOR NI DEL CLIENTE FINAL, SÓLO PARA VER CÓMO FUNCIONA LA COMUNICACIÓN ENTRE AMBOS CON SOCKETS Y JSON
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
