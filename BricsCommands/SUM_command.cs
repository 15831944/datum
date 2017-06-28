using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

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
    class SUM_command
    {
        static string[] boxNames = { "KN-A", "KN-C", "KN-V27"};
        static string markLayerName = "K60";

        List<Mark> total_stats;
        Dictionary<Area, List<Mark>> local_stats;

        Document doc;
        Database db;
        Editor ed;

        public SUM_command()
        {
            total_stats = new List<Mark>();
            local_stats = new Dictionary<Area, List<Mark>>();

            doc = Application.DocumentManager.MdiActiveDocument;
            db = doc.Database;
            ed = doc.Editor;
        }


        public void run(bool multy)
        {
            writeCadMessage("START");

            List<Area> areas = new List<Area>();

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
                writeCadMessage("ERROR - " + boxNames[0] + " / " + boxNames[1] + " / " + boxNames[2] + " not found");
            }

            List<Mark> allMarks = getAllMarks(markLayerName);
            if (allMarks.Count < 1)
            {
                writeCadMessage("ERROR - " + "Reinforcement marks" + " not found");
                return;
            }

            local_stats = matchMarkArea(areas, allMarks);

            return;
        }


        public void dump_csv()
        {
            total_stats = getGlobalSummary(local_stats);

            dump();

            writeCadMessage("DONE");

            return;
        }


        public void output_local()
        {
            foreach (Area current in local_stats.Keys)
            {
                outputTable(current, local_stats[current]);
            }

            writeCadMessage("DONE");

            return;
        }


        private List<Mark> getGlobalSummary(Dictionary<Area, List<Mark>> sorted)
        {
            List<Mark> allValidMarks = new List<Mark>();

            foreach (Area area in sorted.Keys)
            {
                allValidMarks.AddRange(sorted[area]);
            }

            List<Mark> summary = getSummary(allValidMarks);

            return summary;
        }


        private void outputTable(Area a, List<Mark> rows)
        {
            Point3d currentPoint = a.IP_reinf;

            foreach (Mark r in rows)
            {
                if (r.Position != "emptyrow")
                {
                    Point3d position_IP = new Point3d(currentPoint.X + 40, currentPoint.Y + 45, currentPoint.Z);
                    Point3d diameter_IP = new Point3d(currentPoint.X + 30 + 244, currentPoint.Y + 45, currentPoint.Z);
                    Point3d number_IP = new Point3d(currentPoint.X + 60 + 390, currentPoint.Y + 45, currentPoint.Z);

                    insertText(r.Position, position_IP, markLayerName);
                    insertText(r.Diameter.ToString(), diameter_IP, markLayerName);
                    insertText(r.Number.ToString(), number_IP, markLayerName);
                }

                currentPoint = new Point3d(currentPoint.X, currentPoint.Y - 160, currentPoint.Z);
            }
        }


        private Dictionary<Area, List<Mark>> matchMarkArea(List<Area> areas, List<Mark> allMarks)
        {
            Dictionary<Area, List<Mark>> sorted = new Dictionary<Area, List<Mark>>();

            foreach (Area area in areas)
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

            Mark emptyRow = new Mark(0, 0, "emptyrow", "", 0);
            sumMarks.Add(emptyRow);
            sumMarks.Add(emptyRow);

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


        private List<Mark> getMarksInArea(Area area, List<Mark> allmarks)
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

        private List<Area> getSelectedAreas(string[] blockNames)
        {
            List<Area> areas = new List<Area>();

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                List<BlockReference> blocks = getSelectedBlockReference(blockNames, trans);
                areas = getBoxAreas(blocks, trans);
            }

            return areas;
        }

        private List<Area> getAllAreas(string[] blockNames)
        {
            List<Area> areas = new List<Area>();

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


        private List<Area> getBoxAreas(List<BlockReference> blocks, Transaction trans)
        {
            List<Area> parse = new List<Area>();

            foreach (BlockReference block in blocks)
            {
                Extents3d blockExtents = block.GeometricExtents;
                Area area = new Area(blockExtents.MinPoint, blockExtents.MaxPoint);
                parse.Add(area);
            }

            parse = parse.OrderBy(o => o.Start.X).ToList();

            return parse;
        }


        private List<BlockReference> getSelectedBlockReference(string[] blockNames, Transaction trans)
        {
            List<BlockReference> refs = new List<BlockReference>();

            PromptSelectionOptions opts = new PromptSelectionOptions();
            opts.MessageForAdding = "\nSelect BLOCK " + blockNames[0] + " / " + blockNames[1] + " / " + blockNames[2];
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
                    acText.Height = 60;
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


        private void dump()
        {
            HostApplicationServices hs = HostApplicationServices.Current;
            string dwg_path = hs.FindFile(doc.Name, doc.Database, FindFileHint.Default);
            string dwg_dir = Path.GetDirectoryName(dwg_path);
            string dwg_name = Path.GetFileNameWithoutExtension(dwg_path);

            if (!dwg_dir.EndsWith(@"\")) { dwg_dir = dwg_dir + @"\"; }
            string csv_dir = dwg_dir + @"temp\";
            string csv_path = csv_dir + dwg_name + ".csv";

            if (total_stats == null || total_stats.Count == 0) return;

            if (!Directory.Exists(Path.GetDirectoryName(csv_path))) Directory.CreateDirectory(Path.GetDirectoryName(csv_path));
            if (File.Exists(csv_path)) { File.Delete(csv_path); }

            StringBuilder txt = new StringBuilder();

            writeCadMessage(csv_path);
            txt.AppendLine("alexi programmi ajutine file");
            txt.AppendLine("shape; positsioon; kogus; diameter");
            txt.AppendLine("");
            txt.AppendLine("SUMMARY");
            txt.AppendLine("");

            foreach (Mark u in total_stats)
            {
                if (u.Number == 0 && u.Diameter == 0) continue;
                txt.AppendLine(u.Position_Shape.ToString() + ";" + u.Position_Nr.ToString() + ";" + u.Number.ToString() + ";" + u.Diameter.ToString());
            }

            txt.AppendLine("");
            txt.AppendLine("---SUMMARY");

            int i = 1;
            foreach (Area a in local_stats.Keys)
            {
                txt.AppendLine("");
                txt.AppendLine("");
                txt.AppendLine("drawing nr: " + i.ToString());
                txt.AppendLine("");
                List<Mark> stats = local_stats[a];

                if (stats.Count == 0)
                {
                    txt.AppendLine("empty");
                }
                else
                {
                    foreach (Mark u in stats)
                    {
                        if (u.Number == 0 && u.Diameter == 0) continue;
                        txt.AppendLine(u.Position_Shape.ToString() + ";" + u.Position_Nr.ToString() + ";" + u.Number.ToString() + ";" + u.Diameter.ToString());
                    }
                }
                i++;
            }

            string csvText = txt.ToString();

            File.AppendAllText(csv_path, csvText);
        }
    }
}