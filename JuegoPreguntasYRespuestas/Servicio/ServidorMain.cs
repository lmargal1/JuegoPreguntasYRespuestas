using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JuegoPreguntasYRespuestas.DAO;
using Newtonsoft.Json;

namespace JuegoPreguntasYRespuestas.Servicio
{
    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine($"IP local: {ObtenerIPLocal()}");
        Console.WriteLine($"Esperando jugadores...");
        await EscucharConexionesAsync();
    }
    internal class ServidorMain
    {
        private const int Puerto = 8080;
        private const int MaxJugadores = 3;
        private const int TiempoPregunta = 20;
    }
}
