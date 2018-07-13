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

using System.Xml;


namespace commands
{
    class WEIGHT_command_v2
    {
        _CONNECTION _c;

        static string name = "alfa";

        static string[] boxNames = { "KN-C", "KN-V27" };

        Dictionary<_Area_v2, int> local_stats;

        string dwg_dir;
        string xml_full;
        string xml_lock_full;

        public WEIGHT_command_v2(ref _CONNECTION c)
        {
            _c = c;

            local_stats = new Dictionary<_Area_v2, int>();

            _Db.HostApplicationServices hs = _Db.HostApplicationServices.Current;
            string dwg_path = hs.FindFile(_c.doc.Name, _c.doc.Database, _Db.FindFileHint.Default);

            dwg_dir = Path.GetDirectoryName(dwg_path);
            if (!dwg_dir.EndsWith(@"\")) { dwg_dir = dwg_dir + @"\"; }

            xml_full = dwg_dir + name + ".xml";
            xml_lock_full = dwg_dir + name + ".LCK";
        }
        

        public void unlock_after_crash()
        {
            if (File.Exists(xml_lock_full))
            {
                File.Delete(xml_lock_full);
            }
        }


        public void run(bool multy)
        {
            List<_Area_v2> areas = new List<_Area_v2>();

            if (multy == true)
            {
                areas = getAllAreas(boxNames);
            }
            else
            {
                areas = getSelectedAreas(boxNames);
            }

            if (areas.Count < 1)
            {
                string names = string.Join(", ", boxNames.ToArray());
                throw new DMTException("[ERROR] - (" + names + ") not found");
            }

            List<_Mark> allMarks = getAllMarks();
            if (allMarks.Count < 1) throw new DMTException("[ERROR] - " + "Reinforcement marks" + " not found");
            if (!File.Exists(xml_full)) throw new DMTException("[ERROR] Joonise kaustas ei ole XML faili nimega: " + name + ".xml");
            if (File.Exists(xml_lock_full)) throw new DMTException("[ERROR] XML fail nimega: " + name + ".xml" + " on lukkus!");

            File.Create(xml_lock_full).Dispose();
            write("[XML] LOCK ON");

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xml_full);
            List<XmlNode> bending = XML_Handle.getAllRebar(xmlDoc);

            File.Delete(xml_lock_full);
            write("[XML] LOCK OFF");

            Dictionary<_Area_v2, List<_Mark>> local_reinforcement = matchMarkArea(areas, allMarks);
            local_stats = generateAllWeights(local_reinforcement, bending);

            return;
        }


        public void output_local()
        {
            foreach (_Area_v2 current in local_stats.Keys)
            {
                outputWeight(current, local_stats[current]);
            }
            
            return;
        }


        private void outputWeight(_Area_v2 a, int weight)
        {
            _Db.DBObject currentEntity = _c.trans.GetObject(a.ID, _Db.OpenMode.ForWrite, false) as _Db.DBObject;

            if (currentEntity is _Db.BlockReference)
            {
                _Db.BlockReference blockRef = currentEntity as _Db.BlockReference;

                foreach (_Db.ObjectId arId in blockRef.AttributeCollection)
                {
                    _Db.DBObject obj = _c.trans.GetObject(arId, _Db.OpenMode.ForWrite);
                    _Db.AttributeReference ar = obj as _Db.AttributeReference;

                    if (ar != null)
                    {
                        if (ar.Tag == "SUMMA_OVRIG_ARMERING")
                        {
                            if (weight == 0)
                            {
                                write("[WARINING] Weight set to 'YYY'");
                                ar.TextString = "YYY";
                            }
                            else
                            {
                                ar.TextString = weight.ToString();
                            }
                        }
                    }
                }
            }

            _c.trans.Commit();
        }


        private Dictionary<_Area_v2, int> generateAllWeights(Dictionary<_Area_v2, List<_Mark>> reinf, List<XmlNode> bending)
        {
            Dictionary<_Area_v2, int> stats = new Dictionary<_Area_v2, int>();

            foreach (_Area_v2 current in reinf.Keys)
            {
                List<_Mark> currentReinf = reinf[current];

                int currentWeight = calculateWeight(currentReinf, bending);
                stats[current] = currentWeight;
            }

            return stats;
        }


        private int calculateWeight(List<_Mark> reinf, List<XmlNode> bending)
        {
            Dictionary<_Mark, XmlNode> matches = new Dictionary<_Mark, XmlNode>();
            Dictionary<_Mark, double> weights = new Dictionary<_Mark, double>();
            matches = findMarksInXML(reinf, bending);

            double currentWeight = 0;

            List<_Mark> emptyMarks = new List<_Mark>();

            foreach (_Mark m in matches.Keys)
            {
                if (matches[m] == null)
                {
                    emptyMarks.Add(m);
                }
            }

            if (emptyMarks.Count == 0)
            {
                foreach (_Mark m in matches.Keys)
                {
                    double weight = getRebarWeights(matches[m]);
                    weights[m] = weight;
                }
            }
            else
            {
                foreach (_Mark m in emptyMarks)
                {
                    write("[ERROR] Can not find match for [" + m.ToString() + "] in XML");
                }

                return 0;
            }

            foreach (_Mark m in weights.Keys)
            {
                if (weights[m] == 0)
                {
                    emptyMarks.Add(m);
                }
            }

            if (emptyMarks.Count == 0)
            {
                foreach (_Mark m in weights.Keys)
                {
                    currentWeight = currentWeight + (m.Number * weights[m]);
                }
            }
            else
            {
                foreach (_Mark m in emptyMarks)
                {
                    write("[ERROR] Not enought information for [" + m.ToString() + "] in XML");
                }

                return 0;
            }

            return (int)Math.Ceiling(currentWeight);
        }


        private double getRebarWeights(XmlNode row)
        {
            XmlNode rebar = row["B2aBar"];

            if (rebar == null) return 0;

            string weightString = XML_Handle.emptyNodehandle(rebar, "Weight");

            double weight = 0.0;

            try
            {
                weight = Double.Parse(weightString, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {

            }

            return weight;
        }


        private Dictionary<_Mark, XmlNode> findMarksInXML(List<_Mark> marks, List<XmlNode> bending)
        {
            Dictionary<_Mark, XmlNode> markMatch = new Dictionary<_Mark, XmlNode>();

            foreach (_Mark m in marks)
            {
                markMatch[m] = matchMarkToXML(m, bending);
            }

            return markMatch;
        }


        private XmlNode matchMarkToXML(_Mark m, List<XmlNode> rows)
        {
            foreach (XmlNode row in rows)
            {
                XmlNode rebar = row["B2aBar"];

                if (rebar == null) return null;

                string type = XML_Handle.emptyNodehandle(rebar, "Type");
                string pos_nr = XML_Handle.emptyNodehandle(rebar, "Litt");
                string diam = XML_Handle.emptyNodehandle(rebar, "Dim");

                if (m.Position_Shape == type && m.Position_Nr.ToString() == pos_nr && m.Diameter.ToString() == diam)
                {
                    return row;
                }
            }

            return null;
        }




        private Dictionary<_Area_v2, List<_Mark>> matchMarkArea(List<_Area_v2> areas, List<_Mark> allMarks)
        {
            Dictionary<_Area_v2, List<_Mark>> sorted = new Dictionary<_Area_v2, List<_Mark>>();

            foreach (_Area_v2 area in areas)
            {
                List<_Mark> marks = getMarksInArea(area, allMarks);
                List<_Mark> validMarks = new List<_Mark>();

                foreach (_Mark m in marks)
                {
                    bool valid = m.validate();
                    if (valid) validMarks.Add(m);
                }

                List<_Mark> sumMarks = getSummary(validMarks);
                sorted[area] = sumMarks;
            }

            return sorted;
        }


        private List<_Mark> getSummary(List<_Mark> marks)
        {
            List<_Mark> sumMarks = new List<_Mark>();

            foreach (_Mark m in marks)
            {
                if (sumMarks.Contains(m))
                {
                    int index = sumMarks.FindIndex(a => a == m);
                    sumMarks[index].Number += m.Number;
                }
                else
                {
                    _Mark nm = new _Mark(m.Number, m.Diameter, m.Position, m.Position_Shape, m.Position_Nr);
                    sumMarks.Add(nm);
                }
            }

            return sumMarks;
        }


        private List<_Mark> getAllMarks()
        {
            List<_Mark> marks = new List<_Mark>();

            List<_Db.MText> allTexts = getAllText();
            marks = getMarkData(allTexts);

            return marks;
        }


        private List<_Mark> getMarksInArea(_Area_v2 area, List<_Mark> allmarks)
        {
            List<_Mark> marks = new List<_Mark>();

            for (int i = allmarks.Count - 1; i >= 0; i--)
            {
                _Mark mark = allmarks[i];
                if (area.isPointInArea(mark.IP))
                {
                    marks.Add(mark);
                    allmarks.RemoveAt(i);
                }
            }

            return marks;
        }


        private List<_Mark> getMarkData(List<_Db.MText> txts)
        {
            List<_Mark> parse = new List<_Mark>();

            foreach (_Db.MText txt in txts)
            {
                _Mark current = new _Mark(txt.Contents, txt.Location, txt.Layer);
                parse.Add(current);
            }

            return parse;
        }


        private List<_Area_v2> getSelectedAreas(string[] blockNames)
        {
            List<_Area_v2> areas = new List<_Area_v2>();

            List<_Db.BlockReference> blocks = getSelectedBlockReference(blockNames);
            areas = getBoxAreas(blocks);

            return areas;
        }


        private List<_Area_v2> getAllAreas(string[] blockNames)
        {
            List<_Area_v2> areas = new List<_Area_v2>();


            List<_Db.BlockReference> blocks = new List<_Db.BlockReference>();

            foreach (string name in blockNames)
            {
                List<_Db.BlockReference> temp = getAllBlockReference(name);
                blocks.AddRange(temp);
            }

            areas = getBoxAreas(blocks);

            return areas;
        }


        private List<_Area_v2> getBoxAreas(List<_Db.BlockReference> blocks)
        {
            List<_Area_v2> parse = new List<_Area_v2>();

            foreach (_Db.BlockReference block in blocks)
            {
                _Db.Extents3d blockExtents = block.GeometricExtents;
                _Area_v2 area = new _Area_v2(block.Id, blockExtents.MinPoint, blockExtents.MaxPoint);
                parse.Add(area);
            }

            parse = parse.OrderBy(o => o.Start.X).ToList();

            return parse;
        }


        private List<_Db.BlockReference> getSelectedBlockReference(string[] blockNames)
        {
            List<_Db.BlockReference> refs = new List<_Db.BlockReference>();
            _Ed.PromptSelectionOptions opts = new _Ed.PromptSelectionOptions();
            opts.MessageForAdding = "\nSelect BLOCK " + blockNames[0] + " / " + blockNames[1];

            _Ed.PromptSelectionResult selection = _c.ed.GetSelection(opts);

            if (selection.Status == _Ed.PromptStatus.OK)
            {
                _Db.ObjectId[] selectionIds = selection.Value.GetObjectIds();

                foreach (_Db.ObjectId id in selectionIds)
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
                            if (blockNames.Contains(block.Name))
                            {
                                refs.Add(blockRef);
                            }
                        }
                    }
                }
            }

            return refs;
        }


        private List<_Db.BlockReference> getAllBlockReference(string blockName)
        {
            List<_Db.BlockReference> refs = new List<_Db.BlockReference>();

            if (_c.blockTable.Has(blockName))
            {
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
                            if (block.Name == blockName)
                            {
                                refs.Add(blockRef);
                            }
                        }
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


        private void insertText(string value, _Ge.Point3d position, string layer)
        {
            _Db.BlockTableRecord btr = _c.trans.GetObject(_c.modelSpace.Id, _Db.OpenMode.ForWrite) as _Db.BlockTableRecord;

            using (_Db.DBText acText = new _Db.DBText())
            {
                acText.Layer = layer;

                acText.Position = position;
                acText.Height = 120;
                acText.TextString = value;

                btr.AppendEntity(acText);
                _c.trans.AddNewlyCreatedDBObject(acText, true);
            }           
        }


        private void write(string message)
        {
            _c.ed.WriteMessage("\n" + message);
        }

    }
}