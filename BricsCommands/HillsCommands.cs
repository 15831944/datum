using System;
using System.Text;
using System.Collections;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using SW = System.Windows.Forms;

//ODA
using Teigha.Runtime;
using Teigha.DatabaseServices;
using Teigha.Geometry;

//Bricsys
using Bricscad.ApplicationServices;
using Bricscad.Runtime;
using Bricscad.EditorInput;

[assembly: CommandClass(typeof(HillsCommands.HillsCommands))]
namespace HillsCommands
{
    public class HillsCommands
    {
        [CommandMethod("AEINFO")]
        public void info()
        {
            SW.MessageBox.Show("Versioon: 07.05.2017\n");
        }

        [CommandMethod("QWE")]
        public void dimentioning()
        {
            try
            {
                QWE program = new QWE();
                program.run();
            }
            catch (System.Exception ex)
            {
                SW.MessageBox.Show("Viga\n" + ex.Message);
            }
        }

        [CommandMethod("TEE")]
        public void testing()
        {
            try
            {
                testing program = new testing();
                program.run();
            }
            catch (System.Exception ex)
            {
                SW.MessageBox.Show("Viga\n" + ex.Message);
            }
        }

    }
}