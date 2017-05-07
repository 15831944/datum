using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//ODA
using Teigha.Runtime;
using Teigha.DatabaseServices;
using Teigha.Geometry;

//Bricsys
using Bricscad.ApplicationServices;
using Bricscad.Runtime;
using Bricscad.EditorInput;

namespace HillsCommands
{
    class testing
    {
        static string boxName = "Drawing_Area";

        public testing()
        {

        }

        public void run()
        {
            List<Area> areas = getAllAreas(boxName);
            if (areas.Count < 1)
            {
                writeCadMessage("ERROR - " + boxName + " not found");
                return;
            }

            foreach (Area current in areas)
            {
                dynamic comApp = Application.AcadApplication;
                ZoomWin(current.Start, current.End);
            }

            return;
        }


        private List<Area> getAllAreas(string blockName)
        {
            List<Area> areas = new List<Area>();

            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                List<BlockReference> blocks = getAllBlockReference(blockName, trans);
                areas = getBoxAreas(blocks, trans);
            }

            return areas;
        }

        private List<BlockReference> getAllBlockReference(string blockName, Transaction trans)
        {
            List<BlockReference> refs = new List<BlockReference>();

            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

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


        private List<Area> getBoxAreas(List<BlockReference> blocks, Transaction trans)
        {
            List<Area> parse = new List<Area>();

            foreach (BlockReference block in blocks)
            {
                Point3d min = block.Position;

                double dX = 0;
                double dY = 0;

                DynamicBlockReferencePropertyCollection aa = block.DynamicBlockReferencePropertyCollection;
                foreach (DynamicBlockReferenceProperty a in aa)
                {
                    if (a.PropertyName == "X Suund") dX = (double)a.Value;
                    else if (a.PropertyName == "Y Suund") dY = (double)a.Value;
                }

                if (dX != 0 || dY != 0)
                {
                    Point3d max = new Point3d(min.X + dX, min.Y + dY, 0);
                    Area area = new Area(min, max);

                    parse.Add(area);
                }
            }

            return parse;
        }

        private void writeCadMessage(string errorMessage)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            ed.WriteMessage("\n" + errorMessage);
        }

        private static void ZoomWin(Point3d min, Point3d max)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            Point2d min2d = new Point2d(min.X, min.Y);
            Point2d max2d = new Point2d(max.X, max.Y);

            ViewTableRecord view = new ViewTableRecord();

            view.CenterPoint = min2d + ((max2d - min2d) / 2.0);
            view.Height = max2d.Y - min2d.Y;
            view.Width = max2d.X - min2d.X;
            ed.SetCurrentView(view);
        }
    }
}