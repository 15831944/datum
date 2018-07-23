//#define BRX_APP
#define ARX_APP

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
    class NOPRINT_command
    {
        _CONNECTION _c;

        public static string[] ingoreLayers = { "Defpoints" };


        public NOPRINT_command(ref _CONNECTION c)
        {
            _c = c;            
        }


        internal void run()
        {
            logic();
        }


        private void logic()
        {
            List<string> wrongNames = new List<string>();

            _Db.LayerTable lt = _c.trans.GetObject(_c.db.LayerTableId, _Db.OpenMode.ForRead) as _Db.LayerTable;

            foreach (_Db.ObjectId layerId in lt)
            {
                _Db.LayerTableRecord layer = _c.trans.GetObject(layerId, _Db.OpenMode.ForWrite) as _Db.LayerTableRecord;

                if (!layer.IsPlottable)
                {
                    if (ingoreLayers.Contains(layer.Name)) continue;

                    wrongNames.Add(layer.Name);
                }                
            }

            write("No print layer count: " + wrongNames.Count.ToString());
            foreach (string name in wrongNames)
            {
                write(" - " + name);
            }            
        }


        private void write(string message)
        {
            _c.ed.WriteMessage("\n" + message);
        }

    }
}
