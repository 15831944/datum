using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Xml;

namespace armering_merge
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter source folder:");
            string location = Console.ReadLine();
            if (!location.EndsWith(@"\")) { location = location + @"\"; }

            List<string> xmls = getFiles(location, "*.XML");

            XmlDocument xmlDoc = new XmlDocument();
            List<XmlNode> all_rebar = new List<XmlNode>();

            foreach (string xml in xmls)
            {                
                xmlDoc.Load(xml);
                xmlDoc = removeEmptyNodes(xmlDoc);

                List<XmlNode> pages = getAllPages(xmlDoc);
                List<XmlNode> rebars = getAllRebar(xmlDoc);
                
                all_rebar = add_handle_rebar(rebars, all_rebar);
            }

            setNumber(all_rebar);

            filtreeri(all_rebar, xmlDoc);
            xmlDoc.Save(location + "aa.xml");

            Console.WriteLine("Done.");
            Console.ReadLine();
        }

        private static void filtreeri(List<XmlNode> rebars, XmlDocument xmlDoc)
        {
            List<XmlNode> sortedRebars = sortRebars(rebars);

            List<XmlNode> pa = getAllPages(xmlDoc);
            foreach (XmlNode p in pa)
            {
                xmlDoc.DocumentElement.RemoveChild(p);
            }

            List<XmlNode> pages = new List<XmlNode>();

            int rebarTotIndex = 0;
            int rebarPageIndex = 1;
            int pageIndex = 0;

            XmlNode lastReb = null;

            XmlNode page = newPageHandle(pages, xmlDoc);
            pages.Add(page);
            xmlDoc.DocumentElement.AppendChild(page);

            while (true)
            {
                if (pageIndex < pages.Count)
                {
                    page = pages[pageIndex];
                }
                else
                {
                    page = newPageHandle(pages, xmlDoc);
                    pages.Add(page);
                    xmlDoc.DocumentElement.AppendChild(page);
                }

                while (true)
                {
                    XmlNode reb = sortedRebars[rebarTotIndex];

                    if (lastReb != null)
                    {
                        if (reb != null)
                        {
                            XmlNode aa = reb["B2aBar"];
                            if (aa != null)
                            {
                                XmlNode ab = aa["Type"];

                                if (ab != null)
                                {
                                    XmlNode ba = lastReb["B2aBar"];

                                    if (ba != null)
                                    {
                                        XmlNode bb = ba["Type"];

                                        if (bb != null)
                                        {
                                            if (ab.InnerText == "A" && bb.InnerText != "A")
                                            {
                                                lastReb = null;
                                                break;
                                            }
                                        }
                                    }
                                }

                            }
                        }
                    }

                    reb.Attributes["RowId"].Value = rebarPageIndex.ToString();
                    page.AppendChild(reb);
                    lastReb = reb;

                    rebarTotIndex++;
                    rebarPageIndex++;

                    if (rebarTotIndex >= rebars.Count) break;
                    if (rebarPageIndex > 20) break;
                }

                if (rebarTotIndex >= rebars.Count) break;

                rebarPageIndex = 1;
                pageIndex++;
            }
        }

        private static List<XmlNode> add_handle_rebar(List<XmlNode> rebars, List<XmlNode> all_rebars)
        {
            List<XmlNode> new_all_rebars = new List<XmlNode>();
            new_all_rebars.AddRange(all_rebars);

            foreach (XmlNode rebarNode1 in rebars)
            {
                XmlNode rebar1 = rebarNode1["B2aBar"];
                if (rebar1 == null) continue;

                XmlNode type1 = rebar1["Type"];
                XmlNode pos_nr1 = rebar1["Litt"];
                XmlNode diam1 = rebar1["Dim"];

                bool found = false;

                foreach (XmlNode rebarNode2 in all_rebars)
                {
                    XmlNode rebar2 = rebarNode2["B2aBar"];
                    if (rebar2 == null) continue;

                    XmlNode type2 = rebar2["Type"];
                    XmlNode pos_nr2 = rebar2["Litt"];
                    XmlNode diam2 = rebar2["Dim"];

                    if (type1 == null) continue;
                    if (type2 == null) continue;
                    if (pos_nr1 == null) continue;
                    if (pos_nr2 == null) continue;
                    if (diam1 == null) continue;
                    if (diam2 == null) continue;


                    if (type1.InnerText == type2.InnerText && pos_nr1.InnerText == pos_nr2.InnerText && diam1.InnerText == diam2.InnerText)
                    {
                        found = true;
                        break;
                    }
                }

                if (found == false)
                {
                    new_all_rebars.Add(rebarNode1);
                }
            }

            return new_all_rebars;
        }

        private static List<XmlNode> add_handle_page(List<XmlNode> pages, List<XmlNode> all_pages)
        {
            List<XmlNode> new_all_pages = new List<XmlNode>();
            new_all_pages.AddRange(all_pages);

            foreach (XmlNode pageNode1 in pages)
            {
                bool found = false;

                foreach (XmlNode pageNode2 in all_pages)
                {
                    XmlAttribute name1 = pageNode1.Attributes["PageLabel"];
                    XmlAttribute name2 = pageNode1.Attributes["PageLabel"];

                    if (name1.Value == name2.Value)
                    {
                        found = true;
                        break;
                    }
                }

                if (found == false)
                {
                    new_all_pages.Add(pageNode1);
                }
            }

            return new_all_pages;
        }

        private static void setNumber(List<XmlNode> rebarNodes)
        {

            foreach (XmlNode rebarNode in rebarNodes)
            {
                XmlNode group = rebarNode["NoStpGrp"];
                XmlNode rebar = rebarNode["B2aBar"];

                if (rebar != null && group != null)
                {
                    XmlNode type = rebar["Type"];
                    XmlNode pos_nr = rebar["Litt"];
                    XmlNode diam = rebar["Dim"];

                    group.InnerText = "9999";
                }
            }
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

        private static List<XmlNode> sortRebars(List<XmlNode> rebars)
        {
            List<XmlNode> a = new List<XmlNode>();
            List<XmlNode> others = new List<XmlNode>();
            List<XmlNode> undef = new List<XmlNode>();

            List<XmlNode> sortedRebars = new List<XmlNode>();
            sortedRebars.AddRange(others);
            sortedRebars.AddRange(a);

            foreach (XmlNode rebar in rebars)
            {
                XmlNode temp_reb = rebar["B2aBar"];

                if (temp_reb == null)
                {
                    undef.Add(rebar);
                    continue;
                }

                XmlNode temp_type = temp_reb["Type"];

                if (temp_type == null)
                {
                    undef.Add(rebar);
                    continue;
                }

                if (temp_type.InnerText == "A")
                {
                    XmlNode temp_litt = temp_reb["Litt"];
                    if (temp_litt == null)
                    {
                        undef.Add(rebar);
                        continue;
                    }
                    else
                    {
                        a.Add(rebar);
                    }
                }
                else
                {
                    XmlNode temp_litt = temp_reb["Litt"];
                    if (temp_litt == null)
                    {
                        undef.Add(rebar);
                        continue;
                    }
                    else
                    {
                        others.Add(rebar);
                    }
                }
            }

            try
            {
                a = a.OrderBy(b => Int32.Parse(b["B2aBar"]["Dim"].InnerText)).ToList();
            }
            catch
            {

            }

            try
            {
                a = a.OrderBy(b => Int32.Parse(b["B2aBar"]["Litt"].InnerText)).ToList();
            }
            catch
            {
                a = a.OrderBy(b => b["B2aBar"]["Litt"].InnerText).ToList();
            }

            try
            {
                others = others.OrderBy(b => Int32.Parse(b["B2aBar"]["Litt"].InnerText)).ToList();
            }
            catch
            {
                others = others.OrderBy(b => b["B2aBar"]["Litt"].InnerText).ToList();
            }

            List<XmlNode> sorted = new List<XmlNode>();
            sorted.AddRange(others);
            sorted.AddRange(a);
            sorted.AddRange(undef);

            return sorted;
        }

        private static XmlDocument removeEmptyNodes(XmlDocument xd) // DONT KNOW THIS MAGIC
        {
            XmlNodeList emptyElements = xd.SelectNodes(@"//*[not(node())]");
            for (int i = emptyElements.Count - 1; i > -1; i--)
            {
                XmlNode nodeToBeRemoved = emptyElements[i];
                nodeToBeRemoved.ParentNode.RemoveChild(nodeToBeRemoved);
            }

            return xd;
        }

        private static List<string> getFiles(string source, string ext)
        {
            List<string> files = new List<string>();

            if (!Directory.Exists(Path.GetDirectoryName(source))) return files;

            string[] importFiles = Directory.GetFiles(source, ext);

            foreach (string file in importFiles)
            {
                files.Add(file);
            }

            return files;
        }

        private static List<XmlNode> getAllRebar(XmlDocument file)
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


        private static List<XmlNode> getAllPages(XmlDocument file)
        {
            List<XmlNode> pages = new List<XmlNode>();

            foreach (XmlNode page in file.DocumentElement)
            {
                if (page.Name != "B2aPage") continue;

                pages.Add(page);
            }

            return pages;
        }
    }
}
