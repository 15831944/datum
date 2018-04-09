using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Diagnostics;

namespace summary_excel
{
    class Program
    {
        static bool kodu = false; //THIS

        static void Main(string[] args)
        {
            string script = @"summary_excel_script.scr";

            string program = "";
            if (kodu) program = @"C:\Program Files\Autodesk\AutoCAD 2013\acad.exe";
            else program = @"C:\Program Files\Bricsys\BricsCAD V16 en_US\bricscad.exe";

            string netload = "";
            //if (kodu) netload = @"C:\Users\Alex\Documents\GitHub\datum\HillsCommands\bin\Release\HillsCommands.dll";
            //else netload = @"C:\Users\aleksandr.ess\Documents\GitHub\datum\HillsCommands\bin\Release\HillsCommands.dll";


            Console.WriteLine("Enter source folder:");
            string location = Console.ReadLine();
            if (!location.EndsWith(@"\")) { location = location + @"\"; }
            string csv_location = location + @"temp_excel\";

            List<string> dwgs = getFiles(location, "*.DWG");

            createScriptFile(dwgs, script, netload);
            runAutocad(program, script);
            deleteScriptFile(script);

            List<string> csvs = getFiles(csv_location, "*.CSV");
            List<string> parsed = parseCSVs(csvs);

            dump(location, parsed);

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

            string procc = "";
            if (kodu) procc = "acad";
            else procc = "bricscad";

            while (IsProcessOpen(procc))
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

            //txt.AppendLine("NETLOAD \"" + netload + "\"");

            foreach (string dwg in dwgs)
            {
                txt.AppendLine("_.open \"" + dwg + "\"");
                txt.AppendLine("CSV_SUM_EXCEL");
            }

            txt.AppendLine("_.quit");

            string scriptText = txt.ToString();

            if (File.Exists(script))
            {
                File.Delete(script);
            }

            File.AppendAllText(script, scriptText);
        }


        private static List<string> parseCSVs(List<string> csvs)
        {
            List<string> file_rows = new List<string>();

            StreamReader reader = null;

            foreach (string csv in csvs)
            {
                reader = new StreamReader(File.OpenRead(csv));
                readCSV(reader, ref file_rows);
            }            

            return file_rows;
        }


        private static void readCSV(StreamReader reader, ref List<string> file_rows)
        {

            bool append = false;

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                
                if (line == null) continue;
                if (line == "") continue;
                if (line == "!---SUMMARY") break;
                
                if (line == "SUMMARY")
                {
                    append = true;
                    continue;
                }

                if (append) file_rows.Add(line);


            }

            reader.Close();
        }


        private static void dump(string location, List<string> data)
        {
            string fileName = "sum_excel";
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

            txt.AppendLine("alexi; programmi; ajutine; file");
            txt.AppendLine("");
            txt.AppendLine("RITN_NR_23; DATUM_23; REV_23; REV_DATE_23; ELEMENT; LENGTH; HEIGHT; WIDTH; ; RITN_NR_27; DATUM_27; REV_27; REV_DATE_27; ELEMENT; ; SUMMA_NATARMERING; SUMMA_OVRIG_ARMERING");

            foreach (string row in data)
            {
                txt.AppendLine(row);
            }

            string csvText = txt.ToString();

            File.AppendAllText(destination, csvText);

            System.Threading.Thread.Sleep(100);

            try
            {
                Process.Start(destination);
            }
            catch 
            {

            }
        }
    }
}