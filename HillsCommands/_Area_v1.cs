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
    class _Area_v1
    {
        _Ge.Point3d start;
        _Ge.Point3d end;
        _Ge.Point3d reinf;
        _Ge.Point3d weight;

        public _Ge.Point3d Start { get { return start; } }
        public _Ge.Point3d End { get { return end; } }
        public _Ge.Point3d IP_reinf { get { return reinf; } }
        public _Ge.Point3d IP_weight { get { return weight; } }


        public _Area_v1(_Ge.Point3d s, _Ge.Point3d e)
        {
            start = s;
            end = e;

            reinf = new _Ge.Point3d(e.X - 2180, e.Y - 1460, e.Z);
            weight = new _Ge.Point3d(e.X - 780, e.Y - 5062, e.Z);
        }


        internal bool isPointInArea(_Ge.Point3d point)
        {
            if (point.X < Start.X) return false;
            if (point.X > End.X) return false;

            if (point.Y < Start.Y) return false;
            if (point.Y > End.Y) return false;

            return true;
        }

    }
}
