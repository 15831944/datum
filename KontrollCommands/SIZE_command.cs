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
    class SIZE_command
    {
        _CONNECTION _c;

        public static double _DEFAULT_TEXT_SIZE = 2.5;
        //public static double _DEFAULT_TEXT_SIZE_SMALL = 2.2;
        //public static double _DEFAULT_TEXT_SIZE_LARGE = 3.0;

        //static string[] ignoreBlocks = { "Katkestus_nelinurk", "Armatuurvarras_rib", "#s150" };
        static string[] ignoreBlocks = { };


        public static double _TOLERANCE = 0.1;


        public SIZE_command(ref _CONNECTION c)
        {
            _c = c;
        }


        internal void run()
        {
            double scale = getScale();

            write("Scale: " + scale.ToString());

            List<_Db.Dimension> dims = getAllDims();
            List <_Db.BlockReference> blocks = getAllBlockReferences(ignoreBlocks);
            List<_Db.MText> txts = getAllText();

            logic(scale, dims, blocks, txts);
        }


        private double getScale()
        {
            _Ed.PromptDoubleOptions pDoubleOpts = new _Ed.PromptDoubleOptions("\nEnter scale: ");
            _Ed.PromptDoubleResult pDoubleRes = _c.ed.GetDouble(pDoubleOpts);

            if (pDoubleRes.Status != _Ed.PromptStatus.OK) throw new DMTException("no scale");

            return pDoubleRes.Value;
        }


        private void logic(double scale, List<_Db.Dimension> dims, List<_Db.BlockReference> blocks, List<_Db.MText> txts)
        {
            _Db.BlockTableRecord btr = _c.trans.GetObject(_c.modelSpace.Id, _Db.OpenMode.ForWrite) as _Db.BlockTableRecord;

            double txtHeight = scale * _DEFAULT_TEXT_SIZE;
            write("TextHeight: " + txtHeight.ToString());

            //checkText(txtHeight, txts, btr);
            //checkDims(scale, dims, btr);
            checkBlocks(scale, blocks, btr);
        }


        private void checkText(double txtHeight, List<_Db.MText> txts, _Db.BlockTableRecord btr)
        {
            List<_Db.MText> wrongTxts = new List<_Db.MText>();
            foreach (_Db.MText txt in txts)
            {
                if (Math.Abs(txt.TextHeight - txtHeight) > _TOLERANCE)
                {
                    wrongTxts.Add(txt);
                }
            }

            write("Wrong text count:" + wrongTxts.Count.ToString());
            foreach (_Db.MText txt in wrongTxts)
            {
                createCircle(200, 1, txt.Location, btr);
            }
        }


        private void checkDims(double scale, List<_Db.Dimension> dims, _Db.BlockTableRecord btr)
        {
            double txtHeight = scale * _DEFAULT_TEXT_SIZE;

            List<_Db.Dimension> wrongDims = new List<_Db.Dimension>();
            foreach (_Db.Dimension dim in dims)
            {
                if (Math.Abs(dim.Dimtxt - txtHeight) > _TOLERANCE)
                {
                    if ((Math.Abs(dim.Dimtxt - _DEFAULT_TEXT_SIZE) > _TOLERANCE) && (Math.Abs(dim.Dimscale - scale) > _TOLERANCE))
                    {
                        wrongDims.Add(dim);
                    }
                }
            }

            write("Wrong dimentions count:" + wrongDims.Count.ToString());
            foreach (_Db.Dimension dim in wrongDims)
            {
                changeFillColor(dim, 1);
            }
        }


        private void checkBlocks(double scale, List<_Db.BlockReference> blocks, _Db.BlockTableRecord btr)
        {
            List<_Db.BlockReference> wrongBlocks = new List<_Db.BlockReference>();
            foreach (_Db.BlockReference block in blocks)
            {
                if (Math.Abs(Math.Abs(block.ScaleFactors.X) - scale) > _TOLERANCE || Math.Abs(Math.Abs(block.ScaleFactors.Y) - scale) > _TOLERANCE)
                {
                    if (Math.Abs(Math.Abs(block.ScaleFactors.Y) - 1) > _TOLERANCE || Math.Abs(Math.Abs(block.ScaleFactors.Y) - 1) > _TOLERANCE)
                    {
                        wrongBlocks.Add(block);
                    }
                }
            }

            write("Wrong Blocks count:" + wrongBlocks.Count.ToString());
            foreach (_Db.BlockReference wrong in wrongBlocks)
            {
                createCircle(200, 1, wrong.Position, btr);
            }
        }


        private List<_Db.Dimension> getAllDims()
        {
            List<_Db.Dimension> dims = new List<_Db.Dimension>();

            _Db.BlockTableRecord btr = _c.trans.GetObject(_c.modelSpace.Id, _Db.OpenMode.ForWrite) as _Db.BlockTableRecord;

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
                    dims.Add(dim);
                }
            }

            return dims;
        }


        private List<_Db.BlockReference> getAllBlockReferences(string[] ignore)
        {
            List<_Db.BlockReference> refs = new List<_Db.BlockReference>();

            _Db.BlockTableRecord btr = _c.trans.GetObject(_c.modelSpace.Id, _Db.OpenMode.ForWrite) as _Db.BlockTableRecord;

            foreach (_Db.ObjectId id in btr)
            {
                _Db.DBObject currentEntity = _c.trans.GetObject(id, _Db.OpenMode.ForWrite, false) as _Db.DBObject;

                if (currentEntity == null)
                {
                    continue;
                }

                else if (currentEntity is _Db.BlockReference)
                {
                    _Db.BlockReference blockRef = currentEntity as _Db.BlockReference;

                    _Db.BlockTableRecord block = null;
                    if (blockRef.IsDynamicBlock)
                    {
                        block = _c.trans.GetObject(blockRef.DynamicBlockTableRecord, _Db.OpenMode.ForRead) as _Db.BlockTableRecord;
                    }
                    else
                    {
                        block = _c.trans.GetObject(blockRef.BlockTableRecord, _Db.OpenMode.ForRead) as _Db.BlockTableRecord;
                    }

                    if (block != null)
                    {
                        if (ignoreBlocks.Contains(block.Name)) continue;

                        refs.Add(blockRef);
                    }
                }
            }

            return refs;
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
                        myMtext.TextHeight = br.Height;
                        txt.Add(myMtext);
                    }

                    if (currentEntity is _Db.MLeader)
                    {
                        _Db.MLeader br = currentEntity as _Db.MLeader;

                        if (br.ContentType == _Db.ContentType.MTextContent)
                        {
                            _Db.MText leaderText = br.MText;
                            txt.Add(leaderText);
                        }
                    }
                }
            }

            return txt;
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


        private void changeFillColor(_Db.Dimension dim, short index)
        {
            dim.Dimtfill = 2;
            dim.Dimtfillclr = _Cm.Color.FromColorIndex(_Cm.ColorMethod.None, index);
        }


        private void write(string message)
        {
            _c.ed.WriteMessage("\n" + message);
        }

    }
}
