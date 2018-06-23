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


namespace commands
{
    class _Mark_K : IEquatable<_Mark_K>
    {
        string original;
        _Ge.Point3d insert;

        int number = 0;
        int diameter = -99;

        string position = "";
        string position_shape = "";
        int position_nr = -99;

        public string Original { get { return original; } }
        public _Ge.Point3d IP { get { return insert; } }

        public int Number { get { return number; } set { number = value; } }
        public int Diameter { get { return diameter; } }

        public string Position { get { return position; } }
        public string Position_Shape { get { return position_shape; } }
        public int Position_Nr { get { return position_nr; } }


        public _Mark_K(string original_text, _Ge.Point3d insp)
        {
            original = original_text;
            insert = insp;
        }


        public _Mark_K(int num, int diam, string pos, string shp, int nr)
        {
            number = num;
            diameter = diam;
            position = pos;
            position_shape = shp;
            position_nr = nr;
        }


        internal bool validate_original()
        {
            string nospaces = original.Replace(@"\P", "");
            nospaces = nospaces.Replace(" FS", "");
            nospaces = nospaces.Replace(" HS", "");
            nospaces = nospaces.Replace(" ", "");

            if (nospaces.Contains("=")) return false; // CHECK 1
            if (!nospaces.Contains("-")) return false; // CHECK 2
            if (!nospaces.Contains("Ø")) return false; // CHECK 3

            string[] split1 = nospaces.Split('Ø');
            if (split1.Count() > 2) return false; // CHECK 4

            string[] split2 = split1[1].Split('-');
            if (split2.Count() > 2) return false; // CHECK 5

            string numb = split1[0];
            string diam = split2[0];
            string pos = split2[1];

            if (diam.Length == 0) return false; // CHECK 6
            if (pos.Length < 2) return false; // CHECK 7

            if (numb.Length == 0)
            {
                numb = "1";
            }
            else if (numb.Contains("+"))
            {
                string[] nmbs = numb.Split('+');

                int i = 0;

                foreach (string n in nmbs)
                {
                    int currentNumber = -99;
                    Int32.TryParse(n, out currentNumber);

                    if (currentNumber < 0) return false; // CHECK 8

                    i = i + currentNumber;
                }

                numb = i.ToString();
            }

            if (diam.Contains("s"))
            {
                string[] diams = diam.Split('s');
                diam = diams[0];
            }

            int temp = 0;
            Int32.TryParse(numb, out temp);
            if (temp == 0) return false; // CHECK 9
            number = temp;

            temp = 0;
            Int32.TryParse(diam, out temp);
            if (temp == 0) return false; // CHECK 10
            diameter = temp;

            temp = 0;
            Int32.TryParse(pos, out temp);

            if (temp != 0)
            {
                position_shape = "A";
                position_nr = temp;
            }
            else
            {
                int numShape = 0;
                for (int j = 0; j < pos.Length; j++)
                {
                    char cur = pos[j];

                    if (Char.IsNumber(cur))
                    {
                        numShape = j;
                        break;
                    }
                }

                position_shape = pos.Substring(0, numShape);
                position_nr = 0;

                Int32.TryParse(pos.Substring(numShape, pos.Length - numShape), out position_nr);

                if (position_nr == 0) return false; // CHECK 11
            }

            position = pos;

            return true;
        }


        internal bool validate()
        {
            string nospaces = original.Replace(@"\P", "");
            nospaces = nospaces.Replace(" FS", "");
            nospaces = nospaces.Replace(" HS", "");
            nospaces = nospaces.Replace(" ", "");

            if (nospaces.Contains("=")) return false; // CHECK 1
            if (!nospaces.Contains("-")) return false; // CHECK 2
            if (!nospaces.Contains("ø")) return false; // CHECK 3

            string[] split1 = nospaces.Split('ø');
            if (split1.Count() > 2) return false; // CHECK 4

            string[] split2 = split1[1].Split('-');
            if (split2.Count() > 2) return false; // CHECK 5

            string numb = split1[0];
            string diam = split2[0];
            string pos = split2[1];

            if (diam.Length == 0) return false; // CHECK 6
            if (pos.Length == 1) return false; // CHECK 7

            if (numb.Length == 0)
            {
                numb = "1";
            }
            else if (numb.Contains("+"))
            {
                string[] nmbs = numb.Split('+');

                int i = 0;

                foreach (string n in nmbs)
                {
                    int currentNumber = -99;
                    Int32.TryParse(n, out currentNumber);

                    if (currentNumber < 0) return false; // CHECK 8

                    i = i + currentNumber;
                }

                numb = i.ToString();
            }

            if (diam.Contains("s"))
            {
                string[] diams = diam.Split('s');
                diam = diams[0];
            }

            int temp = 0;
            Int32.TryParse(numb, out temp);
            if (temp == 0) return false; // CHECK 9
            number = temp;

            temp = 0;
            Int32.TryParse(diam, out temp);
            if (temp == 0) return false; // CHECK 10
            diameter = temp;

            temp = 0;
            Int32.TryParse(pos, out temp);

            if (temp != 0)
            {
                position_shape = "A";
                position_nr = temp;
            }
            else
            {
                int numShape = 0;
                for (int j = 0; j < pos.Length; j++)
                {
                    char cur = pos[j];

                    if (Char.IsNumber(cur))
                    {
                        numShape = j;
                        break;
                    }
                }

                position_shape = pos.Substring(0, numShape);
                position_nr = 0;

                Int32.TryParse(pos.Substring(numShape, pos.Length - numShape), out position_nr);

                if (position_nr != 0) return false; // CHECK 11
            }

            position_nr = 999999999;

            return true;
        }


        internal bool validate2()
        {
            string nospaces = original.Replace(@"\P", "");
            nospaces = nospaces.Replace(" FS", "");
            nospaces = nospaces.Replace(" HS", "");
            nospaces = nospaces.Replace(" ", "");

            if (nospaces.Contains("=")) return false; // CHECK 1
            if (!nospaces.Contains("-")) return false; // CHECK 2
            if (!nospaces.Contains("Ø")) return false; // CHECK 3

            string[] split1 = nospaces.Split('Ø');
            if (split1.Count() > 2) return false; // CHECK 4

            string[] split2 = split1[1].Split('-');
            if (split2.Count() > 2) return false; // CHECK 5

            string numb = split1[0];
            string diam = split2[0];
            string pos = split2[1];

            if (diam.Length == 0) return false; // CHECK 6
            if (pos.Length != 1) return false; // CHECK 7

            if (numb.Length == 0)
            {
                numb = "1";
            }
            else if (numb.Contains("+"))
            {
                string[] nmbs = numb.Split('+');

                int i = 0;

                foreach (string n in nmbs)
                {
                    int currentNumber = -99;
                    Int32.TryParse(n, out currentNumber);

                    if (currentNumber < 0) return false; // CHECK 8

                    i = i + currentNumber;
                }

                numb = i.ToString();
            }

            if (diam.Contains("s"))
            {
                string[] diams = diam.Split('s');
                diam = diams[0];
            }

            int temp = 0;
            Int32.TryParse(numb, out temp);
            if (temp == 0) return false; // CHECK 9
            number = temp;

            temp = 0;
            Int32.TryParse(diam, out temp);
            if (temp == 0) return false; // CHECK 10
            diameter = temp;

            temp = 0;
            Int32.TryParse(pos, out temp);

            return true;
        }


        public bool Equals(_Mark_K other)
        {
            if (other == null) return false;
            double dX = this.insert.X - other.insert.X;
            double dY = this.insert.Y - other.insert.Y;

            double dL = Math.Sqrt((dX * dX) + (dY * dY));

            if (dL < 1.0) return true;
            else return false;
        }


        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as _Mark_K);
        }


        public static bool operator ==(_Mark_K a, _Mark_K b)
        {
            return object.Equals(a, b);
        }


        public static bool operator !=(_Mark_K a, _Mark_K b)
        {
            return !object.Equals(a, b);
        }


        public override string ToString()
        {
            return Position_Shape + " " + Position_Nr + " Ø" + Diameter + " (" + Original + ")";
        }

    }
}