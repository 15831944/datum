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
    class TXT_command
    {
        _CONNECTION _c;

        string[] forbiddenChars = { "{", "}", "|", ";" };

        string kontrollLayer = "_AUTO_KONTROLL_";


        public TXT_command(ref _CONNECTION c)
        {
            _c = c;
        }


        internal void run()
        {
            List<_Db.MText> allTexts = getAllText();
            List<_Db.MText> wrong = checkText(allTexts);
            drawWrongMarks(wrong);
        }


        private List<_Db.MText> checkText(List<_Db.MText> txts)
        {
            List<_Db.MText> wrong = new List<_Db.MText>();

            foreach (_Db.MText txt in txts)
            {
                foreach (string forbidden in forbiddenChars)
                {
                    if (txt.Contents.Contains(forbidden))
                    {
                        wrong.Add(txt);
                        break;
                    }
                }
            }

            return wrong;
        }


        private void drawWrongMarks(List<_Db.MText> txts)
        {
            write("Vigade arv: " + txts.Count().ToString());

            if (txts.Count > 0)
            {
                initLayer(kontrollLayer);
            }            

            foreach (_Db.MText txt in txts)
            {
                createCircle(2000, 1, txt.Location);
                createCircle(200, 1, txt.Location);
            }
        }


        private List<_Db.MText> getAllText()
        {
            List<_Db.MText> txt = new List<_Db.MText>();

            _Db.BlockTableRecord btr = _c.trans.GetObject(_c.modelSpace.Id, _Db.OpenMode.ForWrite) as _Db.BlockTableRecord;

            foreach (_Db.ObjectId id in btr)
            {
                _Db.Entity currentEntity = _c.trans.GetObject(id, _Db.OpenMode.ForWrite, false) as _Db.Entity;

                if (currentEntity != null)
                {
                    if (currentEntity is _Db.MText)
                    {
                        _Db.MText br = currentEntity as _Db.MText;
                        txt.Add(br);
                    }
                    else if (currentEntity is _Db.DBText)
                    {
                        _Db.DBText br = currentEntity as _Db.DBText;
                        _Db.MText myMtext = new _Db.MText();

                        myMtext.Contents = br.TextString;
                        myMtext.Layer = br.Layer;

                        myMtext.Location = br.Position;
                        txt.Add(myMtext);
                    }
                    else if (currentEntity is _Db.MLeader)
                    {
                        _Db.MLeader br = currentEntity as _Db.MLeader;

                        if (br.ContentType == _Db.ContentType.MTextContent)
                        {
                            _Db.MText leaderText = br.MText;
                            leaderText.Layer = br.Layer;
                            txt.Add(leaderText);
                        }
                    }
                }
            }

            return txt;
        }


        private void createCircle(double radius, int index, _Ge.Point3d ip)
        {
            using (_Db.Circle circle = new _Db.Circle())
            {
                circle.Center = ip;
                circle.Radius = radius;
                circle.ColorIndex = index;
                circle.Layer = kontrollLayer;
                _c.modelSpace.AppendEntity(circle);
                _c.trans.AddNewlyCreatedDBObject(circle, true);
            }
        }
        

        public void initLayer(string layerName)
        {
            _Db.LayerTable layerTable = _c.trans.GetObject(_c.db.LayerTableId, _Db.OpenMode.ForWrite) as _Db.LayerTable;

            if (!layerTable.Has(layerName))
            {
                _Db.LayerTableRecord newLayer = new _Db.LayerTableRecord();
                newLayer.Name = layerName;
                newLayer.Color = _Cm.Color.FromColorIndex(_Cm.ColorMethod.None, 1);

                _Db.ObjectId layerId = layerTable.Add(newLayer);
                _c.trans.AddNewlyCreatedDBObject(newLayer, true);
            }
        }


        private void write(string message)
        {
            _c.ed.WriteMessage("\n" + message);
        }

    }
}
