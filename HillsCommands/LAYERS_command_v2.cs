using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

//Autocad
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.PlottingServices;
using Autodesk.AutoCAD.Publishing;

////Bricsys
//using Teigha.Runtime;
//using Teigha.DatabaseServices;
//using Teigha.Geometry;
//using Bricscad.ApplicationServices;
//using Bricscad.Runtime;
//using Bricscad.EditorInput;

namespace commands
{
    class LAYERS_command_v2
    {
        string[] newBoxNames = { "KN-C", "KN-V23", "KN-V27" };

        List<Area_v2> local_stats;

        Document doc;
        Database db;
        Editor ed;

        Transaction trans;
        LayoutManager layerManager;

        public LAYERS_command_v2()
        {
            local_stats = new List<Area_v2>();

            doc = Application.DocumentManager.MdiActiveDocument;
            db = doc.Database;
            ed = doc.Editor;

            trans = db.TransactionManager.StartTransaction();
            layerManager = LayoutManager.Current;
        }

        public void run()
        {
            writeCadMessage("");
            writeCadMessage("START");

            plotSingle();

            //List<Area_v2> areas = new List<Area_v2>();

            //areas = getAllAreas(newBoxNames);

            //if (areas.Count < 1)
            //{
            //    string names = string.Join(", ", newBoxNames.ToArray());
            //    writeCadMessage("[ERROR] - (" + names + ") not found");
            //}

            //mainCreationLoop(areas);

            writeCadMessage("DONE");

            return;
        }


        private void mainCreationLoop(List<Area_v2> areas)
        {
            foreach (Area_v2 area in areas)
            {
                string name = getAreaName(area);
                double scale = getAreaScale(area);
                Point3d centerPoint = getAreaCenter(area);

                Layout lay = createLayoutandSetActive(name);
                setPlotSettings(lay, "ISO_full_bleed_A3_(297.00_x_420.00_MM)", "monochrome.ctb", "DWG To PDF.pc3");

                Viewport vp = layoutViewportGetter(lay);
                Extents2d ext = getMaximumExtents(lay);
                setViewportGeometry(vp, ext, 1.05);
                setViewportParameters(vp, scale, centerPoint);



                //plotSingle(lay, name);
            }

            ed.Regen();
        }

        //private void plotSingle(Layout lay, string name)
        private void plotSingle()
        {
            // Reference the Layout Manager
            LayoutManager acLayoutMgr = LayoutManager.Current;

            // Get the current layout and output its name in the Command Line window
            Layout acLayout = trans.GetObject(acLayoutMgr.GetLayoutId(acLayoutMgr.CurrentLayout), OpenMode.ForRead) as Layout;

            // Get the PlotInfo from the layout
            using (PlotInfo acPlInfo = new PlotInfo())
            {
                acPlInfo.Layout = acLayout.ObjectId;

                // Get a copy of the PlotSettings from the layout
                using (PlotSettings acPlSet = new PlotSettings(acLayout.ModelType))
                {
                    acPlSet.CopyFrom(acLayout);

                    // Update the PlotSettings object
                    PlotSettingsValidator acPlSetVdr = PlotSettingsValidator.Current;

                    // Set the plot type
                    acPlSetVdr.SetPlotType(acPlSet, Autodesk.AutoCAD.DatabaseServices.PlotType.Extents);

                    // Set the plot scale
                    acPlSetVdr.SetUseStandardScale(acPlSet, true);
                    acPlSetVdr.SetStdScaleType(acPlSet, StdScaleType.ScaleToFit);

                    // Center the plot
                    acPlSetVdr.SetPlotCentered(acPlSet, true);

                    // Set the plot device to use
                    acPlSetVdr.SetPlotConfigurationName(acPlSet, "DWG To PDF.pc3", "ISO_full_bleed_A3_(297.00_x_420.00_MM)");

                    // Set the plot info as an override since it will not be saved back to the layout
                    acPlInfo.OverrideSettings = acPlSet;

                    // Validate the plot info
                    using (PlotInfoValidator acPlInfoVdr = new PlotInfoValidator())
                    {
                        acPlInfoVdr.MediaMatchingPolicy = MatchingPolicy.MatchEnabled;
                        acPlInfoVdr.Validate(acPlInfo);

                        // Check to see if a plot is already in progress
                        if (PlotFactory.ProcessPlotState == ProcessPlotState.NotPlotting)
                        {
                            using (PlotEngine acPlEng = PlotFactory.CreatePublishEngine())
                            {
                                // Track the plot progress with a Progress dialog
                                using (PlotProgressDialog acPlProgDlg = new PlotProgressDialog(false, 1, true))
                                {
                                    using ((acPlProgDlg))
                                    {
                                        // Define the status messages to display when plotting starts
                                        acPlProgDlg.set_PlotMsgString(PlotMessageIndex.DialogTitle, "Plot Progress");
                                        acPlProgDlg.set_PlotMsgString(PlotMessageIndex.CancelJobButtonMessage, "Cancel Job");
                                        acPlProgDlg.set_PlotMsgString(PlotMessageIndex.CancelSheetButtonMessage, "Cancel Sheet");
                                        acPlProgDlg.set_PlotMsgString(PlotMessageIndex.SheetSetProgressCaption, "Sheet Set Progress");
                                        acPlProgDlg.set_PlotMsgString(PlotMessageIndex.SheetProgressCaption, "Sheet Progress");

                                        // Set the plot progress range
                                        acPlProgDlg.LowerPlotProgressRange = 0;
                                        acPlProgDlg.UpperPlotProgressRange = 100;
                                        acPlProgDlg.PlotProgressPos = 0;

                                        // Display the Progress dialog
                                        acPlProgDlg.OnBeginPlot();
                                        acPlProgDlg.IsVisible = true;

                                        // Start to plot the layout
                                        acPlEng.BeginPlot(acPlProgDlg, null);

                                        // Define the plot output
                                        acPlEng.BeginDocument(acPlInfo, doc.Name, null, 1, true, "c:\\myplot");

                                        // Display information about the current plot
                                        acPlProgDlg.set_PlotMsgString(PlotMessageIndex.Status, "Plotting: " + doc.Name + " - " + acLayout.LayoutName);

                                        // Set the sheet progress range
                                        acPlProgDlg.OnBeginSheet();
                                        acPlProgDlg.LowerSheetProgressRange = 0;
                                        acPlProgDlg.UpperSheetProgressRange = 100;
                                        acPlProgDlg.SheetProgressPos = 0;

                                        // Plot the first sheet/layout
                                        using (PlotPageInfo acPlPageInfo = new PlotPageInfo())
                                        {
                                            acPlEng.BeginPage(acPlPageInfo, acPlInfo, true, null);
                                        }

                                        acPlEng.BeginGenerateGraphics(null);
                                        acPlEng.EndGenerateGraphics(null);

                                        // Finish plotting the sheet/layout
                                        acPlEng.EndPage(null);
                                        acPlProgDlg.SheetProgressPos = 100;
                                        acPlProgDlg.OnEndSheet();

                                        // Finish plotting the document
                                        acPlEng.EndDocument(null);

                                        // Finish the plot
                                        acPlProgDlg.PlotProgressPos = 100;
                                        acPlProgDlg.OnEndPlot();
                                        acPlEng.EndPlot(null);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private string getAreaName(Area_v2 area)
        {
            string ritn_nr = "x";

            DBObject currentEntity = trans.GetObject(area.ID, OpenMode.ForWrite, false) as DBObject;

            if (currentEntity is BlockReference)
            {
                BlockReference blockRef = currentEntity as BlockReference;

                foreach (ObjectId arId in blockRef.AttributeCollection)
                {
                    DBObject obj = trans.GetObject(arId, OpenMode.ForWrite);
                    AttributeReference ar = obj as AttributeReference;
                    if (ar != null)
                    {
                        if (ar.Tag == "RITN_NR") ritn_nr = ar.TextString;
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

        private double getAreaScale(Area_v2 area)
        {
            double scale = 1;

            DBObject currentEntity = trans.GetObject(area.ID, OpenMode.ForWrite, false) as DBObject;

            if (currentEntity is BlockReference)
            {
                BlockReference blockRef = currentEntity as BlockReference;
                scale = blockRef.ScaleFactors.X;
            }

            return scale;
        }

        private Point3d getAreaCenter(Area_v2 area)
        {
            Point3d center = new Point3d(0, 0, 0);

            DBObject currentEntity = trans.GetObject(area.ID, OpenMode.ForWrite, false) as DBObject;

            if (currentEntity is BlockReference)
            {
                BlockReference blockRef = currentEntity as BlockReference;
                Point3d max = blockRef.GeometricExtents.MaxPoint;
                Point3d min = blockRef.GeometricExtents.MinPoint;

                center = new Point3d(min.X + ((max.X - min.X) / 2), min.Y + ((max.Y - min.Y) / 2), 0);
            }

            return center;
        }

        private Layout createLayoutandSetActive(string name)
        {
            ObjectId id = layerManager.GetLayoutId(name);

            if (!id.IsValid)
            {
                id = layerManager.CreateLayout(name);
            }
            else
            {
                writeCadMessage("Layout " + name + " already exists.");
            }

            Layout layout = trans.GetObject(id, OpenMode.ForWrite) as Layout;
            if (layout.TabSelected == false)
            {
                layerManager.CurrentLayout = name;
            }

            return layout;
        }

        private void setPlotSettings(Layout lay, string pageSize, string styleSheet, string device)
        {
            using (var ps = new PlotSettings(lay.ModelType))
            {
                ps.CopyFrom(lay);

                var psv = PlotSettingsValidator.Current;

                // Set the device
                var devs = psv.GetPlotDeviceList();
                if (devs.Contains(device))
                {
                    psv.SetPlotConfigurationName(ps, device, null);
                    psv.RefreshLists(ps);
                }
                else
                {
                    writeCadMessage("[WARNING] Device not found!");
                }

                // Set the media name/size
                var mns = psv.GetCanonicalMediaNameList(ps);
                if (mns.Contains(pageSize))
                {
                    psv.SetCanonicalMediaName(ps, pageSize);
                }
                else
                {
                    writeCadMessage("[WARNING] Paper not found!");
                }

                // Set the pen settings
                var ssl = psv.GetPlotStyleSheetList();
                if (ssl.Contains(styleSheet))
                {
                    psv.SetCurrentStyleSheet(ps, styleSheet);
                }
                else
                {
                    writeCadMessage("[WARNING] Style not found!");
                }

                lay.CopyFrom(ps);
            }
        }

        private Viewport layoutViewportGetter(Layout lay)
        {
            ObjectIdCollection viewIds = lay.GetViewports();
            Viewport vp = null;

            foreach (ObjectId id in viewIds)
            {
                Viewport vp2 = trans.GetObject(id, OpenMode.ForWrite) as Viewport;
                if (vp2 != null && vp2.Number == 2)
                {
                    vp = vp2;
                    break;
                }
            }

            if (vp == null)
            {
                BlockTableRecord btr = trans.GetObject(lay.BlockTableRecordId, OpenMode.ForWrite) as BlockTableRecord;

                vp = new Viewport();

                btr.AppendEntity(vp);
                trans.AddNewlyCreatedDBObject(vp, true);

                vp.On = true;
                vp.GridOn = false;
            }

            return vp;
        }

        private void setViewportGeometry(Viewport vp, Extents2d ext, double fac = 1.0)
        {
            vp.Width = (ext.MaxPoint.X - ext.MinPoint.X) * fac;
            vp.Height = (ext.MaxPoint.Y - ext.MinPoint.Y) * fac;

            Point2d gg = Point2d.Origin + (ext.MaxPoint - ext.MinPoint) * 0.5;
            vp.CenterPoint = flatten(gg);
        }

        private void setViewportParameters(Viewport vp, double scale, Point3d center)
        {
            vp.ViewCenter = flatten(center);
            vp.CustomScale = 1 / scale;
        }

        private List<Area_v2> getAllAreas(string[] blockNames)
        {
            List<Area_v2> areas = new List<Area_v2>();

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                List<BlockReference> blocks = new List<BlockReference>();

                foreach (string name in blockNames)
                {
                    List<BlockReference> temp = getAllBlockReference(name, trans);
                    blocks.AddRange(temp);
                }

                areas = getBoxAreas(blocks, trans);
            }

            return areas;
        }


        private List<Area_v2> getBoxAreas(List<BlockReference> blocks, Transaction trans)
        {
            List<Area_v2> parse = new List<Area_v2>();

            foreach (BlockReference block in blocks)
            {
                Extents3d blockExtents = block.GeometricExtents;

                Area_v2 area = new Area_v2(block.ObjectId, blockExtents.MinPoint, blockExtents.MaxPoint);
                parse.Add(area);
            }

            parse = parse.OrderBy(o => o.Start.X).ToList();

            return parse;
        }


        private List<BlockReference> getAllBlockReference(string blockName, Transaction trans)
        {
            List<BlockReference> refs = new List<BlockReference>();

            BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;

            if (bt.Has(blockName))
            {
                BlockTableRecord btr = trans.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForWrite) as BlockTableRecord;

                foreach (ObjectId id in btr)
                {
                    DBObject currentEntity = trans.GetObject(id, OpenMode.ForWrite, false) as DBObject;

                    if (currentEntity == null)
                    {
                        continue;
                    }

                    else if (currentEntity is BlockReference)
                    {
                        BlockReference blockRef = currentEntity as BlockReference;

                        BlockTableRecord block = null;
                        if (blockRef.IsDynamicBlock)
                        {
                            block = trans.GetObject(blockRef.DynamicBlockTableRecord, OpenMode.ForRead) as BlockTableRecord;
                        }
                        else
                        {
                            block = trans.GetObject(blockRef.BlockTableRecord, OpenMode.ForRead) as BlockTableRecord;
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

        private Extents2d getMaximumExtents(Layout lay)
        {
            double div = lay.PlotPaperUnits == PlotPaperUnit.Inches ? 25.4 : 1.0;
            bool trigger = lay.PlotRotation == PlotRotation.Degrees090 || lay.PlotRotation == PlotRotation.Degrees270;

            var min = swapCoords(lay.PlotPaperMargins.MinPoint, trigger) / div;
            var max = (swapCoords(lay.PlotPaperSize, trigger) - swapCoords(lay.PlotPaperMargins.MaxPoint, trigger).GetAsVector()) / div;

            return new Extents2d(min, max);
        }

        private Point3d flatten(Point2d pt)
        {
            return new Point3d(pt.X, pt.Y, 0);
        }

        private Point2d flatten(Point3d pt)
        {
            return new Point2d(pt.X, pt.Y);
        }

        private Point2d swapCoords(Point2d pt, bool flip = true)
        {
            return flip ? new Point2d(pt.Y, pt.X) : pt;
        }

        private void writeCadMessage(string errorMessage)
        {
            ed.WriteMessage(errorMessage + "\n");
        }

        internal void close()
        {
            trans.Commit();
            trans.Dispose();

            ed.Regen();
        }
    }
}
