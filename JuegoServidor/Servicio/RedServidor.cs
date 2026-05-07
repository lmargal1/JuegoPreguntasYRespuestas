using System;
using System.Collections.Generic;
using System.Linq; 
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using JuegoPreguntasYRespuestas.Modelo; 
using JuegoServidor.DAO; 

namespace JuegoServidor.Servicio 
{
    public class DatosPartida {
        public string Jugador { get; set; }
        public int IdCategoria { get; set; }
        public int Correctas { get; set; }
        public int Incorrectas { get; set; }
        public List<Tuple<int, string, bool>> Respuestas { get; set; }
    }

    public class RedServidor 
    {
        public static Dictionary<string, int> PuntajesGlobales = new Dictionary<string, int>();
        public static List<string> JugadoresConectados = new List<string>();
        public static List<Socket> ClientesConectados = new List<Socket>();
        public static string TablaFinal = "";

        public static string ObtenerIPLocal() {
            try {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0)) {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    return endPoint.Address.ToString();
                }
            } catch {
                return "127.0.0.1";
            }
        }

        public static async Task IniciarServidorAsync(int puerto = 11000) {
            IPEndPoint localEP = new IPEndPoint(IPAddress.Any, puerto);
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try {
                listener.Bind(localEP);
                listener.Listen(10);
                Console.WriteLine($"\n[INFO] Servidor escuchando en la IP: {ObtenerIPLocal()}");
                
                while (true) {
                    Socket handler = await Task.Factory.FromAsync(listener.BeginAccept, listener.EndAccept, null);
                    ClientesConectados.Add(handler);
                    Console.WriteLine("[RED] Un nuevo jugador se ha conectado.");
                    _ = AtenderSocketAsync(handler);
                }
            } catch (Exception ex) {
                Console.WriteLine("[ERROR CRÍTICO] " + ex.Message);
            } 
        }

        private static async Task AtenderSocketAsync(Socket socket) {
            try {
                byte[] buffer = new byte[65536];
                while (true) {
                    int recibidos = await Task.Factory.FromAsync((cb, st) => socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, cb, st), socket.EndReceive, null);
                    if (recibidos == 0) break; 
                    
                    string texto = Encoding.UTF8.GetString(buffer, 0, recibidos);
                    ProcesarMensajeInterno(texto, socket);
                }
            } catch { } 
            finally {
                ClientesConectados.Remove(socket);
                Console.WriteLine("[RED] Un jugador se ha desconectado.");
            }
        }

        private static void ProcesarMensajeInterno(string texto, Socket clienteFuente) {
            string[] mensajes = texto.Split(new[] { "<|EOM|>" }, StringSplitOptions.RemoveEmptyEntries);
            foreach(var msg in mensajes) {
                try {
                    if (msg.Contains("\"Tipo\":\"NUEVO_CLIENTE\"")) {
                        var m = JsonConvert.DeserializeObject<MensajeRed>(msg);
                        if (!JugadoresConectados.Contains(m.Contenido)) {
                            JugadoresConectados.Add(m.Contenido);
                            Console.WriteLine($"[SALA] Jugador unido: {m.Contenido}");
                        }
                        var actualizacion = new MensajeRed { 
                            Tipo = "ACTUALIZAR_SALA", 
                            Contenido = string.Join(",", JugadoresConectados) 
                        };
                        _ = DifundirMensajeAsync(JsonConvert.SerializeObject(actualizacion));
                    }
                    else if (msg.Contains("\"Tipo\":\"PEDIR_CATEGORIAS\"")) {
                        var categorias = new JuegoDao().ObtenerCategorias();
                        var respuesta = new MensajeRed { Tipo = "RECIBIR_CATEGORIAS", Contenido = JsonConvert.SerializeObject(categorias) };
                        _ = EnviarAClienteAsync(clienteFuente, JsonConvert.SerializeObject(respuesta));
                    }
                    else if (msg.Contains("\"Tipo\":\"PEDIR_PREGUNTAS_CAT\"")) {
                        var m = JsonConvert.DeserializeObject<MensajeRed>(msg);
                        var preguntas = new JuegoDao().ObtenerPreguntasPorCategoria(int.Parse(m.Contenido));
                        var respuesta = new MensajeRed { Tipo = "RECIBIR_PREGUNTAS", Contenido = JsonConvert.SerializeObject(preguntas) };
                        _ = EnviarAClienteAsync(clienteFuente, JsonConvert.SerializeObject(respuesta));
                    }
                    else if (msg.Contains("\"Tipo\":\"PEDIR_PREGUNTAS_TODAS\"")) {
                        var preguntas = new JuegoDao().ObtenerTodasLasPreguntas();
                        var respuesta = new MensajeRed { Tipo = "RECIBIR_PREGUNTAS", Contenido = JsonConvert.SerializeObject(preguntas) };
                        _ = EnviarAClienteAsync(clienteFuente, JsonConvert.SerializeObject(respuesta));
                    }
                    else if (msg.Contains("\"Tipo\":\"PEDIR_OPCIONES\"")) {
                        var m = JsonConvert.DeserializeObject<MensajeRed>(msg);
                        var opciones = new JuegoDao().ObtenerOpcionesPorPregunta(int.Parse(m.Contenido));
                        var respuesta = new MensajeRed { Tipo = "RECIBIR_OPCIONES", Contenido = JsonConvert.SerializeObject(opciones) };
                        _ = EnviarAClienteAsync(clienteFuente, JsonConvert.SerializeObject(respuesta));
                    }
                    else if (msg.Contains("\"Tipo\":\"PEDIR_HISTORIAL\"")) {
                        var historial = new JuegoDao().ObtenerHistorialDesplegable();
                        var respuesta = new MensajeRed { Tipo = "RECIBIR_HISTORIAL", Contenido = JsonConvert.SerializeObject(historial) };
                        _ = EnviarAClienteAsync(clienteFuente, JsonConvert.SerializeObject(respuesta));
                    }
                    else if (msg.Contains("\"Tipo\":\"GUARDAR_PARTIDA\"")) {
                        var m = JsonConvert.DeserializeObject<MensajeRed>(msg);
                        var datos = JsonConvert.DeserializeObject<DatosPartida>(m.Contenido);
                        int idPartida = new JuegoDao().GuardarPartida(datos.Jugador, datos.IdCategoria, datos.Correctas, datos.Incorrectas);
                        if (idPartida > 0 && datos.Respuestas != null) {
                            foreach(var reg in datos.Respuestas) {
                                new JuegoDao().GuardarRespuesta(idPartida, reg.Item1, null, reg.Item3);
                            }
                        }
                    }
                    else if (msg.Contains("\"Tipo\":\"INICIAR_CON_CAT\"")) {
                        _ = DifundirMensajeAsync(msg);
                        Console.WriteLine("[JUEGO] Orden de inicio enviada a todos.");
                    }
                    else if (msg.Contains("\"Tipo\":\"REPORTE_PUNTAJE\"")) {
                        var m = JsonConvert.DeserializeObject<MensajeRed>(msg);
                        string[] partes = m.Contenido.Split(':');
                        PuntajesGlobales[partes[0]] = int.Parse(partes[1]);
                        if (JugadoresConectados.Count > 0 && PuntajesGlobales.Count >= JugadoresConectados.Count) {
                            var lineas = new List<string>();
                            var ordenados = PuntajesGlobales.ToList();
                            ordenados.Sort((p1, p2) => p2.Value.CompareTo(p1.Value)); 
                            int pos = 1;
                            foreach (var p in ordenados) {
                                lineas.Add($"#{pos} {p.Key.PadRight(15)} {p.Value} pts"); pos++;
                            }
                            TablaFinal = string.Join("\n", lineas);
                            var finalMsg = new MensajeRed { Tipo = "TABLA_FINAL", Contenido = TablaFinal };
                            _ = DifundirMensajeAsync(JsonConvert.SerializeObject(finalMsg));
                        }
                    } 
                    else {
                        _ = DifundirMensajeAsync(msg);
                    }
                } catch (Exception ex) {
                    Console.WriteLine("[ERROR] " + ex.Message);
                }
            }
        }

        private static async Task EnviarAClienteAsync(Socket cliente, string mensaje) {
            string mensajeLimpio = mensaje.Replace("<|EOM|>", "") + "<|EOM|>";
            byte[] datos = Encoding.UTF8.GetBytes(mensajeLimpio);
            try { 
                await Task.Factory.FromAsync((cb, st) => cliente.BeginSend(datos, 0, datos.Length, SocketFlags.None, cb, st), cliente.EndSend, null); 
            } catch { }
        }

        public static async Task DifundirMensajeAsync(string mensaje) {
            string mensajeLimpio = mensaje.Replace("<|EOM|>", "") + "<|EOM|>";
            byte[] datos = Encoding.UTF8.GetBytes(mensajeLimpio);
            foreach (var cliente in ClientesConectados.ToList()) {
                try { 
                    await Task.Factory.FromAsync((cb, st) => cliente.BeginSend(datos, 0, datos.Length, SocketFlags.None, cb, st), cliente.EndSend, null); 
                } catch { }
            }
        }
    }
}