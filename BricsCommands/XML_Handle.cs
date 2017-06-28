using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SW = System.Windows.Forms;

//ODA
using Teigha.Runtime;
using Teigha.DatabaseServices;
using Teigha.Geometry;

//Bricsys
using Bricscad.ApplicationServices;
using Bricscad.Runtime;
using Bricscad.EditorInput;

using System.Xml;

namespace commands
{
    static class XML_Handle
    {
        public static string getXMLRebarString(XmlNode rebar)
        {
            string result = "";

            string type = emptyNodehandle(rebar, "Type");

            string pos_nr = "Pos: [" + emptyNodehandle(rebar, "Litt") + "]";
            string diam = "Diameter: [" + emptyNodehandle(rebar, "Dim") + "]";
            string length = "Length: [" + emptyNodehandle(rebar, "Length") + "]";
            XmlNode geometry = rebar["B2aGeometry"];

            result = "Type [" + type + "]" + " - " + pos_nr + " - " + diam + " - " + length;

            if (type == "A")
            {

            }
            else
            {
                if (geometry == null)
                {
                    result = result + " - [NO GEOMETRY]";
                }
                else
                {
                    if (type == "B")
                    {
                        string a = "A: [" + emptyNodehandle(geometry, "A") + "]";
                        string b = "B: [" + emptyNodehandle(geometry, "B") + "]";
                        string r = "R: [" + emptyNodehandle(geometry, "R") + "]";

                        result = result + " - " + a + " - " + b + " - " + r;
                    }
                    else if (type == "C")
                    {
                        string a = "A: [" + emptyNodehandle(geometry, "A") + "]";
                        string b = "B: [" + emptyNodehandle(geometry, "B") + "]";
                        string c = "C: [" + emptyNodehandle(geometry, "C") + "]";
                        string r = "R: [" + emptyNodehandle(geometry, "R") + "]";

                        result = result + " - " + a + " - " + b + " - " + c + " - " + r;
                    }
                    else if (type == "D")
                    {
                        string a = "A: [" + emptyNodehandle(geometry, "A") + "]";
                        string b = "B: [" + emptyNodehandle(geometry, "B") + "]";
                        string v = "V: [" + emptyNodehandle(geometry, "V") + "]";
                        string r = "R: [" + emptyNodehandle(geometry, "R") + "]";

                        result = result + " - " + a + " - " + b + " - " + v + " - " + r;
                    }
                    else if (type == "E")
                    {
                        string a = "A: [" + emptyNodehandle(geometry, "A") + "]";
                        string b = "B: [" + emptyNodehandle(geometry, "B") + "]";
                        string c = "C: [" + emptyNodehandle(geometry, "C") + "]";
                        string v = "V: [" + emptyNodehandle(geometry, "V") + "]";
                        string u = "U: [" + emptyNodehandle(geometry, "U") + "]";
                        string r = "R: [" + emptyNodehandle(geometry, "R") + "]";

                        result = result + " - " + a + " - " + b + " - " + c + " - " + v + " - " + u + " - " + r;
                    }
                    else if (type == "EX")
                    {
                        string a = "A: [" + emptyNodehandle(geometry, "A") + "]";
                        string b = "B: [" + emptyNodehandle(geometry, "B") + "]";
                        string c = "C: [" + emptyNodehandle(geometry, "C") + "]";
                        string y = "Y: [" + emptyNodehandle(geometry, "Y") + "]";
                        string v = "V: [" + emptyNodehandle(geometry, "V") + "]";
                        string u = "U: [" + emptyNodehandle(geometry, "U") + "]";
                        string r = "R: [" + emptyNodehandle(geometry, "R") + "]";

                        result = result + " - " + a + " - " + b + " - " + c + " - " + y + " - " + v + " - " + u + " - " + r;
                    }
                    else if (type == "F")
                    {
                        string a = "A: [" + emptyNodehandle(geometry, "A") + "]";
                        string b = "B: [" + emptyNodehandle(geometry, "B") + "]";
                        string v = "V: [" + emptyNodehandle(geometry, "V") + "]";
                        string r = "R: [" + emptyNodehandle(geometry, "R") + "]";

                        result = result + " - " + a + " - " + b + " - " + v + " - " + r;
                    }
                    else if (type == "G")
                    {
                        string a = "A: [" + emptyNodehandle(geometry, "A") + "]";
                        string b = "B: [" + emptyNodehandle(geometry, "B") + "]";
                        string c = "C: [" + emptyNodehandle(geometry, "C") + "]";
                        string x = "X: [" + emptyNodehandle(geometry, "X") + "]";
                        string v = "V: [" + emptyNodehandle(geometry, "V") + "]";
                        string s = "S: [" + emptyNodehandle(geometry, "S") + "]";
                        string r = "R: [" + emptyNodehandle(geometry, "R") + "]";

                        result = result + " - " + a + " - " + b + " - " + c + " - " + x + " - " + v + " - " + s + " - " + r;
                    }
                    else if (type == "H")
                    {
                        string a = "A: [" + emptyNodehandle(geometry, "A") + "]";
                        string b = "B: [" + emptyNodehandle(geometry, "B") + "]";
                        string c = "C: [" + emptyNodehandle(geometry, "C") + "]";
                        string d = "D: [" + emptyNodehandle(geometry, "D") + "]";
                        string e = "E: [" + emptyNodehandle(geometry, "E") + "]";
                        string x = "X: [" + emptyNodehandle(geometry, "X") + "]";
                        string v = "V: [" + emptyNodehandle(geometry, "V") + "]";
                        string s = "S: [" + emptyNodehandle(geometry, "S") + "]";
                        string u = "U: [" + emptyNodehandle(geometry, "U") + "]";
                        string r = "R: [" + emptyNodehandle(geometry, "R") + "]";

                        result = result + " - " + a + " - " + b + " - " + c + " - " + x + " - " + v + " - " + s + " - " + r;
                    }
                    else if (type == "J")
                    {
                        string a = "A: [" + emptyNodehandle(geometry, "A") + "]";
                        string b = "B: [" + emptyNodehandle(geometry, "B") + "]";
                        string c = "C: [" + emptyNodehandle(geometry, "C") + "]";
                        string d = "D: [" + emptyNodehandle(geometry, "D") + "]";
                        string e = "E: [" + emptyNodehandle(geometry, "E") + "]";
                        string x = "X: [" + emptyNodehandle(geometry, "X") + "]";
                        string y = "Y: [" + emptyNodehandle(geometry, "Y") + "]";
                        string v = "V: [" + emptyNodehandle(geometry, "V") + "]";
                        string s = "S: [" + emptyNodehandle(geometry, "S") + "]";
                        string t = "T: [" + emptyNodehandle(geometry, "T") + "]";
                        string u = "U: [" + emptyNodehandle(geometry, "U") + "]";
                        string r = "R: [" + emptyNodehandle(geometry, "R") + "]";

                        result = result + " - " + a + " - " + b + " - " + c + " - " + d + " - " + e + " - " + x + " - " + y + " - " + v + " - " + s + " - " + t + " - " + u + " - " + r;
                    }
                    else if (type == "K") // TODO
                    {

                    }
                    else if (type == "L") // TODO
                    {

                    }
                    else if (type == "LX") // TODO
                    {

                    }
                    else if (type == "M") // TODO
                    {

                    }
                    else if (type == "N")
                    {
                        string a = "A: [" + emptyNodehandle(geometry, "A") + "]";
                        string b = "B: [" + emptyNodehandle(geometry, "B") + "]";
                        string r = "R: [" + emptyNodehandle(geometry, "R") + "]";
                        string end1 = "End1: [" + emptyNodehandle(geometry, "End1") + "]";
                        string end2 = "End2: [" + emptyNodehandle(geometry, "End2") + "]";

                        result = result + " - " + a + " - " + b + " - " + r + " - " + end1 + " - " + end2;

                    }
                    else if (type == "NX")
                    {
                        string a = "A: [" + emptyNodehandle(geometry, "A") + "]";
                        string b = "B: [" + emptyNodehandle(geometry, "B") + "]";
                        string c = "C: [" + emptyNodehandle(geometry, "C") + "]";
                        string d = "D: [" + emptyNodehandle(geometry, "D") + "]";
                        string v = "V: [" + emptyNodehandle(geometry, "V") + "]";
                        string u = "U: [" + emptyNodehandle(geometry, "U") + "]";
                        string r = "R: [" + emptyNodehandle(geometry, "R") + "]";

                        string end1 = "End1: [" + emptyNodehandle(geometry, "End1") + "]";
                        string end2 = "End2: [" + emptyNodehandle(geometry, "End2") + "]";

                        result = result + " - " + a + " - " + b + " - " + c + " - " + d + " - " + v + " - " + u + " - " + r + " - " + end1 + " - " + end2;
                    }
                    else if (type == "O") // TODO
                    {

                    }
                    else if (type == "Q") // TODO
                    {

                    }
                    else if (type == "R") // TODO
                    {

                    }
                    else if (type == "S")
                    {
                        string a = "A: [" + emptyNodehandle(geometry, "A") + "]";
                        string b = "B: [" + emptyNodehandle(geometry, "B") + "]";
                        string y = "Y: [" + emptyNodehandle(geometry, "Y") + "]";

                        result = result + " - " + a + " - " + b + " - " + y;
                    }
                    else if (type == "SH") // TODO
                    {

                    }
                    else if (type == "SX") // TODO
                    {

                    }
                    else if (type == "T") // TODO
                    {

                    }
                    else if (type == "U")
                    {
                        string a = "A: [" + emptyNodehandle(geometry, "A") + "]";
                        string b = "B: [" + emptyNodehandle(geometry, "B") + "]";
                        string c = "C: [" + emptyNodehandle(geometry, "C") + "]";
                        string x = "X: [" + emptyNodehandle(geometry, "X") + "]";
                        string y = "Y: [" + emptyNodehandle(geometry, "Y") + "]";
                        string v = "V: [" + emptyNodehandle(geometry, "V") + "]";
                        string u = "U: [" + emptyNodehandle(geometry, "U") + "]";
                        string r = "R: [" + emptyNodehandle(geometry, "R") + "]";

                        string end1 = "End1: [" + emptyNodehandle(geometry, "End1") + "]";
                        string end2 = "End2: [" + emptyNodehandle(geometry, "End2") + "]";

                        result = result + " - " + a + " - " + b + " - " + c + " - " + x + " - " + y + " - " + v + " - " + u + " - " + r + " - " + end1 + " - " + end2;
                    }
                    else if (type == "V")
                    {
                        string a = "A: [" + emptyNodehandle(geometry, "A") + "]";
                        string b = "B: [" + emptyNodehandle(geometry, "B") + "]";
                        string x = "X: [" + emptyNodehandle(geometry, "X") + "]";
                        string y = "Y: [" + emptyNodehandle(geometry, "Y") + "]";
                        string v = "V: [" + emptyNodehandle(geometry, "V") + "]";
                        string u = "U: [" + emptyNodehandle(geometry, "U") + "]";
                        string r = "R: [" + emptyNodehandle(geometry, "R") + "]";

                        string end1 = "End1: [" + emptyNodehandle(geometry, "End1") + "]";
                        string end2 = "End2: [" + emptyNodehandle(geometry, "End2") + "]";

                        result = result + " - " + a + " - " + b + " - " + x + " - " + y + " - " + v + " - " + u + " - " + r + " - " + end1 + " - " + end2;
                    }
                    else if (type == "W") // TODO
                    {

                    }
                    else if (type == "X") // TODO
                    {

                    }
                    else if (type == "XX")
                    {
                        string a = "A: [" + emptyNodehandle(geometry, "A") + "]";
                        string b = "B: [" + emptyNodehandle(geometry, "B") + "]";
                        string c = "C: [" + emptyNodehandle(geometry, "C") + "]";
                        string d = "D: [" + emptyNodehandle(geometry, "D") + "]";
                        string e = "E: [" + emptyNodehandle(geometry, "E") + "]";
                        string v = "V: [" + emptyNodehandle(geometry, "V") + "]";

                        result = result + " - " + a + " - " + b + " - " + c + " - " + d + " - " + e + " - " + v;
                    }
                    else if (type == "Z") // TODO
                    {

                    }
                }
            }

            return result;
        }


        public static XmlNode newNodeHandle(Mark undef, string materjal, XmlDocument xmlDoc, Editor ed)
        {
            XmlNode row = xmlDoc.CreateElement("B2aPageRow");
            XmlAttribute attribute = xmlDoc.CreateAttribute("RowId");
            attribute.Value = "99";
            row.Attributes.Append(attribute);

            XmlNode group = xmlDoc.CreateElement("NoGrps");
            group.InnerText = "1";
            XmlNode count = xmlDoc.CreateElement("NoStpGrp");
            count.InnerText = "999";
            XmlNode bar = xmlDoc.CreateElement("B2aBar");

            XmlNode type_node = xmlDoc.CreateElement("Type");
            type_node.InnerText = undef.Position_Shape;
            XmlNode pos = xmlDoc.CreateElement("Litt");
            pos.InnerText = undef.Position_Nr.ToString();
            XmlNode material = xmlDoc.CreateElement("fu01:B2aStlSorts");
            material.InnerText = materjal;
            XmlNode dim = xmlDoc.CreateElement("Dim");
            dim.InnerText = undef.Diameter.ToString();

            XmlNode geo = xmlDoc.CreateElement("B2aGeometry");

            string type = undef.Position_Shape;

            if (type == "A")
            {
                XmlNode a = xmlDoc.CreateElement("A");
                a.InnerText = undef.Position_Nr.ToString();
                geo.AppendChild(a);
            }
            else if (type == "B")
            {
                XmlNode a = xmlDoc.CreateElement("A");
                XmlNode b = xmlDoc.CreateElement("B");
                XmlNode r = xmlDoc.CreateElement("R");

                a.InnerText = prompt("A", ed);
                b.InnerText = prompt("B", ed);
                r.InnerText = prompt("R", ed);

                geo.AppendChild(a);
                geo.AppendChild(b);
                geo.AppendChild(r);
            }
            else if (type == "C")
            {
                XmlNode a = xmlDoc.CreateElement("A");
                XmlNode b = xmlDoc.CreateElement("B");
                XmlNode c = xmlDoc.CreateElement("C");
                XmlNode r = xmlDoc.CreateElement("R");

                a.InnerText = prompt("A", ed);
                b.InnerText = prompt("B", ed);
                c.InnerText = prompt("C", ed);
                r.InnerText = prompt("R", ed);

                geo.AppendChild(a);
                geo.AppendChild(b);
                geo.AppendChild(c);
                geo.AppendChild(r);
            }
            else if (type == "D")
            {
                XmlNode a = xmlDoc.CreateElement("A");
                XmlNode b = xmlDoc.CreateElement("B");
                XmlNode v = xmlDoc.CreateElement("V");
                XmlNode r = xmlDoc.CreateElement("R");

                a.InnerText = prompt("A", ed);
                b.InnerText = prompt("B", ed);
                v.InnerText = prompt("V", ed);
                r.InnerText = prompt("R", ed);

                geo.AppendChild(a);
                geo.AppendChild(b);
                geo.AppendChild(v);
                geo.AppendChild(r);
            }
            else if (type == "E")
            {
                XmlNode a = xmlDoc.CreateElement("A");
                XmlNode b = xmlDoc.CreateElement("B");
                XmlNode c = xmlDoc.CreateElement("C");
                XmlNode v = xmlDoc.CreateElement("V");
                XmlNode u = xmlDoc.CreateElement("U");
                XmlNode r = xmlDoc.CreateElement("R");

                a.InnerText = prompt("A", ed);
                b.InnerText = prompt("B", ed);
                c.InnerText = prompt("C", ed);
                v.InnerText = prompt("V", ed);
                u.InnerText = prompt("U", ed);
                r.InnerText = prompt("R", ed);

                geo.AppendChild(a);
                geo.AppendChild(b);
                geo.AppendChild(c);
                geo.AppendChild(v);
                geo.AppendChild(u);
                geo.AppendChild(r);
            }
            else if (type == "EX")
            {
                XmlNode a = xmlDoc.CreateElement("A");
                XmlNode b = xmlDoc.CreateElement("B");
                XmlNode c = xmlDoc.CreateElement("C");
                XmlNode y = xmlDoc.CreateElement("Y");
                XmlNode v = xmlDoc.CreateElement("V");
                XmlNode u = xmlDoc.CreateElement("U");
                XmlNode r = xmlDoc.CreateElement("R");

                a.InnerText = prompt("A", ed);
                b.InnerText = prompt("B", ed);
                c.InnerText = prompt("C", ed);
                b.InnerText = prompt("Y", ed);
                v.InnerText = prompt("V", ed);
                u.InnerText = prompt("U", ed);
                r.InnerText = prompt("R", ed);

                geo.AppendChild(a);
                geo.AppendChild(b);
                geo.AppendChild(c);
                geo.AppendChild(y);
                geo.AppendChild(v);
                geo.AppendChild(u);
                geo.AppendChild(r);
            }
            else if (type == "F")
            {
                XmlNode a = xmlDoc.CreateElement("A");
                XmlNode b = xmlDoc.CreateElement("B");
                XmlNode v = xmlDoc.CreateElement("V");
                XmlNode r = xmlDoc.CreateElement("R");

                a.InnerText = prompt("A", ed);
                b.InnerText = prompt("B", ed);
                v.InnerText = prompt("V", ed);
                r.InnerText = prompt("R", ed);

                geo.AppendChild(a);
                geo.AppendChild(b);
                geo.AppendChild(v);
                geo.AppendChild(r);
            }
            else if (type == "G")
            {
                XmlNode a = xmlDoc.CreateElement("A");
                XmlNode b = xmlDoc.CreateElement("B");
                XmlNode c = xmlDoc.CreateElement("C");
                XmlNode x = xmlDoc.CreateElement("X");
                XmlNode v = xmlDoc.CreateElement("V");
                XmlNode s = xmlDoc.CreateElement("S");
                XmlNode r = xmlDoc.CreateElement("R");

                a.InnerText = prompt("A", ed);
                b.InnerText = prompt("B", ed);
                c.InnerText = prompt("C", ed);
                x.InnerText = prompt("X", ed);
                v.InnerText = prompt("V", ed);
                s.InnerText = prompt("S", ed);
                r.InnerText = prompt("R", ed);

                geo.AppendChild(a);
                geo.AppendChild(b);
                geo.AppendChild(c);
                geo.AppendChild(x);
                geo.AppendChild(v);
                geo.AppendChild(s);
                geo.AppendChild(r);
            }
            else if (type == "H")
            {
                XmlNode a = xmlDoc.CreateElement("A");
                XmlNode b = xmlDoc.CreateElement("B");
                XmlNode c = xmlDoc.CreateElement("C");
                XmlNode d = xmlDoc.CreateElement("D");
                XmlNode e = xmlDoc.CreateElement("E");
                XmlNode x = xmlDoc.CreateElement("X");
                XmlNode v = xmlDoc.CreateElement("V");
                XmlNode s = xmlDoc.CreateElement("S");
                XmlNode u = xmlDoc.CreateElement("U");
                XmlNode r = xmlDoc.CreateElement("R");

                a.InnerText = prompt("A", ed);
                b.InnerText = prompt("B", ed);
                c.InnerText = prompt("C", ed);
                d.InnerText = prompt("D", ed);
                e.InnerText = prompt("E", ed);
                x.InnerText = prompt("X", ed);
                v.InnerText = prompt("V", ed);
                s.InnerText = prompt("S", ed);
                u.InnerText = prompt("U", ed);
                r.InnerText = prompt("R", ed);

                geo.AppendChild(a);
                geo.AppendChild(b);
                geo.AppendChild(c);
                geo.AppendChild(d);
                geo.AppendChild(e);
                geo.AppendChild(x);
                geo.AppendChild(v);
                geo.AppendChild(s);
                geo.AppendChild(u);
                geo.AppendChild(r);
            }
            else if (type == "J")
            {
                XmlNode a = xmlDoc.CreateElement("A");
                XmlNode c = xmlDoc.CreateElement("C");
                XmlNode e = xmlDoc.CreateElement("E");

                XmlNode x = xmlDoc.CreateElement("X");
                XmlNode y = xmlDoc.CreateElement("Y");

                XmlNode v = xmlDoc.CreateElement("V");
                XmlNode s = xmlDoc.CreateElement("S");
                XmlNode t = xmlDoc.CreateElement("T");
                XmlNode u = xmlDoc.CreateElement("U");

                XmlNode r = xmlDoc.CreateElement("R");

                a.InnerText = prompt("A", ed);
                c.InnerText = prompt("C", ed);
                e.InnerText = prompt("E", ed);
                x.InnerText = prompt("X", ed);
                y.InnerText = prompt("Y", ed);
                v.InnerText = prompt("V", ed);
                s.InnerText = prompt("S", ed);
                t.InnerText = prompt("T", ed);
                u.InnerText = prompt("U", ed);
                r.InnerText = prompt("R", ed);

                geo.AppendChild(a);
                geo.AppendChild(c);
                geo.AppendChild(e);
                geo.AppendChild(x);
                geo.AppendChild(y);
                geo.AppendChild(v);
                geo.AppendChild(s);
                geo.AppendChild(t);
                geo.AppendChild(u);
                geo.AppendChild(r);
            }
            else if (type == "K") // TODO
            {

            }
            else if (type == "L") // TODO
            {

            }
            else if (type == "LX") // TODO
            {

            }
            else if (type == "M") // TODO
            {

            }
            else if (type == "N") // TODO
            {
                XmlNode a = xmlDoc.CreateElement("A");
                XmlNode b = xmlDoc.CreateElement("B");
                XmlNode r = xmlDoc.CreateElement("R");
                XmlNode e1 = xmlDoc.CreateElement("End1");
                XmlNode e2 = xmlDoc.CreateElement("End2");

                e1.InnerText = "L";
                a.InnerText = prompt("A", ed);
                b.InnerText = prompt("B", ed);
                e2.InnerText = "L";
                r.InnerText = prompt("R", ed);

                geo.AppendChild(e1);
                geo.AppendChild(a);
                geo.AppendChild(b);
                geo.AppendChild(e2);
                geo.AppendChild(r);
            }
            else if (type == "NX")
            {
                XmlNode a = xmlDoc.CreateElement("A");
                XmlNode d = xmlDoc.CreateElement("D");
                XmlNode v = xmlDoc.CreateElement("V");
                XmlNode r = xmlDoc.CreateElement("R");
                XmlNode e1 = xmlDoc.CreateElement("End1");
                XmlNode e2 = xmlDoc.CreateElement("End2");

                e1.InnerText = "L";
                a.InnerText = prompt("A", ed);
                d.InnerText = prompt("D", ed);
                v.InnerText = prompt("V", ed);
                e2.InnerText = "L";
                r.InnerText = prompt("R", ed);

                geo.AppendChild(e1);
                geo.AppendChild(a);
                geo.AppendChild(d);
                geo.AppendChild(v);
                geo.AppendChild(e2);
                geo.AppendChild(r);
            }
            else if (type == "O") // TODO
            {

            }
            else if (type == "Q") // TODO
            {

            }
            else if (type == "R") // TODO
            {

            }
            else if (type == "S")
            {
                XmlNode a = xmlDoc.CreateElement("A");
                XmlNode b = xmlDoc.CreateElement("B");
                XmlNode y = xmlDoc.CreateElement("Y");

                a.InnerText = prompt("A", ed);
                b.InnerText = prompt("B", ed);
                y.InnerText = prompt("Y", ed);

                geo.AppendChild(a);
                geo.AppendChild(b);
                geo.AppendChild(y);
            }
            else if (type == "SH") // TODO
            {

            }
            else if (type == "SX") // TODO
            {

            }
            else if (type == "T") // TODO
            {

            }
            else if (type == "U")
            {
                XmlNode a = xmlDoc.CreateElement("A");
                XmlNode c = xmlDoc.CreateElement("C");
                XmlNode x = xmlDoc.CreateElement("X");
                XmlNode y = xmlDoc.CreateElement("Y");
                XmlNode r = xmlDoc.CreateElement("R");


                a.InnerText = prompt("A", ed);
                c.InnerText = prompt("C", ed);
                x.InnerText = prompt("X", ed);
                y.InnerText = prompt("Y", ed);
                r.InnerText = prompt("R", ed);

                geo.AppendChild(a);
                geo.AppendChild(c);
                geo.AppendChild(x);
                geo.AppendChild(y);
                geo.AppendChild(r);
            }
            else if (type == "V")
            {
                XmlNode a = xmlDoc.CreateElement("A");
                XmlNode x = xmlDoc.CreateElement("X");
                XmlNode y = xmlDoc.CreateElement("Y");
                XmlNode r = xmlDoc.CreateElement("R");


                a.InnerText = prompt("A", ed);
                x.InnerText = prompt("X", ed);
                y.InnerText = prompt("Y", ed);
                r.InnerText = prompt("R", ed);

                geo.AppendChild(a);
                geo.AppendChild(x);
                geo.AppendChild(y);
                geo.AppendChild(r);
            }
            else if (type == "W") // TODO
            {

            }
            else if (type == "X") // TODO
            {

            }
            else if (type == "XX")
            {
                XmlNode a = xmlDoc.CreateElement("A");
                XmlNode b = xmlDoc.CreateElement("B");
                XmlNode c = xmlDoc.CreateElement("C");
                XmlNode d = xmlDoc.CreateElement("D");
                XmlNode e = xmlDoc.CreateElement("E");
                XmlNode v = xmlDoc.CreateElement("V");
                XmlNode r = xmlDoc.CreateElement("R");

                a.InnerText = prompt("A", ed);
                b.InnerText = prompt("B", ed);
                c.InnerText = prompt("C", ed);
                d.InnerText = prompt("D", ed);
                e.InnerText = prompt("E", ed);
                v.InnerText = prompt("V", ed);
                r.InnerText = prompt("R", ed);

                geo.AppendChild(a);
                geo.AppendChild(b);
                geo.AppendChild(c);
                geo.AppendChild(d);
                geo.AppendChild(e);
                geo.AppendChild(v);
                geo.AppendChild(r);
            }
            else if (type == "Z") // TODO
            {

            }

            bar.AppendChild(type_node);
            bar.AppendChild(pos);
            bar.AppendChild(material);
            bar.AppendChild(dim);
            bar.AppendChild(geo);

            row.AppendChild(group);
            row.AppendChild(count);
            row.AppendChild(bar);

            return row;
        }


        public static XmlNode compare(XmlNode newReb, XmlNode rebar)
        {
            XmlNode old_bar = rebar["B2aBar"];
            XmlNode new_bar = newReb["B2aBar"];
            
            if (old_bar == null) return null;
            if (new_bar == null) return null;

            string new_type = new_bar["Type"].InnerText;

            XmlNode old_geometry = old_bar["B2aGeometry"];
            XmlNode new_geometry = new_bar["B2aGeometry"];

            if (new_type == "A")
            {
                return null;
            }
            else
            {
                if (old_geometry == null) return null;
                if (new_geometry == null) return null;

                string old_dim = emptyNodehandle(old_bar, "Dim");
                string new_dim = emptyNodehandle(new_bar, "Dim");

                if (old_dim == "???") return null;
                if (new_dim == "???") return null;
                if (old_dim != new_dim) return null;

                if (new_type == "B")
                {
                    string old_a = emptyNodehandle(old_geometry, "A");
                    string old_b = emptyNodehandle(old_geometry, "B");
                    string old_r = emptyNodehandle(old_geometry, "R");

                    string new_a = emptyNodehandle(new_geometry, "A");
                    string new_b = emptyNodehandle(new_geometry, "B");
                    string new_r = emptyNodehandle(new_geometry, "R");


                    if (old_a == "???" || old_b == "???" || old_r == "???") return null;
                    if (new_a == "???" || new_b == "???" || new_r == "???") return null;

                    if (old_a == new_a && 
                        old_b == new_b && 
                        old_r == new_r) return rebar;

                }
                else if (new_type == "C")
                {
                    string old_a = emptyNodehandle(old_geometry, "A");
                    string old_b = emptyNodehandle(old_geometry, "B");
                    string old_c = emptyNodehandle(old_geometry, "C");
                    string old_r = emptyNodehandle(old_geometry, "R");

                    string new_a = emptyNodehandle(new_geometry, "A");
                    string new_b = emptyNodehandle(new_geometry, "B");
                    string new_c = emptyNodehandle(new_geometry, "C");
                    string new_r = emptyNodehandle(new_geometry, "R");

                    if (old_a == "???" || old_b == "???" || old_c == "???" || old_r == "???") return null;
                    if (new_a == "???" || new_b == "???" || new_c == "???" || new_r == "???") return null;

                    if (old_a == new_a && 
                        old_b == new_b && 
                        old_c == new_c && 
                        old_r == new_r) return rebar;

                }
                else if (new_type == "D")
                {
                    string old_a = emptyNodehandle(old_geometry, "A");
                    string old_b = emptyNodehandle(old_geometry, "B");
                    string old_v = emptyNodehandle(old_geometry, "V");
                    string old_r = emptyNodehandle(old_geometry, "R");

                    string new_a = emptyNodehandle(new_geometry, "A");
                    string new_b = emptyNodehandle(new_geometry, "B");
                    string new_v = emptyNodehandle(new_geometry, "V");
                    string new_r = emptyNodehandle(new_geometry, "R");

                    if (old_a == "???" || old_b == "???" || old_v == "???" || old_r == "???") return null;
                    if (new_a == "???" || new_b == "???" || new_v == "???" || new_r == "???") return null;

                    if (old_a == new_a && 
                        old_b == new_b &&
                        old_v == new_v &&
                        old_r == new_r) return rebar;

                }
                else if (new_type == "E")
                {
                    string old_a = emptyNodehandle(old_geometry, "A");
                    string old_b = emptyNodehandle(old_geometry, "B");
                    string old_c = emptyNodehandle(old_geometry, "C");
                    string old_v = emptyNodehandle(old_geometry, "V");
                    string old_u = emptyNodehandle(old_geometry, "U");
                    string old_r = emptyNodehandle(old_geometry, "R");

                    string new_a = emptyNodehandle(new_geometry, "A");
                    string new_b = emptyNodehandle(new_geometry, "B");
                    string new_c = emptyNodehandle(new_geometry, "C");
                    string new_v = emptyNodehandle(new_geometry, "V");
                    string new_u = emptyNodehandle(new_geometry, "U");
                    string new_r = emptyNodehandle(new_geometry, "R");

                    if (old_a == "???" || old_b == "???" || old_c == "???" || old_v == "???" || old_u == "???" || old_r == "???") return null;
                    if (new_a == "???" || new_b == "???" || new_c == "???" || new_v == "???" || new_u == "???" || new_r == "???") return null;

                    if (old_a == new_a &&
                        old_b == new_b &&
                        old_c == new_c &&
                        old_v == new_v &&
                        old_u == new_u &&
                        old_r == new_r) return rebar;

                }
                else if (new_type == "EX")
                {
                    string old_a = emptyNodehandle(old_geometry, "A");
                    string old_b = emptyNodehandle(old_geometry, "B");
                    string old_c = emptyNodehandle(old_geometry, "C");
                    string old_y = emptyNodehandle(old_geometry, "Y");
                    string old_v = emptyNodehandle(old_geometry, "V");
                    string old_u = emptyNodehandle(old_geometry, "U");
                    string old_r = emptyNodehandle(old_geometry, "R");

                    string new_a = emptyNodehandle(new_geometry, "A");
                    string new_b = emptyNodehandle(new_geometry, "B");
                    string new_c = emptyNodehandle(new_geometry, "C");
                    string new_y = emptyNodehandle(new_geometry, "Y");
                    string new_v = emptyNodehandle(new_geometry, "V");
                    string new_u = emptyNodehandle(new_geometry, "U");
                    string new_r = emptyNodehandle(new_geometry, "R");

                    if (old_a == "???" || old_b == "???" || old_c == "???" || old_y == "???" || old_v == "???" || old_u == "???" || old_r == "???") return null;
                    if (new_a == "???" || new_b == "???" || new_c == "???" || new_y == "???" || new_v == "???" || new_u == "???" || new_r == "???") return null;

                    if (old_a == new_a &&
                        old_b == new_b &&
                        old_c == new_c &&
                        old_y == new_y &&
                        old_v == new_v &&
                        old_u == new_u &&
                        old_r == new_r) return rebar;

                }
                else if (new_type == "F")
                {
                    string old_a = emptyNodehandle(old_geometry, "A");
                    string old_b = emptyNodehandle(old_geometry, "B");
                    string old_v = emptyNodehandle(old_geometry, "V");
                    string old_r = emptyNodehandle(old_geometry, "R");

                    string new_a = emptyNodehandle(new_geometry, "A");
                    string new_b = emptyNodehandle(new_geometry, "B");
                    string new_v = emptyNodehandle(new_geometry, "V");
                    string new_r = emptyNodehandle(new_geometry, "R");

                    if (old_a == "???" || old_b == "???" || old_v == "???" || old_r == "???") return null;
                    if (new_a == "???" || new_b == "???" || new_v == "???" || new_r == "???") return null;

                    if (old_a == new_a &&
                        old_b == new_b &&
                        old_v == new_v &&
                        old_r == new_r) return rebar;

                }
                else if (new_type == "G")
                {
                    string old_a = emptyNodehandle(old_geometry, "A");
                    string old_b = emptyNodehandle(old_geometry, "B");
                    string old_c = emptyNodehandle(old_geometry, "C");
                    string old_x = emptyNodehandle(old_geometry, "X");
                    string old_v = emptyNodehandle(old_geometry, "V");
                    string old_s = emptyNodehandle(old_geometry, "S");
                    string old_r = emptyNodehandle(old_geometry, "R");

                    string new_a = emptyNodehandle(new_geometry, "A");
                    string new_b = emptyNodehandle(new_geometry, "B");
                    string new_c = emptyNodehandle(new_geometry, "C");
                    string new_x = emptyNodehandle(new_geometry, "X");
                    string new_v = emptyNodehandle(new_geometry, "V");
                    string new_s = emptyNodehandle(new_geometry, "S");
                    string new_r = emptyNodehandle(new_geometry, "R");

                    if (old_a == "???" || old_b == "???" || old_c == "???" || old_x == "???" || old_v == "???" || old_s == "???" || old_r == "???") return null;
                    if (new_a == "???" || new_b == "???" || new_c == "???" || new_x == "???" || new_v == "???" || new_s == "???" || new_r == "???") return null;

                    if (old_a == new_a &&
                        old_b == new_b &&
                        old_c == new_c &&
                        old_x == new_x &&
                        old_v == new_v &&
                        old_s == new_s &&
                        old_r == new_r) return rebar;

                }
                else if (new_type == "H")
                {
                    string old_a = emptyNodehandle(old_geometry, "A");
                    string old_b = emptyNodehandle(old_geometry, "B");
                    string old_c = emptyNodehandle(old_geometry, "C");
                    string old_d = emptyNodehandle(old_geometry, "D");
                    string old_e = emptyNodehandle(old_geometry, "E");
                    string old_x = emptyNodehandle(old_geometry, "X");
                    string old_v = emptyNodehandle(old_geometry, "V");
                    string old_s = emptyNodehandle(old_geometry, "S");
                    string old_u = emptyNodehandle(old_geometry, "U");
                    string old_r = emptyNodehandle(old_geometry, "R");

                    string new_a = emptyNodehandle(new_geometry, "A");
                    string new_b = emptyNodehandle(new_geometry, "B");
                    string new_c = emptyNodehandle(new_geometry, "C");
                    string new_d = emptyNodehandle(new_geometry, "D");
                    string new_e = emptyNodehandle(new_geometry, "E");
                    string new_x = emptyNodehandle(new_geometry, "X");
                    string new_v = emptyNodehandle(new_geometry, "V");
                    string new_s = emptyNodehandle(new_geometry, "S");
                    string new_u = emptyNodehandle(new_geometry, "U");
                    string new_r = emptyNodehandle(new_geometry, "R");
                    
                    if (old_a == "???" || old_b == "???" || old_c == "???" || old_d == "???" || old_e == "???" || old_x == "???" || old_v == "???" || old_s == "???" || old_u == "???" || old_r == "???") return null;
                    if (new_a == "???" || new_b == "???" || new_c == "???" || new_d == "???" || new_e == "???" || new_x == "???" || new_v == "???" || new_s == "???" || new_u == "???" || new_r == "???") return null;

                    if (old_a == new_a &&
                        old_b == new_b &&
                        old_c == new_c &&
                        old_d == new_d &&
                        old_e == new_e &&
                        old_x == new_x &&
                        old_v == new_v &&
                        old_s == new_s &&
                        old_u == new_u &&
                        old_r == new_r) return rebar;
                }
                else if (new_type == "J")
                {
                    string old_a = emptyNodehandle(old_geometry, "A");
                    string old_c = emptyNodehandle(old_geometry, "C");
                    string old_e = emptyNodehandle(old_geometry, "E");
                    string old_x = emptyNodehandle(old_geometry, "X");
                    string old_y = emptyNodehandle(old_geometry, "Y");
                    string old_v = emptyNodehandle(old_geometry, "V");
                    string old_s = emptyNodehandle(old_geometry, "S");
                    string old_t = emptyNodehandle(old_geometry, "T");
                    string old_u = emptyNodehandle(old_geometry, "U");
                    string old_r = emptyNodehandle(old_geometry, "R");

                    string new_a = emptyNodehandle(new_geometry, "A");
                    string new_c = emptyNodehandle(new_geometry, "C");
                    string new_e = emptyNodehandle(new_geometry, "E");
                    string new_x = emptyNodehandle(new_geometry, "X");
                    string new_y = emptyNodehandle(new_geometry, "Y");
                    string new_v = emptyNodehandle(new_geometry, "V");
                    string new_s = emptyNodehandle(new_geometry, "S");
                    string new_t = emptyNodehandle(new_geometry, "T");
                    string new_u = emptyNodehandle(new_geometry, "U");
                    string new_r = emptyNodehandle(new_geometry, "R");

                    if (old_a == "???" || old_c == "???" || old_e == "???" || old_x == "???" || old_y == "???" || old_v == "???" || old_s == "???" || old_t == "???" || old_u == "???" || old_r == "???") return null;
                    if (new_a == "???" || new_c == "???" || new_e == "???" || new_x == "???" || new_y == "???" || new_v == "???" || new_s == "???" || new_t == "???" || new_u == "???" || new_r == "???") return null;

                    if (old_a == new_a &&
                        old_c == new_c &&
                        old_e == new_e &&
                        old_x == new_x &&
                        old_y == new_y &&
                        old_v == new_v &&
                        old_s == new_s &&
                        old_t == new_t &&
                        old_u == new_u &&
                        old_r == new_r) return rebar;
                }
                else if (new_type == "K") // TODO
                {

                }
                else if (new_type == "L") // TODO
                {

                }
                else if (new_type == "LX") // TODO
                {

                }
                else if (new_type == "M") // TODO
                {

                }
                else if (new_type == "N") // TODO
                {
                    string old_a = emptyNodehandle(old_geometry, "A");
                    string old_b = emptyNodehandle(old_geometry, "B");
                    string old_r = emptyNodehandle(old_geometry, "R");
                    string old_e1 = emptyNodehandle(old_geometry, "End1");
                    string old_e2 = emptyNodehandle(old_geometry, "End2");

                    string new_a = emptyNodehandle(new_geometry, "A");
                    string new_b = emptyNodehandle(new_geometry, "B");
                    string new_r = emptyNodehandle(new_geometry, "R");
                    string new_e1 = emptyNodehandle(new_geometry, "End1");
                    string new_e2 = emptyNodehandle(new_geometry, "End2");

                    if (old_a == "???" || old_b == "???" || old_r == "???" || old_e1 == "???" || old_e2 == "???") return null;
                    if (new_a == "???" || new_b == "???" || new_r == "???" || new_e1 == "???" || new_e2 == "???") return null;

                    if (old_a == new_a &&
                        old_b == new_b &&
                        old_r == new_r &&
                        old_e1 == new_e1 &&
                        old_e2 == new_e2) return rebar;

                }
                else if (new_type == "NX")
                {
                    string old_a = emptyNodehandle(old_geometry, "A");
                    string old_d = emptyNodehandle(old_geometry, "D");
                    string old_v = emptyNodehandle(old_geometry, "V");
                    string old_r = emptyNodehandle(old_geometry, "R");
                    string old_e1 = emptyNodehandle(old_geometry, "End1");
                    string old_e2 = emptyNodehandle(old_geometry, "End2");

                    string new_a = emptyNodehandle(new_geometry, "A");
                    string new_d = emptyNodehandle(new_geometry, "D");
                    string new_v = emptyNodehandle(new_geometry, "V");
                    string new_r = emptyNodehandle(new_geometry, "R");
                    string new_e1 = emptyNodehandle(new_geometry, "End1");
                    string new_e2 = emptyNodehandle(new_geometry, "End2");

                    if (old_a == "???" || old_d == "???" || old_v == "???" || old_r == "???" || old_e1 == "???" || old_e2 == "???") return null;
                    if (new_a == "???" || new_d == "???" || new_v == "???" || new_r == "???" || new_e1 == "???" || new_e2 == "???") return null;

                    if (old_a == new_a &&
                        old_d == new_d &&
                        old_v == new_v &&
                        old_r == new_r &&
                        old_e1 == new_e1 &&
                        old_e2 == new_e2) return rebar;
                }
                else if (new_type == "O") // TODO
                {

                }
                else if (new_type == "Q") // TODO
                {

                }
                else if (new_type == "R") // TODO
                {

                }
                else if (new_type == "S") // TODO
                {
                    string old_a = emptyNodehandle(old_geometry, "A");
                    string old_b = emptyNodehandle(old_geometry, "B");
                    string old_y = emptyNodehandle(old_geometry, "Y");

                    string new_a = emptyNodehandle(new_geometry, "A");
                    string new_b = emptyNodehandle(new_geometry, "B");
                    string new_y = emptyNodehandle(new_geometry, "Y");

                    if (old_a == "???" || old_b == "???" || old_y == "???") return null;
                    if (new_a == "???" || new_b == "???" || new_y == "???") return null;

                    if (old_a == new_a &&
                        old_b == new_b &&
                        old_y == new_y) return rebar;
                }
                else if (new_type == "SH") // TODO
                {

                }
                else if (new_type == "SX") // TODO
                {

                }
                else if (new_type == "T") // TODO
                {

                }
                else if (new_type == "U")
                {
                    string old_a = emptyNodehandle(old_geometry, "A");
                    string old_c = emptyNodehandle(old_geometry, "C");
                    string old_x = emptyNodehandle(old_geometry, "X");
                    string old_y = emptyNodehandle(old_geometry, "Y");
                    string old_r = emptyNodehandle(old_geometry, "R");

                    string new_a = emptyNodehandle(new_geometry, "A");
                    string new_c = emptyNodehandle(new_geometry, "C");
                    string new_x = emptyNodehandle(new_geometry, "X");
                    string new_y = emptyNodehandle(new_geometry, "Y");
                    string new_r = emptyNodehandle(new_geometry, "R");

                    if (old_a == "???" || old_c == "???" || old_x == "???" || old_y == "???" || old_r == "???") return null;
                    if (new_a == "???" || new_c == "???" || new_x == "???" || new_y == "???" || new_r == "???") return null;

                    if (old_a == new_a &&
                        old_c == new_c &&
                        old_x == new_x &&
                        old_y == new_y &&
                        old_r == new_r) return rebar;
                }
                else if (new_type == "V")
                {
                    string old_a = emptyNodehandle(old_geometry, "A");
                    string old_x = emptyNodehandle(old_geometry, "X");
                    string old_y = emptyNodehandle(old_geometry, "Y");
                    string old_r = emptyNodehandle(old_geometry, "R");

                    string new_a = emptyNodehandle(new_geometry, "A");
                    string new_x = emptyNodehandle(new_geometry, "X");
                    string new_y = emptyNodehandle(new_geometry, "Y");
                    string new_r = emptyNodehandle(new_geometry, "R");

                    if (old_a == "???" || old_x == "???" || old_y == "???" || old_r == "???") return null;
                    if (new_a == "???" || new_x == "???" || new_y == "???" || new_r == "???") return null;

                    if (old_a == new_a &&
                        old_x == new_x &&
                        old_y == new_y &&
                        old_r == new_r) return rebar;
                }
                else if (new_type == "W") // TODO
                {

                }
                else if (new_type == "X") // TODO
                {

                }
                else if (new_type == "XX")
                {
                    string old_a = emptyNodehandle(old_geometry, "A");
                    string old_b = emptyNodehandle(old_geometry, "B");
                    string old_c = emptyNodehandle(old_geometry, "C");
                    string old_d = emptyNodehandle(old_geometry, "D");
                    string old_e = emptyNodehandle(old_geometry, "E");
                    string old_v = emptyNodehandle(old_geometry, "V");
                    string old_r = emptyNodehandle(old_geometry, "R");

                    string new_a = emptyNodehandle(new_geometry, "A");
                    string new_b = emptyNodehandle(new_geometry, "B");
                    string new_c = emptyNodehandle(new_geometry, "C");
                    string new_d = emptyNodehandle(new_geometry, "D");
                    string new_e = emptyNodehandle(new_geometry, "E");
                    string new_v = emptyNodehandle(new_geometry, "V");
                    string new_r = emptyNodehandle(new_geometry, "R");

                    if (old_a == "???" || old_b == "???" || old_c == "???" || old_d == "???" || old_e == "???" || old_v == "???" || old_r == "???") return null;
                    if (new_a == "???" || new_b == "???" || new_c == "???" || new_d == "???" || new_e == "???" || new_v == "???" || new_r == "???") return null;

                    if (old_a == new_a &&
                        old_b == new_b &&
                        old_c == new_c &&
                        old_d == new_d &&
                        old_e == new_e &&
                        old_v == new_v &&
                        old_r == new_r) return rebar;
                }
                else if (new_type == "Z") // TODO
                {

                }
            }

            return null;
        }


        public static List<XmlNode> filter(List<XmlNode> rows, string userFilter)
        {
            List<XmlNode> filtered = new List<XmlNode>();
            if (userFilter == "ALL")
            {
                filtered = rows;
            }
            else if (userFilter == "SPEC")
            {
                foreach (XmlNode row in rows)
                {
                    XmlNode temp_reb = row["B2aBar"];
                    if (temp_reb == null)
                    {
                        filtered.Add(row);
                        continue;
                    }

                    XmlNode temp_type = temp_reb["Type"];

                    if (temp_type == null)
                    {
                        filtered.Add(row);
                        continue;
                    }

                    if (temp_type.InnerText == "A")
                    {
                        continue;
                    }

                    filtered.Add(row);
                }
            }
            else if (userFilter == "LAST")
            {
                foreach (XmlNode row in rows)
                {
                    XmlNode temp_reb = row["B2aBar"];
                    if (temp_reb == null)
                    {
                        filtered.Add(row);
                        continue;
                    }

                    XmlNode temp_type = temp_reb["Type"];

                    if (temp_type == null)
                    {
                        filtered.Add(row);
                        continue;
                    }

                    if (temp_type.InnerText == "A")
                    {
                        continue;
                    }

                    filtered.Add(row);
                }

                XmlNode last = filtered[filtered.Count - 1];
                filtered = new List<XmlNode>();
                filtered.Add(last);
            }
            else
            {
                foreach (XmlNode row in rows)
                {
                    XmlNode temp_reb = row["B2aBar"];
                    if (temp_reb == null)
                    {
                        continue;
                    }

                    XmlNode temp_type = temp_reb["Type"];

                    if (temp_type == null)
                    {
                        continue;
                    }

                    if (temp_type.InnerText == userFilter)
                    {
                        filtered.Add(row);
                    }
                }
            }

            return filtered;
        }


        private static string prompt(string suurus, Editor ed)
        {
            string res = "";

            PromptStringOptions promptOptions = new PromptStringOptions("");
            promptOptions.Message = "\n" + suurus;
            promptOptions.DefaultValue = "";
            PromptResult promptResult = ed.GetString(promptOptions);

            if (promptResult.Status == PromptStatus.OK)
            {
                res = promptResult.StringResult;
            }

            return res;
        }


        public static XmlNode newPageHandle(List<XmlNode> pages, XmlDocument xmlDoc)
        {
            XmlNode row = xmlDoc.CreateElement("B2aPage");
            XmlAttribute attribute = xmlDoc.CreateAttribute("PageLabel");
            attribute.Value = "A-" + (pages.Count + 1).ToString("D2");
            row.Attributes.Append(attribute);

            XmlNode head = xmlDoc.CreateElement("B2aPageHead");

            XmlNode c10 = xmlDoc.CreateElement("ConcreteFctk10");
            XmlNode cf10 = xmlDoc.CreateElement("ConcreteCoverFactor10");

            head.AppendChild(c10);
            head.AppendChild(cf10);

            row.AppendChild(head);

            return row;
        }


        public static string emptyNodehandle(XmlNode rebar, string search)
        {
            XmlNode result = rebar[search];
            if (result == null)
            {
                return "???";
            }
            else
            {
                string innerText = result.InnerText;
                if (innerText.Length > 0)
                {
                    return result.InnerText;
                }
                else
                {
                    return "???";
                }

            }
        }

        public static List<XmlNode> getAllRebar(XmlDocument file)
        {
            List<XmlNode> rebars = new List<XmlNode>();

            foreach (XmlNode page in file.DocumentElement)
            {
                if (page.Name != "B2aPage") continue;

                foreach (XmlNode row in page.ChildNodes)
                {
                    if (row.Name != "B2aPageRow") continue;

                    rebars.Add(row);
                }
            }

            return rebars;
        }


        public static List<XmlNode> getAllPages(XmlDocument file)
        {
            List<XmlNode> pages = new List<XmlNode>();

            foreach (XmlNode page in file.DocumentElement)
            {
                if (page.Name != "B2aPage") continue;

                pages.Add(page);
            }

            return pages;
        }


        public static XmlDocument removeEmptyNodes(XmlDocument xd) // DONT KNOW THIS MAGIC
        {
            XmlNodeList emptyElements = xd.SelectNodes(@"//*[not(node())]");
            for (int i = emptyElements.Count - 1; i > -1; i--)
            {
                XmlNode nodeToBeRemoved = emptyElements[i];
                nodeToBeRemoved.ParentNode.RemoveChild(nodeToBeRemoved);
            }

            return xd;
        }


        private static void removeHandle(XmlNode parent, XmlNode child)
        {
                string content = child.InnerText;
                content = content.Replace(" ", "");
                if (content.Length == 0)
                {
                    parent.RemoveChild(child);
                }
        }
    }
}
