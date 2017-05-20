using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SW = System.Windows.Forms;


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
                        string x = "x: [" + emptyNodehandle(geometry, "X") + "]";
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
                        string x = "x: [" + emptyNodehandle(geometry, "X") + "]";
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
