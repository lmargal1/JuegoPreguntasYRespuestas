using System;
using System.Collections.Generic;
using System.Linq; 
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using JuegoPreguntasYRespuestas.Modelo; 

namespace JuegoPreguntasYRespuestas.Servicio {
    internal class Red {
        public static Action<string> AlRecibirMensaje;
        public static Dictionary<string, int> PuntajesGlobales = new Dictionary<string, int>();
        
        public static string NombreLocal = "Jugador";
        public static List<string> JugadoresConectados = new List<string>();

        public static List<Socket> ClientesConectados = new List<Socket>();
        public static Socket ConexionAlServidor;
        public static bool EsServidor = false;
        public static string TablaFinal = "";

        public static string ObtenerIPLocal() {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList) {
                if (ip.AddressFamily == AddressFamily.InterNetwork) return ip.ToString();
            }
            return "127.0.0.1";
        }

        public static async Task IniciarServidorAsync(int puerto = 11000) {
            EsServidor = true;
            IPEndPoint localEP = new IPEndPoint(IPAddress.Any, puerto);
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try {
                listener.Bind(localEP);
                listener.Listen(10);
                while (true) {
                    Socket handler = await Task.Factory.FromAsync(listener.BeginAccept, listener.EndAccept, null);
                    ClientesConectados.Add(handler);
                    _ = AtenderSocketAsync(handler);
                }
            } catch { } 
        }

        public static async Task ConectarComoClienteAsync(string ip, int puerto = 11000) {
            EsServidor = false;
            ConexionAlServidor = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await Task.Factory.FromAsync(ConexionAlServidor.BeginConnect, ConexionAlServidor.EndConnect, new IPEndPoint(IPAddress.Parse(ip), puerto), null);
            _ = AtenderSocketAsync(ConexionAlServidor);
        }

        private static async Task AtenderSocketAsync(Socket socket) {
            try {
                byte[] buffer = new byte[2048];
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
                try {
                    if (msg.Contains("\"Tipo\":\"REPORTE_PUNTAJE\"")) {
                        var m = JsonConvert.DeserializeObject<MensajeRed>(msg);
                        string[] partes = m.Contenido.Split(':');
                        PuntajesGlobales[partes[0]] = int.Parse(partes[1]);
                        
                        if (EsServidor && JugadoresConectados.Count > 0 && PuntajesGlobales.Count >= JugadoresConectados.Count) {
                            var lineas = new List<string>();
                            var ordenados = PuntajesGlobales.ToList();
                            ordenados.Sort((p1, p2) => p2.Value.CompareTo(p1.Value)); 
                            
                            int pos = 1;
                            foreach (var p in ordenados) {
                                lineas.Add($"#{pos} {p.Key.PadRight(15)} {p.Value} pts");
                                pos++;
                            }
                            TablaFinal = string.Join("\n", lineas);
                            var finalMsg = new MensajeRed { Tipo = "TABLA_FINAL", Contenido = TablaFinal };
                            string json = JsonConvert.SerializeObject(finalMsg);
                            _ = DifundirMensajeAsync(json);
                            AlRecibirMensaje?.Invoke(json); 
                        }
                    } else {
                        AlRecibirMensaje?.Invoke(msg);
                    }
                } catch { }
            }
        }

        public static async Task DifundirMensajeAsync(string mensaje) {
            byte[] datos = Encoding.UTF8.GetBytes(mensaje + "<|EOM|>");
            foreach (var cliente in ClientesConectados) {
                try { await Task.Factory.FromAsync((cb, st) => cliente.BeginSend(datos, 0, datos.Length, SocketFlags.None, cb, st), cliente.EndSend, null); } catch { }
            }
        }

        public static async Task EnviarAlServidorAsync(string mensaje) {
            if (EsServidor) {
                ProcesarMensajeInterno(mensaje + "<|EOM|>");
            } else if (ConexionAlServidor != null && ConexionAlServidor.Connected) {
                byte[] datos = Encoding.UTF8.GetBytes(mensaje + "<|EOM|>");
                await Task.Factory.FromAsync((cb, st) => ConexionAlServidor.BeginSend(datos, 0, datos.Length, SocketFlags.None, cb, st), ConexionAlServidor.EndSend, null);
            }
        }
    }
}