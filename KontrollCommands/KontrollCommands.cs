﻿using System;
using System.Text;
using System.Collections;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using SW = System.Windows.Forms;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

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

    }
}