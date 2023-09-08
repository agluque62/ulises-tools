using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;

namespace TaggingTool2
{
    public class TaggingService
    {
        class FileInfo
        {
            public string Size { get; set; } = "";
            public string Date { get; set; } = "";
            public string Hash { get; set; } = "";
        }
        protected string File { get; }
        protected string Sheet { get; }
        public TaggingService(string file = "./tagdata.xlsx", string sheet = "Hoja1")
        { 
            File = file;
            Sheet = sheet;
        }
        public async Task Update()
        {
            using (var workbook = new XLWorkbook(File))
            {
                // Selecciona la hoja de trabajo que deseas leer
                var worksheet = workbook.Worksheet(Sheet);
                // Itera a través de las celdas y lee los datos
                foreach (var row in worksheet.RowsUsed())
                {
                    if (row.Cell(1).Value.IsBlank == false && row.Cell(2).Value.IsBlank == false)
                    {
                        var mode = row.Cell(1).Value.ToString();
                        var findFrom = row.Cell(2).Value.ToString();
                        var file = row.Cell(3).Value.ToString() + "." + row.Cell(4).Value.ToString();

                        var fileInfo = await Find(findFrom, file, mode);
                        PrintInfo(findFrom, file, mode, fileInfo.Hash);

                        row.Cell(5).Value = fileInfo.Size;
                        row.Cell(6).Value = fileInfo.Date;
                        row.Cell(7).Value = fileInfo.Hash;
                    }
                }
                workbook.Save();
            }
        }
        private async Task<FileInfo> Find(string from, string file, string mode)
        {
            await Task.Delay(100);
            return new FileInfo() { Hash = "Not Found" };
        }
        private void PrintInfo (string findFrom, string file, string mode, string hash)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = hash == "Not Found" ? ConsoleColor.Red : ConsoleColor.White;

            file = file.Length > 48 ? file.Substring(0, 48) + " ..." : file;
            findFrom = findFrom.Length > 32 ? findFrom.Substring(0, 32) + " ..." : findFrom;

            Console.WriteLine($"Processing {file} FROM {findFrom}, MODE {mode} => {hash}");

            Console.ForegroundColor = color;
        }
    }
}
