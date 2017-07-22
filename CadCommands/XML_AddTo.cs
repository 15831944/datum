using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Xml;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

namespace commands
{
    class XML_testing
    {
        string dwg_dir;

        string xml_full;
        string xml_lock_full;
        string xml_output_full;

        static string name = "alfa";

        Document doc;
        Database db;
        Editor ed;

        public XML_testing()
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
            xml_output_full = dwg_dir + name + ".xml";
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
            xmlDoc = XML_Handle.removeEmptyNodes(xmlDoc);

            List<Mark> marks = getSelectedMarks();
            List<Mark> filteredMarks = filterSelectedMarks(marks);

            List<XmlNode> pages = XML_Handle.getAllPages(xmlDoc);
            List<XmlNode> rebars = XML_Handle.getAllRebar(xmlDoc);

            Dictionary<Mark ,XmlNode> warning = new Dictionary<Mark, XmlNode>();
            List<Mark> undefined = findMarksInXML(rebars, filteredMarks, ref warning);

            if (undefined.Count != 0)
            {
                List<XmlNode> newRebars = handleUndefined(undefined, rebars, xmlDoc);
                rebars.AddRange(newRebars);
                filtreeri(pages, rebars, xmlDoc);
                xmlDoc.Save(xml_output_full);

            }

            foreach (Mark m in warning.Keys)
            {
                writeCadMessage("--- WARINING DIAMETER: " + m.ToString());
                string rebarString = XML_Handle.getXMLRebarString(warning[m]);
                writeCadMessage("--- WARINING DIAMETER: " + rebarString);
                writeCadMessage("");
            }

            File.Delete(xml_lock_full);
            writeCadMessage("LOCK OFF");
        }


        private void filtreeri (List<XmlNode> pages, List<XmlNode> rebars, XmlDocument xmlDoc)
        {
            List<XmlNode> sortedRebars= sortRebars(rebars);

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

            while(true)
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

                while(true)
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

        private List<XmlNode> handleUndefined(List<Mark> undefined, List<XmlNode> rebars, XmlDocument xmlDoc)
        {
            List<XmlNode> newRebar = new List<XmlNode>();

            writeCadMessage(" ");
            foreach (Mark u in undefined)
            {
                writeCadMessage("--- Not found: " + u.ToString());
            }

            string materjal = promptGetMaterial();

            foreach (Mark u in undefined)
            {
                bool add = promptAddRebarToXml(u);
                if (add)
                {
                    XmlNode newNode = XML_Handle.newNodeHandle(u, materjal, xmlDoc, ed);
                    XmlNode dublicate = checkIfRebarExists(newNode, rebars);
                    if (dublicate == null)
                    {
                        newRebar.Add(newNode);
                    }
                    else
                    {
                        writeCadMessage("Sama kujuga raud on juba olemas!");
                        XmlNode rebar = dublicate["B2aBar"];
                        string rebarString = XML_Handle.getXMLRebarString(rebar);
                        writeCadMessage(rebarString);
                    }
                }
                else
                {
                    writeCadMessage("Skip: " + u.ToString());
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


        private bool promptAddRebarToXml(Mark u)
        {
            PromptKeywordOptions promptOptions = new PromptKeywordOptions("");
            promptOptions.Message = "\nAdd to XML: " + u.ToString();
            promptOptions.Keywords.Add("Yes");
            promptOptions.Keywords.Add("No");
            promptOptions.AllowNone = false;
            PromptResult promptResult = ed.GetKeywords(promptOptions);

            if (promptResult.Status == PromptStatus.OK)
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

            PromptStringOptions promptOptions2 = new PromptStringOptions("");
            promptOptions2.Message = "\nArmatuuri teras: ";
            promptOptions2.DefaultValue = "K500C-T";
            PromptResult promptResult2 = ed.GetString(promptOptions2);

            if (promptResult2.Status == PromptStatus.OK)
            {
                    materjal = promptResult2.StringResult;
            }

            return materjal;
        }


        private List<Mark> getSelectedMarks()
        {
            List<Mark> parse = new List<Mark>();

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                List<MText> selected = getSelectedText(doc, trans);

                foreach (MText txt in selected)
                {
                    Mark current = new Mark(txt.Contents, txt.Location);
                    bool valid = current.validate();
                    if (valid) parse.Add(current);
                }
            }

            return parse;
        }


        private List<Mark> filterSelectedMarks(List<Mark> marks)
        {
            List<Mark> unique = new List<Mark>();

            foreach (Mark m in marks)
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

            List<Mark> rows_num = unique.FindAll(x => x.Position_Shape == "A").ToList();
            List<Mark> rows_char = unique.FindAll(x => x.Position_Shape != "A").ToList();

            rows_num = rows_num.OrderBy(b => b.Position_Nr).ToList();
            rows_char = rows_char.OrderBy(b => b.Position_Nr).ToList();

            unique = new List<Mark>();
            unique.AddRange(rows_num);
            unique.AddRange(rows_char);

            return unique;
        }


        private List<MText> getSelectedText(Document doc, Transaction trans)
        {
            List<MText> txt = new List<MText>();

            PromptSelectionResult selection = ed.GetSelection();
            if (selection.Status == PromptStatus.OK)
            {
                ObjectId[] objIds = selection.Value.GetObjectIds();

                foreach (ObjectId objId in objIds)
                {
                    Entity currentEntity = trans.GetObject(objId, OpenMode.ForRead) as Entity;

                    if (currentEntity != null)
                    {
                        if (currentEntity is MText)
                        {
                            MText br = currentEntity as MText;
                            txt.Add(br);
                        }

                        if (currentEntity is DBText)
                        {
                            DBText br = currentEntity as DBText;
                            MText myMtext = new MText();
                            myMtext.Contents = br.TextString;
                            myMtext.Location = br.Position;
                            txt.Add(myMtext);
                        }

                        if (currentEntity is MLeader)
                        {
                            MLeader br = currentEntity as MLeader;

                            if (br.ContentType == ContentType.MTextContent)
                            {
                                MText leaderText = br.MText;
                                txt.Add(leaderText);
                            }
                        }
                    }
                }
            }

            return txt;
        }


        private List<Mark> findMarksInXML(List<XmlNode> rebars, List<Mark> marks, ref Dictionary<Mark, XmlNode> warning)
        {
            List<Mark> undefined = new List<Mark>();

            foreach (Mark m in marks)
            {
                bool found = matchMarkToXML(m, rebars, ref warning);
                if (found == false) { undefined.Add(m); }
            }

            return undefined;
        }


        private bool matchMarkToXML(Mark m, List<XmlNode> rows, ref Dictionary<Mark, XmlNode> warning)
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
                        writeCadMessage("Found in XML: " + m.ToString());
                        string rebarString = XML_Handle.getXMLRebarString(rebar);
                        writeCadMessage(rebarString);
                        writeCadMessage("");
                        return true;
                    }
                }
                else
                {
                    if (m.Position_Shape == type && m.Position_Nr.ToString() == pos_nr)
                    {
                        if (m.Diameter.ToString() != diam)
                        {
                            warning[m] = rebar;
                        }

                        writeCadMessage("Found in XML: " + m.ToString());
                        string rebarString = XML_Handle.getXMLRebarString(rebar);
                        writeCadMessage(rebarString);
                        writeCadMessage("");
                        return true;
                    }
                }
            }

            return false;
        }

        private void writeCadMessage(string errorMessage)
        {
            ed.WriteMessage("\n" + errorMessage);
        }
    }
}