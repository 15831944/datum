using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

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
    class element
    {
        public _Area_v2 _dim;
        public _Area_v2 _reinf;

        public string _nr;

        public int _length;
        public int _height;

        public string _sum_net;
        public string _sum_reinf;

        public element(_Area_v2 dim, _Area_v2 reinf, string rig)
        {
            _dim = dim;
            _reinf = reinf;
            _nr = rig;
        }

    }

    class SUM_command_v2
    {
        Document doc;
        Database db;
        Editor ed;

        Transaction trans;

        static string[] dimBoxNames = { "KN-C", "KN-V23" };
        static string[] reinfBoxNames = { "KN-C", "KN-V27" };

        List<element> local_stats;


        public SUM_command_v2()
        {
            doc = Application.DocumentManager.MdiActiveDocument;
            db = doc.Database;
            ed = doc.Editor;

            trans = db.TransactionManager.StartTransaction();

            local_stats = new List<element>();
        }


        public void run()
        {
            writeCadMessage("[START]");

            List<_Area_v2> areas_dimentions = new List<_Area_v2>();
            List<_Area_v2> areas_reinf = new List<_Area_v2>();

            areas_dimentions = getAllAreas(dimBoxNames);
            areas_reinf = getAllAreas(reinfBoxNames);

            if (areas_dimentions.Count < 1)
            {
                string names = string.Join(", ", dimBoxNames.ToArray());
                writeCadMessage("[ERROR] - (" + names + ") not found");
                return;
            }

            if (areas_reinf.Count < 1)
            {
                string names = string.Join(", ", reinfBoxNames.ToArray());
                writeCadMessage("[ERROR] - (" + names + ") not found");
                return;
            }

            List<element> a = matchAreaToArea(areas_dimentions, areas_reinf);
            getWeights(a);
            getDimentions(a);

            local_stats = a;

            return;
        }


        internal void close()
        {
            trans.Commit();
            trans.Dispose();

            ed.Regen();
        }


        public void dump_csv()
        {
            dump();

            writeCadMessage("[DONE]");

            return;
        }


        private List<element> matchAreaToArea(List<_Area_v2> areas_dimentions, List<_Area_v2> areas_reinf)
        {
            List<element> elements = new List<element>();

            foreach (_Area_v2 area_dim in areas_dimentions)
            {
                string ritn_nr_dim = "x";

                DBObject currentEntity = trans.GetObject(area_dim.ID, OpenMode.ForWrite, false) as DBObject;

                if (currentEntity is BlockReference)
                {
                    BlockReference blockRef = currentEntity as BlockReference;

                    foreach (ObjectId arId in blockRef.AttributeCollection)
                    {
                        DBObject obj = trans.GetObject(arId, OpenMode.ForWrite);
                        AttributeReference ar = obj as AttributeReference;
                        if (ar != null)
                        {
                            if (ar.Tag == "RITN_27_NR") ritn_nr_dim = ar.TextString;
                            if (ar.Tag == "RITN_NR") ritn_nr_dim = ar.TextString;
                        }
                    }
                }

                foreach (_Area_v2 area_reinf in areas_reinf)
                {
                    string ritn_nr_reinf = "x";

                    DBObject currentEntity_2 = trans.GetObject(area_reinf.ID, OpenMode.ForWrite, false) as DBObject;

                    if (currentEntity_2 is BlockReference)
                    {
                        BlockReference blockRef = currentEntity_2 as BlockReference;

                        foreach (ObjectId arId in blockRef.AttributeCollection)
                        {
                            DBObject obj = trans.GetObject(arId, OpenMode.ForWrite);
                            AttributeReference ar = obj as AttributeReference;
                            if (ar != null)
                            {
                                if (ar.Tag == "RITN_27_NR") ritn_nr_reinf = ar.TextString;
                                if (ar.Tag == "RITN_NR") ritn_nr_reinf = ar.TextString;
                            }
                        }
                    }


                    if (ritn_nr_dim == ritn_nr_reinf)
                    {
                        if (ritn_nr_reinf != "x")
                        {
                            element el = new element(area_dim, area_reinf, ritn_nr_dim);
                            elements.Add(el);
                            break;
                        }
                    }
                }
            }

            return elements;
        }


        private void getWeights(List<element> elements)
        {
            foreach (element el in elements)
            {
                _Area_v2 area = el._reinf;

                string net_weight = "y";
                string reinf_weight = "z";

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
                            if (ar.Tag == "SUMMA_NATARMERING") net_weight = ar.TextString;
                            if (ar.Tag == "SUMMA_OVRIG_ARMERING") reinf_weight = ar.TextString;
                        }
                    }
                }

                writeCadMessage(net_weight);
                el._sum_net = net_weight;
                el._sum_reinf = reinf_weight;
            }

        }


        private void getDimentions(List<element> elements)
        {
            List<RotatedDimension> allDims = getAllDims();

            foreach (element el in elements)
            {
                _Area_v2 area = el._dim;

                List<RotatedDimension> sortedDims = getDimsInArea(area, allDims);

                double length = 0;
                double height = 0;
                foreach (RotatedDimension rd in sortedDims)
                {
                    if (rd.Rotation % Math.PI == 0)
                    {
                        if (length < rd.Measurement) length = rd.Measurement;
                    }
                    else if (rd.Rotation % Math.PI == Math.PI / 2)
                    {
                        if (height < rd.Measurement) height = rd.Measurement;
                    }
                }

                el._length = (int)Math.Round(length, 0);
                el._height = (int)Math.Round(height, 0);
            }
        }


        private List<RotatedDimension> getDimsInArea(_Area_v2 area, List<RotatedDimension> allDims)
        {
            List<RotatedDimension> dims = new List<RotatedDimension>();

            for (int i = allDims.Count - 1; i >= 0; i--)
            {
                RotatedDimension dim = allDims[i];
                Point3d p1 = dim.XLine1Point;

                if (area.isPointInArea(p1))
                {
                    dims.Add(dim);
                    allDims.RemoveAt(i);
                }
            }

            return dims;
        }


        private List<RotatedDimension> getAllDims()
        {
            List<RotatedDimension> dims = new List<RotatedDimension>();

            BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForWrite) as BlockTable;
            foreach (ObjectId btrId in bt)
            {
                BlockTableRecord btr = trans.GetObject(btrId, OpenMode.ForWrite) as BlockTableRecord;
                if (!(btr.IsFromExternalReference))
                {
                    foreach (ObjectId bid in btr)
                    {
                        Entity currentEntity = trans.GetObject(bid, OpenMode.ForWrite, false) as Entity;

                        if (currentEntity == null)
                        {
                            continue;
                        }

                        if (currentEntity is RotatedDimension)
                        {
                            RotatedDimension dim = currentEntity as RotatedDimension;
                            dims.Add(dim);
                        }
                    }
                }
            }

            return dims;
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
            string csv_dir = dwg_dir + @"weights\";
            string csv_path = csv_dir + dwg_name + ".csv";

            if (local_stats == null || local_stats.Count == 0) return;

            if (!Directory.Exists(Path.GetDirectoryName(csv_path))) Directory.CreateDirectory(Path.GetDirectoryName(csv_path));
            if (File.Exists(csv_path)) { File.Delete(csv_path); }

            StringBuilder txt = new StringBuilder();

            writeCadMessage(csv_path);
            txt.AppendLine("alexi programmi ajutine file");
            txt.AppendLine("");
            txt.AppendLine("RITN_NR; LENGTH; HEIGHT; WIDTH; SUMMA_NATARMERING; SUMMA_OVRIG_ARMERING");

            foreach (element e in local_stats)
            {
                txt.AppendLine(e._nr + ";" + e._length.ToString() + ";" + e._height.ToString() + ";" + ";" + e._sum_net + ";" + e._sum_reinf);
            }

            string csvText = txt.ToString();

            File.AppendAllText(csv_path, csvText);

            try
            {
                Process.Start(csv_path);
            }
            catch
            {

            }
        }

    }
}