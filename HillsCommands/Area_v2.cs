using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    class Area_v2
    {
        ObjectId _id;
        Point3d _start;
        Point3d _end;

        public ObjectId ID { get { return _id; } }
        public Point3d Start { get { return _start; } } // for sorting


        public Area_v2(ObjectId id, Point3d s, Point3d e)
        {
            _id = id;

            _start = s;
            _end = e;
        }


        internal bool isPointInArea(Point3d point)
        {
            if (point.X <= _start.X) return false;
            if (point.X >= _end.X) return false;

            if (point.Y <= _start.Y) return false;
            if (point.Y >= _end.Y) return false;

            return true;
        }
    }
}
