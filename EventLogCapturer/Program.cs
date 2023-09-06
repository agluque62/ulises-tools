using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EventLogCapturer.helpers;

namespace EventLogCapturer
{
    class Program
    {
        static Task state = null;
        static void Main(string[] args)
        {
            ConsoleKeyInfo key;
            do
            {
                PrintTitulo();
                PrintMenu();
                key = Console.ReadKey(true);
                if (key.Key== ConsoleKey.D1)
                {
                    if (state == null)
                    {
                        state = Task.Run(() =>
                        {
                            using (var srv = new UdpSocket(Properties.Settings.Default.ListenPort))
                            {
                                srv.NewDataEvent += Srv_NewDataEvent;
                                srv.BeginReceive();

                                while (state != null)
                                {
                                    Task.Delay(100).Wait();
                                }
                            }
                        });
                    }
                    else
                    {
                        state = null;
                        Task.Delay(200).Wait();
                    }
                }
            } while (key.Key != ConsoleKey.Escape);

            state = null;
            Task.Delay(200).Wait();
        }

        private static void Srv_NewDataEvent(object sender, DataGram e)
        {
            var strRec = Encoding.ASCII.GetString(e.Data);
            var xmlData = new Log4jEvent(strRec);
            Logger.Info<Program>($"From {e.Client.Address} => {xmlData.Message}");
        }

        static void PrintTitulo()
        {
            Console.Clear();
            ConsoleColor last = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Capturador de Logs de Red. Amper 2022");
            Console.WriteLine("Escuchando en puerto {0}", Properties.Settings.Default.ListenPort);
            Console.WriteLine();
            Console.ForegroundColor = last;
        }
        static void PrintMenu()
        {
            ConsoleColor last = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine();
            Console.WriteLine("Estado => {0}", state==null ? "Parado" : "Arrancado");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  [1] Start / Stop");
            Console.WriteLine("  [ESC] Salir");
            Console.WriteLine();

            Console.ForegroundColor = last;
        }
    }
}
