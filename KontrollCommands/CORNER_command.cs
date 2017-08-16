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

//Bricsys
using Teigha.Runtime;
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Bricscad.ApplicationServices;
using Bricscad.Runtime;
using Bricscad.EditorInput;

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
                        createCircle(100, 1, p1, dims[dim]);
                        createCircle(150, 2, rdim.TextPosition, dims[dim]);
                        wrongs.Add(dim);
                    }
                    if (pp2 == false)
                    {
                        createCircle(100, 1, p2, dims[dim]);
                        createCircle(150, 2, rdim.TextPosition, dims[dim]);
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

                if (dL < 0.01) return true;
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

                if (currentEntity == null)
                {
                    continue;
                }
                else if (currentEntity is Polyline)
                {
                    Polyline poly = currentEntity as Polyline;
                    List<Point3d> polyPoints = getPolylinePoints(poly);
                    points.AddRange(polyPoints);
                }
                else if (currentEntity is Line)
                {
                    Line line = currentEntity as Line;
                    List<Point3d> linePoints = getLinePoints(line);
                    points.AddRange(linePoints);
                }
                else if (currentEntity is Arc)
                {
                    Arc arc = currentEntity as Arc;
                    List<Point3d> arcPoints = getArcPoints(arc);
                    points.AddRange(arcPoints);
                }
                else if (currentEntity is Circle)
                {
                    Circle circle = currentEntity as Circle;
                    List<Point3d> circlePoints = getCirclePoints(circle);
                    points.AddRange(circlePoints);
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
                    List<Point3d> btrPoints = getBlockTableRecordPoints(block);

                    double scale_X = blockRef.ScaleFactors.X;
                    double scale_Y = blockRef.ScaleFactors.Y;
                    double rotation = blockRef.Rotation;
                    foreach (Point3d p in btrPoints)
                    {
                        double scaled_X = p.X * scale_X;
                        double scaled_Y = p.Y * scale_Y;
                        double new_X = scaled_X * Math.Cos(rotation) - scaled_Y * Math.Sin(rotation);
                        double new_Y = scaled_X * Math.Sin(rotation) + scaled_Y * Math.Cos(rotation);
                        Point3d pp = new Point3d(new_X + blockRef.Position.X, new_Y + blockRef.Position.Y, 0);
                        points.Add(pp);
                    }
                    
                }
            }

            memory[btr] = points;

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

            points.Add(center);

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