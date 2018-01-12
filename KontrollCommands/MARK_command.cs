using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DR = System.Drawing;

////Autocad
//using Autodesk.AutoCAD.Runtime;
//using Autodesk.AutoCAD.ApplicationServices;
//using Autodesk.AutoCAD.DatabaseServices;
//using Autodesk.AutoCAD.Geometry;
//using Autodesk.AutoCAD.EditorInput;
//using Autodesk.AutoCAD.PlottingServices;

//Bricsys
using Teigha.Runtime;
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Bricscad.ApplicationServices;
using Bricscad.Runtime;
using Bricscad.EditorInput;
using Bricscad.PlottingServices;


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

            List<_Mark> wrongMarks = getAllWrongMarks();
            drawWrongMarks(wrongMarks);

            writeCadMessage("END");

            return;
        }

        
        private void drawWrongMarks(List<_Mark> marks)
        {
            writeCadMessage("Vigade arv: " + marks.Count().ToString());

            foreach (_Mark mark in marks)
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


        private List<_Mark> getAllWrongMarks()
        {
            List<_Mark> wrongMarks = new List<_Mark>();

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                List<MText> allTexts = getAllText(trans);
                wrongMarks = filterWrongMarks(allTexts, trans);
            }

            return wrongMarks;
        }


        private List<_Mark> filterWrongMarks(List<MText> txts, Transaction trans)
        {
            List<_Mark> good = new List<_Mark>();
            List<_Mark> bad = new List<_Mark>();

            foreach (MText txt in txts)
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
                
                if (txt.Contents.Contains("-"))
                {
                    _Mark current = new _Mark(txt.Contents, txt.Location);
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


        private List<MText> getAllText(Transaction trans)
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

                        txt.Add(br);
                    }

                    if (currentEntity is DBText)
                    {
                        DBText br = currentEntity as DBText;

                        MText myMtext = new MText();
                        myMtext.Contents = br.TextString;
                        myMtext.Location = br.Position;
                        myMtext.Layer = br.Layer;
                        txt.Add(myMtext);

                    }

                    if (currentEntity is MLeader)
                    {
                        MLeader br = currentEntity as MLeader;
                        if (br.ContentType == ContentType.MTextContent)
                        {
                            MText leaderText = br.MText;
                            leaderText.Layer = br.Layer;
                            txt.Add(leaderText);
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