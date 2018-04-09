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
        public _Area_v2 _23;
        
        public string _nr23;
        public string _date23;
        public string _rev23;
        public int _length;
        public int _height;

        public _Area_v2 _27;
        public string _nr27;
        public string _date27;
        public string _rev27;
        public string _sum_net;
        public string _sum_reinf;

        public element(_Area_v2 dim, _Area_v2 reinf, string nr23, string nr27)
        {
            _23 = dim;
            _27 = reinf;
            _nr23 = nr23;
            _nr27 = nr27;
        }

        public override string ToString()
        {
            string result = _nr23 + ";" + _date23 + ";" + _rev23 + ";" + ";" + _nr23.Replace("Vi23-", "V-").Replace("V23-", "V-") + ";" + _length.ToString() + ";" + _height.ToString() + ";" + ";" + ";"  + _nr27 + ";" + _date27 + ";" + _rev27 + ";" + ";" + _nr27.Replace("Vi27-", "V-").Replace("V27-", "V-") + ";" + ";" + _sum_net + ";" + _sum_reinf;
            return result;
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

        bool _open;

        public SUM_command_v2(bool open)
        {
            doc = Application.DocumentManager.MdiActiveDocument;
            db = doc.Database;
            ed = doc.Editor;

            trans = db.TransactionManager.StartTransaction();

            local_stats = new List<element>();

            _open = open;
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
            get27Data(a);
            get23Data(a);
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
                string ritn_nr_dim_a = "x";
                string ritn_nr_dim_b = "x";

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
                            if (ar.Tag == "RITN_23_NR") ritn_nr_dim_a = ar.TextString;
                            if (ar.Tag == "RITN_27_NR") ritn_nr_dim_b = ar.TextString;
                            if (ar.Tag == "RITN_NR") ritn_nr_dim_a = ar.TextString;
                            if (ar.Tag == "RITN_NR") ritn_nr_dim_b = ar.TextString;
                        }
                    }
                }

                foreach (_Area_v2 area_reinf in areas_reinf)
                {
                    string ritn_nr_reinf_a = "x";
                    string ritn_nr_reinf_b = "x";

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
                                if (ar.Tag == "RITN_23_NR") ritn_nr_reinf_a = ar.TextString;
                                if (ar.Tag == "RITN_27_NR") ritn_nr_reinf_b = ar.TextString;
                                if (ar.Tag == "RITN_NR") ritn_nr_reinf_a = ar.TextString;
                                if (ar.Tag == "RITN_NR") ritn_nr_reinf_b = ar.TextString;
                            }
                        }
                    }


                    if (ritn_nr_dim_a == ritn_nr_reinf_a)
                    {
                        if (ritn_nr_reinf_a != "x")
                        {
                            element el = new element(area_dim, area_reinf, ritn_nr_dim_a, ritn_nr_dim_b);
                            elements.Add(el);
                            break;
                        }
                    }
                }
            }

            return elements;
        }


        private void get23Data(List<element> elements)
        {
            foreach (element el in elements)
            {
                _Area_v2 area = el._23;

                string date = "xyz";
                string rev = "xyz";

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
                            if (ar.Tag == "DATUM") date = ar.TextString;
                            if (ar.Tag == "REVISION") rev = ar.TextString;
                        }
                    }
                }

                el._date23 = date;
                el._rev23 = rev;
            }

        }


        private void get27Data(List<element> elements)
        {
            foreach (element el in elements)
            {
                _Area_v2 area = el._27;

                string date = "xyz";
                string rev = "xyz";
                string net_weight = "xyz";
                string reinf_weight = "xyz";

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
                            if (ar.Tag == "DATUM") date = ar.TextString;
                            if (ar.Tag == "REVISION") rev = ar.TextString;
                            if (ar.Tag == "SUMMA_NATARMERING") net_weight = ar.TextString;
                            if (ar.Tag == "SUMMA_OVRIG_ARMERING") reinf_weight = ar.TextString;
                        }
                    }
                }
                
                el._date27 = date;
                el._rev27 = rev;
                el._sum_net = net_weight;
                el._sum_reinf = reinf_weight;
            }
        }


        private void getDimentions(List<element> elements)
        {
            List<RotatedDimension> allDims = getAllDims();

            foreach (element el in elements)
            {
                _Area_v2 area = el._23;

                List<RotatedDimension> sortedDims = getDimsInArea(area, allDims);

                double length = 0;
                double height = 0;
                foreach (RotatedDimension rd in sortedDims)
                {
                    double rot0 = Math.Abs(rd.Rotation % Math.PI);
                    double rot90 = Math.Abs(Math.Abs(rd.Rotation % Math.PI) - Math.PI / 2);
                    if (rot0 < 0.01)
                    {
                        if (length < rd.Measurement) length = rd.Measurement;
                    }
                    else if (rot90 < 0.01)
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
            string csv_dir = dwg_dir + @"temp_excel\";
            string csv_path = csv_dir + dwg_name + ".csv";

            if (local_stats == null || local_stats.Count == 0) return;

            if (!Directory.Exists(Path.GetDirectoryName(csv_path))) Directory.CreateDirectory(Path.GetDirectoryName(csv_path));
            if (File.Exists(csv_path)) { File.Delete(csv_path); }

            StringBuilder txt = new StringBuilder();

            writeCadMessage(csv_path);
            txt.AppendLine("alexi programmi ajutine file");
            txt.AppendLine("");
            txt.AppendLine("RITN_NR_23; DATUM_23; REV_23; REV_DATE_23; ELEMENT; LENGTH; HEIGHT; WIDTH; ; RITN_NR_27; DATUM_27; REV_27; REV_DATE_27; ELEMENT; ; SUMMA_NATARMERING; SUMMA_OVRIG_ARMERING");
            txt.AppendLine("");
            txt.AppendLine("SUMMARY");
            foreach (element e in local_stats)
            {
                txt.AppendLine(e.ToString());
            }

            txt.AppendLine("!---SUMMARY");

            string csvText = txt.ToString();

            File.AppendAllText(csv_path, csvText);

            if (_open)
            {
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
}