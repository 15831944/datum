using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

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
    class LAYERS_command_v2
    {
        static string[] newBoxNames = { "KN-C", "KN-V23", "KN-V27" };

        List<Area_v2> local_stats;

        Document doc;
        Database db;
        Editor ed;

        public LAYERS_command_v2()
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
            
            createLayout(areas);

            writeCadMessage("DONE");

            return;
        }


        private void createLayout(List<Area_v2> areas)
        {
            foreach (Area_v2 area in areas)
            {
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
                                if (ar.Tag == "RITN_NR") ritn_nr = ar.TextString;
                            }
                        }

                        if (blockRef.Name == "KN-V23")
                        {
                            ritn_nr = "V23-" + ritn_nr;
                        }
                        else if (blockRef.Name == "KN-V27")
                        {
                            ritn_nr = "V27-" + ritn_nr;
                        }
                    }

                    LayoutManager layerManager = LayoutManager.Current;
                    ObjectId id = layerManager.CreateLayout(ritn_nr);
                    Layout layout = trans.GetObject(id, OpenMode.ForRead) as Layout;

                    if (layout.TabSelected == false)
                    {
                        layerManager.CurrentLayout = layout.LayoutName;
                    }

                    trans.Commit();
                }
            }

            ed.Regen();
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
    }
}