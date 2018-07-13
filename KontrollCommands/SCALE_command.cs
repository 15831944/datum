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
    class SCALE_command
    {
        _CONNECTION _c;

        Dictionary<_Db.Dimension, _Db.BlockTableRecord> dims;


        public SCALE_command(ref _CONNECTION c)
        {
            _c = c;

            dims = new Dictionary<_Db.Dimension, _Db.BlockTableRecord>();
        }


        internal void run()
        {
            //getAllDims();
            //logic();
        }


        //private void logic()
        //{
        //    foreach (_Db.Dimension dim in dims.Keys)
        //    {
        //        if (dim.Dimlfac == 1.0)
        //        {

        //        }
        //        else if (dim.Dimlfac == 0.5)
        //        {
        //            createCircle(2000, 2, dim.TextPosition, dims[dim]);
        //            createCircle(200, 2, dim.TextPosition, dims[dim]);
        //            scale_05.Add(dim);

        //            changeFillColor(dim, 2);
        //        }
        //        else
        //        {
        //            createCircle(2000, 1, dim.TextPosition, dims[dim]);
        //            createCircle(200, 1, dim.TextPosition, dims[dim]);
        //            scale_other.Add(dim);

        //            changeFillColor(dim, 1);
        //        }                                
        //    }

        //    write("Scale 0.5: " + scale_05.Count.ToString());
        //    write("Scale other: " + scale_other.Count.ToString());
        //}

        
        //private void getAllDims()
        //{
        //    foreach (_Db.ObjectId btrId in _c.blockTable)
        //    {
        //        _Db.BlockTableRecord btr = _c.trans.GetObject(btrId, _Db.OpenMode.ForWrite) as _Db.BlockTableRecord;

        //        if (!(btr.IsFromExternalReference))
        //        {
        //            foreach (_Db.ObjectId bid in btr)
        //            {
        //                _Db.Entity currentEntity = _c.trans.GetObject(bid, _Db.OpenMode.ForWrite, false) as _Db.Entity;

        //                if (currentEntity == null)
        //                {
        //                    continue;
        //                }

        //                if (currentEntity is _Db.Dimension)
        //                {
        //                    _Db.Dimension dim = currentEntity as _Db.Dimension;
        //                    dims[dim] = btr;
        //                }
        //            }
        //        }
        //    }
        //}


        //private void createCircle(double radius, int index, _Ge.Point3d ip, _Db.BlockTableRecord btr)
        //{
        //    using (_Db.Circle circle = new _Db.Circle())
        //    {
        //        circle.Center = ip;
        //        circle.Radius = radius;
        //        circle.ColorIndex = index;
        //        btr.AppendEntity(circle);
        //        _c.trans.AddNewlyCreatedDBObject(circle, true);
        //    }
        //}


        //private void changeFillColor(_Db.Dimension dim, short index)
        //{
        //    dim.Dimtfill = 2;
        //    dim.Dimtfillclr = _Cm.Color.FromColorIndex(_Cm.ColorMethod.None, index);
        //}


        //private void write(string message)
        //{
        //    _c.ed.WriteMessage("\n" + message);
        //}

    }
}
