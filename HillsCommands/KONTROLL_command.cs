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
    class KONTROLL_command
    {
        _CONNECTION _c;
        
        static string markLayerName = "K60";


        public KONTROLL_command(ref _CONNECTION c)
        {
            _c = c;
        }


        public void run()
        {
            List<_Mark_K> wrongMarks = getAllWrongMarks();
            drawWrongMarks(wrongMarks);
        }

        
        private void drawWrongMarks(List<_Mark_K> marks)
        {
            write("Vigade arv: " + marks.Count().ToString());

            foreach (_Mark_K mark in marks)
            {
                createCircle(2000, 1, mark.IP);
                createCircle(200, 1, mark.IP);
            }
        }


        private void createCircle(double radius, int index, _Ge.Point3d ip)
        {
            _Db.BlockTableRecord btr = _c.trans.GetObject(_c.modelSpace.Id, _Db.OpenMode.ForWrite) as _Db.BlockTableRecord;

            using (_Db.Circle circle = new _Db.Circle())
            {
                circle.Center = ip;
                circle.Radius = radius;
                circle.ColorIndex = index;
                btr.AppendEntity(circle);
                _c.trans.AddNewlyCreatedDBObject(circle, true);
            }
        }


        private List<_Mark_K> getAllWrongMarks()
        {
            List<_Mark_K> wrongMarks = new List<_Mark_K>();

            List<_Db.MText> allTexts = getAllText();
            wrongMarks = filterWrongMarks(allTexts);

            return wrongMarks;
        }


        private List<_Mark_K> filterWrongMarks(List<_Db.MText> txts)
        {
            List<_Mark_K> good = new List<_Mark_K>();
            List<_Mark_K> bad = new List<_Mark_K>();

            foreach (_Db.MText txt in txts)
            {
                if (txt.Contents.StartsWith("IG-")) continue;
                if (txt.Contents.StartsWith("NEO-")) continue;
                if (txt.Contents.StartsWith("TAGGLIST-")) continue;
                if (txt.Contents.StartsWith("STAGHYLSA")) continue;
                if (txt.Contents.StartsWith("WELDA")) continue;
                if (txt.Contents.StartsWith("LYFT")) continue;
                if (txt.Contents.StartsWith("HFV")) continue;
                if (txt.Contents.StartsWith("6300S")) continue;
                if (txt.Contents.StartsWith("DUBBURSPARING")) continue;
                if (txt.Contents.Length < 3) continue;
                
                if (txt.Contents.Contains("-"))
                {
                    _Mark_K current = new _Mark_K(txt.Contents, txt.Location);
                    bool valid = current.validate_original();

                    if (valid == false)
                    {
                        bad.Add(current);
                    }
                    else
                    {
                        if (good.Contains(current))
                        {
                            bad.Add(current);
                        }
                        else
                        {
                            if (txt.Layer != markLayerName)
                            {
                                bad.Add(current);
                            }
                            else
                            {
                                good.Add(current);
                            }                            
                        }
                    }
                }
            }

            return bad;
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

                    if (currentEntity is _Db.DBText)
                    {
                        _Db.DBText br = currentEntity as _Db.DBText;

                        _Db.MText myMtext = new _Db.MText();
                        myMtext.Contents = br.TextString;
                        myMtext.Location = br.Position;
                        myMtext.Layer = br.Layer;
                        txt.Add(myMtext);

                    }

                    if (currentEntity is _Db.MLeader)
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


        private void write(string message)
        {
            _c.ed.WriteMessage("\n" + message);
        }

    }
}