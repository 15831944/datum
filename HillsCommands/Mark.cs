using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

////Autocad
//using Autodesk.AutoCAD.Runtime;
//using Autodesk.AutoCAD.ApplicationServices;
//using Autodesk.AutoCAD.DatabaseServices;
//using Autodesk.AutoCAD.Geometry;
//using Autodesk.AutoCAD.EditorInput;

//Bricsys
using Teigha.Runtime;
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Bricscad.ApplicationServices;
using Bricscad.Runtime;
using Bricscad.EditorInput;

namespace commands
{
    class Mark : IEquatable<Mark>
    {
        string original;
        Point3d insert;

        int number = 0;
        int diameter = -99;

        string position = "";
        string position_shape = "";
        int position_nr = -99;

        public string Original { get { return original; } }
        public Point3d IP { get { return insert; } }

        public int Number { get { return number; } set { number = value; } }
        public int Diameter { get { return diameter; } }

        public string Position { get { return position; } }
        public string Position_Shape { get { return position_shape; } }
        public int Position_Nr { get { return position_nr; } }

        public Mark(string original_text, Point3d insp)
        {
            original = original_text;
            insert = insp;
        }

        public Mark(int num, int diam, string pos, string shp, int nr)
        {
            number = num;
            diameter = diam;
            position = pos;
            position_shape = shp;
            position_nr = nr;
        }

        internal bool validate()
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

        public bool Equals(Mark other)
        {
            if (other == null) return false;
            return (this.Position_Shape == other.Position_Shape &&
                    this.Position_Nr == other.Position_Nr &&
                    this.Diameter == other.Diameter);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as Mark);
        }

        public static bool operator ==(Mark a, Mark b)
        {
            return object.Equals(a, b);
        }

        public static bool operator !=(Mark a, Mark b)
        {
            return !object.Equals(a, b);
        }

        public override string ToString()
        {
            return Position_Shape + " " + Position_Nr + " Ø" + Diameter + " (" + Original + ")";
        }
    }
}