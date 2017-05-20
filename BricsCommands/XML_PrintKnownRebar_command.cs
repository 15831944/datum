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
        string xml_output_full;

        static string name = "alfa";

        Document doc;
        Database db;
        Editor ed;

        public XML_PrintKnownRebar_command()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            HostApplicationServices hs = HostApplicationServices.Current;
            string dwg_path = hs.FindFile(doc.Name, doc.Database, FindFileHint.Default);

            dwg_dir = Path.GetDirectoryName(dwg_path);
            if (!dwg_dir.EndsWith(@"\")) { dwg_dir = dwg_dir + @"\"; }

            xml_full = dwg_dir + name + ".xml";
            xml_lock_full = dwg_dir + name + ".LCK";
            xml_output_full = dwg_dir + "gg" + ".xml";

            doc = Application.DocumentManager.MdiActiveDocument;
            db = doc.Database;
            ed = doc.Editor;
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

            //rebars = rebars.FindAll(x => x["Type"].InnerText != "A").ToList();

            foreach (XmlNode row in rows)
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
    }
}