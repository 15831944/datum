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
    class XML_Add_command
    {
        _CONNECTION _c;

        static string name = "alfa";

        string dwg_dir;
        string xml_full;
        string xml_lock_full;
        string xml_output_full;


        public XML_Add_command(ref _CONNECTION c)
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

            List<_Mark> marks = getSelectedMarks();
            List<_Mark> filteredMarks = filterSelectedMarks(marks);

            List<XmlNode> pages = XML_Handle.getAllPages(xmlDoc);
            List<XmlNode> rebars = XML_Handle.getAllRebar(xmlDoc);

            Dictionary<_Mark, XmlNode> warning = new Dictionary<_Mark, XmlNode>();
            List<_Mark> undefined = findMarksInXML(rebars, filteredMarks, ref warning);

            if (undefined.Count != 0)
            {
                List<XmlNode> newRebars = handleUndefined(undefined, rebars, xmlDoc);
                rebars.AddRange(newRebars);
                filtreeri(pages, rebars, xmlDoc);
                xmlDoc.Save(xml_output_full);
            }

            foreach (_Mark m in warning.Keys)
            {
                write("--- WARINING: " + m.ToString());
                string rebarString = XML_Handle.getXMLRebarString(warning[m]);
                write("--- WARINING: " + rebarString);
                write("");
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


        private List<XmlNode> handleUndefined(List<_Mark> undefined, List<XmlNode> rebars, XmlDocument xmlDoc)
        {
            List<XmlNode> newRebar = new List<XmlNode>();

            write(" ");
            foreach (_Mark u in undefined)
            {
                write("--- Not found: " + u.ToString());
            }

            string materjal = promptGetMaterial();

            foreach (_Mark u in undefined)
            {
                bool add = promptAddRebarToXml(u);
                if (add)
                {
                    XmlNode newNode = XML_Handle.newNodeHandle(u, materjal, xmlDoc, _c.ed);
                    XmlNode dublicate = checkIfRebarExists(newNode, rebars);
                    if (dublicate == null)
                    {
                        newRebar.Add(newNode);
                    }
                    else
                    {
                        write("Sama kujuga raud on juba olemas!");
                        XmlNode rebar = dublicate["B2aBar"];
                        string rebarString = XML_Handle.getXMLRebarString(rebar);
                        write(rebarString);
                    }
                }
                else
                {
                    write("Skip: " + u.ToString());
                }
            }

            return newRebar;
        }


        private XmlNode checkIfRebarExists(XmlNode newNode, List<XmlNode> rebars)
        {
            string filter = newNode["B2aBar"]["Type"].InnerText;
            List<XmlNode> filtered = XML_Handle.filter(rebars, filter);

            foreach (XmlNode rebar in filtered)
            {
                XmlNode dublicate = XML_Handle.compare(newNode, rebar);
                if (dublicate != null) return dublicate;
            }

            return null;
        }


        private bool promptAddRebarToXml(_Mark u)
        {
            _Ed.PromptKeywordOptions promptOptions = new _Ed.PromptKeywordOptions("");
            promptOptions.Message = "\nAdd to XML: " + u.ToString();
            promptOptions.Keywords.Add("Yes");
            promptOptions.Keywords.Add("No");
            promptOptions.AllowNone = false;
            _Ed.PromptResult promptResult = _c.ed.GetKeywords(promptOptions);

            if (promptResult.Status == _Ed.PromptStatus.OK)
            {
                if (promptResult.StringResult == "Yes")
                {
                    return true;
                }
            }

            return false;
        }


        private string promptGetMaterial()
        {
            string materjal = "K500C-T";

            _Ed.PromptStringOptions promptOptions2 = new _Ed.PromptStringOptions("");
            promptOptions2.Message = "\nArmatuuri teras: ";
            promptOptions2.DefaultValue = "K500C-T";
            _Ed.PromptResult promptResult2 = _c.ed.GetString(promptOptions2);

            if (promptResult2.Status == _Ed.PromptStatus.OK)
            {
                materjal = promptResult2.StringResult;
            }

            return materjal;
        }


        private List<_Mark> getSelectedMarks()
        {
            List<_Mark> parse = new List<_Mark>();

            List<_Db.MText> selected = getSelectedText();

            foreach (_Db.MText txt in selected)
            {
                _Mark current = new _Mark(txt.Contents, txt.Location, txt.Layer.ToString());
                bool valid = current.validate();

                if (valid) parse.Add(current);
                else write("[WARNING] - VALE VIIDE - " + txt.Contents);
            }

            return parse;
        }


        private List<_Mark> filterSelectedMarks(List<_Mark> marks)
        {
            List<_Mark> unique = new List<_Mark>();

            foreach (_Mark m in marks)
            {
                if (unique.Contains(m))
                {
                    continue;
                }
                else
                {
                    unique.Add(m);
                }
            }

            unique = unique.OrderBy(b => b.Diameter).ToList();

            List<_Mark> rows_num = unique.FindAll(x => x.Position_Shape == "A").ToList();
            List<_Mark> rows_char = unique.FindAll(x => x.Position_Shape != "A").ToList();

            rows_num = rows_num.OrderBy(b => b.Position_Nr).ToList();
            rows_char = rows_char.OrderBy(b => b.Position_Nr).ToList();

            unique = new List<_Mark>();
            unique.AddRange(rows_num);
            unique.AddRange(rows_char);

            return unique;
        }


        private List<_Db.MText> getSelectedText()
        {
            List<_Db.MText> txt = new List<_Db.MText>();

            _Ed.PromptSelectionResult selection = _c.ed.GetSelection();
            if (selection.Status == _Ed.PromptStatus.OK)
            {
                _Db.ObjectId[] objIds = selection.Value.GetObjectIds();

                foreach (_Db.ObjectId objId in objIds)
                {
                    _Db.Entity currentEntity = _c.trans.GetObject(objId, _Db.OpenMode.ForRead) as _Db.Entity;

                    if (currentEntity == null) continue;

                    if (currentEntity is _Db.MText)
                    {
                        _Db.MText br = currentEntity as _Db.MText;
                        txt.Add(br);
                    }
                    else if (currentEntity is _Db.DBText)
                    {
                        _Db.DBText br = currentEntity as _Db.DBText;
                        _Db.MText myMtext = new _Db.MText();

                        myMtext.Contents = br.TextString;
                        myMtext.Layer = br.Layer;
                        myMtext.Location = br.Position;
                        txt.Add(myMtext);
                    }
                    else if (currentEntity is _Db.MLeader)
                    {
                        _Db.MLeader br = currentEntity as _Db.MLeader;

                        if (br.ContentType == _Db.ContentType.MTextContent)
                        {
                            _Db.MText leaderText = br.MText;
                            leaderText.Layer = br.Layer;
                            txt.Add(leaderText);
                        }
                    }                    
                }
        
            }

            return txt;
        }


        private List<_Mark> findMarksInXML(List<XmlNode> rebars, List<_Mark> marks, ref Dictionary<_Mark, XmlNode> warning)
        {
            List<_Mark> undefined = new List<_Mark>();

            foreach (_Mark m in marks)
            {
                bool found = matchMarkToXML(m, rebars, ref warning);
                if (found == false) { undefined.Add(m); }
            }

            return undefined;
        }


        private bool matchMarkToXML(_Mark m, List<XmlNode> rows, ref Dictionary<_Mark, XmlNode> warning)
        {
            foreach (XmlNode row in rows)
            {
                XmlNode rebar = row["B2aBar"];

                if (rebar == null) return false;

                string type = XML_Handle.emptyNodehandle(rebar, "Type");
                string pos_nr = XML_Handle.emptyNodehandle(rebar, "Litt");
                string diam = XML_Handle.emptyNodehandle(rebar, "Dim");

                if (m.Position_Shape == "A")
                {
                    if (m.Position_Shape == type && m.Position_Nr.ToString() == pos_nr && m.Diameter.ToString() == diam)
                    {
                        write("Found in XML: " + m.ToString());
                        string rebarString = XML_Handle.getXMLRebarString(rebar);
                        write(rebarString);
                        write("");
                        return true;
                    }
                }
                else
                {
                    if (m.Position_Nr.ToString() == pos_nr)
                    {
                        if (m.Position_Shape.ToString() != type)
                        {
                            warning[m] = rebar;
                        }
                        else if (m.Diameter.ToString() != diam)
                        {
                            warning[m] = rebar;
                        }

                        write("Found in XML: " + m.ToString());
                        string rebarString = XML_Handle.getXMLRebarString(rebar);
                        write(rebarString);
                        write("");
                        return true;
                    }
                }
            }

            return false;
        }


        private void write(string message)
        {
            _c.ed.WriteMessage("\n" + message);
        }

    }
}