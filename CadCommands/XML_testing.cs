//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//using System.IO;
//using System.Xml;

//using Autodesk.AutoCAD.Runtime;
//using Autodesk.AutoCAD.ApplicationServices;
//using Autodesk.AutoCAD.DatabaseServices;
//using Autodesk.AutoCAD.Geometry;
//using Autodesk.AutoCAD.EditorInput;

//namespace commands
//{
//    class XML_testing
//    {
//        string dwg_dir;

//        string xml_full;
//        string xml_lock_full;
//        string xml_output_full;

//        static string name = "alfa";

//        Document doc;
//        Database db;
//        Editor ed;

//        public XML_testing()
//        {
//            Document doc = Application.DocumentManager.MdiActiveDocument;
//            HostApplicationServices hs = HostApplicationServices.Current;
//            string dwg_path = hs.FindFile(doc.Name, doc.Database, FindFileHint.Default);

//            dwg_dir = Path.GetDirectoryName(dwg_path);
//            if (!dwg_dir.EndsWith(@"\")) { dwg_dir = dwg_dir + @"\"; }

//            xml_full = dwg_dir + name + ".xml";
//            xml_lock_full = dwg_dir + name + ".LCK";
//            xml_output_full = dwg_dir + "gg" + ".xml";

//            doc = Application.DocumentManager.MdiActiveDocument;
//            db = doc.Database;
//            ed = doc.Editor;
//        }

//        public void run()
//        {
//            writeCadMessage("start");

//            if (!File.Exists(xml_full))
//            {
//                writeCadMessage("[ERROR] Joonise kaustas ei ole XML faili nimega: " + name + ".xml");
//                return;
//            }

//            if (File.Exists(xml_lock_full))
//            {
//                writeCadMessage("[ERROR] XML fail nimega: " + name + ".xml" + " on lukkus!");
//                return;
//            }

//            File.Create(xml_lock_full).Dispose();
//            writeCadMessage("LOCK ON");

//            XmlDocument xmlDoc = new XmlDocument();
//            xmlDoc.Load(xml_full);

//            List<Mark> marks = getSelectedMarks();
//            List<Mark> filteredMarks = filterSelectedMarks(marks);
//            List<XmlNode> rebars = getAllRebar(xmlDoc);
//            List<Mark> undefined = findMarksInXML(rebars, filteredMarks);

//            if (undefined.Count != 0)
//            {
//                handleUndefined(undefined, ref xmlDoc);
//                //filtreeri
//                //xmlDoc.Save(xml_output_full);
//            }

//            File.Delete(xml_lock_full);
//            writeCadMessage("LOCK OFF");       
//            writeCadMessage("end");
//        }

//        private void handleUndefined(List<Mark> undefined, ref XmlDocument xmlDoc)
//        {
//            writeCadMessage(" ");
//            foreach (Mark u in undefined)
//            {
//                writeCadMessage("--- Not found: " + u.ToString());
//            }

//            foreach (Mark u in undefined)
//            {
//                PromptKeywordOptions promptOptions = new PromptKeywordOptions("");
//                promptOptions.Message = "\nAdd to XML: " + u.ToString();
//                promptOptions.Keywords.Add("Yes");
//                promptOptions.Keywords.Add("No");
//                promptOptions.AllowNone = false;
//                PromptResult promptResult = ed.GetKeywords(promptOptions);

//                if (promptResult.Status == PromptStatus.OK)
//                {
//                    if (promptResult.StringResult == "Yes")
//                    {
//                        XmlNode newNode = newNodeHandle(u, ref xmlDoc);
//                        writeCadMessage("Yep");
//                    }
//                    else
//                    {
//                        writeCadMessage("Skip: " + u.ToString() );
//                    }
//                }


//            }
//        }


//        private XmlNode newNodeHandle(Mark u, ref XmlDocument xmlDoc)
//        {
//            XmlNode rowNode = xmlDoc.CreateElement("B2aPageRow");
//            XmlAttribute attribute = xmlDoc.CreateAttribute("RowId");
//            attribute.Value = "99";
//            rowNode.Attributes.Append(attribute);

//            return rowNode;
//        }


//        private List<Mark> getSelectedMarks()
//        {
//            List<Mark> parse = new List<Mark>();

//            using (Transaction trans = db.TransactionManager.StartTransaction())
//            {
//                List<MText> selected = getSelectedText(doc, trans);

//                foreach (MText txt in selected)
//                {
//                    Mark current = new Mark(txt.Contents, txt.Location);
//                    bool valid = current.validate();
//                    if (valid) parse.Add(current);
//                }
//            }

//            return parse;
//        }


//        private List<Mark> filterSelectedMarks(List<Mark> marks)
//        {
//            List<Mark> unique = new List<Mark>();

//            foreach (Mark m in marks)
//            {
//                if (unique.Contains(m))
//                {
//                    continue;
//                }
//                else
//                {
//                    unique.Add(m);
//                }
//            }

//            unique = unique.OrderBy(b => b.Diameter).ToList();

//            List<Mark> rows_num = unique.FindAll(x => x.Position_Shape == "A").ToList();
//            List<Mark> rows_char = unique.FindAll(x => x.Position_Shape != "A").ToList();

//            rows_num = rows_num.OrderBy(b => b.Position_Nr).ToList();
//            rows_char = rows_char.OrderBy(b => b.Position_Nr).ToList();

//            unique = new List<Mark>();
//            unique.AddRange(rows_num);
//            unique.AddRange(rows_char);

//            return unique;
//        }


//        private List<MText> getSelectedText(Document doc, Transaction trans)
//        {
//            List<MText> txt = new List<MText>();

//            PromptSelectionResult selection = ed.GetSelection();
//            if (selection.Status == PromptStatus.OK)
//            {
//                ObjectId[] objIds = selection.Value.GetObjectIds();

//                foreach (ObjectId objId in objIds)
//                {
//                    Entity currentEntity = trans.GetObject(objId, OpenMode.ForRead) as Entity;

//                    if (currentEntity != null)
//                    {
//                        if (currentEntity is MText)
//                        {
//                            MText br = currentEntity as MText;
//                            txt.Add(br);
//                        }

//                        if (currentEntity is DBText)
//                        {
//                            DBText br = currentEntity as DBText;
//                            MText myMtext = new MText();
//                            myMtext.Contents = br.TextString;
//                            myMtext.Location = br.Position;
//                            txt.Add(myMtext);
//                        }

//                        if (currentEntity is MLeader)
//                        {
//                            MLeader br = currentEntity as MLeader;

//                            if (br.ContentType == ContentType.MTextContent)
//                            {
//                                MText leaderText = br.MText;
//                                txt.Add(leaderText);
//                            }
//                        }
//                    }
//                }
//            }
            
//            return txt;
//        }
        

//        private List<Mark> findMarksInXML(List<XmlNode> rebars, List<Mark> marks)
//        {
//            List<Mark> undefined = new List<Mark>();

//            foreach (Mark m in marks)
//            {
//                bool found = matchMarkToXML(m, rebars);
//                if (found == false) { undefined.Add(m); }
//            }

//            return undefined;
//        }


//        private bool matchMarkToXML(Mark m, List<XmlNode> rebars)
//        {
//            foreach (XmlNode rebar in rebars)
//            {
//                XmlNodeList children = rebar.ChildNodes;

//                string type = emptyXMLhandle(rebar, "Type");
//                string pos_nr = emptyXMLhandle(rebar, "Litt");
//                string diam = emptyXMLhandle(rebar, "Dim");

//                if (m.Position_Shape == "A")
//                {
//                    if (m.Position_Shape == type && m.Position_Nr.ToString() == pos_nr && m.Diameter.ToString() == diam)
//                    {
//                        writeCadMessage("Found in XML: " + m.ToString());
//                        printXMLNodeRebar(rebar);
//                        writeCadMessage("");
//                        return true;
//                    }
//                }
//                else
//                {
//                    if (m.Position_Shape == type && m.Position_Nr.ToString() == pos_nr)
//                    {
//                        writeCadMessage("Found in XML: " + m.ToString());
//                        printXMLNodeRebar(rebar);
//                        writeCadMessage("");
//                        return true;
//                    }
//                }
//            }

//            return false;
//        }

//        private List<XmlNode> getAllRebar(XmlDocument file)
//        {
//            List<XmlNode> rebars = new List<XmlNode>();

//            foreach (XmlNode page in file.DocumentElement)
//            {
//                if (page.Name != "B2aPage") continue;

//                foreach (XmlNode row in page.ChildNodes)
//                {
//                    if (row.Name != "B2aPageRow") continue;

//                    foreach (XmlNode cell in row.ChildNodes)
//                    {
//                        if (cell.Name != "B2aBar") continue;

//                        rebars.Add(cell);
//                    }
//                }
//            }

//            return rebars;
//        }


//        private void writeCadMessage(string errorMessage)
//        {
//            ed.WriteMessage("\n" + errorMessage);
//        }

//        private void printXMLNodeRebar(XmlNode rebar)
//        {
//            string result = "";

//            string type = emptyXMLhandle(rebar, "Type");

//            string pos_nr = "Pos: [" + emptyXMLhandle(rebar, "Litt") + "]";
//            string diam = "Diameter: [" + emptyXMLhandle(rebar, "Dim") + "]";
//            string length = "Length: [" + emptyXMLhandle(rebar, "Length") + "]";
//            XmlNode geometry = rebar["B2aGeometry"];

//            if (type == "A")
//            {
//                type = "Type [" + type + "]";

//                result = type + " - " + pos_nr + " - " + diam + " - " + length;
//            }
//            else if (type == "B")
//            {

//            }
//            else if (type == "C")
//            {
//                type = "Type [" + type + "]";

//                if (geometry == null)
//                {
//                    result = type + " - " + pos_nr + " - " + diam + " - " + length + " - [NO GEOMETRY]";
//                }
//                else
//                {
//                    string a = "A: [" + emptyXMLhandle(geometry, "A") + "]";
//                    string b = "B: [" + emptyXMLhandle(geometry, "B") + "]";
//                    string c = "C: [" + emptyXMLhandle(geometry, "C") + "]";
//                    string r = "R: [" + emptyXMLhandle(geometry, "R") + "]";

//                    result =  type + " - " + pos_nr + " - " + diam + " - " + length + " - " + a + " - " + b + " - " + c + " - " + r;
//                }
//            }
//            else if (type == "D")
//            {

//            }
//            else if (type == "E")
//            {

//            }
//            else if (type == "F")
//            {
//                type = "Type [" + type + "]";

//                if (geometry == null)
//                {
//                    result = type + " - " + pos_nr + " - " + diam + " - " + length + " - [NO GEOMETRY]";
//                }
//                else
//                {
//                    string a = "A: [" + emptyXMLhandle(geometry, "A") + "]";
//                    string b = "B: [" + emptyXMLhandle(geometry, "B") + "]";
//                    string v = "V: [" + emptyXMLhandle(geometry, "V") + "]";
//                    string r = "R: [" + emptyXMLhandle(geometry, "R") + "]";

//                    result = type + " - " + pos_nr + " - " + diam + " - " + length + " - " + a + " - " + b + " - " + v + " - " + r;
//                }
//            }
//            else if (type == "G")
//            {

//            }
//            else if (type == "H")
//            {

//            }
//            else if (type == "I")
//            {

//            }
//            else if (type == "J")
//            {

//            }
//            else if (type == "K")
//            {

//            }
//            else if (type == "L")
//            {

//            }
//            else if (type == "M")
//            {

//            }
//            else if (type == "N")
//            {
//                type = "Type: [" + type + "]";

//                if (geometry == null)
//                {
//                    result = type + " - " + pos_nr + " - " + diam + " - " + length + " - [NO GEOMETRY]";
//                }
//                else
//                {
//                    string end1 = "End1: [" + emptyXMLhandle(geometry, "End1") + "]";
//                    string a = "A: [" + emptyXMLhandle(geometry, "A") + "]";
//                    string b = "B: [" + emptyXMLhandle(geometry, "B") + "]";
//                    string end2 = "End2: [" + emptyXMLhandle(geometry, "End2") + "]";
//                    string r = "R: [" + emptyXMLhandle(geometry, "R") + "]";

//                    result = type + " - " + pos_nr + " - " + diam + " - " + length + " - " + end1 + " - " + a + " - " + b + " - " + end2 + " - " + r;
//                }
//            }

//            writeCadMessage(result);
//        }

//        private string emptyXMLhandle(XmlNode node, string search)
//        {
//            XmlNode result = node[search];
//            if (result == null)
//            {
//                return "???";
//            }
//            else
//            {
//                return result.InnerText;
//            }

//        }
//    }
//}