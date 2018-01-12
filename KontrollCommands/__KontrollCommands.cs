using System;
using System.Text;
using System.Collections;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using SW = System.Windows.Forms;

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


[assembly: CommandClass(typeof(commands.__KontrollCommands))]
namespace commands
{
    public class __KontrollCommands
    {
        [CommandMethod("KONTROLL_INFO")]
        public void info()
        {
            string version = String.Format("Dated: {0}", System.IO.File.GetLastWriteTime(System.Reflection.Assembly.GetExecutingAssembly().Location).ToShortDateString());
            SW.MessageBox.Show("AE Kontroll " + version + "\n");
        }


        [CommandMethod("KONTROLL_CORNER")]
        public void kontroll_corner()
        {
            try
            {
                CORNER_command program = new CORNER_command();
                program.run();
                program.close();
            }
            catch (System.Exception ex)
            {
                SW.MessageBox.Show("Viga\n" + ex.Message);
            }
        }


        [CommandMethod("KONTROLL_MARK")]
        public void kontroll_diam()
        {
            try
            {
                MARK_command program = new MARK_command();
                program.run();
                program.close();
            }
            catch (System.Exception ex)
            {
                SW.MessageBox.Show("Viga\n" + ex.Message);
            }
        }


        [CommandMethod("KONTROLL_OVERRIDE")]
        public void kontroll_override()
        {
            try
            {
                OVERRIDE_command program = new OVERRIDE_command();
                program.run();
                program.close();
            }
            catch (System.Exception ex)
            {
                SW.MessageBox.Show("Viga\n" + ex.Message);
            }
        }


        [CommandMethod("KONTROLL_SCALE")]
        public void kontroll_scale()
        {
            try
            {
                SCALE_command program = new SCALE_command();
                program.run();
                program.close();
            }
            catch (System.Exception ex)
            {
                SW.MessageBox.Show("Viga\n" + ex.Message);
            }
        }

    }
}