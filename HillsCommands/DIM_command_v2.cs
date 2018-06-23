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
    class DIM_command_v2
    {
        _CONNECTION _c;

        double smallCircleRadius = 15.0;
        double bigCircleRadius = 50.0;
        double bigCircleOffset = 180.0;
        double textHeight = 60.0;
        double textOffset = 20.0;

        bool success;
        _Ge.Point3d ptStart;
        _Ge.Point3d ptEnd;
        _Ge.Point3d ptPos;
        _Ge.Point3d ptNext;

        double rotation;


        public DIM_command_v2(ref _CONNECTION c)
        {
            _c = c;

            success = false;
            ptStart = new _Ge.Point3d();
            ptEnd = new _Ge.Point3d();
            ptPos = new _Ge.Point3d();
            ptNext = new _Ge.Point3d();

            rotation = 0.0;
        }


        public void run()
        {
            success = getInitPoint("\nStart: ", ref ptStart);
            if (!success) { return; }

            success = getInitPoint("\nEnd: ", ref ptEnd);
            if (!success) { return; }

            success = getPositionPoint("\nPosition: ", ptStart, ptEnd, ref ptPos, ref rotation);
            if (!success) { return; }

            insertDimLine(ptStart, ptEnd, ptPos, rotation);
            _Ge.Point3d ptLast = ptEnd;

            while (true)
            {
                bool finish = false;

                success = getPoint("\nNext: ", ptStart, ref ptNext, ref finish);
                if (!success) { return; }

                if (finish)
                {
                    insertDimLine(ptLast, ptNext, ptPos, rotation);
                    break;
                }
                else
                {
                    insertDimLine(ptStart, ptNext, ptPos, rotation);
                }

                ptLast = ptNext;
            }

            insertCircle(ptPos, smallCircleRadius);

            double dir = getDirectionVector(ptStart, ptEnd, rotation);
            _Ge.Point3d ptBiggerCircle = getBigCirclePoint(ptPos, dir, rotation);

            success = insertNumber(ptBiggerCircle, rotation);
            if (!success) { return; }

            insertCircle(ptBiggerCircle, bigCircleRadius);
            insertLine(ptPos, dir, rotation);

            success = insertFormSide(ptPos, dir, rotation);
            if (!success) { return; }
        }


        private bool getInitPoint(string prompt, ref _Ge.Point3d pt)
        {
            _Ed.PromptPointOptions pPtOpts = new _Ed.PromptPointOptions("");
            pPtOpts.Message = prompt;
            pPtOpts.UseBasePoint = false;

            _Ed.PromptPointResult pPtRes;
            pPtRes = _c.ed.GetPoint(pPtOpts);
            pt = pPtRes.Value;

            if (pPtRes.Status == _Ed.PromptStatus.Cancel) return false;

            return true;
        }


        private bool getPositionPoint(string prompt, _Ge.Point3d ptStart, _Ge.Point3d ptEnd, ref _Ge.Point3d ptPos, ref double rotation)
        {
            _Ge.Point3d ptBase = new _Ge.Point3d((ptEnd.X + ptStart.X) / 2, (ptEnd.Y + ptStart.Y) / 2, (ptEnd.Z + ptStart.Z) / 2);

            _Ed.PromptPointOptions pPtOpts = new _Ed.PromptPointOptions("");
            pPtOpts.Message = prompt;
            pPtOpts.UseBasePoint = true;
            pPtOpts.BasePoint = ptBase;

            _Ed.PromptPointResult pPtRes;
            pPtRes = _c.ed.GetPoint(pPtOpts);
            _Ge.Point3d temp = pPtRes.Value;

            if (pPtRes.Status == _Ed.PromptStatus.Cancel) return false;

            double dX = temp.X - ptBase.X;
            double dY = temp.Y - ptBase.Y;

            if (Math.Abs(dX) > Math.Abs(dY))
            {
                rotation = Math.PI / 2;

                double newX = temp.X;
                double newY = ptStart.Y;
                ptPos = new _Ge.Point3d(newX, newY, ptStart.Z);
            }
            else
            {
                rotation = 0.0;

                double newX = ptStart.X;
                double newY = temp.Y;
                ptPos = new _Ge.Point3d(newX, newY, ptStart.Z);
            }

            return true;
        }


        private bool getPoint(string prompt, _Ge.Point3d ptBase, ref _Ge.Point3d pt, ref bool finish)
        {
            _Ed.PromptPointOptions pPtOpts = new _Ed.PromptPointOptions("");
            pPtOpts.Keywords.Add("F");
            pPtOpts.Message = prompt;
            pPtOpts.UseBasePoint = true;
            pPtOpts.BasePoint = ptBase;

            _Ed.PromptPointResult pPtRes;
            pPtRes = _c.ed.GetPoint(pPtOpts);

            if (pPtRes.Status == _Ed.PromptStatus.Keyword)
            {
                _Ed.PromptPointOptions lastPtOpts = new _Ed.PromptPointOptions("");
                pPtOpts.Message = "Last Point";
                pPtOpts.UseBasePoint = true;
                pPtOpts.BasePoint = ptBase;

                pPtRes = _c.ed.GetPoint(lastPtOpts);
                pt = pPtRes.Value;

                finish = true;
            }
            else if (pPtRes.Status == _Ed.PromptStatus.OK)
            {
                pt = pPtRes.Value;
            }
            else if (pPtRes.Status == _Ed.PromptStatus.Cancel)
            {
                return false;
            }

            return true;
        }


        private void insertDimLine(_Ge.Point3d ptStart, _Ge.Point3d ptEnd, _Ge.Point3d ptPos, double rotation)
        {
            _Db.BlockTableRecord btr = _c.trans.GetObject(_c.modelSpace.Id, _Db.OpenMode.ForWrite) as _Db.BlockTableRecord;

            using (_Db.RotatedDimension rotDim = new _Db.RotatedDimension())
            {
                rotDim.XLine1Point = ptStart;
                rotDim.XLine2Point = ptEnd;
                rotDim.Rotation = rotation;
                rotDim.DimLinePoint = ptPos;
                rotDim.DimensionStyle = _c.db.Dimstyle;

                btr.AppendEntity(rotDim);
                _c.trans.AddNewlyCreatedDBObject(rotDim, true);
            }
        }


        private void insertCircle(_Ge.Point3d center, double radius)
        {
            _Db.BlockTableRecord btr = _c.trans.GetObject(_c.modelSpace.Id, _Db.OpenMode.ForWrite) as _Db.BlockTableRecord;

            using (_Db.Circle circle = new _Db.Circle())
            {
                circle.Center = center;
                circle.Radius = radius;
                btr.AppendEntity(circle);
                _c.trans.AddNewlyCreatedDBObject(circle, true);
            }
        }


        private double getDirectionVector(_Ge.Point3d ptStart, _Ge.Point3d ptEnd, double rotation)
        {
            double dir = 0.0;

            if (rotation == 0.0)
            {
                double dX = ptEnd.X - ptStart.X;
                dir = dX / Math.Abs(dX);
            }
            else
            {
                double dY = ptEnd.Y - ptStart.Y;
                dir = dY / Math.Abs(dY);
            }

            return dir;
        }


        private _Ge.Point3d getBigCirclePoint(_Ge.Point3d ptPos, double dir, double rotation)
        {
            _Ge.Point3d ptNew = new _Ge.Point3d();
            if (rotation == 0.0)
            {
                double newX = ptPos.X - bigCircleOffset * dir;
                double newY = ptPos.Y;
                ptNew = new _Ge.Point3d(newX, newY, ptPos.Z);
            }
            else
            {
                double newX = ptPos.X;
                double newY = ptPos.Y - bigCircleOffset * dir;
                ptNew = new _Ge.Point3d(newX, newY, ptPos.Z);
            }

            return ptNew;
        }


        private void insertLine(_Ge.Point3d ptPos, double dir, double rotation)
        {
            _Db.BlockTableRecord btr = _c.trans.GetObject(_c.modelSpace.Id, _Db.OpenMode.ForWrite) as _Db.BlockTableRecord;

            _Ge.Point3d ptEnd = new _Ge.Point3d();

            if (rotation == 0.0)
            {
                double newX = ptPos.X - (bigCircleOffset - bigCircleRadius) * dir;
                double newY = ptPos.Y;
                ptEnd = new _Ge.Point3d(newX, newY, ptPos.Z);
            }
            else
            {
                double newX = ptPos.X;
                double newY = ptPos.Y - (bigCircleOffset - bigCircleRadius) * dir;
                ptEnd = new _Ge.Point3d(newX, newY, ptPos.Z);
            }

            using (_Db.Line line = new _Db.Line(ptPos, ptEnd))
            {
                btr.AppendEntity(line);
                _c.trans.AddNewlyCreatedDBObject(line, true);
            }
        }


        private bool insertNumber(_Ge.Point3d center, double rotation)
        {
            _Ed.PromptStringOptions pStrOpts = new _Ed.PromptStringOptions("\nNumber: ");
            pStrOpts.AllowSpaces = false;
            _Ed.PromptResult pStrRes = _c.ed.GetString(pStrOpts);
            string result = pStrRes.StringResult;

            if (pStrRes.Status == _Ed.PromptStatus.Cancel) return false;

            _Db.AttachmentPoint a = _Db.AttachmentPoint.MiddleCenter;
            if (result.Length > 2)
            {
                _Ge.Point3d insert = center;

                a = _Db.AttachmentPoint.BottomRight;
                if (rotation == 0.0)
                {
                    double newX = center.X;
                    double newY = center.Y + textOffset;

                    insert = new _Ge.Point3d(newX, newY, center.Z);
                }
                else
                {
                    double newX = center.X - textOffset;
                    double newY = center.Y;

                    insert = new _Ge.Point3d(newX, newY, center.Z);
                }

                insertText(insert, a, result, rotation);
                return false;
            }
            else
            {
                a = _Db.AttachmentPoint.MiddleCenter;
                insertText(center, a, result, 0.0);
                return true;
            }
        }


        private void insertText(_Ge.Point3d ptInsert, _Db.AttachmentPoint a, string txt, double rotation)
        {
            _Db.BlockTableRecord btr = _c.trans.GetObject(_c.modelSpace.Id, _Db.OpenMode.ForWrite) as _Db.BlockTableRecord;

            using (_Db.MText acMText = new _Db.MText())
            {
                acMText.Attachment = a;
                acMText.Location = ptInsert;
                acMText.Contents = txt;
                acMText.TextHeight = textHeight;
                acMText.Rotation = rotation;

                btr.AppendEntity(acMText);
                _c.trans.AddNewlyCreatedDBObject(acMText, true);
            }
        }


        private bool insertFormSide(_Ge.Point3d ptPos, double dir, double rotation)
        {
            _Ed.PromptStringOptions pStrOpts = new _Ed.PromptStringOptions("\nFormSide: ");
            pStrOpts.AllowSpaces = false;
            _Ed.PromptResult pStrRes = _c.ed.GetString(pStrOpts);
            string result = pStrRes.StringResult;

            if (pStrRes.Status == _Ed.PromptStatus.Cancel) return false;

            _Ge.Point3d insert = ptPos;
            _Db.AttachmentPoint a = _Db.AttachmentPoint.BottomCenter;

            if (rotation == 0.0)
            {
                a = _Db.AttachmentPoint.BottomCenter;
                double newX = ptPos.X - (bigCircleOffset - bigCircleRadius) * dir / 2;
                double newY = ptPos.Y + textOffset;
                insert = new _Ge.Point3d(newX, newY, ptPos.Z);
            }
            else
            {
                a = _Db.AttachmentPoint.MiddleRight;
                double newX = ptPos.X - textOffset;
                double newY = ptPos.Y - (bigCircleOffset - bigCircleRadius) * dir / 2;
                insert = new _Ge.Point3d(newX, newY, ptPos.Z);
            }

            insertText(insert, a, result, 0);
            return true;
        }


        private void write(string message)
        {
            _c.ed.WriteMessage("\n" + message);
        }

    }
}