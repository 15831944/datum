using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DR = System.Drawing;

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
    class MARK_command
    {
        static string markLayerName = "K60";
        
        Transaction trans;

        Document doc;
        Database db;
        Editor ed;

        public MARK_command()
        {
            doc = Application.DocumentManager.MdiActiveDocument;
            db = doc.Database;
            ed = doc.Editor;

            trans = db.TransactionManager.StartTransaction();
        }


        public void run()
        {
            writeCadMessage("START");

            List<Mark> allMarks = getAllMarks(markLayerName);
            logic(allMarks);

            writeCadMessage("END");

            return;
        }

        
        private void logic(List<Mark> marks)
        {
            writeCadMessage("Vigade arv: " + marks.Count().ToString());

            foreach (Mark mark in marks)
            {
                createCircle(2000, 1, mark.IP);
                createCircle(200, 1, mark.IP);
            }
        }

        private void createCircle(double radius, int index, Point3d ip)
        {
            BlockTableRecord btr = trans.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForWrite) as BlockTableRecord;

            using (Circle circle = new Circle())
            {
                circle.Center = ip;
                circle.Radius = radius;
                circle.ColorIndex = index;
                btr.AppendEntity(circle);
                trans.AddNewlyCreatedDBObject(circle, true);
            }
        }

        private List<Mark> getAllMarks(string layer)
        {
            List<Mark> marks = new List<Mark>();

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                List<MText> allTexts = getAllText(layer, trans);
                marks = getMarkData(allTexts, trans);
            }

            return marks;
        }



        private List<Mark> getMarkData(List<MText> txts, Transaction trans)
        {
            List<Mark> parse = new List<Mark>();

            foreach (MText txt in txts)
            {
                Mark current = new Mark(txt.Contents, txt.Location);

                bool valid = current.validate();
                bool valid2 = current.validate2();

                if (valid || valid2) parse.Add(current);
            }

            return parse;
        }


        private List<MText> getAllText(string layer, Transaction trans)
        {
            List<MText> txt = new List<MText>();

            BlockTableRecord btr = trans.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForWrite) as BlockTableRecord;

            foreach (ObjectId id in btr)
            {
                Entity currentEntity = trans.GetObject(id, OpenMode.ForWrite, false) as Entity;

                if (currentEntity != null)
                {
                    if (currentEntity is MText)
                    {
                        MText br = currentEntity as MText;
                        if (br.Layer == layer)
                        {
                            txt.Add(br);
                        }
                    }

                    if (currentEntity is DBText)
                    {
                        DBText br = currentEntity as DBText;
                        if (br.Layer == layer)
                        {
                            MText myMtext = new MText();
                            myMtext.Contents = br.TextString;
                            myMtext.Location = br.Position;
                            txt.Add(myMtext);
                        }
                    }

                    if (currentEntity is MLeader)
                    {
                        MLeader br = currentEntity as MLeader;
                        if (br.Layer == layer)
                        {
                            if (br.ContentType == ContentType.MTextContent)
                            {
                                MText leaderText = br.MText;
                                txt.Add(leaderText);
                            }
                        }
                    }
                }
            }

            return txt;
        }


        private void writeCadMessage(string errorMessage)
        {
            ed.WriteMessage("\n" + errorMessage);
        }


        internal void close()
        {
            trans.Commit();
            trans.Dispose();

            ed.Regen();
        }

    }
}