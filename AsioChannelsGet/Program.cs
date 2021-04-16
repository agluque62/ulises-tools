using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Asio;

namespace AsioChannelsGet
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("AsioChannelGet. Obteniendo los nombres de los canales ASIO");
            Console.WriteLine();
            try
            {
                AsioChannels.Init();

                /** Entradas */
                int iChannel = 0;
                Console.WriteLine($"Canales de Entrada ({AsioChannels.InChannels.Count})");
                AsioChannels.InChannels.ForEach((channelName) =>
                {
                    Console.WriteLine($"\tEntrada ({iChannel++}): {channelName}");
                });
                Console.WriteLine();

                /** Salidas */
                iChannel = 0;
                Console.WriteLine($"Canales de Salida ({AsioChannels.OutChannels.Count})");
                AsioChannels.OutChannels.ForEach((channelName) =>
                {
                    Console.WriteLine($"\tSalida ({iChannel++}): {channelName}");
                });
                Console.WriteLine();
            }
            catch (Exception x)
            {
                Console.WriteLine($"Error => {x.Message}");
            }

            Console.WriteLine("Pulse ENTER para finalizar...");
            Console.ReadLine();
        }
    }
}
