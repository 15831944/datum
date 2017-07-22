//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

////ODA
//using Teigha.Runtime;
//using Teigha.DatabaseServices;
//using Teigha.Geometry;

////Bricsys
//using Bricscad.ApplicationServices;
//using Bricscad.Runtime;
//using Bricscad.EditorInput;

//namespace commands
//{
//    class Area
//    {
//        Point3d start;
//        Point3d end;
//        Point3d reinf;
//        Point3d weight;

//        public Point3d Start { get { return start; } }
//        public Point3d End { get { return end; } }
//        public Point3d IP_reinf { get { return reinf; } }
//        public Point3d IP_weight { get { return weight; } }


//        public Area(Point3d s, Point3d e)
//        {
//            start = s;
//            end = e;

//            reinf = new Point3d(e.X - 2180, e.Y - 1460, e.Z);
//            weight = new Point3d(e.X - 780, e.Y - 5062, e.Z);
//        }

//        internal bool isPointInArea(Point3d point)
//        {
//            if (point.X < Start.X) return false;
//            if (point.X > End.X) return false;

//            if (point.Y < Start.Y) return false;
//            if (point.Y > End.Y) return false;

//            return true;
//        }
//    }
//}
