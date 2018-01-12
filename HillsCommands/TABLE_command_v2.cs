using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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
    class TABLE_command_v2
    {
        Document doc;
        Database db;
        Editor ed;

        Transaction trans;

        static string[] newBoxNames = { "KN-C", "KN-V27" };
        static string markLayerName = "K60";

        List<_Mark> total_stats;
        Dictionary<_Area_v2, List<_Mark>> local_stats;


        public TABLE_command_v2()
        {
            doc = Application.DocumentManager.MdiActiveDocument;
            db = doc.Database;
            ed = doc.Editor;

            trans = db.TransactionManager.StartTransaction();

            total_stats = new List<_Mark>();
            local_stats = new Dictionary<_Area_v2, List<_Mark>>();
        }


        public void run(bool multy)
        {
            writeCadMessage("[START]");

            List<_Area_v2> areas = new List<_Area_v2>();

            if (multy == true)
            {
                areas = getAllAreas(newBoxNames);
            }
            else
            {
                areas = getSelectedAreas(newBoxNames);
            }

            if (areas.Count < 1)
            {
                string names = string.Join(", ", newBoxNames.ToArray());
                writeCadMessage("[ERROR] - (" + names + ") not found");
            }

            List<_Mark> allMarks = getAllMarks(markLayerName);
            if (allMarks.Count < 1)
            {
                writeCadMessage("[ERROR] - " + "Reinforcement marks" + " not found");
                return;
            }

            local_stats = matchMarkToArea(areas, allMarks);

            return;
        }


        public void dump_csv()
        {
            total_stats = getGlobalSummary(local_stats);

            dump();

            writeCadMessage("[DONE]");

            return;
        }


        public void output_local()
        {
            foreach (_Area_v2 current in local_stats.Keys)
            {
                outputTable(current, local_stats[current]);
            }

            writeCadMessage("DONE");

            return;
        }


        internal void close()
        {
            trans.Commit();
            trans.Dispose();

            ed.Regen();
        }


        private List<_Mark> getGlobalSummary(Dictionary<_Area_v2, List<_Mark>> sorted)
        {
            List<_Mark> allValidMarks = new List<_Mark>();

            foreach (_Area_v2 area in sorted.Keys)
            {
                allValidMarks.AddRange(sorted[area]);
            }

            List<_Mark> summary = getSummary(allValidMarks);

            return summary;
        }


        private void outputTable(_Area_v2 a, List<_Mark> rows)
        {
            DBObject currentEntity = trans.GetObject(a.ID, OpenMode.ForWrite, false) as DBObject;

            if (currentEntity is BlockReference)
            {
                BlockReference blockRef = currentEntity as BlockReference;

                double scale = blockRef.ScaleFactors.X;
                string name = blockRef.Name;


                Point3d currentPoint = a.Start;

                if (name == newBoxNames[0])
                {
                    currentPoint = new Point3d(a.Start.X + (366 * scale), a.Start.Y + (141.8 * scale), a.Start.Z);
                }
                else if (name == newBoxNames[1])
                {
                    currentPoint = new Point3d(a.Start.X + (366 * scale), a.Start.Y + (263.5 * scale), a.Start.Z);
                }

                int counter = 0;
                foreach (_Mark r in rows)
                {
                    if (r.Position != "emptyrow")
                    {
                        double txtHeight = 1.5 * scale;

                        Point3d position_IP = new Point3d(currentPoint.X + (1 * scale), currentPoint.Y + (1.125 * scale), currentPoint.Z);
                        Point3d diameter_IP = new Point3d(currentPoint.X + (0.75 * scale) + (6.1 * scale), currentPoint.Y + (1.125 * scale), currentPoint.Z);
                        Point3d number_IP = new Point3d(currentPoint.X + (1.5 * scale) + (9.75 * scale), currentPoint.Y + (1.125 * scale), currentPoint.Z);

                        insertText(r.Position, position_IP, markLayerName, txtHeight);
                        insertText(r.Diameter.ToString(), diameter_IP, markLayerName, txtHeight);
                        insertText(r.Number.ToString(), number_IP, markLayerName, txtHeight);
                    }

                    if (counter == 21)
                    {
                        if (name == newBoxNames[0])
                        {
                            currentPoint = new Point3d(a.Start.X + (336 * scale), a.Start.Y + (141.8 * scale), a.Start.Z);
                        }
                        else if (name == newBoxNames[1])
                        {
                            currentPoint = new Point3d(a.Start.X + (336 * scale), a.Start.Y + (263.5 * scale), a.Start.Z);
                        }
                    }
                    else
                    {
                        currentPoint = new Point3d(currentPoint.X, currentPoint.Y - (4 * scale), currentPoint.Z);
                    }

                    counter++;

                }
            }
        }


        private Dictionary<_Area_v2, List<_Mark>> matchMarkToArea(List<_Area_v2> areas, List<_Mark> allMarks)
        {
            Dictionary<_Area_v2, List<_Mark>> sorted = new Dictionary<_Area_v2, List<_Mark>>();

            foreach (_Area_v2 area in areas)
            {
                List<_Mark> marks = getMarksInArea(area, allMarks);
                List<_Mark> validMarks = new List<_Mark>();

                foreach (_Mark m in marks)
                {
                    bool valid = m.validate();
                    if (valid) validMarks.Add(m);
                }

                List<_Mark> sumMarks = getSummary(validMarks);
                sorted[area] = sumMarks;
            }

            return sorted;
        }


        private List<_Mark> getSummary(List<_Mark> marks)
        {
            List<_Mark> sumMarks = new List<_Mark>();

            foreach (_Mark m in marks)
            {
                if (sumMarks.Contains(m))
                {
                    int index = sumMarks.FindIndex(a => a == m);
                    sumMarks[index].Number += m.Number;
                }
                else
                {
                    _Mark nm = new _Mark(m.Number, m.Diameter, m.Position, m.Position_Shape, m.Position_Nr);
                    sumMarks.Add(nm);
                }
            }

            sumMarks = sumMarks.OrderBy(b => b.Diameter).ToList();

            List<_Mark> rows_num = sumMarks.FindAll(x => x.Position_Shape == "A").ToList();
            List<_Mark> rows_char = sumMarks.FindAll(x => x.Position_Shape != "A").ToList();

            rows_num = rows_num.OrderBy(b => b.Position_Nr).ToList();
            rows_char = rows_char.OrderBy(b => b.Position_Nr).ToList();

            sumMarks = new List<_Mark>();
            sumMarks.AddRange(rows_num);

            _Mark emptyRow = new _Mark(0, 0, "emptyrow", "", 0);
            sumMarks.Add(emptyRow);
            sumMarks.Add(emptyRow);

            sumMarks.AddRange(rows_char);

            return sumMarks;
        }


        private List<_Mark> getAllMarks(string layer)
        {
            List<_Mark> marks = new List<_Mark>();

            List<MText> allTexts = getAllText(layer);
            marks = getMarkData(allTexts);

            return marks;
        }


        private List<_Mark> getMarksInArea(_Area_v2 area, List<_Mark> allmarks)
        {
            List<_Mark> marks = new List<_Mark>();

            for (int i = allmarks.Count - 1; i >= 0; i--)
            {
                _Mark mark = allmarks[i];
                if (area.isPointInArea(mark.IP))
                {
                    marks.Add(mark);
                    allmarks.RemoveAt(i);
                }
            }

            return marks;
        }


        private List<_Mark> getMarkData(List<MText> txts)
        {
            List<_Mark> parse = new List<_Mark>();

            foreach (MText txt in txts)
            {
                _Mark current = new _Mark(txt.Contents, txt.Location);
                parse.Add(current);
            }

            return parse;
        }


        private List<_Area_v2> getSelectedAreas(string[] blockNames)
        {
            List<_Area_v2> areas = new List<_Area_v2>();


            List<BlockReference> blocks = getSelectedBlockReference(blockNames);
            areas = getBoxAreas(blocks);


            return areas;
        }


        private List<_Area_v2> getAllAreas(string[] blockNames)
        {
            List<_Area_v2> areas = new List<_Area_v2>();

            List<BlockReference> blocks = new List<BlockReference>();

            foreach (string name in blockNames)
            {
                List<BlockReference> temp = getAllBlockReference(name);
                blocks.AddRange(temp);
            }

            areas = getBoxAreas(blocks);

            return areas;
        }


        private List<_Area_v2> getBoxAreas(List<BlockReference> blocks)
        {
            List<_Area_v2> parse = new List<_Area_v2>();

            foreach (BlockReference block in blocks)
            {
                Extents3d blockExtents = block.GeometricExtents;

                _Area_v2 area = new _Area_v2(block.ObjectId, blockExtents.MinPoint, blockExtents.MaxPoint);
                parse.Add(area);
            }

            parse = parse.OrderBy(o => o.Start.X).ToList();

            return parse;
        }


        private List<BlockReference> getSelectedBlockReference(string[] blockNames)
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


        private List<BlockReference> getAllBlockReference(string blockName)
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


        private List<MText> getAllText(string layer)
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


        private void insertText(string value, Point3d position, string layer, double txtHeight)
        {
            BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

            using (DBText acText = new DBText())
            {
                acText.Layer = layer;

                acText.Position = position;
                acText.Height = txtHeight;
                acText.TextString = value;

                btr.AppendEntity(acText);
                trans.AddNewlyCreatedDBObject(acText, true);
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

            foreach (_Mark u in total_stats)
            {
                if (u.Number == 0 && u.Diameter == 0) continue;
                txt.AppendLine(u.Position_Shape.ToString() + ";" + u.Position_Nr.ToString() + ";" + u.Number.ToString() + ";" + u.Diameter.ToString());
            }

            txt.AppendLine("---SUMMARY");

            // LOCAL

            foreach (_Area_v2 a in local_stats.Keys)
            {
                string ritn_nr = "x";

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
                            if (ar.Tag == "RITN_NR") ritn_nr = ar.TextString;

                            if (blockRef.Name == "KN-V23")
                            {
                                if (ar.Tag == "RITN_23_NR") ritn_nr = ar.TextString;
                            }
                            else if (blockRef.Name == "KN-V27")
                            {
                                if (ar.Tag == "RITN_27_NR") ritn_nr = ar.TextString;
                            }
                        }
                    }
                }


                txt.AppendLine("");
                txt.AppendLine("[" + ritn_nr + "]");
                List<_Mark> stats = local_stats[a];

                if (stats.Count == 0)
                {
                    txt.AppendLine("empty");
                }
                else
                {
                    foreach (_Mark u in stats)
                    {
                        if (u.Number == 0 && u.Diameter == 0) continue;
                        txt.AppendLine(u.Position_Shape.ToString() + ";" + u.Position_Nr.ToString() + ";" + u.Number.ToString() + ";" + u.Diameter.ToString());
                    }
                }
            }

            string csvText = txt.ToString();

            File.AppendAllText(csv_path, csvText);
        }

    }
}