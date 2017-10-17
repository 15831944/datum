using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Xml;
using System.Diagnostics;

namespace xml_armering_gen
{
    class Program
    {
        static bool kodu = false; //THIS

        static void Main(string[] args)
        {
            string script = @"xml_gen_script.scr";

            string program = "";
            if (kodu) program = @"C:\Program Files\Autodesk\AutoCAD 2013\acad.exe";
            else program = @"C:\Program Files\Bricsys\BricsCAD V16 en_US\bricscad.exe";            

            string netload = @"C:\Users\Alex\Documents\GitHub\datum\HillsCommands\bin\Debug\HillsCommands.dll";

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

            Console.WriteLine(parsed.Count.ToString());

            dump(location, parsed);
            xml_dump(location, parsed);

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

            txt.AppendLine("_.quit");

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


        private static void xml_dump(string location, List<Row> data)
        {
            string name = "alfa";
            string output_name = "bravo";

            string xml_full = location + name + ".xml";
            string xml_lock_full = location + name + ".LCK";
            string xml_output_full = location + output_name + ".xml";

            if (!File.Exists(xml_full))
            {
                Console.WriteLine("[ERROR] Antud kaustas ei ole XML faili nimega: " + name + ".xml");
                return;
            }

            if (File.Exists(xml_lock_full))
            {
                Console.WriteLine("[ERROR] XML fail nimega: " + name + ".xml" + " on lukkus!");
                return;
            }

            if (File.Exists(xml_output_full))
            {
                Console.WriteLine("[ERROR] XML fail nimega " + output_name + ".xml" + " on juba olemas");
                return;
            }

            File.Create(xml_lock_full).Dispose();
            Console.WriteLine("LOCK ON");

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xml_full);
            xmlDoc = removeEmptyNodes(xmlDoc);
            List<XmlNode> rebarNodes = getAllRebar(xmlDoc);

            setNumber(rebarNodes, data);
            List<XmlNode> foundNodes = removeNotFoundNodes(rebarNodes);
            sortNodes(foundNodes, xmlDoc);

            xmlDoc.Save(xml_output_full);

            File.Delete(xml_lock_full);
            Console.WriteLine("LOCK OFF");
        }

        private static XmlDocument removeEmptyNodes(XmlDocument xd) // DONT KNOW THIS MAGIC
        {
            XmlNodeList emptyElements = xd.SelectNodes(@"//*[not(node())]");
            for (int i = emptyElements.Count - 1; i > -1; i--)
            {
                XmlNode nodeToBeRemoved = emptyElements[i];
                nodeToBeRemoved.ParentNode.RemoveChild(nodeToBeRemoved);
            }

            return xd;
        }


        private static List<XmlNode> getAllRebar(XmlDocument file)
        {
            List<XmlNode> rebars = new List<XmlNode>();

            foreach (XmlNode page in file.DocumentElement)
            {
                if (page.Name != "B2aPage") continue;

                foreach (XmlNode row in page.ChildNodes)
                {
                    if (row.Name != "B2aPageRow") continue;

                    rebars.Add(row);
                }
            }

            return rebars;
        }


        private static List<XmlNode> getAllPages(XmlDocument file)
        {
            List<XmlNode> pages = new List<XmlNode>();

            foreach (XmlNode page in file.DocumentElement)
            {
                if (page.Name != "B2aPage") continue;

                pages.Add(page);
            }

            return pages;
        }


        private static void setNumber(List<XmlNode> rebarNodes, List<Row> data)
        {
            List<XmlNode> notFoundNode = new List<XmlNode>();
            List<Row> foundRebar = new List<Row>();

            foreach (XmlNode rebarNode in rebarNodes)
            {
                bool found = false;

                XmlNode group = rebarNode["NoStpGrp"];
                XmlNode rebar = rebarNode["B2aBar"];

                if (rebar != null && group != null)
                {
                    XmlNode type = rebar["Type"];
                    XmlNode pos_nr = rebar["Litt"];
                    XmlNode diam = rebar["Dim"];

                    group.InnerText = "9999";

                    if (type != null && pos_nr != null && diam != null)
                    {
                        string t = type.InnerText;
                        string p = pos_nr.InnerText;
                        string d = diam.InnerText;
                        
                        foreach (Row reb in data)
                        {
                            if (reb.Position_Shape == t && reb.Position_Nr.ToString() == p && reb.Diameter.ToString() == d)
                            {
                                found = true;
                                group.InnerText = reb.Number.ToString();
                                foundRebar.Add(reb);
                                data.Remove(reb);
                                break;
                            }
                        }


                    }
                }

                if (found == false)
                {
                    notFoundNode.Add(rebarNode);
                }
            }

            Console.WriteLine("");
            Console.WriteLine(notFoundNode.Count.ToString() + " - rauda ei ole joonistel kasutuses");
            Console.WriteLine(data.Count.ToString() + " - rauda ei ole XML-is defineeritud");

            Console.WriteLine("");
            Console.WriteLine("Ei ole defineeritud:");
            foreach (Row reb in data)
            {
                Console.WriteLine(reb.Position_Shape + " " + reb.Position_Nr.ToString() + " " + reb.Diameter.ToString() );
            }

        }


        private static List<XmlNode> removeNotFoundNodes(List<XmlNode> all)
        {
            List<XmlNode> found = new List<XmlNode>();

            foreach (XmlNode single in all)
            {
                XmlNode group = single["NoStpGrp"];
                XmlNode rebar = single["B2aBar"];

                if (rebar != null && group != null)
                {
                    if (group.InnerText != "9999")
                    {
                        found.Add(single);
                    }
                }
            }

            return found;
        }


        private static void sortNodes(List<XmlNode> rebars, XmlDocument xmlDoc)
        {
            List<XmlNode> sortedRebars = sortRebars(rebars);

            List<XmlNode> pa = getAllPages(xmlDoc);
            foreach (XmlNode p in pa)
            {
                xmlDoc.DocumentElement.RemoveChild(p);
            }

            List<XmlNode> pages = new List<XmlNode>();

            int rebarTotIndex = 0;
            int rebarPageIndex = 1;
            int pageIndex = 0;

            XmlNode lastReb = null;

            XmlNode page = newPageHandle(pages, xmlDoc);
            pages.Add(page);
            xmlDoc.DocumentElement.AppendChild(page);

            while (true)
            {
                if (pageIndex < pages.Count)
                {
                    page = pages[pageIndex];
                }
                else
                {
                    page = newPageHandle(pages, xmlDoc);
                    pages.Add(page);
                    xmlDoc.DocumentElement.AppendChild(page);
                }

                while (true)
                {
                    XmlNode reb = sortedRebars[rebarTotIndex];

                    if (lastReb != null)
                    {
                        if (reb != null)
                        {
                            XmlNode aa = reb["B2aBar"];
                            if (aa != null)
                            {
                                XmlNode ab = aa["Type"];

                                if (ab != null)
                                {
                                    XmlNode ba = lastReb["B2aBar"];

                                    if (ba != null)
                                    {
                                        XmlNode bb = ba["Type"];

                                        if (bb != null)
                                        {
                                            if (ab.InnerText == "A" && bb.InnerText != "A")
                                            {
                                                lastReb = null;
                                                break;
                                            }
                                        }
                                    }
                                }

                            }
                        }
                    }

                    reb.Attributes["RowId"].Value = rebarPageIndex.ToString();
                    page.AppendChild(reb);
                    lastReb = reb;

                    rebarTotIndex++;
                    rebarPageIndex++;

                    if (rebarTotIndex >= rebars.Count) break;
                    if (rebarPageIndex > 20) break;
                }

                if (rebarTotIndex >= rebars.Count) break;

                rebarPageIndex = 1;
                pageIndex++;
            }
        }

        public static XmlNode newPageHandle(List<XmlNode> pages, XmlDocument xmlDoc)
        {
            XmlNode row = xmlDoc.CreateElement("B2aPage");
            XmlAttribute attribute = xmlDoc.CreateAttribute("PageLabel");
            attribute.Value = "A-" + (pages.Count + 1).ToString("D2");
            row.Attributes.Append(attribute);

            XmlNode head = xmlDoc.CreateElement("B2aPageHead");

            XmlNode c10 = xmlDoc.CreateElement("ConcreteFctk10");
            XmlNode cf10 = xmlDoc.CreateElement("ConcreteCoverFactor10");

            head.AppendChild(c10);
            head.AppendChild(cf10);

            row.AppendChild(head);

            return row;
        }

        private static List<XmlNode> sortRebars(List<XmlNode> rebars)
        {
            List<XmlNode> a = new List<XmlNode>();
            List<XmlNode> others = new List<XmlNode>();
            List<XmlNode> undef = new List<XmlNode>();

            List<XmlNode> sortedRebars = new List<XmlNode>();
            sortedRebars.AddRange(others);
            sortedRebars.AddRange(a);

            foreach (XmlNode rebar in rebars)
            {
                XmlNode temp_reb = rebar["B2aBar"];

                if (temp_reb == null)
                {
                    undef.Add(rebar);
                    continue;
                }

                XmlNode temp_type = temp_reb["Type"];

                if (temp_type == null)
                {
                    undef.Add(rebar);
                    continue;
                }

                if (temp_type.InnerText == "A")
                {
                    XmlNode temp_litt = temp_reb["Litt"];
                    if (temp_litt == null)
                    {
                        undef.Add(rebar);
                        continue;
                    }
                    else
                    {
                        a.Add(rebar);
                    }
                }
                else
                {
                    XmlNode temp_litt = temp_reb["Litt"];
                    if (temp_litt == null)
                    {
                        undef.Add(rebar);
                        continue;
                    }
                    else
                    {
                        others.Add(rebar);
                    }
                }
            }

            try
            {
                a = a.OrderBy(b => Int32.Parse(b["B2aBar"]["Dim"].InnerText)).ToList();
            }
            catch
            {

            }

            try
            {
                a = a.OrderBy(b => Int32.Parse(b["B2aBar"]["Litt"].InnerText)).ToList();
            }
            catch
            {
                a = a.OrderBy(b => b["B2aBar"]["Litt"].InnerText).ToList();
            }

            try
            {
                others = others.OrderBy(b => Int32.Parse(b["B2aBar"]["Litt"].InnerText)).ToList();
            }
            catch
            {
                others = others.OrderBy(b => b["B2aBar"]["Litt"].InnerText).ToList();
            }

            List<XmlNode> sorted = new List<XmlNode>();
            sorted.AddRange(others);
            sorted.AddRange(a);
            sorted.AddRange(undef);

            return sorted;
        }
    }
}

