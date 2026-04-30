using Newtonsoft.Json;

namespace JuegoPreguntasYRespuestas.Servicio
{
    internal static class Serializador
    {
        //Objeto C# -> String JSON
        public static string AJson<T>(T objeto)
        {
            return JsonConvert.SerializeObject(objeto);
        }

        //String JSON -> Objeto C#
        public static T DesdeJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}

//EEEEEH intalen Newtonsoft.Json desde NuGet para que esto funcione
/*
 * Herramientas -> Administrador de paquetes NuGet -> Consola del administrador de paquetes
 * Y en la consola escriben: Install-Package Newtonsoft.Json
*/
