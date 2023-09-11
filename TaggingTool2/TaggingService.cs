using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Security.Cryptography;

using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text.RegularExpressions;
using System.Globalization;

namespace TaggingTool2
{
    public class TaggingService
    {
        class ItemFileInfo
        {
            public long Size { get; set; } = 0;
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
                        var file = nameOfFile(row.Cell(3).Value.ToString(), row.Cell(4).Value.ToString());

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
        private async Task<ItemFileInfo> Find(string from, string file, string mode)
        {
            //await Task.Delay(100);
            try 
            {
                var found = Directory.GetFiles(/*dirFrom*/from, file, SearchOption.AllDirectories).FirstOrDefault();
                if (found == null)
                {
                    return new ItemFileInfo() { Hash = "Not Found" };
                }
                FileInfo fileInfo = new FileInfo(found);
                return await ItemFileInfoGet(fileInfo, mode);
            }
            catch 
            { 
                return new ItemFileInfo() { Hash = "Not Found" };
            }
        }
        private async Task<ItemFileInfo> ItemFileInfoGet(FileInfo fileInfo, string mode)
        {
            var itemFileInfo = new ItemFileInfo();
            itemFileInfo.Size = fileInfo.Length;
            itemFileInfo.Hash = GetHash(fileInfo.FullName);
            itemFileInfo.Date = GetDate(fileInfo, mode);
            await Task.Delay(50);
            return itemFileInfo;
        }
        void PrintInfo (string findFrom, string file, string mode, string hash)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = hash == "Not Found" ? ConsoleColor.Red : ConsoleColor.Green;

            file = file.Length > 30 ? file.Substring(0, 30) + "..." : file;
            findFrom = findFrom.Length > 20 ? findFrom.Substring(0, 20) + "..." : findFrom;
            hash = hash.Length > 30 ? hash.Substring(0, 30) + "..." : hash;

            Console.WriteLine($" Processing {file, 33} FROM {findFrom, 23}, MODE {mode} => {hash, 33}");

            Console.ForegroundColor = color;
        }
        string nameOfFile(string name, string ext) => ext == "" ? name : name + "." + ext;
        string GetHash(string filePath)
        {
            using (var sha256 = SHA256.Create())
            {
                using (var stream = System.IO.File.OpenRead(filePath))
                {
                    return BitConverter.ToString(sha256.ComputeHash(stream)).Replace("-", string.Empty).ToUpper();
                }
            }
        }
        string GetDate(FileInfo fileInfo, string mode)
        {
            switch (mode)
            {
                case "0":
                    return fileInfo.LastWriteTime.ToShortDateString();
                case "1": // Buscar "NUCLEODF 2017-01-26 16:35:50" Grupo 1.
                case "2":
                    return DateSearch(fileInfo.FullName, mode, "NUCLEODF ((19|20)\\d\\d[- /.](0[1-9]|1[012])[- /.](0[1-9]|[12][0-9]|3[01]))", 1);
                case "3": // Buscar "Driver PCM 82xx MCC. $Revision: 1.3 $ (Dec 20 2016 18:13:11)", Grupo 3
                    return DateSearch(fileInfo.FullName, mode, "Revision:(.*)(\\((.*)\\))", 3);
                case "4": // Buscar "05/01/2017, 15:39 <291328>" Grupo 1
                    return DateSearch(fileInfo.FullName, mode, "([0-9][0-9][\\/][0-9][0-9][\\/](19|20)[1-9][1-9]), [0-9][0-9]:[0-9][0-9] <", 1);
                case "5": // Buscar "NUCLEODF 2017-01-26 16:35:50" Grupo 1, 2 veces,
                    return DateSearch(fileInfo.FullName, mode, "NUCLEODF ((19|20)\\d\\d[- /.](0[1-9]|1[012])[- /.](0[1-9]|[12][0-9]|3[01]))", 1, 2);
            }
            return "Date not Found";
        }
        string DateSearch(string filePath, string mode, string pattern, int grp, int rep = 0)
        {
            var found = Search(filePath, pattern, grp, rep);
            if (found != "")
            {
                return DateNormalize(found, mode);
            }
            return "Date not Found";
        }
        string Search(string filePath, string pattern, int grp, int rep = 0)
        {
            var data = System.Text.Encoding.UTF8.GetString(System.IO.File.ReadAllBytes(filePath));
            MatchCollection matches = Regex.Matches(data, pattern);
            if (matches.Count > rep)
            {
                return matches[rep].Groups[grp].Value;
            }
            return "";
        }
        string DateNormalize(string inputDate, string mode)
        {
            switch (mode)
            {
                case "1": // AAAA-MM-dd
                case "2":
                case "5":
                    return DateTime.ParseExact(inputDate, format: "yyyy-MM-dd", provider: CultureInfo.InvariantCulture)
                        .ToShortDateString();
                case "3": // Feb 20 202 HH:mm:ss
                    return DateTime.Parse(inputDate)
                        .ToShortDateString();
                case "4":
                    return inputDate;
            }
            return $"({inputDate}) => Date Format Error";
        }
    }
}
