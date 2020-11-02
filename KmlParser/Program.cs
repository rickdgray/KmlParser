using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace KmlParser
{
    public class Program
    {
        static void Main()
        {
            var zinjPath = Path.Combine(Directory.GetCurrentDirectory(), "Zinj.kml");
            using var readFileStream = File.OpenRead(zinjPath);

            var parsedPath = Path.Combine(Directory.GetCurrentDirectory(), "Zinj.txt");
            using var writeFileStream = File.CreateText(parsedPath);

            var parser = new Parser();
            parser.Parse(readFileStream, writeFileStream);

            writeFileStream.Flush();
            writeFileStream.Close();

            readFileStream.Close();

            readFileStream.Dispose();
            writeFileStream.Dispose();

            var zinjPathTxt = Path.Combine(Directory.GetCurrentDirectory(), "Zinj.txt");
            using var readFileStreamTxt = File.OpenRead(zinjPathTxt);

            var csvPath = Path.Combine(Directory.GetCurrentDirectory(), "Zinj.csv");
            using var writeFileStreamCsv = File.CreateText(csvPath);

            using var streadReader = new StreamReader(readFileStreamTxt);

            writeFileStreamCsv.WriteLine("Name,Type,Coords");
            Console.WriteLine("Name,Type,Coords");

            var column = 0;
            string line;
            while ((line = streadReader.ReadLine()) != null)
            {
                if (column % 3 == 2)
                {
                    writeFileStreamCsv.WriteLine($"\"{line.Trim()}\"");
                    Console.WriteLine($"\"{line.Trim()}\"");
                }
                else
                {
                    writeFileStreamCsv.Write(line.Trim());
                    Console.Write(line.Trim());

                    writeFileStreamCsv.Write(",");
                    Console.Write(",");
                }

                column++;
            }

            writeFileStreamCsv.Flush();
            writeFileStreamCsv.Close();

            readFileStreamTxt.Close();

            writeFileStreamCsv.Dispose();
            readFileStreamTxt.Dispose();
        }
    }

    public class Parser
    {
        public void Parse(FileStream readFileStream, StreamWriter writeFileStream)
        {
            var printNextValue = false;
            //flag specifically used to avoid inner boundaries
            var printNextcoords = false;
            using XmlReader reader = XmlReader.Create(readFileStream, new XmlReaderSettings
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

                            writeFileStream.WriteLine(reader.Value);
                            Console.WriteLine(reader.Value);
                        }
                        break;
                    case XmlNodeType.CDATA:
                        var cdata = ParseCdata(reader.Value);
                        writeFileStream.WriteLine(cdata);
                        Console.WriteLine(cdata);
                        break;
                    default:
                        break;
                }
            }
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
