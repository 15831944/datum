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
    class CORNER_command
    {
        _CONNECTION _c;

        Dictionary<_Db.Dimension, _Db.BlockTableRecord> dims;
        Dictionary<_Db.BlockTableRecord, List<_Ge.Point3d>> memory;


        public CORNER_command(ref _CONNECTION c)
        {
            _c = c;

            dims = new Dictionary<_Db.Dimension, _Db.BlockTableRecord>();
            memory = new Dictionary<_Db.BlockTableRecord, List<_Ge.Point3d>>();
        }


        internal void run()
        {
            getAllDims();
            logic();
        }


        private void logic()
        {
            List<_Db.Dimension> wrongs = new List<_Db.Dimension>();

            foreach (_Db.Dimension dim in dims.Keys)
            {
                _Db.BlockTableRecord btr = dims[dim];
                List<_Ge.Point3d> points = getBlockTableRecordPoints(btr);

                if (dim is _Db.RotatedDimension)
                {

                    _Db.RotatedDimension rdim = dim as _Db.RotatedDimension;
                    _Ge.Point3d p1 = rdim.XLine1Point;
                    _Ge.Point3d p2 = rdim.XLine2Point;

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
            
            write("Vigade arv: " + wrongs.Count().ToString());
        }


        private bool matchPoints(_Ge.Point3d point, List<_Ge.Point3d> points)
        {
            foreach (_Ge.Point3d p in points)
            {
                double dX = p.X - point.X;
                double dY = p.Y - point.Y;

                double dL = Math.Pow(Math.Pow(dX, 2) + Math.Pow(dY, 2), 0.5);

                if (dL < 0.05) return true;
            }

            return false;
        }


        private List<_Ge.Point3d> getBlockTableRecordPoints(_Db.BlockTableRecord btr)
        {
            List<_Ge.Point3d> points = new List<_Ge.Point3d>();

            if (memory.Keys.Contains(btr))
            {
                return memory[btr];
            }

            foreach (_Db.ObjectId bid in btr)
            {
                _Db.Entity currentEntity = _c.trans.GetObject(bid, _Db.OpenMode.ForWrite, false) as _Db.Entity;
                List<_Ge.Point3d> currentPoints = handle(currentEntity);
                points.AddRange(currentPoints);
            }
            
            memory[btr] = points;
            
            return points;
        }


        private List<_Ge.Point3d> handle(_Db.Entity ent)
        {
            List<_Ge.Point3d> points = new List<_Ge.Point3d>();
            
            if (ent == null)
            {
                return points;
            }


            if (ent is _Db.Curve && !(ent is _Db.Polyline || ent is _Db.Polyline2d || ent is _Db.Polyline3d))
            {
                try
                {
                    _Db.Curve cur = ent as _Db.Curve;

                    int segs = 3; //(ent is Line ? 3 : 20);

                    double param = cur.EndParam - cur.StartParam;

                    for (int i = 0; i < segs; i++)
                    {
                        try
                        {
                            _Ge.Point3d pt = cur.GetPointAtParameter(cur.StartParam + (i * param / (segs - 1)));
                            points.Add(pt);
                        }
                        catch { }
                    }
                }
                catch { }
            }
            else
            {
                _Db.DBObjectCollection objectCollection = new _Db.DBObjectCollection();
                try
                {
                    ent.Explode(objectCollection);
                    if (objectCollection.Count > 0)
                    {
                        foreach (_Db.DBObject bid in objectCollection)
                        {
                            _Db.Entity ent2 = bid as _Db.Entity;
                            if (ent2 != null && ent2.Visible)
                            {
                                List<_Ge.Point3d> currentPoints = handle(ent2);
                                points.AddRange(currentPoints);
                            }
                            bid.Dispose();
                        }
                    }
                }
                catch { }
            }

            if (ent is _Db.Circle)
            {
                _Db.Circle circle = ent as _Db.Circle;
                List<_Ge.Point3d> circlePoints = getCirclePoints(circle);
                points.AddRange(circlePoints);
            }
            else if (ent is _Db.Arc)
            {
                _Db.Arc arc = ent as _Db.Arc;
                List<_Ge.Point3d> arcPoints = getArcPoints(arc);
                points.AddRange(arcPoints);
            }

            return points;
        }


        private List<_Ge.Point3d> getPolylinePoints(_Db.Polyline poly)
        {
            List<_Ge.Point3d> points = new List<_Ge.Point3d>();

            int verts = poly.NumberOfVertices;

            for (int i = 1; i < verts; i++)
            {
                _Ge.Point3d start = poly.GetPoint3dAt(i - 1);
                _Ge.Point3d end = poly.GetPoint3dAt(i);

                double midX = start.X + ((end.X - start.X) / 2);
                double midY = start.Y + ((end.Y - start.Y) / 2);

                _Ge.Point3d mid = new _Ge.Point3d(midX, midY, 0);

                points.Add(start);
                points.Add(end);
                points.Add(mid);
            }

            if (poly.Closed)
            {
                _Ge.Point3d start = poly.GetPoint3dAt(verts - 1);
                _Ge.Point3d end = poly.GetPoint3dAt(0);

                double midX = start.X + ((end.X - start.X) / 2);
                double midY = start.Y + ((end.Y - start.Y) / 2);

                _Ge.Point3d mid = new _Ge.Point3d(midX, midY, 0);

                points.Add(mid);
            }

            return points;
        }


        private List<_Ge.Point3d> getLinePoints(_Db.Line line)
        {
            List<_Ge.Point3d> points = new List<_Ge.Point3d>();

            _Ge.Point3d start = line.StartPoint;
            _Ge.Point3d end = line.EndPoint;

            double midX = start.X + ((end.X - start.X) / 2);
            double midY = start.Y + ((end.Y - start.Y) / 2);

            _Ge.Point3d mid = new _Ge.Point3d(midX, midY, 0);

            points.Add(start);
            points.Add(end);
            points.Add(mid);

            return points;
        }


        private List<_Ge.Point3d> getArcPoints(_Db.Arc arc)
        {
            List<_Ge.Point3d> points = new List<_Ge.Point3d>();

            _Ge.Point3d center = arc.Center;

            points.Add(center);

            return points;
        }


        private List<_Ge.Point3d> getCirclePoints(_Db.Circle circle)
        {
            List<_Ge.Point3d> points = new List<_Ge.Point3d>();

            _Ge.Point3d center = circle.Center;
            double r = circle.Radius;

            _Ge.Point3d a = new _Ge.Point3d(center.X + r, center.Y, 0);
            _Ge.Point3d b = new _Ge.Point3d(center.X - r, center.Y, 0);
            _Ge.Point3d c = new _Ge.Point3d(center.X, center.Y + r, 0);
            _Ge.Point3d d = new _Ge.Point3d(center.X, center.Y - r, 0);

            points.Add(center);
            points.Add(a);
            points.Add(b);
            points.Add(c);
            points.Add(d);

            return points;
        }


        private void getAllDims()
        {
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

                        if (currentEntity is _Db.Dimension)
                        {
                            _Db.Dimension dim = currentEntity as _Db.Dimension;
                            dims[dim] = btr;
                        }
                    }
                }                
            }
        }


        private void createCircle(double radius, int index, _Ge.Point3d ip, _Db.BlockTableRecord btr)
        {
            using (_Db.Circle circle = new _Db.Circle())
            {
                circle.Center = ip;
                circle.Radius = radius;
                circle.ColorIndex = index;
                btr.AppendEntity(circle);
                _c.trans.AddNewlyCreatedDBObject(circle, true);
            }
        }


        private void write(string message)
        {
            _c.ed.WriteMessage("\n" + message);
        }

    }
}
