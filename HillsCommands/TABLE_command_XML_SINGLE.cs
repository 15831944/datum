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
    class TABLE_command_XML_SINGLE
    {
        static string[] newBoxNames = { "KN-C", "KN-V27" };
        static string markLayerName = "K60";

        Dictionary<Area_v2, List<Mark>> local_stats;

        Document doc;
        Database db;
        Editor ed;

        public TABLE_command_XML_SINGLE()
        {
            local_stats = new Dictionary<Area_v2, List<Mark>>();

            doc = Application.DocumentManager.MdiActiveDocument;
            db = doc.Database;
            ed = doc.Editor;
        }


        public void run()
        {
            writeCadMessage("START");

            List<Area_v2> areas = new List<Area_v2>();
            areas = getAllAreas(newBoxNames);

            if (areas.Count < 1)
            {
                string names = string.Join(", ", newBoxNames.ToArray());
                writeCadMessage("[ERROR] - (" + names + ") not found");
            }

            List<Mark> allMarks = getAllMarks(markLayerName);
            if (allMarks.Count < 1)
            {
                writeCadMessage("[ERROR] - " + "Reinforcement marks" + " not found");
                return;
            }

            local_stats = matchMarkToArea(areas, allMarks);


            foreach (Area_v2 current in local_stats.Keys)
            {
                List<Mark> currentMarks = local_stats[current];
                if (currentMarks.Count > 1)
                {
                    xml_dump(current, currentMarks);
                }
            }

            writeCadMessage("DONE");

            return;
        }


        private Dictionary<Area_v2, List<Mark>> matchMarkToArea(List<Area_v2> areas, List<Mark> allMarks)
        {
            Dictionary<Area_v2, List<Mark>> sorted = new Dictionary<Area_v2, List<Mark>>();

            foreach (Area_v2 area in areas)
            {
                List<Mark> marks = getMarksInArea(area, allMarks);
                List<Mark> validMarks = new List<Mark>();

                foreach (Mark m in marks)
                {
                    bool valid = m.validate();
                    if (valid) validMarks.Add(m);
                }

                List<Mark> sumMarks = getSummary(validMarks);
                sorted[area] = sumMarks;
            }

            return sorted;
        }


        private List<Mark> getSummary(List<Mark> marks)
        {
            List<Mark> sumMarks = new List<Mark>();

            foreach (Mark m in marks)
            {
                if (sumMarks.Contains(m))
                {
                    int index = sumMarks.FindIndex(a => a == m);
                    sumMarks[index].Number += m.Number;
                }
                else
                {
                    Mark nm = new Mark(m.Number, m.Diameter, m.Position, m.Position_Shape, m.Position_Nr);
                    sumMarks.Add(nm);
                }
            }

            sumMarks = sumMarks.OrderBy(b => b.Diameter).ToList();

            List<Mark> rows_num = sumMarks.FindAll(x => x.Position_Shape == "A").ToList();
            List<Mark> rows_char = sumMarks.FindAll(x => x.Position_Shape != "A").ToList();

            rows_num = rows_num.OrderBy(b => b.Position_Nr).ToList();
            rows_char = rows_char.OrderBy(b => b.Position_Nr).ToList();

            sumMarks = new List<Mark>();
            sumMarks.AddRange(rows_num);
            sumMarks.AddRange(rows_char);

            return sumMarks;
        }


        private List<Mark> getAllMarks(string layer)
        {
            List<Mark> marks = new List<Mark>();

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                List<MText> allTexts = getAllText(layer, trans);
                marks = getMarkData(allTexts, trans);
            }

            return marks;
        }


        private List<Mark> getMarksInArea(Area_v2 area, List<Mark> allmarks)
        {
            List<Mark> marks = new List<Mark>();

            for (int i = allmarks.Count - 1; i >= 0; i--)
            {
                Mark mark = allmarks[i];
                if (area.isPointInArea(mark.IP))
                {
                    marks.Add(mark);
                    allmarks.RemoveAt(i);
                }
            }

            return marks;
        }


        private List<Mark> getMarkData(List<MText> txts, Transaction trans)
        {
            List<Mark> parse = new List<Mark>();

            foreach (MText txt in txts)
            {
                Mark current = new Mark(txt.Contents, txt.Location);
                parse.Add(current);
            }

            return parse;
        }


        private List<Area_v2> getAllAreas(string[] blockNames)
        {
            List<Area_v2> areas = new List<Area_v2>();

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                List<BlockReference> blocks = new List<BlockReference>();

                foreach (string name in blockNames)
                {
                    List<BlockReference> temp = getAllBlockReference(name, trans);
                    blocks.AddRange(temp);
                }

                areas = getBoxAreas(blocks, trans);
            }

            return areas;
        }


        private List<Area_v2> getBoxAreas(List<BlockReference> blocks, Transaction trans)
        {
            List<Area_v2> parse = new List<Area_v2>();

            foreach (BlockReference block in blocks)
            {
                Extents3d blockExtents = block.GeometricExtents;

                Area_v2 area = new Area_v2(block.ObjectId, blockExtents.MinPoint, blockExtents.MaxPoint);
                parse.Add(area);
            }

            parse = parse.OrderBy(o => o.Start.X).ToList();

            return parse;
        }


        private List<BlockReference> getAllBlockReference(string blockName, Transaction trans)
        {
            List<BlockReference> refs = new List<BlockReference>();

            BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;

            if (bt.Has(blockName))
            {
                BlockTableRecord btr = trans.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForWrite) as BlockTableRecord;

                foreach (ObjectId id in btr)
                {
                    DBObject currentEntity = trans.GetObject(id, OpenMode.ForWrite, false) as DBObject;

                    if (currentEntity == null)
                    {
                        continue;
                    }

                    else if (currentEntity is BlockReference)
                    {
                        BlockReference blockRef = currentEntity as BlockReference;

                        BlockTableRecord block = null;
                        if (blockRef.IsDynamicBlock)
                        {
                            block = trans.GetObject(blockRef.DynamicBlockTableRecord, OpenMode.ForRead) as BlockTableRecord;
                        }
                        else
                        {
                            block = trans.GetObject(blockRef.BlockTableRecord, OpenMode.ForRead) as BlockTableRecord;
                        }

                        if (block != null)
                        {
                            if (block.Name == blockName)
                            {
                                refs.Add(blockRef);
                            }
                        }
                    }
                }
            }

            return refs;
        }


        private List<MText> getAllText(string layer, Transaction trans)
        {
            List<MText> txt = new List<MText>();

            BlockTableRecord btr = trans.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForWrite) as BlockTableRecord;

            foreach (ObjectId id in btr)
            {
                Entity currentEntity = trans.GetObject(id, OpenMode.ForWrite, false) as Entity;

                if (currentEntity != null)
                {
                    if (currentEntity is MText)
                    {
                        MText br = currentEntity as MText;
                        if (br.Layer == layer)
                        {
                            txt.Add(br);
                        }
                    }

                    if (currentEntity is DBText)
                    {
                        DBText br = currentEntity as DBText;
                        if (br.Layer == layer)
                        {
                            MText myMtext = new MText();
                            myMtext.Contents = br.TextString;
                            myMtext.Location = br.Position;
                            txt.Add(myMtext);
                        }
                    }

                    if (currentEntity is MLeader)
                    {
                        MLeader br = currentEntity as MLeader;
                        if (br.Layer == layer)
                        {
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


        private void writeCadMessage(string errorMessage)
        {
            ed.WriteMessage("\n" + errorMessage);
        }


        private void xml_dump(Area_v2 area, List<Mark> data)
        {
            string name = "alfa";

            string ritn_nr = "x";
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                DBObject currentEntity = trans.GetObject(area.ID, OpenMode.ForWrite, false) as DBObject;

                if (currentEntity is BlockReference)
                {
                    BlockReference blockRef = currentEntity as BlockReference;

                    foreach (ObjectId arId in blockRef.AttributeCollection)
                    {
                        DBObject obj = trans.GetObject(arId, OpenMode.ForWrite);
                        AttributeReference ar = obj as AttributeReference;
                        if (ar != null)
                        {
                            if (blockRef.Name == "KN-V23")
                            {
                                if (ar.Tag == "RITN_23_NR") ritn_nr = ar.TextString;
                            }
                            else if (blockRef.Name == "KN-V27")
                            {
                                if (ar.Tag == "RITN_27_NR") ritn_nr = ar.TextString;
                            }

                            if (ar.Tag == "RITN_NR") ritn_nr = ar.TextString;
                        }
                    }
                }
            }

            string output_name = ritn_nr;

            HostApplicationServices hs = HostApplicationServices.Current;
            string dwg_path = hs.FindFile(doc.Name, doc.Database, FindFileHint.Default);
            string dwg_dir = Path.GetDirectoryName(dwg_path);

            if (!dwg_dir.EndsWith(@"\")) { dwg_dir = dwg_dir + @"\"; }
            string single_dir = dwg_dir + @"single\";

            string xml_full = dwg_dir + name + ".xml";
            string xml_lock_full = dwg_dir + name + ".LCK";
            string xml_output_full = single_dir + output_name + ".xml";

            if (!Directory.Exists(Path.GetDirectoryName(single_dir))) Directory.CreateDirectory(Path.GetDirectoryName(single_dir));
            if (File.Exists(xml_output_full)) { File.Delete(xml_output_full); }

            if (!File.Exists(xml_full))
            {
                writeCadMessage("[ERROR] Antud kaustas ei ole XML faili nimega: " + name + ".xml");
                return;
            }

            if (File.Exists(xml_lock_full))
            {
                writeCadMessage("[ERROR] XML fail nimega: " + name + ".xml" + " on lukkus!");
                return;
            }

            if (File.Exists(xml_output_full))
            {
                writeCadMessage("[ERROR] XML fail nimega " + output_name + ".xml" + " on juba olemas");
                return;
            }

            File.Create(xml_lock_full).Dispose();
            writeCadMessage("LOCK ON");

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xml_full);
            xmlDoc = removeEmptyNodes(xmlDoc);
            List<XmlNode> rebarNodes = getAllRebar(xmlDoc);

            setNumber(rebarNodes, data);
            List<XmlNode> foundNodes = removeNotFoundNodes(rebarNodes);

            if (foundNodes.Count > 1)
            {
                sortNodes(foundNodes, xmlDoc);
                xmlDoc.Save(xml_output_full);
            }

            

            File.Delete(xml_lock_full);
            writeCadMessage("LOCK OFF");
        }


        private XmlDocument removeEmptyNodes(XmlDocument xd) // DONT KNOW THIS MAGIC
        {
            XmlNodeList emptyElements = xd.SelectNodes(@"//*[not(node())]");
            for (int i = emptyElements.Count - 1; i > -1; i--)
            {
                XmlNode nodeToBeRemoved = emptyElements[i];
                nodeToBeRemoved.ParentNode.RemoveChild(nodeToBeRemoved);
            }

            return xd;
        }


        private List<XmlNode> getAllPages(XmlDocument file)
        {
            List<XmlNode> pages = new List<XmlNode>();

            foreach (XmlNode page in file.DocumentElement)
            {
                if (page.Name != "B2aPage") continue;

                pages.Add(page);
            }

            return pages;
        }

        private List<XmlNode> getAllRebar(XmlDocument file)
        {
            List<XmlNode> rebars = new List<XmlNode>();

            foreach (XmlNode page in file.DocumentElement)
            {
                if (page.Name != "B2aPage") continue;

                foreach (XmlNode row in page.ChildNodes)
                {
                    if (row.Name != "B2aPageRow") continue;

                    rebars.Add(row);
                }
            }

            return rebars;
        }


        private void setNumber(List<XmlNode> rebarNodes, List<Mark> data)
        {
            List<XmlNode> notFoundNode = new List<XmlNode>();
            List<Mark> foundRebar = new List<Mark>();

            foreach (XmlNode rebarNode in rebarNodes)
            {
                bool found = false;

                XmlNode group = rebarNode["NoStpGrp"];
                XmlNode rebar = rebarNode["B2aBar"];

                if (rebar != null && group != null)
                {
                    XmlNode type = rebar["Type"];
                    XmlNode pos_nr = rebar["Litt"];
                    XmlNode diam = rebar["Dim"];

                    group.InnerText = "9999";

                    if (type != null && pos_nr != null && diam != null)
                    {
                        string t = type.InnerText;
                        string p = pos_nr.InnerText;
                        string d = diam.InnerText;

                        foreach (Mark reb in data)
                        {
                            if (reb.Position_Shape == t && reb.Position_Nr.ToString() == p && reb.Diameter.ToString() == d)
                            {
                                found = true;
                                group.InnerText = reb.Number.ToString();
                                foundRebar.Add(reb);
                                data.Remove(reb);
                                break;
                            }
                        }


                    }
                }

                if (found == false)
                {
                    notFoundNode.Add(rebarNode);
                }
            }

            writeCadMessage("");
            writeCadMessage(notFoundNode.Count.ToString() + " - rauda ei ole joonistel kasutuses");
            writeCadMessage(data.Count.ToString() + " - rauda ei ole XML-is defineeritud");

            if (data.Count > 1)
            {
                writeCadMessage("");
                writeCadMessage("Ei ole defineeritud:");
                foreach (Mark reb in data)
                {
                    writeCadMessage(reb.Position_Shape + " " + reb.Position_Nr.ToString() + " " + reb.Diameter.ToString());
                }
            }
        }


        private List<XmlNode> removeNotFoundNodes(List<XmlNode> all)
        {
            List<XmlNode> found = new List<XmlNode>();

            foreach (XmlNode single in all)
            {
                XmlNode group = single["NoStpGrp"];
                XmlNode rebar = single["B2aBar"];

                if (rebar != null && group != null)
                {
                    if (group.InnerText != "9999")
                    {
                        found.Add(single);
                    }
                }
            }

            return found;
        }


        private void sortNodes(List<XmlNode> rebars, XmlDocument xmlDoc)
        {
            List<XmlNode> sortedRebars = sortRebars(rebars);

            List<XmlNode> pa = getAllPages(xmlDoc);
            foreach (XmlNode p in pa)
            {
                xmlDoc.DocumentElement.RemoveChild(p);
            }

            List<XmlNode> pages = new List<XmlNode>();

            int rebarTotIndex = 0;
            int rebarPageIndex = 1;
            int pageIndex = 0;

            XmlNode lastReb = null;

            XmlNode page = newPageHandle(pages, xmlDoc);
            pages.Add(page);
            xmlDoc.DocumentElement.AppendChild(page);

            while (true)
            {
                if (pageIndex < pages.Count)
                {
                    page = pages[pageIndex];
                }
                else
                {
                    page = newPageHandle(pages, xmlDoc);
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


        public XmlNode newPageHandle(List<XmlNode> pages, XmlDocument xmlDoc)
        {
            XmlNode row = xmlDoc.CreateElement("B2aPage");
            XmlAttribute attribute = xmlDoc.CreateAttribute("PageLabel");
            attribute.Value = "A-" + (pages.Count + 1).ToString("D2");
            row.Attributes.Append(attribute);

            XmlNode head = xmlDoc.CreateElement("B2aPageHead");

            XmlNode c10 = xmlDoc.CreateElement("ConcreteFctk10");
            XmlNode cf10 = xmlDoc.CreateElement("ConcreteCoverFactor10");

            head.AppendChild(c10);
            head.AppendChild(cf10);

            row.AppendChild(head);

            return row;
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
    }
}