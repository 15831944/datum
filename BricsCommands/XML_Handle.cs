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

            string type = emptyXMLhandle(rebar, "Type");

            string pos_nr = "Pos: [" + emptyXMLhandle(rebar, "Litt") + "]";
            string diam = "Diameter: [" + emptyXMLhandle(rebar, "Dim") + "]";
            string length = "Length: [" + emptyXMLhandle(rebar, "Length") + "]";
            XmlNode geometry = rebar["B2aGeometry"];

            result = "Type [" + type + "]" + " - " + pos_nr + " - " + diam + " - " + length;

            if (type == "A")
            {

            }
            else if (type == "B") // TODO
            {

            }
            else if (type == "C")
            {
                if (geometry == null)
                {
                    result = result + " - [NO GEOMETRY]";
                }
                else
                {
                    string a = "A: [" + emptyXMLhandle(geometry, "A") + "]";
                    string b = "B: [" + emptyXMLhandle(geometry, "B") + "]";
                    string c = "C: [" + emptyXMLhandle(geometry, "C") + "]";
                    string r = "R: [" + emptyXMLhandle(geometry, "R") + "]";

                    result = result + " - " + a + " - " + b + " - " + c + " - " + r;
                }
            }
            else if (type == "D") // TODO
            {

            }
            else if (type == "E") // TODO
            {

            }
            else if (type == "EX") // TODO
            {

            }
            else if (type == "F") // TODO
            {

                if (geometry == null)
                {
                    result = result + " - [NO GEOMETRY]";
                }
                else
                {
                    string a = "A: [" + emptyXMLhandle(geometry, "A") + "]";
                    string b = "B: [" + emptyXMLhandle(geometry, "B") + "]";
                    string v = "V: [" + emptyXMLhandle(geometry, "V") + "]";
                    string r = "R: [" + emptyXMLhandle(geometry, "R") + "]";

                    result = result + " - " + a + " - " + b + " - " + v + " - " + r;
                }
            }
            else if (type == "G") // TODO
            {

            }
            else if (type == "H") // TODO
            {

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
                if (geometry == null)
                {
                    result = result + " - [NO GEOMETRY]";
                }
                else
                {
                    string a = "A: [" + emptyXMLhandle(geometry, "A") + "]";
                    string b = "B: [" + emptyXMLhandle(geometry, "B") + "]";
                    string r = "R: [" + emptyXMLhandle(geometry, "R") + "]";
                    string end1 = "End1: [" + emptyXMLhandle(geometry, "End1") + "]";
                    string end2 = "End2: [" + emptyXMLhandle(geometry, "End2") + "]";

                    result = result + " - " + a + " - " + b + " - " + r + " - " + end1 + " - " + end2;
                }
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

            return result;
        }

        public static string emptyXMLhandle(XmlNode rebar, string search)
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
