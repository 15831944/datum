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


[assembly: CommandClass(typeof(commands.__HillsCommands))]
namespace commands
{
    public class __HillsCommands
    {

        [CommandMethod("HILLS_INFO")]
        public void info()
        {
            string version = String.Format("Dated: {0}", System.IO.File.GetLastWriteTime(System.Reflection.Assembly.GetExecutingAssembly().Location).ToShortDateString());
            SW.MessageBox.Show("AE Hills " + version + "\n");
        }


        [CommandMethod("BBB")]
        public void arm_table_sum_selected()
        {
            TABLE_command_v2 program = new TABLE_command_v2();

            try
            {
                program.run(false);
                program.output_local();
            }
            catch (System.Exception ex)
            {
                SW.MessageBox.Show("Viga\n" + ex.Message);
            }
            finally
            {
                program.close();
            }
        }


        [CommandMethod("BBB_OLD")]
        public void arm_table_sum_selected_v1()
        {
            TABLE_command_v1 program = new TABLE_command_v1();

            try
            {
                program.run(false);
                program.output_local();
            }
            catch (System.Exception ex)
            {
                SW.MessageBox.Show("Viga\n" + ex.Message);
            }
        }


        [CommandMethod("CCC")]
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
            finally
            {
                program.close();
            }
        }


        [CommandMethod("CCC_OLD")]
        public void arm_weights_selected_v1()
        {
            WEIGHT_command_v1 program = new WEIGHT_command_v1();

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


        [CommandMethod("DDD")]
        public void csv_sum_weights()
        {
            SUM_command_v2 program = new SUM_command_v2(true);

            try
            {
                program.run();
                program.dump_csv();
            }
            catch (System.Exception ex)
            {
                SW.MessageBox.Show("Viga\n" + ex.Message);
            }
            finally
            {
                program.close();
            }
        }


        [CommandMethod("QWE")]
        public void dimentioning()
        {
            DIM_command_v2 program = new DIM_command_v2();

            try
            {
                program.run();
            }
            catch (System.Exception ex)
            {
                SW.MessageBox.Show("Viga\n" + ex.Message);
            }
            finally
            {
                program.close();
            }
        }


        [CommandMethod("CSV_SUM_MARKS")]
        public void csv_sum_marks()
        {
            TABLE_command_v2 program = new TABLE_command_v2();

            try
            {
                program.run(true);
                program.dump_csv();
            }
            catch (System.Exception ex)
            {
                SW.MessageBox.Show("Viga\n" + ex.Message);
            }
            finally
            {
                //program.close();
            }
        }


        [CommandMethod("CSV_SUM_EXCEL")]
        public void csv_sum_excel()
        {
            SUM_command_v2 program = new SUM_command_v2(false);

            try
            {
                program.run();
                program.dump_csv();
            }
            catch (System.Exception ex)
            {
                SW.MessageBox.Show("Viga\n" + ex.Message);
            }
            finally
            {
                //program.close();
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
                SW.MessageBox.Show("Viga\n" + ex.Message);
            }
            finally
            {
                program.unlock_after_crash();
                program.close();
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
                SW.MessageBox.Show("Viga\n" + ex.Message);
            }
            finally
            {
                program.unlock_after_crash();
            }
        }


        [CommandMethod("XMLF")]
        public void find_rebar_from_XML()
        {
            XML_FindRebar_command program = new XML_FindRebar_command();

            try
            {
                program.run();
            }
            catch (System.Exception ex)
            {
                SW.MessageBox.Show("Viga\n" + ex.Message);
            }
            finally
            {
                program.unlock_after_crash();
            }
        }


        //[CommandMethod("pppp")]
        //public void print_all()
        //{
        //    PRINT_command_v2 program = new PRINT_command_v2();

        //    try
        //    {
        //        program.run(true);
        //    }
        //    catch (System.Exception ex)
        //    {
        //        SW.MessageBox.Show("Viga\n" + ex.Message);
        //    }
        //    finally
        //    {
        //        program.close();
        //    }
        //}


        //[CommandMethod("HILLS_PRINT_SELECTED")]
        //public void print_selected()
        //{
        //    PRINT_command_v2 program = new PRINT_command_v2();

        //    try
        //    {
        //        program.run(false);
        //    }
        //    catch (System.Exception ex)
        //    {
        //        SW.MessageBox.Show("Viga\n" + ex.Message);
        //    }
        //    finally
        //    {
        //        program.close();
        //    }
        //}

    }
}