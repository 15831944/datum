using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

////Autocad
//using Autodesk.AutoCAD.Runtime;
//using Autodesk.AutoCAD.ApplicationServices;
//using Autodesk.AutoCAD.DatabaseServices;
//using Autodesk.AutoCAD.Geometry;
//using Autodesk.AutoCAD.EditorInput;
//using Autodesk.AutoCAD.PlottingServices;

//Bricsys
using Teigha.Runtime;
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Bricscad.ApplicationServices;
using Bricscad.Runtime;
using Bricscad.EditorInput;
using Bricscad.PlottingServices;


namespace commands
{
    class XML_PrintKnownRebar_command
    {
        Document doc;
        Database db;
        Editor ed;

        static string name = "alfa";

        string dwg_dir;
        string xml_full;
        string xml_lock_full;


        public XML_PrintKnownRebar_command()
        {
            doc = Application.DocumentManager.MdiActiveDocument;
            db = doc.Database;
            ed = doc.Editor;

            HostApplicationServices hs = HostApplicationServices.Current;
            string dwg_path = hs.FindFile(doc.Name, doc.Database, FindFileHint.Default);

            dwg_dir = Path.GetDirectoryName(dwg_path);
            if (!dwg_dir.EndsWith(@"\")) { dwg_dir = dwg_dir + @"\"; }

            xml_full = dwg_dir + name + ".xml";
            xml_lock_full = dwg_dir + name + ".LCK";
        }
        

        public void unlock_after_crash()
        {
            if (File.Exists(xml_lock_full))
            {
                File.Delete(xml_lock_full);
            }
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
            List<XmlNode> filteredRows = XML_Handle.filter(rows, userFilter);

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