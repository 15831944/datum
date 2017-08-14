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

//Bricsys
using Teigha.Runtime;
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Bricscad.ApplicationServices;
using Bricscad.Runtime;
using Bricscad.EditorInput;

namespace commands
{
    class SUM_command_v2
    {
        static string[] newBoxNames = { "KN-C", "KN-V27" };

        List<Area_v2> local_stats;

        Document doc;
        Database db;
        Editor ed;

        public SUM_command_v2()
        {
            local_stats = new List<Area_v2>();

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

            local_stats = areas;

            return;
        }


        public void dump_csv()
        {
            dump();

            writeCadMessage("DONE");

            return;
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
            txt.AppendLine("RITN_NR; SUMMA_NATARMERING; SUMMA_OVRIG_ARMERING");
            txt.AppendLine("");

            foreach (Area_v2 a in local_stats)
            {
                string ritn_nr = "x";
                string net_weight = "y";
                string reinf_weight = "z";
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
                                if (ar.Tag == "RITN_NR") ritn_nr = ar.TextString;
                                if (ar.Tag == "SUMMA_NATARMERING") net_weight = ar.TextString;
                                if (ar.Tag == "SUMMA_OVRIG_ARMERING") reinf_weight = ar.TextString;
                            }
                        }
                    }
                }

                txt.AppendLine(ritn_nr + ";" + net_weight + ";" + reinf_weight);
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