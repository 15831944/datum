using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xml_armering_gen
{
    class Row
    {
        int number = -99;
        int diameter = -99;

        string position = "";
        string position_shape = "";
        int position_nr = -99;


        public int Number { get { return number; } set { number = value; } }
        public int Diameter { get { return diameter; } }

        public string Position { get { return position; } }
        public string Position_Shape { get { return position_shape; } }
        public int Position_Nr { get { return position_nr; } }

        public Row(int num, int diam, string pos, string shp, int nr)
        {
            number = num;
            diameter = diam;
            position = pos;
            position_shape = shp;
            position_nr = nr;
        }
    }
}