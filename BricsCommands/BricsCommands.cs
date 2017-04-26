using System;
using System.Text;
using System.Collections;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

//ODA
using Teigha.Runtime;
using Teigha.DatabaseServices;
using Teigha.Geometry;

//Bricsys
using Bricscad.ApplicationServices;
using Bricscad.Runtime;
using Bricscad.EditorInput;

[assembly: CommandClass(typeof(commands.CadCommands))]
namespace commands
{
    public class CadCommands
    {
        [CommandMethod("te")]
        public void testing()
        {

        }
    }
}