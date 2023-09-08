// See https://aka.ms/new-console-template for more information
using TaggingTool2;

Console.WriteLine("Starting Tagging Process...");

var service = new TaggingService();
await service.Update();

Console.WriteLine("Tagging Process Finished... ENTER for exit.");
Console.ReadLine();
