using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

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
    public class PlotDriver_Plagiaat
    {
        private string dwgFile, pdfFile, dsdFile, outputDir;
        private int sheetNum;
        IEnumerable<Layout> layouts;
        
        public PlotDriver_Plagiaat(string dwgFile, string pdfFile, string outputDir, IEnumerable<Layout> layouts)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            this.dwgFile = dwgFile;
            this.pdfFile = pdfFile;
            this.outputDir = outputDir;
            this.dsdFile = Path.ChangeExtension(this.pdfFile, "dsd");
            this.layouts = layouts;
        }

        public void Publish()
        {
            if (TryCreateDSD())
            {
                Publisher publisher = Application.Publisher;
                PlotProgressDialog plotDlg = new PlotProgressDialog(false, this.sheetNum, true);
                publisher.PublishDsd(this.dsdFile, plotDlg);
                plotDlg.Destroy();
                File.Delete(this.dsdFile);
            }
        }

        private bool TryCreateDSD()
        {
            using (DsdData dsd = new DsdData())
            using (DsdEntryCollection dsdEntries = CreateDsdEntryCollection(this.layouts))
            {
                if (dsdEntries == null || dsdEntries.Count <= 0) return false;
                if (!Directory.Exists(this.outputDir))  Directory.CreateDirectory(this.outputDir);

                this.sheetNum = dsdEntries.Count;

                dsd.SetDsdEntryCollection(dsdEntries);

                dsd.SetUnrecognizedData("PwdProtectPublishedDWF", "FALSE");
                dsd.SetUnrecognizedData("PromptForPwd", "FALSE");
                dsd.SheetType = SheetType.MultiDwf;
                dsd.NoOfCopies = 1;
                dsd.DestinationName = this.pdfFile;
                dsd.IsHomogeneous = false;

                PostProcessDSD(dsd);

                return true;
            }
        }

        private DsdEntryCollection CreateDsdEntryCollection(IEnumerable<Layout> layouts)
        {
            DsdEntryCollection entries = new DsdEntryCollection();

            foreach (Layout layout in layouts)
            {
                DsdEntry dsdEntry = new DsdEntry();
                dsdEntry.DwgName = this.dwgFile;
                dsdEntry.Layout = layout.LayoutName;
                dsdEntry.Title = Path.GetFileNameWithoutExtension(this.dwgFile) + "-" + layout.LayoutName;
                dsdEntry.Nps = layout.TabOrder.ToString();
                entries.Add(dsdEntry);
            }
            return entries;
        }

        private void PostProcessDSD(DsdData dsd)
        {
            string str, newStr;
            string tmpFile = Path.Combine(this.outputDir, "temp.dsd");

            dsd.WriteDsd(tmpFile);

            using (StreamReader reader = new StreamReader(tmpFile, Encoding.Default))
            using (StreamWriter writer = new StreamWriter(this.dsdFile, false, Encoding.Default))
            {
                while (!reader.EndOfStream)
                {
                    str = reader.ReadLine();
                    if (str.Contains("Has3DDWF"))
                    {
                        newStr = "Has3DDWF=0";
                    }
                    else if (str.Contains("OriginalSheetPath"))
                    {
                        newStr = "OriginalSheetPath=" + this.dwgFile;
                    }
                    else if (str.Contains("Type"))
                    {
                        newStr = "Type=6";
                    }
                    else if (str.Contains("OUT"))
                    {
                        newStr = "OUT=" + this.outputDir;
                    }
                    else if (str.Contains("IncludeLayer"))
                    {
                        newStr = "IncludeLayer=TRUE";
                    }
                    else if (str.Contains("PromptForDwfName"))
                    {
                        newStr = "PromptForDwfName=FALSE";
                    }
                    else
                    {
                        newStr = str;
                    }
                    writer.WriteLine(newStr);
                }
            }
            File.Delete(tmpFile);
        }
    }
}