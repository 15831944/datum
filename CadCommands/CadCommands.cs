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

[assembly: CommandClass(typeof(commands.HillsCommands))]
namespace commands
{
    public class HillsCommands
    {
        [CommandMethod("HLS_INFO")]
        public void info()
        {
            SW.MessageBox.Show("AE Hills Versioon: 20.07.2017\n");
        }

        [CommandMethod("QWE")]
        public void dimentioning()
        {
            try
            {
                QWE_command program = new QWE_command();
                program.run();
            }
            catch (System.Exception ex)
            {
                SW.MessageBox.Show("Viga\n" + ex.Message);
            }
        }

        [CommandMethod("aaa")]
        public void arm_table_sum_all()
        {
            try
            {
                SUM_command_v2 program = new SUM_command_v2();
                program.run(true);
                program.output_local();
            }
            catch (System.Exception ex)
            {
                SW.MessageBox.Show("Viga\n" + ex.Message);
            }
        }

        [CommandMethod("bbb")]
        public void arm_table_sum_selected()
        {
            try
            {
                SUM_command_v2 program = new SUM_command_v2();
                program.run(false);
                program.output_local();
            }
            catch (System.Exception ex)
            {
                SW.MessageBox.Show("Viga\n" + ex.Message);
            }
        }

        [CommandMethod("ccc")]
        public void arm_weights_selected()
        {
            WEIGHT_command_v2 program = new WEIGHT_command_v2();

            try
            {
                program.run(false);
                program.output_local();
            }
            catch (System.Exception ex)
            {
                program.unlock_after_crash();
                SW.MessageBox.Show("Viga\n" + ex.Message);
            }
        }

        [CommandMethod("ddd")]
        public void arm_weights_all()
        {
            WEIGHT_command_v2 program = new WEIGHT_command_v2();

            try
            {
                program.run(true);
                program.output_local();
            }
            catch (System.Exception ex)
            {
                program.unlock_after_crash();
                SW.MessageBox.Show("Viga\n" + ex.Message);
            }
        }

        [CommandMethod("CSV_SUM_MARKS")]
        public void csv_sum()
        {
            try
            {
                SUM_command_v2 program = new SUM_command_v2();
                program.run(true);
                program.dump_csv();
            }
            catch (System.Exception ex)
            {
                SW.MessageBox.Show("Viga\n" + ex.Message);
            }
        }

        [CommandMethod("XMLA")]
        public void testing()
        {
            XML_testing program = new XML_testing();

            try
            {
                program.run();
            }
            catch (System.Exception ex)
            {
                program.unlock_after_crash();
                SW.MessageBox.Show("Viga\n" + ex.Message);
            }
        }

        [CommandMethod("XMLP")]
        public void printXMLall()
        {
            XML_PrintKnownRebar_command program = new XML_PrintKnownRebar_command();

            try
            {
                program.run();
            }
            catch (System.Exception ex)
            {
                program.unlock_after_crash();
                SW.MessageBox.Show("Viga\n" + ex.Message);
            }
        }
    }
}