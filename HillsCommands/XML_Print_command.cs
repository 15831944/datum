//#define BRX_APP
#define ARX_APP

using System;
using System.Text;
using System.Collections;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using _SWF = System.Windows.Forms;


#if BRX_APP
    using _Ap = Bricscad.ApplicationServices;
    //using _Br = Teigha.BoundaryRepresentation;
    using _Cm = Teigha.Colors;
    using _Db = Teigha.DatabaseServices;
    using _Ed = Bricscad.EditorInput;
    using _Ge = Teigha.Geometry;
    using _Gi = Teigha.GraphicsInterface;
    using _Gs = Teigha.GraphicsSystem;
    using _Gsk = Bricscad.GraphicsSystem;
    using _Pl = Bricscad.PlottingServices;
    using _Brx = Bricscad.Runtime;
    using _Trx = Teigha.Runtime;
    using _Wnd = Bricscad.Windows;
    //using _Int = Bricscad.Internal;
#elif ARX_APP
    using _Ap = Autodesk.AutoCAD.ApplicationServices;
    //using _Br = Autodesk.AutoCAD.BoundaryRepresentation;
    using _Cm = Autodesk.AutoCAD.Colors;
    using _Db = Autodesk.AutoCAD.DatabaseServices;
    using _Ed = Autodesk.AutoCAD.EditorInput;
    using _Ge = Autodesk.AutoCAD.Geometry;
    using _Gi = Autodesk.AutoCAD.GraphicsInterface;
    using _Gs = Autodesk.AutoCAD.GraphicsSystem;
    using _Pl = Autodesk.AutoCAD.PlottingServices;
    using _Brx = Autodesk.AutoCAD.Runtime;
    using _Trx = Autodesk.AutoCAD.Runtime;
    using _Wnd = Autodesk.AutoCAD.Windows;
#endif

using System.Xml;


namespace commands
{
    class XML_Print_command
    {
        _CONNECTION _c;

        static string name = "alfa";

        string dwg_dir;
        string xml_full;
        string xml_lock_full;


        public XML_Print_command(ref _CONNECTION c)
        {
            _c = c;

            _Db.HostApplicationServices hs = _Db.HostApplicationServices.Current;
            string dwg_path = hs.FindFile(_c.doc.Name, _c.doc.Database, _Db.FindFileHint.Default);

            dwg_dir = Path.GetDirectoryName(dwg_path);
            if (!dwg_dir.EndsWith(@"\")) { dwg_dir = dwg_dir + @"\"; }

            xml_full = dwg_dir + name + ".xml";
            xml_lock_full = dwg_dir + name + ".LCK";
        }


        public void unlock_after_crash(bool pre_locked)
        {
            if (pre_locked == true) return;

            if (File.Exists(xml_lock_full))
            {
                write("[XML] LOCK OFF");

                File.Delete(xml_lock_full);
            }
        }


        public void run()
        {
            if (!File.Exists(xml_full)) throw new DMTException("[ERROR] Joonise kaustas ei ole XML faili nimega: " + name + ".xml");
            if (File.Exists(xml_lock_full)) throw new DMTLockedException("[ERROR] XML fail nimega: " + name + ".xml" + " on lukkus!");

            File.Create(xml_lock_full).Dispose();
            write("[XML] LOCK ON");

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
                    write("error reading");
                    continue;
                }

                string rebarString = XML_Handle.getXMLRebarString(rebar);
                write(rebarString);
            }
          
        }


        private string promptFilter()
        {
            _Ed.PromptKeywordOptions promptOptions = new _Ed.PromptKeywordOptions("");
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

            _Ed.PromptResult promptResult = _c.ed.GetKeywords(promptOptions);

            if (promptResult.Status == _Ed.PromptStatus.OK)
            {
                if (promptResult.StringResult == "") return "LAST";
                else
                {
                    return promptResult.StringResult;
                }
            }

            return "LAST";
        }


        private void write(string message)
        {
            _c.ed.WriteMessage("\n" + message);
        }

    }
}