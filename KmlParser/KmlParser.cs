using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace KmlParser
{
    public class KmlParser
    {
        public List<string> Parse(string path)
        {
            using var readStream = File.OpenRead(path);

            var parsedData = new List<string>();

            var printNextValue = false;
            //flag specifically used to avoid inner boundaries
            var printNextcoords = false;
            using XmlReader reader = XmlReader.Create(readStream, new XmlReaderSettings
            {
                IgnoreComments = false
            });

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.Name)
                        {
                            case "name":
                                printNextValue = true;
                                break;
                            case "SimpleData":
                                if (reader.AttributeCount > 0
                                    && reader.GetAttribute("name").Equals("nature"))
                                {
                                    printNextValue = true;
                                }
                                break;
                            case "outerBoundaryIs":
                                printNextcoords = true;
                                break;
                            case "coordinates":
                                //only print outer boundaries
                                if (printNextcoords)
                                {
                                    printNextValue = true;
                                }
                                break;
                        }
                        break;
                    case XmlNodeType.Text:
                        if (printNextValue)
                        {
                            printNextValue = false;
                            printNextcoords = false;

                            if (reader.Value.Equals("Zone_1")
                                || reader.Value.Equals("Zone_2")
                                || reader.Value.Equals("Zone_3"))
                            {
                                //ignore zone names
                                continue;
                            }

                            parsedData.Add(reader.Value);
                        }
                        break;
                    case XmlNodeType.CDATA:
                        var cdata = ParseCdata(reader.Value);
                        parsedData.Add(cdata);
                        break;
                    default:
                        break;
                }
            }

            readStream.Close();
            readStream.Dispose();

            return parsedData;
        }

        public string ParseCdata(string cdataString)
        {
            //enclose in parent to prevent multiple root nodes
            cdataString = $"<parent>{cdataString}</parent>";
            var cdataParent = XElement.Parse(cdataString);
            var cdata = cdataParent.Descendants();
            //disgusting hack that assumes the second i tag is the value of the nature
            return cdata.FirstOrDefault(i => i.Name == "i").Value;
        }
    }
}
