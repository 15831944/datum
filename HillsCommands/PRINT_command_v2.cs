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

using System.Collections.Specialized;


namespace commands
{
    class PRINT_command_v2
    {
        _CONNECTION _c;

        string[] newBoxNames = { "KN-C", "KN-V23", "KN-V27" };

        List<_Area_v2> local_stats;

        _Pl.PlotEngine engine;

        _Db.LayoutManager layoutManager;


        public PRINT_command_v2(ref _CONNECTION c)
        {
            _c = c;

            local_stats = new List<_Area_v2>();

            engine = _Pl.PlotFactory.CreatePublishEngine();
            
            layoutManager = _Db.LayoutManager.Current;
        }


        public void run(bool multy)
        {
            List<_Area_v2> areas = new List<_Area_v2>();

            if (multy == true)
            {
                areas = getAllAreas(newBoxNames);
            }
            else
            {
                areas = getSelectedAreas(newBoxNames);
            }

            if (areas.Count < 1)
            {
                string names = string.Join(", ", newBoxNames.ToArray());
                throw new DMTException("[ERROR] - (" + names + ") not found");
            }

            mainCreationLoop(areas);

            return;
        }
        

        private void mainCreationLoop(List<_Area_v2> areas)
        {
            foreach (_Area_v2 area in areas)
            {
                string name = getAreaName(area);
                double scale = getAreaScale(area);
                _Ge.Point3d centerPoint = getAreaCenter(area);

                _Db.Layout lay = createLayoutandSetActive(name);
                //setLayoutPlotSettings(lay, "ISO_full_bleed_A3_(297.00_x_420.00_MM)", "monochrome.ctb", "DWG To PDF.pc3");
                setLayoutPlotSettings(lay, "PDFCreator", "A3", "monochrome.ctb");

                _Db.Viewport vp = layoutViewportGetter(lay);
                _Db.Extents2d ext = getMaximumExtents(lay);
                setViewportGeometry(vp, ext, 1.05);
                setViewportParameters(vp, scale, centerPoint);

                Dictionary<_Db.Layout, string> layouts = new Dictionary<_Db.Layout, string>();
                layouts[lay] = name;
                plotDriver(layouts);

                removeLayout(lay);
            }                     

            _c.ed.Regen();
        }


        public void plotDriver(Dictionary<_Db.Layout, string> layouts)
        {
            short bgp = (short)_Ap.Application.GetSystemVariable("BACKGROUNDPLOT");

            if (layouts.Keys.Count > 0)
            {
                _Ap.Application.SetSystemVariable("BACKGROUNDPLOT", 0);

                try
                {
                    string dwgFile = _c.db.Filename;
                    string outputDir = Path.GetDirectoryName(_c.db.Filename);
                    string layoutName = layouts[layouts.Keys.First()];

                    if (layoutName.Length == 0) { layoutName = generateRandomString(10); }

                    string pdf = layoutName + ".pdf";
                    string pdfFile = Path.Combine(outputDir, pdf);


                    //TODO invalid chars


                    if (File.Exists(pdfFile))
                    {
                        layoutName = generateRandomString(20);
                        pdf = layoutName + ".pdf";
                        pdfFile = Path.Combine(outputDir, pdf);
                    }

                    //print_export(layouts.Keys.First(), pdfFile);

                    PlotLayout(layouts.Keys.First(), pdfFile);

                }
                catch (System.Exception e)
                {
                    _c.ed.WriteMessage("\nError: {0}\n{1}", e.Message, e.StackTrace);
                }
                finally
                {
                    _Ap.Application.SetSystemVariable("BACKGROUNDPLOT", bgp);
                }

            }
        }


        public void PlotLayout(_Db.Layout lay, string location)
        {
            using (_Pl.PlotInfo plotInfo = new _Pl.PlotInfo())
            {
                plotInfo.Layout = lay.ObjectId;

                using (_Db.PlotSettings plotSettings = new _Db.PlotSettings(lay.ModelType))
                {
                    plotSettings.CopyFrom(lay);
                    plotInfo.OverrideSettings = plotSettings;

                    _Db.PlotSettingsValidator plotValidator = _Db.PlotSettingsValidator.Current;

                    using (_Pl.PlotInfoValidator infoValidator = new _Pl.PlotInfoValidator())
                    {
                        infoValidator.MediaMatchingPolicy = _Pl.MatchingPolicy.MatchEnabled;
                        infoValidator.Validate(plotInfo);

                        using (_Pl.PlotProgressDialog dialog = new _Pl.PlotProgressDialog(false, 1, true))
                        {
                            write("Plotting: " + _c.doc.Name + " - " + lay.LayoutName);

                            engine.BeginPlot(dialog, null);
                            engine.BeginDocument(plotInfo, _c.doc.Name, null, 1, true, location);
                            using (_Pl.PlotPageInfo pageInfo = new _Pl.PlotPageInfo())
                            {
                                engine.BeginPage(pageInfo, plotInfo, true, null);
                            }
                            engine.BeginGenerateGraphics(null);

                            engine.EndGenerateGraphics(null);
                            engine.EndPage(null);
                            engine.EndDocument(null);
                            engine.EndPlot(null);
                        }
                    }

                }
            }
        }


        private string getAreaName(_Area_v2 area)
        {
            string ritn_nr = "x";

            _Db.DBObject currentEntity = _c.trans.GetObject(area.ID, _Db.OpenMode.ForWrite, false) as _Db.DBObject;

            if (currentEntity is _Db.BlockReference)
            {
                _Db.BlockReference blockRef = currentEntity as _Db.BlockReference;

                foreach (_Db.ObjectId arId in blockRef.AttributeCollection)
                {
                    _Db.DBObject obj = _c.trans.GetObject(arId, _Db.OpenMode.ForWrite);
                    _Db.AttributeReference ar = obj as _Db.AttributeReference;
                    if (ar != null)
                    {
                        if (ar.Tag == "RITN_NR") ritn_nr = ar.TextString;

                        if (blockRef.Name == "KN-V23")
                        {
                            if (ar.Tag == "RITN_23_NR") ritn_nr = ar.TextString;
                        }
                        else if (blockRef.Name == "KN-V27")
                        {
                            if (ar.Tag == "RITN_27_NR") ritn_nr = ar.TextString;
                        }
                    }
                }

                if (blockRef.Name == "KN-V23")
                {
                    if (!ritn_nr.Contains("23"))
                    {
                        ritn_nr = "V23-" + ritn_nr;
                    }
                }
                else if (blockRef.Name == "KN-V27")
                {
                    if (!ritn_nr.Contains("73"))
                    {
                        ritn_nr = "V27-" + ritn_nr;
                    }
                }
            }

            return ritn_nr;
        }


        private double getAreaScale(_Area_v2 area)
        {
            double scale = 1;

            _Db.DBObject currentEntity = _c.trans.GetObject(area.ID, _Db.OpenMode.ForWrite, false) as _Db.DBObject;

            if (currentEntity is _Db.BlockReference)
            {
                _Db.BlockReference blockRef = currentEntity as _Db.BlockReference;
                scale = blockRef.ScaleFactors.X;
            }

            return scale;
        }


        private _Ge.Point3d getAreaCenter(_Area_v2 area)
        {
            _Ge.Point3d center = new _Ge.Point3d(0, 0, 0);

            _Db.DBObject currentEntity = _c.trans.GetObject(area.ID, _Db.OpenMode.ForWrite, false) as _Db.DBObject;

            if (currentEntity is _Db.BlockReference)
            {
                _Db.BlockReference blockRef = currentEntity as _Db.BlockReference;
                _Ge.Point3d max = blockRef.GeometricExtents.MaxPoint;
                _Ge.Point3d min = blockRef.GeometricExtents.MinPoint;

                center = new _Ge.Point3d(min.X + ((max.X - min.X) / 2), min.Y + ((max.Y - min.Y) / 2), 0);
            }

            return center;
        }


        private void removeLayout(_Db.Layout lay)
        {
            layoutManager.DeleteLayout(lay.LayoutName);
            layoutManager.CurrentLayout = "Model";
        }


        private _Db.Layout createLayoutandSetActive(string name) //EH?
        {
            string randomName = generateRandomString(20);

            _Db.ObjectId id = layoutManager.GetLayoutId(randomName);

            if (!id.IsValid)
            {
                id = layoutManager.CreateLayout(randomName);
            }
            else
            {
                write("Layout " + randomName + " already exists.");
            }

            _Db.Layout layout = _c.trans.GetObject(id, _Db.OpenMode.ForWrite) as _Db.Layout;
            if (layout.TabSelected == false)
            {
                layoutManager.CurrentLayout = randomName;
            }

            return layout;
        }


        private void setLayoutPlotSettings(_Db.Layout lay, string device, string pageSize, string styleSheet)
        {
            using (_Db.PlotSettings plotSettings = new _Db.PlotSettings(lay.ModelType))
            {
                plotSettings.CopyFrom(lay);
                _Db.PlotSettingsValidator validator = _Db.PlotSettingsValidator.Current;

                StringCollection devices = validator.GetPlotDeviceList();
                if (devices.Contains(device))
                {
                    validator.SetPlotConfigurationName(plotSettings, device, null);
                    validator.RefreshLists(plotSettings);
                }
                else
                {
                    write("[WARNING] Device not found!");
                }

                StringCollection paperSizes = validator.GetCanonicalMediaNameList(plotSettings);
                if (paperSizes.Contains(pageSize))
                {
                    validator.SetCanonicalMediaName(plotSettings, pageSize);
                }
                else
                {
                    write("[WARNING] Paper not found!");
                }

                StringCollection styleSheets = validator.GetPlotStyleSheetList();
                if (styleSheets.Contains(styleSheet))
                {
                    validator.SetCurrentStyleSheet(plotSettings, styleSheet);
                }
                else
                {
                    write("[WARNING] Style not found!");
                }

                lay.CopyFrom(plotSettings);
            }
        }


        private _Db.Viewport layoutViewportGetter(_Db.Layout lay)
        {
            _Db.ObjectIdCollection viewIds = lay.GetViewports();
            _Db.Viewport vp = null;

            foreach (_Db.ObjectId id in viewIds)
            {
                _Db.Viewport vp2 = _c.trans.GetObject(id, _Db.OpenMode.ForWrite) as _Db.Viewport;
                if (vp2 != null && vp2.Number == 2)
                {
                    vp = vp2;
                    break;
                }
            }

            if (vp == null)
            {
                _Db.BlockTableRecord btr = _c.trans.GetObject(lay.BlockTableRecordId, _Db.OpenMode.ForWrite) as _Db.BlockTableRecord;

                vp = new _Db.Viewport();

                btr.AppendEntity(vp);
                _c.trans.AddNewlyCreatedDBObject(vp, true);

                vp.On = true;
                vp.GridOn = false;
            }

            return vp;
        }


        private void setViewportGeometry(_Db.Viewport vp, _Db.Extents2d ext, double fac = 1.0)
        {
            vp.Width = (ext.MaxPoint.X - ext.MinPoint.X) * fac;
            vp.Height = (ext.MaxPoint.Y - ext.MinPoint.Y) * fac;

            _Ge.Point2d gg = _Ge.Point2d.Origin + (ext.MaxPoint - ext.MinPoint) * 0.5;
            vp.CenterPoint = flatten(gg);
        }


        private void setViewportParameters(_Db.Viewport vp, double scale, _Ge.Point3d center)
        {
            vp.ViewCenter = flatten(center);
            vp.CustomScale = 1 / scale;
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

                _Area_v2 area = new _Area_v2(block.ObjectId, blockExtents.MinPoint, blockExtents.MaxPoint);
                parse.Add(area);
            }

            parse = parse.OrderBy(o => o.Start.X).ToList();

            return parse;
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


        private _Db.Extents2d getMaximumExtents(_Db.Layout lay)
        {
            double div = lay.PlotPaperUnits == _Db.PlotPaperUnit.Inches ? 25.4 : 1.0;
            bool trigger = lay.PlotRotation == _Db.PlotRotation.Degrees090 || lay.PlotRotation == _Db.PlotRotation.Degrees270;

            var min = swapCoords(lay.PlotPaperMargins.MinPoint, trigger) / div;
            var max = (swapCoords(lay.PlotPaperSize, trigger) - swapCoords(lay.PlotPaperMargins.MaxPoint, trigger).GetAsVector()) / div;

            return new _Db.Extents2d(min, max);
        }


        private _Ge.Point3d flatten(_Ge.Point2d pt)
        {
            return new _Ge.Point3d(pt.X, pt.Y, 0);
        }


        private _Ge.Point2d flatten(_Ge.Point3d pt)
        {
            return new _Ge.Point2d(pt.X, pt.Y);
        }


        private _Ge.Point2d swapCoords(_Ge.Point2d pt, bool flip = true)
        {
            return flip ? new _Ge.Point2d(pt.Y, pt.X) : pt;
        }


        private string generateRandomString(int number)
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            char[] stringChars = new char[number];
            Random random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            string finalString = new String(stringChars);
            return finalString;
        }


        private void write(string message)
        {
            _c.ed.WriteMessage("\n" + message);
        }

    }
}
