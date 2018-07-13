#define BRX_APP
//#define ARX_APP

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
    class XML_Sort_command
    {
        _CONNECTION _c;

        static string name = "alfa";

        string dwg_dir;
        string xml_full;
        string xml_lock_full;
        string xml_output_full;


        public XML_Sort_command(ref _CONNECTION c)
        {
            _c = c;

            _Db.HostApplicationServices hs = _Db.HostApplicationServices.Current;
            string dwg_path = hs.FindFile(_c.doc.Name, _c.doc.Database, _Db.FindFileHint.Default);

            dwg_dir = Path.GetDirectoryName(dwg_path);
            if (!dwg_dir.EndsWith(@"\")) { dwg_dir = dwg_dir + @"\"; }

            xml_full = dwg_dir + name + ".xml";
            xml_lock_full = dwg_dir + name + ".LCK";
            xml_output_full = dwg_dir + name + ".xml";
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
            xmlDoc = XML_Handle.removeEmptyNodes(xmlDoc);

            List<XmlNode> pages = XML_Handle.getAllPages(xmlDoc);
            List<XmlNode> rebars = XML_Handle.getAllRebar(xmlDoc);

            if (rebars.Count > 0)
            {
                filtreeri(pages, rebars, xmlDoc);
                xmlDoc.Save(xml_output_full);
            }
        }


        private void filtreeri(List<XmlNode> pages, List<XmlNode> rebars, XmlDocument xmlDoc)
        {
            List<XmlNode> sortedRebars = sortRebars(rebars);

            foreach (XmlNode page in pages)
            {
                XmlNodeList rows = page.ChildNodes;

                for (int k = rows.Count - 1; k > 0; k--)
                {
                    if (rows[k].Name != "B2aPageRow") continue;

                    page.RemoveChild(rows[k]);
                }
            }

            int rebarTotIndex = 0;
            int rebarPageIndex = 1;
            int pageIndex = 0;

            XmlNode lastReb = null;

            while (true)
            {
                XmlNode page;
                if (pageIndex < pages.Count)
                {
                    page = pages[pageIndex];
                }
                else
                {
                    page = XML_Handle.newPageHandle(pages, xmlDoc);
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


        private List<XmlNode> sortRebars(List<XmlNode> rebars)
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


        private void write(string message)
        {
            _c.ed.WriteMessage("\n" + message);
        }

    }
}
