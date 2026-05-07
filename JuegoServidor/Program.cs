using System;
using System.Threading.Tasks;
using JuegoServidor.Servicio; // Nos aseguramos de apuntar a la clase del servidor

namespace JuegoServidor
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=================================");
            Console.WriteLine("   SERVIDOR DE TRIVIA INICIADO   ");
            Console.WriteLine("=================================");
            
            // Usamos RedServidor en lugar de RedCliente
            await RedServidor.IniciarServidorAsync(11000);
            
            Console.WriteLine("Esperando conexiones de jugadores...");
            Console.WriteLine("Presiona ENTER en cualquier momento para apagar el servidor.");
            Console.ReadLine(); 
        }
    }
}