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


namespace commands
{
    class TABLE_command_v1
    {
        _CONNECTION _c;

        static string[] boxNames = { "KN-A", "KN-C", "KN-V27" };
        static string markLayerName = "K60";

        List<_Mark> total_stats;
        Dictionary<_Area_v1, List<_Mark>> local_stats;


        public TABLE_command_v1(ref _CONNECTION c)
        {
            _c = c;

            total_stats = new List<_Mark>();
            local_stats = new Dictionary<_Area_v1, List<_Mark>>();
        }


        public void run(bool multy)
        {
            List<_Area_v1> areas = new List<_Area_v1>();

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
                throw new DMTException("[ERROR] - (" + names + ") not found");
            }

            List<_Mark> allMarks = getAllMarks(markLayerName);
            if (allMarks.Count < 1) throw new DMTException("ERROR - " + "Reinforcement marks" + " not found");

            local_stats = matchMarkArea(areas, allMarks);

            return;
        }


        public void dump_csv()
        {
            total_stats = getGlobalSummary(local_stats);

            dump();

            return;
        }


        public void output_local()
        {
            foreach (_Area_v1 current in local_stats.Keys)
            {
                outputTable(current, local_stats[current]);
            }            

            return;
        }


        private List<_Mark> getGlobalSummary(Dictionary<_Area_v1, List<_Mark>> sorted)
        {
            List<_Mark> allValidMarks = new List<_Mark>();

            foreach (_Area_v1 area in sorted.Keys)
            {
                allValidMarks.AddRange(sorted[area]);
            }

            List<_Mark> summary = getSummary(allValidMarks);

            return summary;
        }


        private void outputTable(_Area_v1 a, List<_Mark> rows)
        {
            _Ge.Point3d currentPoint = a.IP_reinf;

            foreach (_Mark r in rows)
            {
                if (r.Position != "emptyrow")
                {
                    _Ge.Point3d position_IP = new _Ge.Point3d(currentPoint.X + 40, currentPoint.Y + 45, currentPoint.Z);
                    _Ge.Point3d diameter_IP = new _Ge.Point3d(currentPoint.X + 30 + 244, currentPoint.Y + 45, currentPoint.Z);
                    _Ge.Point3d number_IP = new _Ge.Point3d(currentPoint.X + 60 + 390, currentPoint.Y + 45, currentPoint.Z);

                    insertText(r.Position, position_IP, markLayerName);
                    insertText(r.Diameter.ToString(), diameter_IP, markLayerName);
                    insertText(r.Number.ToString(), number_IP, markLayerName);
                }

                currentPoint = new _Ge.Point3d(currentPoint.X, currentPoint.Y - 160, currentPoint.Z);
            }
        }


        private Dictionary<_Area_v1, List<_Mark>> matchMarkArea(List<_Area_v1> areas, List<_Mark> allMarks)
        {
            Dictionary<_Area_v1, List<_Mark>> sorted = new Dictionary<_Area_v1, List<_Mark>>();

            foreach (_Area_v1 area in areas)
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

            List<_Db.MText> allTexts = getAllText(layer);
            marks = getMarkData(allTexts);

            return marks;
        }


        private List<_Mark> getMarksInArea(_Area_v1 area, List<_Mark> allmarks)
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


        private List<_Mark> getMarkData(List<_Db.MText> txts)
        {
            List<_Mark> parse = new List<_Mark>();

            foreach (_Db.MText txt in txts)
            {
                _Mark current = new _Mark(txt.Contents, txt.Location);
                parse.Add(current);
            }

            return parse;
        }

        private List<_Area_v1> getSelectedAreas(string[] blockNames)
        {
            List<_Area_v1> areas = new List<_Area_v1>();
            
            List<_Db.BlockReference> blocks = getSelectedBlockReference(blockNames);
            areas = getBoxAreas(blocks);
            
            return areas;
        }


        private List<_Area_v1> getAllAreas(string[] blockNames)
        {
            List<_Area_v1> areas = new List<_Area_v1>();

            List<_Db.BlockReference> blocks = new List<_Db.BlockReference>();

            foreach (string name in blockNames)
            {
                List<_Db.BlockReference> temp = getAllBlockReference(name);
                blocks.AddRange(temp);
            }

            areas = getBoxAreas(blocks);


            return areas;
        }


        private List<_Area_v1> getBoxAreas(List<_Db.BlockReference> blocks)
        {
            List<_Area_v1> parse = new List<_Area_v1>();

            foreach (_Db.BlockReference block in blocks)
            {
                _Db.Extents3d blockExtents = block.GeometricExtents;
                _Area_v1 area = new _Area_v1(blockExtents.MinPoint, blockExtents.MaxPoint);
                parse.Add(area);
            }

            parse = parse.OrderBy(o => o.Start.X).ToList();

            return parse;
        }


        private List<_Db.BlockReference> getSelectedBlockReference(string[] blockNames)
        {
            List<_Db.BlockReference> refs = new List<_Db.BlockReference>();

            _Ed.PromptSelectionOptions opts = new _Ed.PromptSelectionOptions();
            opts.MessageForAdding = "\nSelect BLOCK " + blockNames[0] + " / " + blockNames[1] + " / " + blockNames[2];
            _Ed.PromptSelectionResult selection = _c.ed.GetSelection(opts);

            if (selection.Status == _Ed.PromptStatus.OK)
            {
                _Db.ObjectId[] selectionIds = selection.Value.GetObjectIds();

                foreach (_Db.ObjectId id in selectionIds)
                {
                    _Db.DBObject currentEntity = _c.trans.GetObject(id, _Db.OpenMode.ForWrite, false) as _Db.DBObject;

                    if (currentEntity == null)
                    {
                        continue;
                    }

                    else if (currentEntity is _Db.BlockReference)
                    {
                        _Db.BlockReference blockRef = currentEntity as _Db.BlockReference;

                        _Db.BlockTableRecord block = null;
                        if (blockRef.IsDynamicBlock)
                        {
                            block = _c.trans.GetObject(blockRef.DynamicBlockTableRecord, _Db.OpenMode.ForRead) as _Db.BlockTableRecord;
                        }
                        else
                        {
                            block = _c.trans.GetObject(blockRef.BlockTableRecord, _Db.OpenMode.ForRead) as _Db.BlockTableRecord;
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


        private List<_Db.BlockReference> getAllBlockReference(string blockName)
        {
            List<_Db.BlockReference> refs = new List<_Db.BlockReference>();

            if (_c.blockTable.Has(blockName))
            {
                _Db.BlockTableRecord btr = _c.trans.GetObject(_c.modelSpace.Id, _Db.OpenMode.ForWrite) as _Db.BlockTableRecord;

                foreach (_Db.ObjectId id in btr)
                {
                    _Db.DBObject currentEntity = _c.trans.GetObject(id, _Db.OpenMode.ForWrite, false) as _Db.DBObject;

                    if (currentEntity == null)
                    {
                        continue;
                    }

                    else if (currentEntity is _Db.BlockReference)
                    {
                        _Db.BlockReference blockRef = currentEntity as _Db.BlockReference;

                        _Db.BlockTableRecord block = null;
                        if (blockRef.IsDynamicBlock)
                        {
                            block = _c.trans.GetObject(blockRef.DynamicBlockTableRecord, _Db.OpenMode.ForRead) as _Db.BlockTableRecord;
                        }
                        else
                        {
                            block = _c.trans.GetObject(blockRef.BlockTableRecord, _Db.OpenMode.ForRead) as _Db.BlockTableRecord;
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


        private List<_Db.MText> getAllText(string layer)
        {
            List<_Db.MText> txt = new List<_Db.MText>();

            _Db.BlockTableRecord btr = _c.trans.GetObject(_c.modelSpace.Id, _Db.OpenMode.ForWrite) as _Db.BlockTableRecord;

            foreach (_Db.ObjectId id in btr)
            {
                _Db.Entity currentEntity = _c.trans.GetObject(id, _Db.OpenMode.ForWrite, false) as _Db.Entity;

                if (currentEntity != null)
                {
                    if (currentEntity is _Db.MText)
                    {
                        _Db.MText br = currentEntity as _Db.MText;
                        if (br.Layer == layer)
                        {
                            txt.Add(br);
                        }
                    }

                    if (currentEntity is _Db.DBText)
                    {
                        _Db.DBText br = currentEntity as _Db.DBText;
                        if (br.Layer == layer)
                        {
                            _Db.MText myMtext = new _Db.MText();
                            myMtext.Contents = br.TextString;
                            myMtext.Location = br.Position;
                            txt.Add(myMtext);
                        }
                    }

                    if (currentEntity is _Db.MLeader)
                    {
                        _Db.MLeader br = currentEntity as _Db.MLeader;
                        if (br.Layer == layer)
                        {
                            if (br.ContentType == _Db.ContentType.MTextContent)
                            {
                                _Db.MText leaderText = br.MText;
                                txt.Add(leaderText);
                            }
                        }
                    }
                }
            }

            return txt;
        }


        private void insertText(string value, _Ge.Point3d position, string layer)
        {
            _Db.BlockTableRecord btr = _c.trans.GetObject(_c.modelSpace.Id, _Db.OpenMode.ForWrite) as _Db.BlockTableRecord;

            using (_Db.DBText acText = new _Db.DBText())
            {
                acText.Layer = layer;

                acText.Position = position;
                acText.Height = 60;
                acText.TextString = value;

                btr.AppendEntity(acText);
                _c.trans.AddNewlyCreatedDBObject(acText, true);
            }
        }


        private void dump()
        {
            _Db.HostApplicationServices hs = _Db.HostApplicationServices.Current;
            string dwg_path = hs.FindFile(_c.doc.Name, _c.doc.Database, _Db.FindFileHint.Default);
            string dwg_dir = Path.GetDirectoryName(dwg_path);
            string dwg_name = Path.GetFileNameWithoutExtension(dwg_path);

            if (!dwg_dir.EndsWith(@"\")) { dwg_dir = dwg_dir + @"\"; }
            string csv_dir = dwg_dir + @"temp_armering\";
            string csv_path = csv_dir + dwg_name + ".csv";

            if (total_stats == null || total_stats.Count == 0) return;

            if (!Directory.Exists(Path.GetDirectoryName(csv_path))) Directory.CreateDirectory(Path.GetDirectoryName(csv_path));
            if (File.Exists(csv_path)) { File.Delete(csv_path); }

            StringBuilder txt = new StringBuilder();

            write(csv_path);
            txt.AppendLine("alexi programmi ajutine file");
            txt.AppendLine("shape; positsioon; kogus; diameter");
            txt.AppendLine("");
            txt.AppendLine("SUMMARY");
            txt.AppendLine("");

            foreach (_Mark u in total_stats)
            {
                if (u.Number == 0 && u.Diameter == 0) continue;
                txt.AppendLine(u.Position_Shape.ToString() + ";" + u.Position_Nr.ToString() + ";" + u.Number.ToString() + ";" + u.Diameter.ToString());
            }

            txt.AppendLine("");
            txt.AppendLine("!---SUMMARY");

            int i = 1;
            foreach (_Area_v1 a in local_stats.Keys)
            {
                txt.AppendLine("");
                txt.AppendLine("");
                txt.AppendLine("drawing nr: " + i.ToString());
                txt.AppendLine("");
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
                i++;
            }

            string csvText = txt.ToString();

            File.AppendAllText(csv_path, csvText);
        }


        private void write(string message)
        {
            _c.ed.WriteMessage("\n" + message);
        }

    }
}