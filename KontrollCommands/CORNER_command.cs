using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DR = System.Drawing;

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
    class CORNER_command
    {
        Document doc;
        Database db;
        Editor ed;

        Transaction trans;

        Dictionary<Dimension, BlockTableRecord> dims;
        Dictionary<BlockTableRecord, List<Point3d>> memory;


        public CORNER_command()
        {
            doc = Application.DocumentManager.MdiActiveDocument;
            db = doc.Database;
            ed = doc.Editor;

            trans = db.TransactionManager.StartTransaction();

            dims = new Dictionary<Dimension, BlockTableRecord>();
            memory = new Dictionary<BlockTableRecord, List<Point3d>>();
        }


        internal void run()
        {
            writeCadMessage("START");

            getAllDims();
            logic2();

            writeCadMessage("END");
        }


        private void logic2()
        {
            List<Dimension> wrongs = new List<Dimension>();

            foreach (Dimension dim in dims.Keys)
            {
                BlockTableRecord btr = dims[dim];
                List<Point3d> points = getBlockTableRecordPoints(btr);

                if (dim is RotatedDimension)
                {
                    RotatedDimension rdim = dim as RotatedDimension;
                    Point3d p1 = rdim.XLine1Point;
                    Point3d p2 = rdim.XLine2Point;

                    bool pp1 = matchPoints(p1, points);
                    bool pp2 = matchPoints(p2, points);

                    if (pp1 == false)
                    {
                        createCircle(50, 1, p1, dims[dim]);
                        createCircle(150, 1, p1, dims[dim]);
                        createCircle(200, 2, rdim.TextPosition, dims[dim]);
                        wrongs.Add(dim);
                    }
                    if (pp2 == false)
                    {
                        createCircle(50, 1, p2, dims[dim]);
                        createCircle(150, 1, p2, dims[dim]);
                        createCircle(200, 2, rdim.TextPosition, dims[dim]);
                        wrongs.Add(dim);
                    }
                }
            }
            
            writeCadMessage("Vigade arv: " + wrongs.Count().ToString());
        }


        private bool matchPoints(Point3d point, List<Point3d> points)
        {
            foreach (Point3d p in points)
            {
                double dX = p.X - point.X;
                double dY = p.Y - point.Y;

                double dL = Math.Pow(Math.Pow(dX, 2) + Math.Pow(dY, 2), 0.5);

                if (dL < 0.05) return true;
            }

            return false;
        }


        private List<Point3d> getBlockTableRecordPoints(BlockTableRecord btr)
        {
            List<Point3d> points = new List<Point3d>();

            if (memory.Keys.Contains(btr))
            {
                return memory[btr];
            }

            foreach (ObjectId bid in btr)
            {
                Entity currentEntity = trans.GetObject(bid, OpenMode.ForWrite, false) as Entity;
                List<Point3d> currentPoints = handle(currentEntity);
                points.AddRange(currentPoints);
            }

            memory[btr] = points;

            return points;
        }

        private List<Point3d> handle(Entity ent)
        {
            List<Point3d> points = new List<Point3d>();

            if (ent == null)
            {
                return points;
            }

            if (ent is Curve && !(ent is Polyline || ent is Polyline2d || ent is Polyline3d))
            {
                Curve cur = ent as Curve;

                int segs = 3; //(ent is Line ? 3 : 20);

                double param = cur.EndParam - cur.StartParam;
                for (int i = 0; i < segs; i++)
                {
                    try
                    {
                        Point3d pt = cur.GetPointAtParameter(cur.StartParam + (i * param / (segs - 1)));
                        points.Add(pt);
                    }
                    catch { }
                }
            }
            else
            {
                DBObjectCollection objectCollection = new DBObjectCollection();
                try
                {
                    ent.Explode(objectCollection);
                    if (objectCollection.Count > 0)
                    {
                        foreach (DBObject bid in objectCollection)
                        {
                            Entity ent2 = bid as Entity;
                            if (ent2 != null && ent2.Visible)
                            {
                                List<Point3d> currentPoints = handle(ent2);
                                points.AddRange(currentPoints);
                            }
                            bid.Dispose();
                        }
                    }
                }
                catch { }
            }

            if (ent is Circle)
            {
                Circle circle = ent as Circle;
                List<Point3d> circlePoints = getCirclePoints(circle);
                points.AddRange(circlePoints);
            }
            else if (ent is Arc)
            {
                Arc arc = ent as Arc;
                List<Point3d> arcPoints = getArcPoints(arc);
                points.AddRange(arcPoints);
            }

            return points;
        }

        private List<Point3d> getPolylinePoints(Polyline poly)
        {
            List<Point3d> points = new List<Point3d>();

            int verts = poly.NumberOfVertices;

            for (int i = 1; i < verts; i++)
            {
                Point3d start = poly.GetPoint3dAt(i - 1);
                Point3d end = poly.GetPoint3dAt(i);

                double midX = start.X + ((end.X - start.X) / 2);
                double midY = start.Y + ((end.Y - start.Y) / 2);

                Point3d mid = new Point3d(midX, midY, 0);

                points.Add(start);
                points.Add(end);
                points.Add(mid);
            }

            if (poly.Closed)
            {
                Point3d start = poly.GetPoint3dAt(verts - 1);
                Point3d end = poly.GetPoint3dAt(0);

                double midX = start.X + ((end.X - start.X) / 2);
                double midY = start.Y + ((end.Y - start.Y) / 2);

                Point3d mid = new Point3d(midX, midY, 0);

                points.Add(mid);
            }

            return points;
        }

        private List<Point3d> getLinePoints(Line line)
        {
            List<Point3d> points = new List<Point3d>();

            Point3d start = line.StartPoint;
            Point3d end = line.EndPoint;

            double midX = start.X + ((end.X - start.X) / 2);
            double midY = start.Y + ((end.Y - start.Y) / 2);

            Point3d mid = new Point3d(midX, midY, 0);

            points.Add(start);
            points.Add(end);
            points.Add(mid);

            return points;
        }


        private List<Point3d> getArcPoints(Arc arc)
        {
            List<Point3d> points = new List<Point3d>();

            Point3d center = arc.Center;

            points.Add(center);

            return points;
        }

        private List<Point3d> getCirclePoints(Circle circle)
        {
            List<Point3d> points = new List<Point3d>();

            Point3d center = circle.Center;
            double r = circle.Radius;

            Point3d a = new Point3d(center.X + r, center.Y, 0);
            Point3d b = new Point3d(center.X - r, center.Y, 0);
            Point3d c = new Point3d(center.X, center.Y + r, 0);
            Point3d d = new Point3d(center.X, center.Y - r, 0);

            points.Add(center);
            points.Add(a);
            points.Add(b);
            points.Add(c);
            points.Add(d);

            return points;
        }

        private void getAllDims()
        {
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

                        if (currentEntity is Dimension)
                        {
                            Dimension dim = currentEntity as Dimension;
                            dims[dim] = btr;
                        }
                    }
                }
            }
        }


        private void createCircle(double radius, int index, Point3d ip, BlockTableRecord btr)
        {
            using (Circle circle = new Circle())
            {
                circle.Center = ip;
                circle.Radius = radius;
                circle.ColorIndex = index;
                btr.AppendEntity(circle);
                trans.AddNewlyCreatedDBObject(circle, true);
            }
        }


        internal void close()
        {
            trans.Commit();
            trans.Dispose();

            ed.Regen();
        }


        private void writeCadMessage(string errorMessage)
        {
            ed.WriteMessage("\n" + errorMessage);
        }
        
    }
}
