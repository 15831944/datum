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
    class XML_Filter_command
    {
        _CONNECTION _c;

        static string name = "alfa";

        string dwg_dir;
        string xml_full;
        string xml_lock_full;


        public XML_Filter_command(ref _CONNECTION c)
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


            string userFilter = promptFilter();
            if (userFilter == null)
            {
                return;
            }

            string userDiameter = promptDiameter();
            if (userDiameter == null || userDiameter == "")
            {
                userDiameter = "???";
            }
            
            File.Create(xml_lock_full).Dispose();
            write("[XML] LOCK ON");

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xml_full);

            List<XmlNode> rows = XML_Handle.getAllRebar(xmlDoc);

            List<XmlNode> filteredRows = XML_Handle.filter(rows, userFilter);
            List<XmlNode> similar = findSimilar(userFilter, userDiameter, filteredRows, xmlDoc);
            printSimilar(similar);
        }


        private string promptFilter()
        {
            _Ed.PromptKeywordOptions promptOptions = new _Ed.PromptKeywordOptions("");
            promptOptions.Message = "\nWhat to search: ";
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

            promptOptions.AllowNone = false;
            _Ed.PromptResult promptResult = _c.ed.GetKeywords(promptOptions);

            if (promptResult.Status == _Ed.PromptStatus.OK)
            {
                return promptResult.StringResult;
            }

            return null;
        }


        private string promptDiameter()
        {
            string userDiameter = "";

            _Ed.PromptIntegerOptions promptOptions = new _Ed.PromptIntegerOptions("Diameter:");

            promptOptions.AllowNone = true;
            _Ed.PromptIntegerResult promptResult = _c.ed.GetInteger(promptOptions);

            if (promptResult.Status == _Ed.PromptStatus.OK)
            {
                userDiameter = promptResult.Value.ToString();
            }

            return userDiameter;
        }


        private List<XmlNode> findSimilar(string filter, string diam, List<XmlNode> rebars, XmlDocument xmlDoc)
        {
            List<XmlNode> similar = new List<XmlNode>();

            _Mark u = new _Mark(0, 10, "", filter, 0);
            XmlNode newNode = XML_Handle.newNodeHandle(u, "", xmlDoc, _c.ed);
            
            newNode["B2aBar"]["Dim"].InnerText = diam;

            similar = XML_Handle.findSimilar(newNode, rebars);

            return similar;
        }


        private void printSimilar(List<XmlNode> rebars)
        {
            foreach (XmlNode row in rebars)
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

            write("[NB! Sümeetrilisust kontrollitakse B, C, D, F, N - tüüpi raudadel]");
        }


        private void write(string message)
        {
            _c.ed.WriteMessage("\n" + message);
        }

    }
}