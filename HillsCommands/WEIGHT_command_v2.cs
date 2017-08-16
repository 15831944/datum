using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

//Autocad
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

////Bricsys
//using Teigha.Runtime;
//using Teigha.DatabaseServices;
//using Teigha.Geometry;
//using Bricscad.ApplicationServices;
//using Bricscad.Runtime;
//using Bricscad.EditorInput;

namespace commands
{
    class WEIGHT_command_v2
    {
        static string name = "alfa";

        static string[] boxNames = { "KN-C", "KN-V27" };
        static string markLayerName = "K60";

        Dictionary<Area_v2, int> local_stats;

        string dwg_dir;
        string xml_full;
        string xml_lock_full;

        Document doc;
        Database db;
        Editor ed;

        public WEIGHT_command_v2()
        {
            doc = Application.DocumentManager.MdiActiveDocument;
            db = doc.Database;
            ed = doc.Editor;

            local_stats = new Dictionary<Area_v2, int>();

            HostApplicationServices hs = HostApplicationServices.Current;
            string dwg_path = hs.FindFile(doc.Name, doc.Database, FindFileHint.Default);

            dwg_dir = Path.GetDirectoryName(dwg_path);
            if (!dwg_dir.EndsWith(@"\")) { dwg_dir = dwg_dir + @"\"; }

            xml_full = dwg_dir + name + ".xml";
            xml_lock_full = dwg_dir + name + ".LCK";
        }


        public void unlock_after_crash()
        {
            if (File.Exists(xml_lock_full))
            {
                File.Delete(xml_lock_full);
            }
        }


        public void run(bool multy)
        {
            writeCadMessage("START");

            List<Area_v2> areas = new List<Area_v2>();

            if (multy == true)
            {
                areas = getAllAreas(boxNames);
            }
            else
            {
                areas = getSelectedAreas(boxNames);
            }

            if (areas.Count < 1)
            {
                string names = string.Join(", ", boxNames.ToArray());
                writeCadMessage("[ERROR] - (" + names + ") not found");
            }

            List<Mark> allMarks = getAllMarks(markLayerName);
            if (allMarks.Count < 1)
            {
                writeCadMessage("[ERROR] - " + "Reinforcement marks" + " not found");
                return;
            }

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
            List<XmlNode> bending = XML_Handle.getAllRebar(xmlDoc);

            File.Delete(xml_lock_full);
            writeCadMessage("LOCK OFF");

            Dictionary<Area_v2, List<Mark>> local_reinforcement = matchMarkArea(areas, allMarks);
            local_stats = generateAllWeights(local_reinforcement, bending);

            return;
        }


        public void output_local()
        {
            foreach (Area_v2 current in local_stats.Keys)
            {
                outputWeight(current, local_stats[current]);
            }

            writeCadMessage("DONE");

            return;
        }


        private void outputWeight(Area_v2 a, int weight)
        {
            if (weight != 0)
            {
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    DBObject currentEntity = trans.GetObject(a.ID, OpenMode.ForWrite, false) as DBObject;

                    if (currentEntity is BlockReference)
                    {
                        BlockReference blockRef = currentEntity as BlockReference;

                        foreach (ObjectId arId in blockRef.AttributeCollection)
                        {
                            DBObject obj = trans.GetObject(arId, OpenMode.ForWrite);
                            AttributeReference ar = obj as AttributeReference;
                            if (ar != null)
                            {
                                if (ar.Tag == "SUMMA_OVRIG_ARMERING") ar.TextString = weight.ToString();
                            }
                        }
                    }

                    trans.Commit();
                }
            }
            else
            {
                writeCadMessage("[SKIP]");
            }
        }


        private Dictionary<Area_v2, int> generateAllWeights(Dictionary<Area_v2, List<Mark>> reinf, List<XmlNode> bending)
        {
            Dictionary<Area_v2, int> stats = new Dictionary<Area_v2, int>();

            foreach (Area_v2 current in reinf.Keys)
            {
                List<Mark> currentReinf = reinf[current];

                int currentWeight = calculateWeight(currentReinf, bending);
                stats[current] = currentWeight;
            }

            return stats;
        }


        private int calculateWeight(List<Mark> reinf, List<XmlNode> bending)
        {
            Dictionary<Mark, XmlNode> matches = new Dictionary<Mark, XmlNode>();
            Dictionary<Mark, double> weights = new Dictionary<Mark, double>();
            matches = findMarksInXML(reinf, bending);

            double currentWeight = 0;

            List<Mark> emptyMarks = new List<Mark>();

            foreach (Mark m in matches.Keys)
            {
                if (matches[m] == null)
                {
                    emptyMarks.Add(m);
                }
            }

            if (emptyMarks.Count == 0)
            {
                foreach (Mark m in matches.Keys)
                {
                    double weight = getRebarWeights(matches[m]);
                    weights[m] = weight;
                }
            }
            else
            {
                foreach (Mark m in emptyMarks)
                {
                    writeCadMessage("[ERROR] Can not find match for [" + m.ToString() + "] in XML");
                }

                return 0;
            }

            foreach (Mark m in weights.Keys)
            {
                if (weights[m] == 0)
                {
                    emptyMarks.Add(m);
                }
            }

            if (emptyMarks.Count == 0)
            {
                foreach (Mark m in weights.Keys)
                {
                    currentWeight = currentWeight + (m.Number * weights[m]);
                }
            }
            else
            {
                foreach (Mark m in emptyMarks)
                {
                    writeCadMessage("[ERROR] Not enought information for [" + m.ToString() + "] in XML");
                }

                return 0;
            }

            return (int)Math.Ceiling(currentWeight);
        }


        private double getRebarWeights(XmlNode row)
        {
            XmlNode rebar = row["B2aBar"];

            if (rebar == null) return 0;

            string weightString = XML_Handle.emptyNodehandle(rebar, "Weight");

            double weight = 0.0;

            try
            {
                weight = Double.Parse(weightString, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {

            }

            return weight;
        }


        private Dictionary<Mark, XmlNode> findMarksInXML(List<Mark> marks, List<XmlNode> bending)
        {
            Dictionary<Mark, XmlNode> markMatch = new Dictionary<Mark, XmlNode>();

            foreach (Mark m in marks)
            {
                markMatch[m] = matchMarkToXML(m, bending);
            }

            return markMatch;
        }


        private XmlNode matchMarkToXML(Mark m, List<XmlNode> rows)
        {
            foreach (XmlNode row in rows)
            {
                XmlNode rebar = row["B2aBar"];

                if (rebar == null) return null;

                string type = XML_Handle.emptyNodehandle(rebar, "Type");
                string pos_nr = XML_Handle.emptyNodehandle(rebar, "Litt");
                string diam = XML_Handle.emptyNodehandle(rebar, "Dim");

                if (m.Position_Shape == type && m.Position_Nr.ToString() == pos_nr && m.Diameter.ToString() == diam)
                {
                    return row;
                }
            }

            return null;
        }




        private Dictionary<Area_v2, List<Mark>> matchMarkArea(List<Area_v2> areas, List<Mark> allMarks)
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

        private List<Area_v2> getSelectedAreas(string[] blockNames)
        {
            List<Area_v2> areas = new List<Area_v2>();

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                List<BlockReference> blocks = getSelectedBlockReference(blockNames, trans);
                areas = getBoxAreas(blocks, trans);
            }

            return areas;
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
                Area_v2 area = new Area_v2(block.Id, blockExtents.MinPoint, blockExtents.MaxPoint);
                parse.Add(area);
            }

            parse = parse.OrderBy(o => o.Start.X).ToList();

            return parse;
        }


        private List<BlockReference> getSelectedBlockReference(string[] blockNames, Transaction trans)
        {
            List<BlockReference> refs = new List<BlockReference>();
            PromptSelectionOptions opts = new PromptSelectionOptions();
            opts.MessageForAdding = "\nSelect BLOCK " + blockNames[0] + " / " + blockNames[1];

            PromptSelectionResult selection = ed.GetSelection(opts);

            if (selection.Status == PromptStatus.OK)
            {
                ObjectId[] selectionIds = selection.Value.GetObjectIds();

                foreach (ObjectId id in selectionIds)
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
                            if (blockNames.Contains(block.Name))
                            {
                                refs.Add(blockRef);
                            }
                        }
                    }
                }
            }

            return refs;
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


        private void insertText(string value, Point3d position, string layer)
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                using (DBText acText = new DBText())
                {
                    acText.Layer = layer;

                    acText.Position = position;
                    acText.Height = 120;
                    acText.TextString = value;

                    btr.AppendEntity(acText);
                    trans.AddNewlyCreatedDBObject(acText, true);
                }

                trans.Commit();
            }
        }


        private void writeCadMessage(string errorMessage)
        {
            ed.WriteMessage("\n" + errorMessage);
        }
    }
}