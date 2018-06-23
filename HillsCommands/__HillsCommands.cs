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


[assembly: _Trx.CommandClass(typeof(commands.__HillsCommands))]
namespace commands
{
    public class __HillsCommands
    {

        [_Trx.CommandMethod("HILLS_INFO")]
        public void info()
        {
            try
            {
                _CONNECTION c = new _CONNECTION();

                try
                {
                    string version = String.Format("{0}", System.IO.File.GetLastWriteTime(System.Reflection.Assembly.GetExecutingAssembly().Location).ToShortDateString());
                    c.ed.WriteMessage("\nHillStatic programmi versioon: " + version + "\n");
                    c.ed.WriteMessage("\n[DONE]");
                }
                catch (DMTException de)
                {
                    c.ed.WriteMessage("\n" + de.Message);
                }
                catch (Exception ex)
                {
                    c.ed.WriteMessage("\n[ERROR] Unknown Exception");
                    c.ed.WriteMessage("\n[ERROR] " + ex.Message);
                    c.ed.WriteMessage("\n[ERROR] " + ex.TargetSite);
                }
                finally
                {
                    c.close();
                }
            }
            catch
            {
                _SWF.MessageBox.Show("\n[ERROR] Connection to BricsCad/AutoCad failed.");
            }
        }


        [_Trx.CommandMethod("BBB")]
        public void arm_table_sum_selected()
        {
            try
            {
                _CONNECTION c = new _CONNECTION();

                TABLE_command_v2 program = new TABLE_command_v2(ref c);

                try
                {
                    program.run(false);
                    program.output_local();

                    c.ed.WriteMessage("\n[DONE]");
                }
                catch (DMTException de)
                {
                    c.ed.WriteMessage("\n" + de.Message);
                }
                catch (Exception ex)
                {
                    c.ed.WriteMessage("\n[ERROR] Unknown Exception");
                    c.ed.WriteMessage("\n[ERROR] " + ex.Message);
                    c.ed.WriteMessage("\n[ERROR] " + ex.TargetSite);
                }
                finally
                {
                    c.close();
                }
            }
            catch
            {
                _SWF.MessageBox.Show("\n[ERROR] Connection to BricsCad/AutoCad failed.");
            }
        }


        [_Trx.CommandMethod("BBB_OLD")]
        public void arm_table_sum_selected_v1()
        {
            try
            {
                _CONNECTION c = new _CONNECTION();

                TABLE_command_v1 program = new TABLE_command_v1(ref c);

                try
                {
                    program.run(false);
                    program.output_local();

                    c.ed.WriteMessage("\n[DONE]");
                }
                catch (DMTException de)
                {
                    c.ed.WriteMessage("\n" + de.Message);
                }
                catch (Exception ex)
                {
                    c.ed.WriteMessage("\n[ERROR] Unknown Exception");
                    c.ed.WriteMessage("\n[ERROR] " + ex.Message);
                    c.ed.WriteMessage("\n[ERROR] " + ex.TargetSite);
                }
                finally
                {
                    c.close();
                }
            }
            catch
            {
                _SWF.MessageBox.Show("\n[ERROR] Connection to BricsCad/AutoCad failed.");
            }
        }


        [_Trx.CommandMethod("CCC")]
        public void arm_weights_selected()
        {
            try
            {
                _CONNECTION c = new _CONNECTION();

                WEIGHT_command_v2 program = new WEIGHT_command_v2(ref c);

                try
                {
                    program.run(false);
                    program.output_local();

                    c.ed.WriteMessage("\n[DONE]");
                }
                catch (DMTException de)
                {
                    c.ed.WriteMessage("\n" + de.Message);
                }
                catch (Exception ex)
                {
                    c.ed.WriteMessage("\n[ERROR] Unknown Exception");
                    c.ed.WriteMessage("\n[ERROR] " + ex.Message);
                    c.ed.WriteMessage("\n[ERROR] " + ex.TargetSite);
                }
                finally
                {
                    program.unlock_after_crash();

                    c.close();
                }
            }
            catch
            {
                _SWF.MessageBox.Show("\n[ERROR] Connection to BricsCad/AutoCad failed.");
            }
        }


        [_Trx.CommandMethod("CCC_OLD")]
        public void arm_weights_selected_v1()
        {
            try
            {
                _CONNECTION c = new _CONNECTION();

                WEIGHT_command_v1 program = new WEIGHT_command_v1(ref c);

                try
                {
                    program.run(false);
                    program.output_local();

                    c.ed.WriteMessage("\n[DONE]");
                }
                catch (DMTException de)
                {
                    c.ed.WriteMessage("\n" + de.Message);
                }
                catch (Exception ex)
                {
                    c.ed.WriteMessage("\n[ERROR] Unknown Exception");
                    c.ed.WriteMessage("\n[ERROR] " + ex.Message);
                    c.ed.WriteMessage("\n[ERROR] " + ex.TargetSite);
                }
                finally
                {
                    program.unlock_after_crash();

                    c.close();
                }
            }
            catch
            {
                _SWF.MessageBox.Show("\n[ERROR] Connection to BricsCad/AutoCad failed.");
            }
        }


        [_Trx.CommandMethod("DDD")]
        public void csv_sum_weights()
        {
            try
            {
                _CONNECTION c = new _CONNECTION();

                KONTROLL_command program = new KONTROLL_command(ref c);

                try
                {
                    program.run();

                    c.ed.WriteMessage("\n[DONE]");
                }
                catch (DMTException de)
                {
                    c.ed.WriteMessage("\n" + de.Message);
                }
                catch (Exception ex)
                {
                    c.ed.WriteMessage("\n[ERROR] Unknown Exception");
                    c.ed.WriteMessage("\n[ERROR] " + ex.Message);
                    c.ed.WriteMessage("\n[ERROR] " + ex.TargetSite);
                }
                finally
                {
                    c.close();
                }
            }
            catch
            {
                _SWF.MessageBox.Show("\n[ERROR] Connection to BricsCad/AutoCad failed.");
            }
        }


        [_Trx.CommandMethod("QWE")]
        public void dimentioning()
        {
            try
            {
                _CONNECTION c = new _CONNECTION();

                DIM_command_v2 program = new DIM_command_v2(ref c);

                try
                {
                    program.run();

                    c.ed.WriteMessage("\n[DONE]");
                }
                catch (DMTException de)
                {
                    c.ed.WriteMessage("\n" + de.Message);
                }
                catch (Exception ex)
                {
                    c.ed.WriteMessage("\n[ERROR] Unknown Exception");
                    c.ed.WriteMessage("\n[ERROR] " + ex.Message);
                    c.ed.WriteMessage("\n[ERROR] " + ex.TargetSite);
                }
                finally
                {
                    c.close();
                }
            }
            catch
            {
                _SWF.MessageBox.Show("\n[ERROR] Connection to BricsCad/AutoCad failed.");
            }
        }


        [_Trx.CommandMethod("CSV_SUM_MARKS")]
        public void csv_sum_marks()
        {
            try
            {
                _CONNECTION c = new _CONNECTION();

                TABLE_command_v2 program = new TABLE_command_v2(ref c);

                try
                {
                    program.run(true);
                    program.dump_csv();

                    c.ed.WriteMessage("\n[DONE]");
                }
                catch (DMTException de)
                {
                    c.ed.WriteMessage("\n" + de.Message);
                }
                catch (Exception ex)
                {
                    c.ed.WriteMessage("\n[ERROR] Unknown Exception");
                    c.ed.WriteMessage("\n[ERROR] " + ex.Message);
                    c.ed.WriteMessage("\n[ERROR] " + ex.TargetSite);
                }
                finally
                {
                    c.close();
                }
            }
            catch
            {
                _SWF.MessageBox.Show("\n[ERROR] Connection to BricsCad/AutoCad failed.");
            }
        }


        [_Trx.CommandMethod("CSV_SUM_EXCEL")]
        public void csv_sum_excel()
        {
            try
            {
                _CONNECTION c = new _CONNECTION();

                SUM_command_v2 program = new SUM_command_v2(ref c, false);

                try
                {
                    program.run();
                    program.dump_csv();

                    c.ed.WriteMessage("\n[DONE]");
                }
                catch (DMTException de)
                {
                    c.ed.WriteMessage("\n" + de.Message);
                }
                catch (Exception ex)
                {
                    c.ed.WriteMessage("\n[ERROR] Unknown Exception");
                    c.ed.WriteMessage("\n[ERROR] " + ex.Message);
                    c.ed.WriteMessage("\n[ERROR] " + ex.TargetSite);
                }
                finally
                {
                    c.close();
                }
            }
            catch
            {
                _SWF.MessageBox.Show("\n[ERROR] Connection to BricsCad/AutoCad failed.");
            }
        }


        [_Trx.CommandMethod("XMLA")]
        public void add_rebar_to_XML()
        {
            try
            {
                _CONNECTION c = new _CONNECTION();

                XML_AddTo_command program = new XML_AddTo_command(ref c);

                try
                {
                    program.run();

                    c.ed.WriteMessage("\n[DONE]");
                }
                catch (DMTException de)
                {
                    c.ed.WriteMessage("\n" + de.Message);
                }
                catch (Exception ex)
                {
                    c.ed.WriteMessage("\n[ERROR] Unknown Exception");
                    c.ed.WriteMessage("\n[ERROR] " + ex.Message);
                    c.ed.WriteMessage("\n[ERROR] " + ex.TargetSite);
                }
                finally
                {
                    program.unlock_after_crash();

                    c.close();
                }
            }
            catch
            {
                _SWF.MessageBox.Show("\n[ERROR] Connection to BricsCad/AutoCad failed.");
            }
        }


        [_Trx.CommandMethod("XMLP")]
        public void print_rebar_from_XML()
        {
            try
            {
                _CONNECTION c = new _CONNECTION();

                XML_PrintKnownRebar_command program = new XML_PrintKnownRebar_command(ref c);

                try
                {
                    program.run();

                    c.ed.WriteMessage("\n[DONE]");
                }
                catch (DMTException de)
                {
                    c.ed.WriteMessage("\n" + de.Message);
                }
                catch (Exception ex)
                {
                    c.ed.WriteMessage("\n[ERROR] Unknown Exception");
                    c.ed.WriteMessage("\n[ERROR] " + ex.Message);
                    c.ed.WriteMessage("\n[ERROR] " + ex.TargetSite);
                }
                finally
                {
                    program.unlock_after_crash();

                    c.close();
                }
            }
            catch
            {
                _SWF.MessageBox.Show("\n[ERROR] Connection to BricsCad/AutoCad failed.");
            }
        }


        [_Trx.CommandMethod("XMLF")]
        public void find_rebar_from_XML()
        {
            try
            {
                _CONNECTION c = new _CONNECTION();

                XML_FindRebar_command program = new XML_FindRebar_command(ref c);

                try
                {
                    program.run();

                    c.ed.WriteMessage("\n[DONE]");
                }
                catch (DMTException de)
                {
                    c.ed.WriteMessage("\n" + de.Message);
                }
                catch (Exception ex)
                {
                    c.ed.WriteMessage("\n[ERROR] Unknown Exception");
                    c.ed.WriteMessage("\n[ERROR] " + ex.Message);
                    c.ed.WriteMessage("\n[ERROR] " + ex.TargetSite);
                }
                finally
                {
                    program.unlock_after_crash();

                    c.close();
                }
            }
            catch
            {
                _SWF.MessageBox.Show("\n[ERROR] Connection to BricsCad/AutoCad failed.");
            }
        }


        //[_Trx.CommandMethod("pppp")]
        //public void print_all()
        //{
        //    try
        //    {
        //        _CONNECTION c = new _CONNECTION();

        //        PRINT_command_v2 program = new PRINT_command_v2(ref c);

        //        try
        //        {
        //            program.run(true);

        //            c.ed.WriteMessage("\n[DONE]");
        //        }                
        //catch (DMTException de)
        //        {
        //            c.ed.WriteMessage("\n" + de.Message);
        //        }
        //        catch (Exception ex)
        //        {
        //            c.ed.WriteMessage("\n[ERROR] Unknown Exception");
        //            c.ed.WriteMessage("\n[ERROR] " + ex.Message);
        //            c.ed.WriteMessage("\n[ERROR] " + ex.TargetSite);
        //        }
        //        finally
        //        {
        //            c.close();
        //        }
        //    }
        //    catch
        //    {
        //        _SWF.MessageBox.Show("\n[ERROR] Connection to BricsCad/AutoCad failed.");
        //    }
        //}


        //[_Trx.CommandMethod("HILLS_PRINT_SELECTED")]
        //public void print_selected()
        //{
        //    try
        //    {
        //        _CONNECTION c = new _CONNECTION();

        //        PRINT_command_v2 program = new PRINT_command_v2(ref c);

        //        try
        //        {
        //            program.run(false);

        //            c.ed.WriteMessage("\n[DONE]");
        //        }
        //    catch (DMTException de)
        //{
        //    c.ed.WriteMessage("\n" + de.Message);
        //}
        //        catch (Exception ex)
        //        {
        //            c.ed.WriteMessage("\n[ERROR] Unknown Exception");
        //            c.ed.WriteMessage("\n[ERROR] " + ex.Message);
        //            c.ed.WriteMessage("\n[ERROR] " + ex.TargetSite);
        //        }
        //        finally
        //        {
        //            c.close();
        //        }
        //    }
        //    catch
        //    {
        //        _SWF.MessageBox.Show("\n[ERROR] Connection to BricsCad/AutoCad failed.");
        //    }
        //}

    }
}