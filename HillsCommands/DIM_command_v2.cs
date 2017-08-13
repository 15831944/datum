using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Autocad
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

////Bricsys
//using Teigha.Runtime;
//using Teigha.DatabaseServices;
//using Teigha.Geometry;
//using Bricscad.ApplicationServices;
//using Bricscad.Runtime;
//using Bricscad.EditorInput;

namespace commands
{
    class DIM_command_v2
    {
        double smallCircleRadius = 15.0;
        double bigCircleRadius = 50.0;
        double bigCircleOffset = 180.0;
        double textHeight = 60.0;
        double textOffset = 20.0;

        bool success;
        Point3d ptStart;
        Point3d ptEnd;
        Point3d ptPos;
        Point3d ptNext;

        double rotation;

        Document doc;
        Database db;
        Editor ed;

        public DIM_command_v2()
        {
            doc = Application.DocumentManager.MdiActiveDocument;
            db = doc.Database;
            ed = doc.Editor;

            success = false;
            ptStart = new Point3d();
            ptEnd = new Point3d();
            ptPos = new Point3d();
            ptNext = new Point3d();

            rotation = 0.0;
        }

        public void run()
        {
            main();
            ed.WriteMessage("\n[Done]");
        }

        public void main()
        {
            success = getInitPoint("\nStart: ", ref ptStart);
            if (!success) { return; }

            success = getInitPoint("\nEnd: ", ref ptEnd);
            if (!success) { return; }

            success = getPositionPoint("\nPosition: ", ptStart, ptEnd, ref ptPos, ref rotation);
            if (!success) { return; }

            insertDimLine(ptStart, ptEnd, ptPos, rotation);
            Point3d ptLast = ptEnd;

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
            Point3d ptBiggerCircle = getBigCirclePoint(ptPos, dir, rotation);

            success = insertNumber(ptBiggerCircle, rotation);
            if (!success) { return; }

            insertCircle(ptBiggerCircle, bigCircleRadius);
            insertLine(ptPos, dir, rotation);

            success = insertFormSide(ptPos, dir, rotation);
            if (!success) { return; }

            return;
        }

        private bool getInitPoint(string prompt, ref Point3d pt)
        {
            PromptPointOptions pPtOpts = new PromptPointOptions("");
            pPtOpts.Message = prompt;
            pPtOpts.UseBasePoint = false;

            PromptPointResult pPtRes;
            pPtRes = ed.GetPoint(pPtOpts);
            pt = pPtRes.Value;

            if (pPtRes.Status == PromptStatus.Cancel) return false;

            return true;
        }

        private bool getPositionPoint(string prompt, Point3d ptStart, Point3d ptEnd, ref Point3d ptPos, ref double rotation)
        {
            Point3d ptBase = new Point3d((ptEnd.X + ptStart.X) / 2, (ptEnd.Y + ptStart.Y) / 2, (ptEnd.Z + ptStart.Z) / 2);

            PromptPointOptions pPtOpts = new PromptPointOptions("");
            pPtOpts.Message = prompt;
            pPtOpts.UseBasePoint = true;
            pPtOpts.BasePoint = ptBase;

            PromptPointResult pPtRes;
            pPtRes = ed.GetPoint(pPtOpts);
            Point3d temp = pPtRes.Value;

            if (pPtRes.Status == PromptStatus.Cancel) return false;

            double dX = temp.X - ptBase.X;
            double dY = temp.Y - ptBase.Y;

            if (Math.Abs(dX) > Math.Abs(dY))
            {
                rotation = Math.PI / 2;

                double newX = temp.X;
                double newY = ptStart.Y;
                ptPos = new Point3d(newX, newY, ptStart.Z);
            }
            else
            {
                rotation = 0.0;

                double newX = ptStart.X;
                double newY = temp.Y;
                ptPos = new Point3d(newX, newY, ptStart.Z);
            }

            return true;
        }

        private bool getPoint(string prompt, Point3d ptBase, ref Point3d pt, ref bool finish)
        {
            PromptPointOptions pPtOpts = new PromptPointOptions("");
            pPtOpts.Keywords.Add("F");
            pPtOpts.Message = prompt;
            pPtOpts.UseBasePoint = true;
            pPtOpts.BasePoint = ptBase;

            PromptPointResult pPtRes;
            pPtRes = ed.GetPoint(pPtOpts);

            if (pPtRes.Status == PromptStatus.Keyword)
            {
                PromptPointOptions lastPtOpts = new PromptPointOptions("");
                pPtOpts.Message = "Last Point";
                pPtOpts.UseBasePoint = true;
                pPtOpts.BasePoint = ptBase;

                pPtRes = ed.GetPoint(lastPtOpts);
                pt = pPtRes.Value;

                finish = true;
            }
            else if (pPtRes.Status == PromptStatus.OK)
            {
                pt = pPtRes.Value;
            }
            else if (pPtRes.Status == PromptStatus.Cancel)
            {
                return false;
            }

            return true;
        }


        private void insertDimLine(Point3d ptStart, Point3d ptEnd, Point3d ptPos, double rotation)
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTableRecord btr = trans.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForWrite) as BlockTableRecord;

                using (RotatedDimension rotDim = new RotatedDimension())
                {
                    rotDim.XLine1Point = ptStart;
                    rotDim.XLine2Point = ptEnd;
                    rotDim.Rotation = rotation;
                    rotDim.DimLinePoint = ptPos;
                    rotDim.DimensionStyle = db.Dimstyle;

                    btr.AppendEntity(rotDim);
                    trans.AddNewlyCreatedDBObject(rotDim, true);
                }

                trans.Commit();
            }
        }

        private void insertCircle(Point3d center, double radius)
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTableRecord btr = trans.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForWrite) as BlockTableRecord;

                using (Circle circle = new Circle())
                {
                    circle.Center = center;
                    circle.Radius = radius;
                    btr.AppendEntity(circle);
                    trans.AddNewlyCreatedDBObject(circle, true);
                }

                trans.Commit();
            }
        }

        private double getDirectionVector(Point3d ptStart, Point3d ptEnd, double rotation)
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

        private Point3d getBigCirclePoint(Point3d ptPos, double dir, double rotation)
        {
            Point3d ptNew = new Point3d();
            if (rotation == 0.0)
            {
                double newX = ptPos.X - bigCircleOffset * dir;
                double newY = ptPos.Y;
                ptNew = new Point3d(newX, newY, ptPos.Z);
            }
            else
            {
                double newX = ptPos.X;
                double newY = ptPos.Y - bigCircleOffset * dir;
                ptNew = new Point3d(newX, newY, ptPos.Z);
            }

            return ptNew;
        }

        private void insertLine(Point3d ptPos, double dir, double rotation)
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTableRecord btr = trans.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForWrite) as BlockTableRecord;

                Point3d ptEnd = new Point3d();

                if (rotation == 0.0)
                {
                    double newX = ptPos.X - (bigCircleOffset - bigCircleRadius) * dir;
                    double newY = ptPos.Y;
                    ptEnd = new Point3d(newX, newY, ptPos.Z);
                }
                else
                {
                    double newX = ptPos.X;
                    double newY = ptPos.Y - (bigCircleOffset - bigCircleRadius) * dir;
                    ptEnd = new Point3d(newX, newY, ptPos.Z);
                }

                using (Line line = new Line(ptPos, ptEnd))
                {
                    btr.AppendEntity(line);
                    trans.AddNewlyCreatedDBObject(line, true);
                }

                trans.Commit();
            }
        } 

        private bool insertNumber(Point3d center, double rotation)
        {
            PromptStringOptions pStrOpts = new PromptStringOptions("\nNumber: ");
            pStrOpts.AllowSpaces = false;
            PromptResult pStrRes = ed.GetString(pStrOpts);
            string result = pStrRes.StringResult;

            if (pStrRes.Status == PromptStatus.Cancel) return false;

            AttachmentPoint a = AttachmentPoint.MiddleCenter;
            if (result.Length > 2)
            {
                Point3d insert = center;

                a = AttachmentPoint.BottomRight;
                if (rotation == 0.0)
                {
                    double newX = center.X;
                    double newY = center.Y + textOffset;

                    insert = new Point3d(newX, newY, center.Z);
                }
                else
                {
                    double newX = center.X - textOffset;
                    double newY = center.Y;

                    insert = new Point3d(newX, newY, center.Z);
                }

                insertText(insert, a, result, rotation);
                return false;
            }
            else
            {
                a = AttachmentPoint.MiddleCenter;
                insertText(center, a, result, 0.0);
                return true;
            }
        }

        private void insertText(Point3d ptInsert, AttachmentPoint a, string txt, double rotation)
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTableRecord btr = trans.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForWrite) as BlockTableRecord;

                using (MText acMText = new MText())
                {
                    acMText.Attachment = a;
                    acMText.Location = ptInsert;
                    acMText.Contents = txt;
                    acMText.TextHeight = textHeight;
                    acMText.Rotation = rotation;

                    btr.AppendEntity(acMText);
                    trans.AddNewlyCreatedDBObject(acMText, true);
                }

                trans.Commit();
            }
        }

        private bool insertFormSide(Point3d ptPos, double dir, double rotation)
        {
            PromptStringOptions pStrOpts = new PromptStringOptions("\nFormSide: ");
            pStrOpts.AllowSpaces = false;
            PromptResult pStrRes = ed.GetString(pStrOpts);
            string result = pStrRes.StringResult;

            if (pStrRes.Status == PromptStatus.Cancel) return false;

            Point3d insert = ptPos;
            AttachmentPoint a = AttachmentPoint.BottomCenter;

            if (rotation == 0.0)
            {
                a = AttachmentPoint.BottomCenter;
                double newX = ptPos.X - (bigCircleOffset - bigCircleRadius) * dir / 2;
                double newY = ptPos.Y + textOffset;
                insert = new Point3d(newX, newY, ptPos.Z);
            }
            else
            {
                a = AttachmentPoint.MiddleRight;
                double newX = ptPos.X - textOffset;
                double newY = ptPos.Y - (bigCircleOffset - bigCircleRadius) * dir / 2;
                insert = new Point3d(newX, newY, ptPos.Z);
            }

            insertText(insert, a, result, 0);
            return true;
        }
    }
}