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


        public static double _TOLERANCE = 0.05;


        public SIZE_command(ref _CONNECTION c)
        {
            _c = c;
        }


        internal void run()
        {
            List<_Db.Dimension> dims = new List<_Db.Dimension>();
            List<_Db.BlockReference> blocks = new List<_Db.BlockReference>();
            List<_Db.MText> txts = new List<_Db.MText>();

            getSelectedObjects(ref dims, ref blocks, ref txts);

            double scale = getScale();
            write("Scale: " + scale.ToString());

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

            checkText(txtHeight, txts, btr);
            checkDims(scale, dims, btr);
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
                if (Math.Abs(dim.Dimscale - scale) < _TOLERANCE)
                {
                    if (Math.Abs(dim.Dimtxt - _DEFAULT_TEXT_SIZE) > _TOLERANCE)
                    {
                        wrongDims.Add(dim);
                    }                    
                }
                else if (Math.Abs(dim.Dimscale - 1) < _TOLERANCE)
                {
                    if (Math.Abs(dim.Dimtxt - txtHeight) > _TOLERANCE)
                    {
                        wrongDims.Add(dim);
                    }
                }
                else
                {
                    wrongDims.Add(dim);
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
                _Ge.Point3d map = wrong.GeometricExtents.MinPoint;
                _Ge.Point3d mup = wrong.GeometricExtents.MaxPoint;

                _Ge.Point3d cep = new _Ge.Point3d((map.X + mup.X) / 2, (map.Y + mup.Y) / 2, (map.Z + mup.Z) / 2);
                double size = Math.Sqrt(Math.Pow(cep.X - mup.X, 2) + Math.Pow(cep.Y - mup.Y, 2) + Math.Pow(cep.Z - mup.Z, 2)) * 1.1;
                size = Math.Max(size, 200);

                createCircle(size, 1, cep, btr);
            }
        }

        
        private void getSelectedObjects(ref List<_Db.Dimension> dims, ref List<_Db.BlockReference> blocks, ref List<_Db.MText> txts)
        {
            _Ed.PromptSelectionOptions opts = new _Ed.PromptSelectionOptions();
            opts.MessageForAdding = "\nSelect area [empty select == ALL]";

            _Ed.PromptSelectionResult selection = _c.ed.GetSelection(opts);

            if (selection.Status == _Ed.PromptStatus.OK)
            {
                _Db.ObjectId[] selectionIds = selection.Value.GetObjectIds();
                foreach (_Db.ObjectId id in selectionIds)
                {
                    alfa(id, ref dims, ref blocks, ref txts);
                }
            }
            else if (selection.Status == _Ed.PromptStatus.Error)
            {
                _Db.BlockTableRecord btr = _c.trans.GetObject(_c.modelSpace.Id, _Db.OpenMode.ForWrite) as _Db.BlockTableRecord;

                foreach (_Db.ObjectId id in btr)
                {
                    alfa(id, ref dims, ref blocks, ref txts);
                }
            }

        }


        private void alfa(_Db.ObjectId id, ref List<_Db.Dimension> dims, ref List<_Db.BlockReference> blocks, ref List<_Db.MText> txts)
        {
            _Db.DBObject currentEntity = _c.trans.GetObject(id, _Db.OpenMode.ForWrite, false) as _Db.DBObject;

            if (currentEntity == null)
            {
                return;
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
                    blocks.Add(blockRef);
                }
            }
            else if (currentEntity is _Db.Dimension)
            {
                _Db.Dimension dim = currentEntity as _Db.Dimension;
                dims.Add(dim);
            }
            else if (currentEntity is _Db.MText)
            {
                _Db.MText br = currentEntity as _Db.MText;
                txts.Add(br);
            }
            else if (currentEntity is _Db.DBText)
            {
                _Db.DBText br = currentEntity as _Db.DBText;

                _Db.MText myMtext = new _Db.MText();
                myMtext.Contents = br.TextString;
                myMtext.Location = br.Position;
                myMtext.TextHeight = br.Height;
                txts.Add(myMtext);
            }
            else if (currentEntity is _Db.MLeader)
            {
                _Db.MLeader br = currentEntity as _Db.MLeader;

                if (br.ContentType == _Db.ContentType.MTextContent)
                {
                    _Db.MText leaderText = br.MText;
                    txts.Add(leaderText);
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
