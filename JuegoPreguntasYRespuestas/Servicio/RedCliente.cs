using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace JuegoPreguntasYRespuestas.Servicio {
    public class RedCliente {
        public static Action<string> AlRecibirMensaje; 
        
        public static string NombreLocal = "Jugador";
        public static Socket ConexionAlServidor;

        public static async Task ConectarComoClienteAsync(string ip, int puerto = 11000) {
            ConexionAlServidor = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await Task.Factory.FromAsync(ConexionAlServidor.BeginConnect, ConexionAlServidor.EndConnect, new IPEndPoint(IPAddress.Parse(ip), puerto), null);
            _ = AtenderSocketAsync(ConexionAlServidor);
        }

        private static async Task AtenderSocketAsync(Socket socket) {
            try {
                byte[] buffer = new byte[65536];
                while (true) {
                    int recibidos = await Task.Factory.FromAsync((cb, st) => socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, cb, st), socket.EndReceive, null);
                    if (recibidos == 0) break; 
                    
                    string texto = Encoding.UTF8.GetString(buffer, 0, recibidos);
                    ProcesarMensajeInterno(texto);
                }
            } catch { }
        }

        private static void ProcesarMensajeInterno(string texto) {
            string[] mensajes = texto.Split(new[] { "<|EOM|>" }, StringSplitOptions.RemoveEmptyEntries);
            foreach(var msg in mensajes) {
                AlRecibirMensaje?.Invoke(msg);
            }
        }

        public static async Task EnviarAlServidorAsync(string mensaje) {
            if (ConexionAlServidor != null && ConexionAlServidor.Connected) {
                string mensajeLimpio = mensaje.Replace("<|EOM|>", "") + "<|EOM|>";
                byte[] datos = Encoding.UTF8.GetBytes(mensajeLimpio);
                await Task.Factory.FromAsync((cb, st) => ConexionAlServidor.BeginSend(datos, 0, datos.Length, SocketFlags.None, cb, st), ConexionAlServidor.EndSend, null);
            }
        }
    }
}