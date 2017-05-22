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
                    else if (type == "J") // TODO
                    {

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
                        string a = "A: [" + emptyNodehandle(geometry, "A") + "]";
                        string b = "B: [" + emptyNodehandle(geometry, "B") + "]";
                        string r = "R: [" + emptyNodehandle(geometry, "R") + "]";
                        string end1 = "End1: [" + emptyNodehandle(geometry, "End1") + "]";
                        string end2 = "End2: [" + emptyNodehandle(geometry, "End2") + "]";

                        result = result + " - " + a + " - " + b + " - " + r + " - " + end1 + " - " + end2;

                    }
                    else if (type == "NX") // TODO
                    {

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
                    else if (type == "S") // TODO
                    {

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
                    else if (type == "U") // TODO
                    {

                    }
                    else if (type == "V") // TODO
                    {

                    }
                    else if (type == "W") // TODO
                    {

                    }
                    else if (type == "X") // TODO
                    {

                    }
                    else if (type == "XX") // TODO
                    {

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
            count.InnerText = "1";
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
            else if (type == "J") // TODO
            {

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
            else if (type == "NX") // TODO
            {

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
            else if (type == "S") // TODO
            {

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
            else if (type == "U") // TODO
            {

            }
            else if (type == "V") // TODO
            {

            }
            else if (type == "W") // TODO
            {

            }
            else if (type == "X") // TODO
            {

            }
            else if (type == "XX") // TODO
            {

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
