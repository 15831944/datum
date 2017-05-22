using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Xml;

//ODA
using Teigha.Runtime;
using Teigha.DatabaseServices;
using Teigha.Geometry;

//Bricsys
using Bricscad.ApplicationServices;
using Bricscad.Runtime;
using Bricscad.EditorInput;

namespace commands
{
    class XML_PrintKnownRebar_command
    {
        string dwg_dir;

        string xml_full;
        string xml_lock_full;

        static string name = "alfa";

        Document doc;
        Database db;
        Editor ed;

        public XML_PrintKnownRebar_command()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            db = doc.Database;
            ed = doc.Editor;

            HostApplicationServices hs = HostApplicationServices.Current;
            string dwg_path = hs.FindFile(doc.Name, doc.Database, FindFileHint.Default);

            dwg_dir = Path.GetDirectoryName(dwg_path);
            if (!dwg_dir.EndsWith(@"\")) { dwg_dir = dwg_dir + @"\"; }

            xml_full = dwg_dir + name + ".xml";
            xml_lock_full = dwg_dir + name + ".LCK";
        }

        public void run()
        {
            if (!File.Exists(xml_full))
            {
                writeCadMessage("[ERROR] Joonise kaustas ei ole XML faili nimega: " + name + ".xml");
                return;
            }

            if (File.Exists(xml_lock_full))
            {
                writeCadMessage("[ERROR] XML fail nimega: " + name + ".xml" + " on lukkus!");
                return;
            }

            File.Create(xml_lock_full).Dispose();
            writeCadMessage("LOCK ON");

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xml_full);

            List<XmlNode> rows = XML_Handle.getAllRebar(xmlDoc);

            string userFilter = promptFilter();
            List<XmlNode> filteredRows = filter(rows, userFilter);

            foreach (XmlNode row in filteredRows)
            {
                XmlNode rebar = row["B2aBar"];
                if (rebar == null)
                {
                    writeCadMessage("error reading");
                    continue;
                }

                string rebarString = XML_Handle.getXMLRebarString(rebar);
                writeCadMessage(rebarString);
            }

            File.Delete(xml_lock_full);
            writeCadMessage("LOCK OFF");
        }

        private void writeCadMessage(string errorMessage)
        {
            ed.WriteMessage("\n" + errorMessage);
        }

        private List<XmlNode> filter(List<XmlNode> rows, string userFilter)
        {
            writeCadMessage(userFilter);
            List<XmlNode> filtered = new List<XmlNode>();
            if (userFilter == "ALL")
            {
                filtered = rows;
            }
            else if (userFilter == "SPEC")
            {                
                foreach (XmlNode row in rows)
                {
                    XmlNode temp_reb = row["B2aBar"];
                    if (temp_reb == null)
                    {
                        filtered.Add(row);
                        continue;
                    }

                    XmlNode temp_type = temp_reb["Type"];

                    if (temp_type == null)
                    {
                        filtered.Add(row);
                        continue;
                    }

                    if (temp_type.InnerText == "A")
                    {
                        continue;
                    }

                    filtered.Add(row);
                }
            }
            else if (userFilter == "LAST")
            {
                foreach (XmlNode row in rows)
                {
                    XmlNode temp_reb = row["B2aBar"];
                    if (temp_reb == null)
                    {
                        filtered.Add(row);
                        continue;
                    }

                    XmlNode temp_type = temp_reb["Type"];

                    if (temp_type == null)
                    {
                        filtered.Add(row);
                        continue;
                    }

                    if (temp_type.InnerText == "A")
                    {
                        continue;
                    }

                    filtered.Add(row);
                }

                XmlNode last = filtered[filtered.Count - 1];
                filtered = new List<XmlNode>();
                filtered.Add(last);
            }
            else
            {
                foreach (XmlNode row in rows)
                {
                    XmlNode temp_reb = row["B2aBar"];
                    if (temp_reb == null)
                    {
                        continue;
                    }

                    XmlNode temp_type = temp_reb["Type"];

                    if (temp_type == null)
                    {
                        continue;
                    }

                    if (temp_type.InnerText == userFilter)
                    {
                        filtered.Add(row);
                    }
                }
            }

            return filtered;
        }

        private string promptFilter()
        {
            PromptKeywordOptions promptOptions = new PromptKeywordOptions("");
            promptOptions.Message = "\nWhat to print: ";
            promptOptions.Keywords.Add("A");
            promptOptions.Keywords.Add("B");
            promptOptions.Keywords.Add("C");
            promptOptions.Keywords.Add("D");
            promptOptions.Keywords.Add("E");
            promptOptions.Keywords.Add("EX");

            promptOptions.Keywords.Add("F");
            promptOptions.Keywords.Add("G");
            promptOptions.Keywords.Add("H");
            promptOptions.Keywords.Add("J");
            promptOptions.Keywords.Add("K");
            promptOptions.Keywords.Add("L");

            promptOptions.Keywords.Add("LX");
            promptOptions.Keywords.Add("M");
            promptOptions.Keywords.Add("N");
            promptOptions.Keywords.Add("NX");
            promptOptions.Keywords.Add("O");
            promptOptions.Keywords.Add("Q");

            promptOptions.Keywords.Add("R");
            promptOptions.Keywords.Add("S");
            promptOptions.Keywords.Add("SH");
            promptOptions.Keywords.Add("SX");
            promptOptions.Keywords.Add("T");
            promptOptions.Keywords.Add("U");

            promptOptions.Keywords.Add("V");
            promptOptions.Keywords.Add("W");
            promptOptions.Keywords.Add("X");
            promptOptions.Keywords.Add("XX");
            promptOptions.Keywords.Add("Z");

            promptOptions.Keywords.Add("ALL");
            promptOptions.Keywords.Add("SPEC");
            promptOptions.Keywords.Add("LAST");
            promptOptions.AllowNone = true;
            PromptResult promptResult = ed.GetKeywords(promptOptions);

            if (promptResult.Status == PromptStatus.OK)
            {
                if (promptResult.StringResult == "") return "SPEC";
                else
                {
                    return promptResult.StringResult;
                }
            }

            return "SPEC";
        }
    }
}