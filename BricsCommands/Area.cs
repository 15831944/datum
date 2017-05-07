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
    class Area
    {
        Point3d start;
        Point3d end;

        public Point3d Start
        {
            get
            {
                return start;
            }
        }


        public Point3d End
        {
            get
            {
                return end;
            }
        }

        public Area(Point3d s, Point3d e)
        {
            start = s;
            end = e;
        }
    }
}
