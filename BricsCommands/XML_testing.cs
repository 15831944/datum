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
            writeCadMessage("start");

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

            List<Mark> undefined = findMarksInXML(rebars, filteredMarks);

            if (undefined.Count != 0)
            {
                handleUndefined(undefined, ref xmlDoc);
                //filtreeri

                using (XmlTextWriter wr = new XmlTextWriter(xml_output_full, Encoding.UTF8))
                {
                    wr.Formatting = Formatting.Indented; // here's the trick !
                    xmlDoc.Save(wr);
                }
            }

            File.Delete(xml_lock_full);
            writeCadMessage("LOCK OFF");
            writeCadMessage("end");
        }


        private void handleUndefined(List<Mark> undefined, ref XmlDocument xmlDoc)
        {
            writeCadMessage(" ");
            foreach (Mark u in undefined)
            {
                writeCadMessage("--- Not found: " + u.ToString());
            }

            string materjal = "";

            foreach (Mark u in undefined)
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
                        if (materjal == "")
                        {
                            PromptStringOptions promptOptions = new PromptStringOptions("");
                            promptOptions.Message = "\nArmatuuri teras: ";
                            promptOptions.DefaultValue = "K500C-T";
                            PromptResult promptResult = ed.GetString(promptOptions);

                            if (promptResult.Status == PromptStatus.OK)
                            {
                                materjal = promptResult.StringResult;
                            }
                        }

                        XmlNode newNode = newNodeHandle(u, materjal, ref xmlDoc);
                        writeCadMessage("Yep");
                    }
                    else
                    {
                        writeCadMessage("Skip: " + u.ToString());
                    }
                }
            }
        }


        private XmlNode newNodeHandle(Mark u, string materjal, ref XmlDocument xmlDoc)
        {
            List<XmlNode> pages = XML_Handle.getAllPages(xmlDoc);
            XmlNode lastPage = pages[pages.Count - 1];

            XmlNode row = xmlDoc.CreateElement("B2aPageRow");
            XmlAttribute attribute = xmlDoc.CreateAttribute("RowId");
            attribute.Value = "10";
            row.Attributes.Append(attribute);

            XmlNode group = xmlDoc.CreateElement("NoGrps");
            group.InnerText = "1";
            XmlNode count = xmlDoc.CreateElement("NoStpGrp");
            count.InnerText = "1";
            XmlNode bar = xmlDoc.CreateElement("B2aBar");

            XmlNode type = xmlDoc.CreateElement("Type");
            type.InnerText = u.Position_Shape;
            XmlNode pos = xmlDoc.CreateElement("Litt");
            pos.InnerText = u.Position_Nr.ToString();
            XmlNode material = xmlDoc.CreateElement("fu01:B2aStlSorts");
            material.InnerText = materjal;
            XmlNode dim = xmlDoc.CreateElement("Dim");
            dim.InnerText = u.Diameter.ToString();

            bar.AppendChild(type);
            bar.AppendChild(pos);
            bar.AppendChild(material);
            bar.AppendChild(dim);

            row.AppendChild(group);
            row.AppendChild(count);
            row.AppendChild(bar);

            lastPage.AppendChild(row);
            return row;
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


        private List<Mark> findMarksInXML(List<XmlNode> rebars, List<Mark> marks)
        {
            List<Mark> undefined = new List<Mark>();

            foreach (Mark m in marks)
            {
                bool found = matchMarkToXML(m, rebars);
                if (found == false) { undefined.Add(m); }
            }

            return undefined;
        }


        private bool matchMarkToXML(Mark m, List<XmlNode> rows)
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