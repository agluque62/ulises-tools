// See https://aka.ms/new-console-template for more information
using TaggingTool2;

const string DefaultTagfile = "./tagdata.xlsx";
const string DefaultSheet = "Hoja1";


Console.WriteLine($"TagTool 2. Herramienta de Etiquetado de ficheros. Grupo Amper 2023");
Console.WriteLine("Starting Tagging Process...");

try
{
    var TagFile = args.Length > 0 ? args[0] : DefaultTagfile;
    var Sheet = args.Length > 1 ? args[1] : DefaultSheet;

    Console.WriteLine($"TaggingTool2 {TagFile}, {Sheet}");
    if (File.Exists(TagFile) == false)
    {
        var color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("No se encuentra el fichero de descripcion < {0} >", TagFile);
        Console.ForegroundColor = color;
        Console.WriteLine($"Pulse ENTER para continuar...");
        Console.ReadLine();
        return;
    }

    var service = new TaggingService(TagFile, Sheet);
    await service.Update();
}
catch (Exception ex)
{
    var color = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Error => {ex.Message}"/*ex.ToString()*/);
    Console.ForegroundColor = color;
}

Console.WriteLine("Tagging Process Finished... ENTER for exit.");
Console.ReadLine();
