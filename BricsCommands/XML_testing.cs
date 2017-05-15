//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//using System.IO;
//using System.Xml;

////ODA
//using Teigha.Runtime;
//using Teigha.DatabaseServices;
//using Teigha.Geometry;

////Bricsys
//using Bricscad.ApplicationServices;
//using Bricscad.Runtime;
//using Bricscad.EditorInput;

//namespace commands
//{
//    class XML_testing
//    {
//        string dwg_dir;

//        string xml_full;
//        string xml_lock_full;

//        static string name = "alfa";

//        public XML_testing()
//        {
//            Document doc = Application.DocumentManager.MdiActiveDocument;
//            HostApplicationServices hs = HostApplicationServices.Current;
//            string dwg_path = hs.FindFile(doc.Name, doc.Database, FindFileHint.Default);

//            dwg_dir = Path.GetDirectoryName(dwg_path);
//            if (!dwg_dir.EndsWith(@"\")) { dwg_dir = dwg_dir + @"\"; }

//            xml_full = dwg_dir + name + ".xml";
//            xml_lock_full = dwg_dir + name + ".LCK";
//        }

//        public void run()
//        {
//            writeCadMessage("start");

//            Document doc = Application.DocumentManager.MdiActiveDocument;
//            Database db = doc.Database;
//            Editor ed = doc.Editor;


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
//            XmlDocument xmlDoc = new XmlDocument();
//            xmlDoc.Load(xml_full);
//            File.Delete(xml_lock_full);

//            List<XmlNode> rebars = getAllRebar(xmlDoc);
//            List<Mark> marks = getSelectedMarks();

//            foreach (Mark m in marks)
//            {
//                findRebarExact(m, rebars);
//                //writeCadMessage("");
//                //findRebarNoDiam(m, rebars);
//            }

//            writeCadMessage("end");
//        }


//        private List<Mark> getSelectedMarks()
//        {
//            List<Mark> parse = new List<Mark>();

//            Document doc = Application.DocumentManager.MdiActiveDocument;
//            Database db = doc.Database;

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

//        private List<MText> getSelectedText(Document doc, Transaction trans)
//        {
//            List<MText> txt = new List<MText>();

//            Editor ed = doc.Editor;
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
//                            //if (br.Layer == layer)
//                            //{
//                            txt.Add(br);
//                            //}
//                        }

//                        if (currentEntity is DBText)
//                        {
//                            DBText br = currentEntity as DBText;
//                            //if (br.Layer == layer)
//                            //{
//                            MText myMtext = new MText();
//                            myMtext.Contents = br.TextString;
//                            myMtext.Location = br.Position;
//                            txt.Add(myMtext);
//                            //}
//                        }

//                        if (currentEntity is MLeader)
//                        {
//                            MLeader br = currentEntity as MLeader;
//                            //if (br.Layer == layer)
//                            //{
//                            if (br.ContentType == ContentType.MTextContent)
//                            {
//                                MText leaderText = br.MText;
//                                txt.Add(leaderText);
//                            }
//                            //}
//                        }
//                    }
//                }
//            }
            
//            return txt;
//        }


//        private List<Mark> getMarkData(List<MText> txts, Transaction trans)
//        {
//            List<Mark> parse = new List<Mark>();

//            foreach (MText txt in txts)
//            {
//                Mark current = new Mark(txt.Contents, txt.Location);
//                parse.Add(current);
//            }

//            return parse;
//        }


//        private void findRebarExact(Mark m, List<XmlNode> rebars)
//        {
//            foreach (XmlNode rebar in rebars)
//            {
//                string type = rebar.ChildNodes[0].InnerText;
//                string pos_nr = rebar.ChildNodes[1].InnerText;
//                string diam = rebar.ChildNodes[3].InnerText;

//                if (m.Position_Shape == type && m.Position_Nr.ToString() == pos_nr && m.Diameter.ToString() == diam)
//                {
//                    writeCadMessage(type + " " + pos_nr + " " + diam);
//                }
//                else
//                {
//                    writeCadMessage("---" + type + " " + pos_nr + " " + diam);
//                }
//            }
//        }


//        private void findRebarNoDiam(Mark m, List<XmlNode> rebars)
//        {
//            foreach (XmlNode rebar in rebars)
//            {
//                string type = rebar.ChildNodes[0].InnerText;
//                string pos_nr = rebar.ChildNodes[1].InnerText;
//                string diam = rebar.ChildNodes[3].InnerText;

//                if (m.Position_Shape == type && m.Position_Nr.ToString() == pos_nr)
//                {
//                    writeCadMessage(type + " " + pos_nr + " " + diam);
//                }
//                else
//                {
//                    writeCadMessage("---" + type + " " + pos_nr + " " + diam);
//                }
//            }
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
//            Document doc = Application.DocumentManager.MdiActiveDocument;
//            Editor ed = doc.Editor;

//            ed.WriteMessage("\n" + errorMessage);
//        }
//    }
//}
