using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Diagnostics;

namespace xml_armering_gen
{
    class Program
    {
        static void Main(string[] args)
        {
            string script = @"xml_gen_script.scr";
            string program = @"C:\Program Files\Bricsys\BricsCAD V16 en_US\bricscad.exe";

            string netload = @"C:\Users\aleksandr.ess\Documents\GitHub\datum\BricsCommands\bin\Debug\HillsCommands.dll";


            Console.WriteLine("Enter source folder:");
            string location = Console.ReadLine();
            if (!location.EndsWith(@"\")) { location = location + @"\";  }
            string csv_location = location + @"temp\";

            List<string> dwgs = getFiles(location, "*.DWG");

            createScriptFile(dwgs, script, netload);
            runAutocad(program, script);
            deleteScriptFile(script);

            List<string> csvs = getFiles(csv_location, "*.CSV");
            List<Row> parsed = parseCSVs(csvs);

            dump(location, parsed);
            dump2(location, parsed);

            Console.WriteLine("Done.");
            Console.ReadLine();
        }

        private static bool IsProcessOpen(string name)
        {
            foreach (Process clsProcess in Process.GetProcesses())
            {
                if (clsProcess.ProcessName.Contains(name))
                {
                    return true;
                }
            }

            return false;
        }

        private static List<Row> parseCSVs(List<string> csvs)
        {
            List<Row> unique_rows = new List<Row>();

            StreamReader reader = null;

            foreach (string csv in csvs)
            {
                reader = new StreamReader(File.OpenRead(csv));
                List<Row> file_rows = readCSV(reader);
                generateUnique(file_rows, ref unique_rows);
            }

            unique_rows = unique_rows.OrderBy(b => b.Diameter).ToList();

            List<Row> rows_num = unique_rows.FindAll(x => x.Position_Shape == "A").ToList();
            List<Row> rows_char = unique_rows.FindAll(x => x.Position_Shape != "A").ToList();

            rows_char = rows_char.OrderBy(b => b.Position_Nr).ToList();
            rows_num = rows_num.OrderBy(b => b.Position_Nr).ToList();

            unique_rows = new List<Row>();
            unique_rows.AddRange(rows_char);
            unique_rows.AddRange(rows_num);

            return unique_rows;
        }

        private static void generateUnique(List<Row> files, ref List<Row> uniques)
        {
            foreach (Row f in files)
            {
                bool newRow = true;

                foreach (Row u in uniques)
                {
                    if (f.Position == u.Position && f.Diameter == u.Diameter)
                    {
                        u.Number = u.Number + f.Number;
                        newRow = false;
                        break;
                    }
                }

                if (newRow)
                {
                    Row nu = new Row(f.Number, f.Diameter, f.Position, f.Position_Shape, f.Position_Nr);
                    uniques.Add(nu);
                }
            }             
        }

        private static List<Row> readCSV(StreamReader reader)
        {
            List<Row> data = new List<Row>();

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                if (line == "[/SUMMARY]") break;

                line.Replace(" ", "");
                string[] rowData = line.Split(';');

                if (rowData.Length == 4)
                {
                    int num = 0;
                    Int32.TryParse(rowData[2], out num);
                    if (num == 0) continue;

                    int diam = 0;
                    Int32.TryParse(rowData[3], out diam);
                    if (diam == 0) continue;

                    int pos = 0;
                    Int32.TryParse(rowData[1], out pos);
                    if (pos == 0) continue;

                    if (rowData[0] == "A")
                    {
                        Row newRow = new Row(num, diam, rowData[1], rowData[0], pos);
                        data.Add(newRow);
                    }
                    else
                    {
                        Row newRow = new Row(num, diam, rowData[0] + rowData[1], rowData[0], pos);
                        data.Add(newRow);
                    }
                }
            }
            
            reader.Close();
            return data;
        }

        private static List<string> getFiles(string source, string ext)
        {
            List<string> files = new List<string>();

            if (!Directory.Exists(Path.GetDirectoryName(source))) return files;

            string[] importFiles = Directory.GetFiles(source, ext);

            foreach (string file in importFiles)
            {
                files.Add(file);
            }

            return files;
        }

        private static void createScriptFile(List<string> dwgs, string script, string netload)
        {
            StringBuilder txt = new StringBuilder();

            txt.AppendLine("NETLOAD \"" + netload + "\"");

            foreach (string dwg in dwgs)
            {
                txt.AppendLine("_.open \"" + dwg + "\"");
                txt.AppendLine("CSV_SUM_MARKS");
            }

            //txt.AppendLine("_.quit");

            string scriptText = txt.ToString();

            if (File.Exists(script))
            {
                File.Delete(script);
            }

            File.AppendAllText(script, scriptText);
        }

        private static void runAutocad(string program, string script)
        {
            string excet = program;
            string args = "/b \"" + script + "\"";
            Process.Start(excet, args);
        }

        private static void deleteScriptFile(string script)
        {
            Console.WriteLine("Waiting for BricsCad to close...");
            System.Threading.Thread.Sleep(5000);

            while (IsProcessOpen("bricscad"))
            {
                System.Threading.Thread.Sleep(1000);
            }
            try
            {
                File.Delete(script);
            }
            catch
            {

            }
        }

        private static void dump(string location, List<Row> data)
        {
            string fileName = "kokku";
            string extention = ".csv";

            int i = 1;

            string destination = location + fileName + extention;

            while (File.Exists(destination))
            {
                destination = location + fileName + i.ToString("D3") + extention;
                i++;
            }

            if (data == null || data.Count == 0) return;

            if (!Directory.Exists(Path.GetDirectoryName(destination))) return;

            StringBuilder txt = new StringBuilder();

            txt.AppendLine("alexi programmi ajutine file");
            txt.AppendLine("shape;positsioon; kogus; diameter");
            txt.AppendLine("");

            foreach (Row row in data)
            {
                txt.AppendLine(row.Position_Shape.ToString() + ";" + row.Position_Nr.ToString() + ";" + row.Number.ToString() + ";" + row.Diameter.ToString());
            }

            string csvText = txt.ToString();

            File.AppendAllText(destination, csvText);
        }
    }
}
