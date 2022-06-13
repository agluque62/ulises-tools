using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaggingTool
{
    class Program
    {
        const string filesRefsName = "vdatain.csv";
        const string filesResult = "tagdataout.csv";

        static string dirFrom = "..";

        static string InputFile { get; set; }
        static string OutputFile { get; set; }
        static string DirFrom { get; set; }

        static void Main(string[] args)
        {
            Console.WriteLine($"TagTool 2.0. Herramienta de Etiquetado de ficheros. Nucleo CC 2018");
            Console.WriteLine($"> tagtool <input file> <source dir>");
            Console.WriteLine();

            InputFile = args.Length > 0 ? args[0] : filesRefsName;
            DirFrom = args.Length > 1 ? args[1] : dirFrom;
            OutputFile = filesResult;

            if (File.Exists(InputFile) == false)
            {
                Console.WriteLine("No se encuentra el fichero de descripcion < {0} >", InputFile);
                return;
            }

            //if (args.Length > 0)
            //    dirFrom = args[0];

            Console.WriteLine($"Leyendo fichero {InputFile} para leer en {DirFrom}");
            VersionDetails versiones = new VersionDetails(VersionDetails.InputType.Csv, DirFrom, InputFile);
            Console.WriteLine("Escribiendo resultado ....");
            File.WriteAllText(OutputFile, versiones.ToCsvString());
            Console.WriteLine($"Resultado en {OutputFile}. Pulse ENTER para salir...");
            Console.ReadLine();
        }
    }
}
