using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using CsvHelper;


namespace TaggingTool
{
    /// <summary>
    /// 
    /// </summary>
    class VersionDetails
    {
        /// <summary>
        /// 
        /// </summary>
        public enum InputType { Json, Csv }
        /// <summary>
        /// 
        /// </summary>
        class Item2Search
        {
            public string Fichero { get; set; }
            public string EXT { get; set; }
            public string MOD { get; set; }
            public string REP { get; set; }
            public string Base { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        class ItemFound
        {
            public string Nombre { get; set; }
            public string Ext { get; set; }
            public string Size { get; set; }
            public string Fecha { get; set; }
            public string MD5 { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        List<Item2Search> Items2Search;
        List<ItemFound> ItemsFound = new List<ItemFound>();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filepath"></param>
        public VersionDetails(InputType input, string dirFrom, string filepath)
        {
            if (input == InputType.Json)
            {
                Items2Search = JsonConvert.DeserializeObject<List<Item2Search>>(File.ReadAllText(@filepath))/*.
                    Where(item => item.Fichero != "")*/.ToList();
            }
            else if (input == InputType.Csv)
            {
                CsvReader csv = new CsvReader(reader: File.OpenText(@filepath));
                csv.Configuration.HasHeaderRecord = true;
                csv.Configuration.HeaderValidated = null;
                csv.Configuration.MissingFieldFound = null;
                csv.Configuration.Delimiter = ";";
                csv.Configuration.Encoding = Encoding.UTF8;
                Items2Search = csv.GetRecords<Item2Search>()/*.
                    Where(item => item.Fichero != "")*/.ToList();
            }
            else
            {
                return;
            }

            //FastRead(dirFrom, "node_modules");

            Read(dirFrom);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(ItemsFound);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string ToCsvString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("Nombre;Ext;Tam;Fecha;MD{0}", Environment.NewLine);
            ItemsFound.ForEach(item =>
            {
                sb.AppendFormat("{0};{1};{2};{3};{4}{5}", item.Nombre, item.Ext, item.Size, item.Fecha, item.MD5, Environment.NewLine);
            });
            return sb.ToString();
        }
        /// <summary>
        /// 
        /// </summary>
        protected void Read(string dirFrom)
        {
            ConsoleColor color = Console.ForegroundColor;
            Items2Search.ForEach(item =>
            {
                if (item.Fichero != "")
                {
                    string namefile = item.EXT == "" ? item.Fichero : item.Fichero + "." + item.EXT;
                    string pathFrom = dirFrom + "\\" + item.Base;
                    try
                    {
                        string found = Directory.GetFiles(/*dirFrom*/pathFrom, namefile, SearchOption.AllDirectories).FirstOrDefault();
                        if (found != null && File.Exists(found))
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                            FileInfo fi = new FileInfo(found);
                            ItemsFound.Add(new ItemFound()
                            {
                                Nombre = item.Fichero,
                                Ext = item.EXT,
                                Size = fi.Length.ToString(),
                                Fecha = DateOfFile(fi, item.MOD, item.REP),
                                MD5 = EncryptionHelper.FileMd5Hash(found)
                            });
                        }
                        else
                        {
                            throw new Exception("File not Found!!");
                        }
                    }
                    catch (Exception)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        ItemsFound.Add(new ItemFound() { Nombre = item.Fichero, Ext = item.EXT });
                    }
                    Console.WriteLine("En {1,-32}: {0}", namefile, pathFrom);
                }
                else
                {
                    ItemsFound.Add(new ItemFound());
                }
            });
            Console.ForegroundColor = color;
            Console.WriteLine("");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dirFrom"></param>
        protected void FastRead(string dirFrom, string exceptDir)
        {
            var allfiles = Directory.EnumerateFiles(dirFrom, "*.*", SearchOption.AllDirectories).
                Where(item => item.Contains(exceptDir)==false);
            ConsoleColor color = Console.ForegroundColor;
            Items2Search.ForEach(item =>
            {
                if (item.Fichero != "")
                {
                    string namefile = item.EXT == "" ? item.Fichero : item.Fichero + "." + item.EXT;
                    string found = allfiles.Where(s => s.EndsWith(namefile) == true).FirstOrDefault();
                    if (found != null && File.Exists(found))
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        FileInfo fi = new FileInfo(found);
                        ItemsFound.Add(new ItemFound()
                        {
                            Nombre = item.Fichero,
                            Ext = item.EXT,
                            Size = fi.Length.ToString(),
                            Fecha = DateOfFile(fi, item.MOD, item.REP),
                            MD5 = EncryptionHelper.FileMd5Hash(found)
                        });
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        ItemsFound.Add(new ItemFound() { Nombre = item.Fichero, Ext = item.EXT });
                    }
                    Console.WriteLine("     {0}, ", namefile);
                }
                else
                {
                    ItemsFound.Add(new ItemFound());
                }
            });
            Console.ForegroundColor = color;
            Console.WriteLine("");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="modo"></param>
        /// <returns></returns>
        protected string DateOfFile(FileInfo file, string modo, string rep)
        {
            if (modo == "0")
                return file.LastWriteTime.ToShortDateString();

            int findcount = Int32.TryParse(rep, out findcount) ? findcount : 0;
            byte[] byteBuffer = File.ReadAllBytes(file.FullName);
            string byteBufferAsString = System.Text.Encoding.UTF8.GetString(byteBuffer);
            string found = "";
            switch (modo)
            {
                case "1":   // Buscar Por ejemplo "NUCLEODF 2017-01-26 16:35:50"
                case "2":
                    found = Search(byteBufferAsString, "NUCLEODF (19|20)\\d\\d[- /.](0[1-9]|1[012])[- /.](0[1-9]|[12][0-9]|3[01])", findcount);
                    if (found != "")
                    {
                        DateTime dt = DateTime.ParseExact(found.Substring(9), format: "yyyy-MM-dd", provider: CultureInfo.InvariantCulture);
                        return dt.ToShortDateString();
                    }
                    break;
                case "3":   // Buscar Por ejemplo "Driver PCM 82xx MCC. $Revision: 1.3 $ (Dec 20 2016 18:13:11)"
                    found = Search(byteBufferAsString, "Revision:(.*?)\\)");                    
                    if (found != "")
                    {
                        found = Search(found, "\\((.*)\\)");
                        if (found != "")
                        {
                            found = found.Substring(1, found.Length - 2);
                            DateTime dt = DateTime.Parse(found);
                            return dt.ToShortDateString();
                        }
                    }
                    break;
                case "4":   // Buscar Por ejemplo "05-01-2017, 15:39 <291328>"
                    // found = Search(byteBufferAsString, "NUCLEODF (19|20)\\d\\d[- /.](0[1-9]|1[012])[- /.](0[1-9]|[12][0-9]|3[01])");
                    found = Search(byteBufferAsString, "[0-9][0-9][-][0-9][0-9][-](19|20)[1-9][1-9], [0-2][0-9]:[0-9][0-9] <");
                    if (found != "")
                    {
                        DateTime dt = DateTime.ParseExact(found.Substring(0, 10), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                        return dt.ToShortDateString();
                    }
                    return "";
            }
            return "";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        protected string Search(string input, string pattern, int pos = 0)
        {

            MatchCollection matches = Regex.Matches(input, pattern);
            if (matches.Count > pos)
            {
                return matches[pos].Value;
            }
            return "";
            //string findfrom = input;
            //Match m;
            //do
            //{
            //    m = Regex.Match(findfrom, pattern);
            //    findfrom = m.Value.Substring(1);

            //} while (--pos > 0);

            //return m.Success ? m.Value : "";
        }
    }
}
