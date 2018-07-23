//#define BRX_APP
#define ARX_APP

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
        _CONNECTION _c;

        static string[] dimBoxNames = { "KN-C", "KN-V23" };
        static string[] reinfBoxNames = { "KN-C", "KN-V27" };

        List<element> local_stats;

        bool _open;

        public SUM_command_v2(ref _CONNECTION c, bool open)
        {
            _c = c;

            local_stats = new List<element>();

            _open = open;
        }


        public void run()
        {
            List<_Area_v2> areas_dimentions = new List<_Area_v2>();
            List<_Area_v2> areas_reinf = new List<_Area_v2>();

            areas_dimentions = getAllAreas(dimBoxNames);
            areas_reinf = getAllAreas(reinfBoxNames);

            if (areas_dimentions.Count < 1)
            {
                string names = string.Join(", ", dimBoxNames.ToArray());
                throw new DMTException("[ERROR] - (" + names + ") not found");
            }

            if (areas_reinf.Count < 1)
            {
                string names = string.Join(", ", reinfBoxNames.ToArray());
                throw new DMTException("[ERROR] - (" + names + ") not found");
            }

            List<element> a = matchAreaToArea(areas_dimentions, areas_reinf);
            get27Data(a);
            get23Data(a);
            getDimentions(a);

            local_stats = a;
        }


        private List<element> matchAreaToArea(List<_Area_v2> areas_dimentions, List<_Area_v2> areas_reinf)
        {
            List<element> elements = new List<element>();

            foreach (_Area_v2 area_dim in areas_dimentions)
            {
                string ritn_nr_dim_a = "x";
                string ritn_nr_dim_b = "x";

                _Db.DBObject currentEntity = _c.trans.GetObject(area_dim.ID, _Db.OpenMode.ForWrite, false) as _Db.DBObject;

                if (currentEntity is _Db.BlockReference)
                {
                    _Db.BlockReference blockRef = currentEntity as _Db.BlockReference;

                    foreach (_Db.ObjectId arId in blockRef.AttributeCollection)
                    {
                        _Db.DBObject obj = _c.trans.GetObject(arId, _Db.OpenMode.ForWrite);
                        _Db.AttributeReference ar = obj as _Db.AttributeReference;
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

                    _Db.DBObject currentEntity_2 = _c.trans.GetObject(area_reinf.ID, _Db.OpenMode.ForWrite, false) as _Db.DBObject;

                    if (currentEntity_2 is _Db.BlockReference)
                    {
                        _Db.BlockReference blockRef = currentEntity_2 as _Db.BlockReference;

                        foreach (_Db.ObjectId arId in blockRef.AttributeCollection)
                        {
                            _Db.DBObject obj = _c.trans.GetObject(arId, _Db.OpenMode.ForWrite);
                            _Db.AttributeReference ar = obj as _Db.AttributeReference;
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

                _Db.DBObject currentEntity = _c.trans.GetObject(area.ID, _Db.OpenMode.ForWrite, false) as _Db.DBObject;

                if (currentEntity is _Db.BlockReference)
                {
                    _Db.BlockReference blockRef = currentEntity as _Db.BlockReference;

                    foreach (_Db.ObjectId arId in blockRef.AttributeCollection)
                    {
                        _Db.DBObject obj = _c.trans.GetObject(arId, _Db.OpenMode.ForWrite);
                        _Db.AttributeReference ar = obj as _Db.AttributeReference;
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

                _Db.DBObject currentEntity = _c.trans.GetObject(area.ID, _Db.OpenMode.ForWrite, false) as _Db.DBObject;

                if (currentEntity is _Db.BlockReference)
                {
                    _Db.BlockReference blockRef = currentEntity as _Db.BlockReference;

                    foreach (_Db.ObjectId arId in blockRef.AttributeCollection)
                    {
                        _Db.DBObject obj = _c.trans.GetObject(arId, _Db.OpenMode.ForWrite);
                        _Db.AttributeReference ar = obj as _Db.AttributeReference;
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
            List<_Db.RotatedDimension> allDims = getAllDims();

            foreach (element el in elements)
            {
                _Area_v2 area = el._23;

                List<_Db.RotatedDimension> sortedDims = getDimsInArea(area, allDims);

                double length = 0;
                double height = 0;

                foreach (_Db.RotatedDimension rd in sortedDims)
                {
                    double rot0 = Math.Abs(rd.Rotation % Math.PI);
                    double rot90 = Math.Abs(Math.Abs(rd.Rotation % Math.PI) - Math.PI / 2);
                    if (rot0 < 0.01)
                    {
                        if (length < rd.Measurement) length = rd.Measurement; // MEASURMENT REQUIRES TO SAVE
                    }
                    else if (rot90 < 0.01)
                    {
                        if (height < rd.Measurement) height = rd.Measurement; // MEASURMENT REQUIRES TO SAVE
                    }
                }

                el._length = (int)Math.Round(length, 0);
                el._height = (int)Math.Round(height, 0);
            }
        }


        private List<_Db.RotatedDimension> getDimsInArea(_Area_v2 area, List<_Db.RotatedDimension> allDims)
        {
            List<_Db.RotatedDimension> dims = new List<_Db.RotatedDimension>();

            for (int i = allDims.Count - 1; i >= 0; i--)
            {
                _Db.RotatedDimension dim = allDims[i];
                _Ge.Point3d p1 = dim.XLine1Point;

                if (area.isPointInArea(p1))
                {
                    dims.Add(dim);
                    allDims.RemoveAt(i);
                }
            }

            return dims;
        }


        private List<_Db.RotatedDimension> getAllDims()
        {
            List<_Db.RotatedDimension> dims = new List<_Db.RotatedDimension>();

            foreach (_Db.ObjectId btrId in _c.blockTable)
            {
                _Db.BlockTableRecord btr = _c.trans.GetObject(btrId, _Db.OpenMode.ForWrite) as _Db.BlockTableRecord;
                if (!(btr.IsFromExternalReference))
                {
                    foreach (_Db.ObjectId bid in btr)
                    {
                        _Db.Entity currentEntity = _c.trans.GetObject(bid, _Db.OpenMode.ForWrite, false) as _Db.Entity;

                        if (currentEntity == null)
                        {
                            continue;
                        }

                        if (currentEntity is _Db.RotatedDimension)
                        {
                            _Db.RotatedDimension dim = currentEntity as _Db.RotatedDimension;
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

            List<_Db.BlockReference> blocks = new List<_Db.BlockReference>();

            foreach (string name in blockNames)
            {
                List<_Db.BlockReference> temp = getAllBlockReference(name);
                blocks.AddRange(temp);
            }

            areas = getBoxAreas(blocks);

            return areas;
        }


        private List<_Area_v2> getBoxAreas(List<_Db.BlockReference> blocks)
        {
            List<_Area_v2> parse = new List<_Area_v2>();

            foreach (_Db.BlockReference block in blocks)
            {
                _Db.Extents3d blockExtents = block.GeometricExtents;

                _Area_v2 area = new _Area_v2(block.ObjectId, blockExtents.MinPoint, blockExtents.MaxPoint);
                parse.Add(area);
            }

            parse = parse.OrderBy(o => o.Start.X).ToList();

            return parse;
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


        public void dump_csv()
        {
            _Db.HostApplicationServices hs = _Db.HostApplicationServices.Current;
            string dwg_path = hs.FindFile(_c.doc.Name, _c.doc.Database, _Db.FindFileHint.Default);
            string dwg_dir = Path.GetDirectoryName(dwg_path);
            string dwg_name = Path.GetFileNameWithoutExtension(dwg_path);

            if (!dwg_dir.EndsWith(@"\")) { dwg_dir = dwg_dir + @"\"; }
            string csv_dir = dwg_dir + @"temp_excel\";
            string csv_path = csv_dir + dwg_name + ".csv";

            if (local_stats == null || local_stats.Count == 0) return;

            if (!Directory.Exists(Path.GetDirectoryName(csv_path))) Directory.CreateDirectory(Path.GetDirectoryName(csv_path));
            if (File.Exists(csv_path)) { File.Delete(csv_path); }

            StringBuilder txt = new StringBuilder();

            write(csv_path);
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


        private void write(string message)
        {
            _c.ed.WriteMessage("\n" + message);
        }

    }
}