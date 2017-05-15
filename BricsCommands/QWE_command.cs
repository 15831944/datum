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
//    class QWE_command
//    {
//        double smallCircleRadius = 15.0;
//        double bigCircleRadius = 50.0;
//        double bigCircleOffset = 180.0;
//        double textHeight = 60.0;
//        double textOffset = 20.0;

//        bool success;
//        Point3d ptStart;
//        Point3d ptEnd;
//        Point3d ptPos;
//        Point3d ptNext;

//        double rotation;

//        public QWE_command()
//        {
//            success = false;
//            ptStart = new Point3d();
//            ptEnd = new Point3d();
//            ptPos = new Point3d();
//            ptNext = new Point3d();

//            rotation = 0.0;
//        }
        
//        public void run()
//        {
//            success = getInitPoint("\nStart: ", ref ptStart);
//            if (!success) { return; }

//            success = getInitPoint("\nEnd: ", ref ptEnd);
//            if (!success) { return; }

//            success = getPositionPoint("\nPosition: ", ptStart, ptEnd, ref ptPos, ref rotation);
//            if (!success) { return; }

//            insertDimLine(ptStart, ptEnd, ptPos, rotation);
//            Point3d ptLast = ptEnd;

//            while (true)
//            {
//                bool finish = false;

//                success = getPoint("\nNext: ", ptStart, ref ptNext, ref finish);
//                if (!success) { return; }

//                if (finish)
//                {
//                    insertDimLine(ptLast, ptNext, ptPos, rotation);
//                    break;
//                }
//                else
//                {
//                    insertDimLine(ptStart, ptNext, ptPos, rotation);
//                }

//                ptLast = ptNext;
//            }

//            insertCircle(ptPos, smallCircleRadius);

//            double dir = getDirectionVector(ptStart, ptEnd, rotation);
//            Point3d ptBiggerCircle = getBigCirclePoint(ptPos, dir, rotation);

//            success = insertNumber(ptBiggerCircle, rotation);
//            if (!success) { return; }

//            insertCircle(ptBiggerCircle, bigCircleRadius);
//            insertLine(ptPos, dir, rotation);

//            success = insertFormSide(ptPos, dir, rotation);
//            if (!success) { return; }

//            return;
//        }

//        private bool getInitPoint(string prompt, ref Point3d pt)
//        {
//            Document acDoc = Application.DocumentManager.MdiActiveDocument;
//            Database acCurDb = acDoc.Database;

//            PromptPointResult pPtRes;
//            PromptPointOptions pPtOpts = new PromptPointOptions("");

//            pPtOpts.Message = prompt;
//            pPtOpts.UseBasePoint = false;
//            pPtRes = acDoc.Editor.GetPoint(pPtOpts);
//            pt = pPtRes.Value;

//            if (pPtRes.Status == PromptStatus.Cancel) return false;

//            return true;
//        }

//        private bool getPositionPoint(string prompt, Point3d ptStart, Point3d ptEnd, ref Point3d ptPos, ref double rotation)
//        {
//            Document acDoc = Application.DocumentManager.MdiActiveDocument;
//            Database acCurDb = acDoc.Database;

//            Point3d ptBase = new Point3d((ptEnd.X + ptStart.X) / 2, (ptEnd.Y + ptStart.Y) / 2, (ptEnd.Z + ptStart.Z) / 2);

//            PromptPointResult pPtRes;
//            PromptPointOptions pPtOpts = new PromptPointOptions("");

//            pPtOpts.Message = prompt;
//            pPtOpts.UseBasePoint = true;
//            pPtOpts.BasePoint = ptBase;
//            pPtRes = acDoc.Editor.GetPoint(pPtOpts);
//            Point3d temp = pPtRes.Value;

//            if (pPtRes.Status == PromptStatus.Cancel) return false;

//            double dX = temp.X - ptBase.X;
//            double dY = temp.Y - ptBase.Y;

//            if (Math.Abs(dX) > Math.Abs(dY))
//            {
//                rotation = Math.PI / 2;

//                double newX = temp.X;
//                double newY = ptStart.Y;
//                ptPos = new Point3d(newX, newY, ptStart.Z);
//            }
//            else
//            {
//                rotation = 0.0;

//                double newX = ptStart.X;
//                double newY = temp.Y;
//                ptPos = new Point3d(newX, newY, ptStart.Z);
//            }

//            return true;
//        }

//        private bool getPoint(string prompt, Point3d ptBase, ref Point3d pt, ref bool finish)
//        {
//            Document acDoc = Application.DocumentManager.MdiActiveDocument;
//            Database acCurDb = acDoc.Database;

//            PromptPointResult pPtRes;
//            PromptPointOptions pPtOpts = new PromptPointOptions("");

//            pPtOpts.Keywords.Add("F");

//            pPtOpts.Message = prompt;
//            pPtOpts.UseBasePoint = true;
//            pPtOpts.BasePoint = ptBase;
//            pPtRes = acDoc.Editor.GetPoint(pPtOpts);

//            while (true)
//            {
//                if (pPtRes.Status == PromptStatus.Keyword)
//                {
//                    pPtRes = acDoc.Editor.GetPoint(pPtOpts);
//                    finish = true;
//                }
//                else if (pPtRes.Status == PromptStatus.OK)
//                {
//                    pt = pPtRes.Value;
//                    break;
//                }
//                else if (pPtRes.Status == PromptStatus.Cancel)
//                {
//                    return false;
//                }
//            }

//            return true;
//        }


//        private void insertDimLine(Point3d ptStart, Point3d ptEnd, Point3d ptPos, double rotation)
//        {
//            Document acDoc = Application.DocumentManager.MdiActiveDocument;
//            Database acCurDb = acDoc.Database;

//            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
//            {
//                BlockTable acBlkTbl;
//                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
//                                                OpenMode.ForRead) as BlockTable;

//                BlockTableRecord acBlkTblRec;
//                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
//                                                OpenMode.ForWrite) as BlockTableRecord;

//                using (RotatedDimension acRotDim = new RotatedDimension())
//                {
//                    acRotDim.XLine1Point = ptStart;
//                    acRotDim.XLine2Point = ptEnd;
//                    acRotDim.Rotation = rotation;
//                    acRotDim.DimLinePoint = ptPos;
//                    acRotDim.DimensionStyle = acCurDb.Dimstyle;

//                    acBlkTblRec.AppendEntity(acRotDim);
//                    acTrans.AddNewlyCreatedDBObject(acRotDim, true);
//                }

//                acTrans.Commit();
//            }
//        }

//        private void insertCircle(Point3d center, double radius)
//        {
//            Document acDoc = Application.DocumentManager.MdiActiveDocument;
//            Database acCurDb = acDoc.Database;

//            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
//            {
//                BlockTable acBlkTbl;
//                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
//                                                OpenMode.ForRead) as BlockTable;

//                BlockTableRecord acBlkTblRec;
//                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
//                                                OpenMode.ForWrite) as BlockTableRecord;

//                using (Circle acCirc = new Circle())
//                {
//                    acCirc.Center = center;
//                    acCirc.Radius = radius;

//                    acBlkTblRec.AppendEntity(acCirc);
//                    acTrans.AddNewlyCreatedDBObject(acCirc, true);
//                }

//                acTrans.Commit();
//            }
//        }

//        private double getDirectionVector(Point3d ptStart, Point3d ptEnd, double rotation)
//        {
//            double dir = 0.0;

//            if (rotation == 0.0)
//            {
//                double dX = ptEnd.X - ptStart.X;
//                dir = dX / Math.Abs(dX);
//            }
//            else
//            {
//                double dY = ptEnd.Y - ptStart.Y;
//                dir = dY / Math.Abs(dY);
//            }

//            return dir;
//        }

//        private Point3d getBigCirclePoint(Point3d ptPos, double dir, double rotation)
//        {
//            Point3d ptNew = new Point3d();
//            if (rotation == 0.0)
//            {
//                double newX = ptPos.X - bigCircleOffset * dir;
//                double newY = ptPos.Y;
//                ptNew = new Point3d(newX, newY, ptPos.Z);
//            }
//            else
//            {
//                double newX = ptPos.X;
//                double newY = ptPos.Y - bigCircleOffset * dir;
//                ptNew = new Point3d(newX, newY, ptPos.Z);
//            }

//            return ptNew;
//        }

//        private void insertLine(Point3d ptPos, double dir, double rotation)
//        {
//            Document acDoc = Application.DocumentManager.MdiActiveDocument;
//            Database acCurDb = acDoc.Database;

//            Point3d ptEnd = new Point3d();

//            if (rotation == 0.0)
//            {
//                double newX = ptPos.X - (bigCircleOffset - bigCircleRadius) * dir;
//                double newY = ptPos.Y;
//                ptEnd = new Point3d(newX, newY, ptPos.Z);
//            }
//            else
//            {
//                double newX = ptPos.X;
//                double newY = ptPos.Y - (bigCircleOffset - bigCircleRadius) * dir;
//                ptEnd = new Point3d(newX, newY, ptPos.Z);
//            }

//            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
//            {
//                BlockTable acBlkTbl;
//                BlockTableRecord acBlkTblRec;

//                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
//                                                OpenMode.ForRead) as BlockTable;

//                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
//                                                OpenMode.ForWrite) as BlockTableRecord;

//                using (Line acLine = new Line(ptPos, ptEnd))
//                {
//                    acBlkTblRec.AppendEntity(acLine);
//                    acTrans.AddNewlyCreatedDBObject(acLine, true);
//                }

//                acTrans.Commit();
//            }
//        }

//        private bool insertNumber(Point3d center, double rotation)
//        {
//            Document acDoc = Application.DocumentManager.MdiActiveDocument;

//            PromptStringOptions pStrOpts = new PromptStringOptions("\nNumber: ");
//            pStrOpts.AllowSpaces = false;
//            PromptResult pStrRes = acDoc.Editor.GetString(pStrOpts);
//            string result = pStrRes.StringResult;

//            if (pStrRes.Status == PromptStatus.Cancel) return false;

//            AttachmentPoint a = AttachmentPoint.MiddleCenter;
//            if (result.Length > 2)
//            {
//                Point3d insert = center;

//                a = AttachmentPoint.BottomRight;
//                if (rotation == 0.0)
//                {
//                    double newX = center.X;
//                    double newY = center.Y + textOffset;

//                    insert = new Point3d(newX, newY, center.Z);
//                }
//                else
//                {
//                    double newX = center.X - textOffset;
//                    double newY = center.Y;

//                    insert = new Point3d(newX, newY, center.Z);
//                }

//                insertText(insert, a, result, rotation);
//                return false;
//            }
//            else
//            {
//                a = AttachmentPoint.MiddleCenter;
//                insertText(center, a, result, 0.0);
//                return true;
//            }
//        }

//        private void insertText(Point3d ptInsert, AttachmentPoint a, string txt, double rotation)
//        {
//            Document acDoc = Application.DocumentManager.MdiActiveDocument;
//            Database acCurDb = acDoc.Database;

//            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
//            {
//                BlockTable acBlkTbl;
//                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
//                                                OpenMode.ForRead) as BlockTable;

//                BlockTableRecord acBlkTblRec;
//                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
//                                                OpenMode.ForWrite) as BlockTableRecord;

//                using (MText acMText = new MText())
//                {
//                    acMText.Attachment = a;
//                    acMText.Location = ptInsert;
//                    acMText.Contents = txt;
//                    acMText.TextHeight = textHeight;
//                    acMText.Rotation = rotation;

//                    acBlkTblRec.AppendEntity(acMText);
//                    acTrans.AddNewlyCreatedDBObject(acMText, true);
//                }

//                acTrans.Commit();
//            }
//        }

//        private bool insertFormSide(Point3d ptPos, double dir, double rotation)
//        {
//            Document acDoc = Application.DocumentManager.MdiActiveDocument;

//            PromptStringOptions pStrOpts = new PromptStringOptions("\nFormSide: ");
//            pStrOpts.AllowSpaces = false;
//            PromptResult pStrRes = acDoc.Editor.GetString(pStrOpts);
//            string result = pStrRes.StringResult;

//            if (pStrRes.Status == PromptStatus.Cancel) return false;

//            Point3d insert = ptPos;
//            AttachmentPoint a = AttachmentPoint.BottomCenter;

//            if (rotation == 0.0)
//            {
//                a = AttachmentPoint.BottomCenter;
//                double newX = ptPos.X - (bigCircleOffset - bigCircleRadius) * dir / 2;
//                double newY = ptPos.Y + textOffset;
//                insert = new Point3d(newX, newY, ptPos.Z);
//            }
//            else
//            {
//                a = AttachmentPoint.MiddleRight;
//                double newX = ptPos.X - textOffset;
//                double newY = ptPos.Y - (bigCircleOffset - bigCircleRadius) * dir / 2;
//                insert = new Point3d(newX, newY, ptPos.Z);
//            }

//            insertText(insert, a, result, 0);
//            return true;
//        }
//    }
//}