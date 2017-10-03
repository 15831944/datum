using System;
using System.Text;
using System.Collections;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using SW = System.Windows.Forms;

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

[assembly: CommandClass(typeof(commands.HillsCommands))]
namespace commands
{
    public class HillsCommands
    {
        [CommandMethod("HLS_INFO")]
        public void info()
        {
            SW.MessageBox.Show("AE Hills Versioon: 14.08.2017\n");
        }


        [CommandMethod("QWE")]
        public void dimentioning()
        {
            try
            {
                DIM_command_v2 program = new DIM_command_v2();
                program.run();
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
                TABLE_command_v2 program = new TABLE_command_v2();
                program.run(false);
                program.output_local();
            }
            catch (System.Exception ex)
            {
                SW.MessageBox.Show("Viga\n" + ex.Message);
            }
        }


        [CommandMethod("bbb_old")]
        public void arm_table_sum_selected_v1()
        {
            try
            {
                TABLE_command_v1 program = new TABLE_command_v1();
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


        [CommandMethod("ccc_old")]
        public void arm_weights_selected_v1()
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


        [CommandMethod("CSV_SUM_MARKS")]
        public void csv_sum_marks()
        {
            try
            {
                TABLE_command_v1 program = new TABLE_command_v1();
                program.run(true);
                program.dump_csv();
            }
            catch (System.Exception ex)
            {
                SW.MessageBox.Show("Viga\n" + ex.Message);
            }
        }


        [CommandMethod("CSV_SUM_WEIGHTS")]
        public void csv_sum_weights()
        {
            try
            {
                SUM_command_v2 program = new SUM_command_v2();
                program.run();
                program.dump_csv();
            }
            catch (System.Exception ex)
            {
                SW.MessageBox.Show("Viga\n" + ex.Message);
            }
        }


        [CommandMethod("XMLA")]
        public void add_rebar_to_XML()
        {
            XML_AddTo_command program = new XML_AddTo_command();

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
        public void print_rebar_from_XML()
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

        [CommandMethod("HILLS_PRINT_ALL")]
        public void print_all()
        {
            try
            {
                PRINT_command_v2 program = new PRINT_command_v2();
                program.run(true);
                program.close();
            }
            catch (System.Exception ex)
            {
                SW.MessageBox.Show("Viga\n" + ex.Message);
            }
        }

        [CommandMethod("HILLS_PRINT_SELECTED")]
        public void print_selected()
        {
            try
            {
                PRINT_command_v2 program = new PRINT_command_v2();
                program.run(false);
                program.close();
            }
            catch (System.Exception ex)
            {
                SW.MessageBox.Show("Viga\n" + ex.Message);
            }
        }
    }
}