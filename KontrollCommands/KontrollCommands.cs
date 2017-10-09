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


[assembly: CommandClass(typeof(commands.KontrollCommands))]
namespace commands
{
    public class KontrollCommands
    {
        [CommandMethod("kontroll_scale")]
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

        [CommandMethod("kontroll_override")]
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


        [CommandMethod("kontroll_corner")]
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




        [CommandMethod("kontroll_diam")]
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

    }
}