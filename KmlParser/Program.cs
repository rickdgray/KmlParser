﻿using System;
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
            var readFileStream = File.OpenRead(zinjPath);

            var parsedPath = Path.Combine(Directory.GetCurrentDirectory(), "Zinj.txt");
            var writeFileStream = File.CreateText(parsedPath);

            var parser = new Parser();
            parser.Parse(readFileStream, writeFileStream);

            writeFileStream.Flush();
            writeFileStream.Close();

            readFileStream.Dispose();
            writeFileStream.Dispose();
        }
    }

    public class Parser
    {
        List<string> ValidNames { get; set; }

        public void Parse(FileStream readFileStream, StreamWriter writeFileStream)
        {
            ValidNames = new List<string>
            {
                "Folder",
                "name",
                "coordinates"
            };

            var printNextValue = false;
            using XmlReader reader = XmlReader.Create(readFileStream, new XmlReaderSettings
            {
                IgnoreComments = false
            });

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (ValidNames.Contains(reader.Name))
                        {
                            printNextValue = true;
                        }
                        else if (reader.Name.Equals("SimpleData")
                            && reader.AttributeCount > 0
                            && reader.GetAttribute("name").Equals("nature"))
                        {
                            printNextValue = true;
                        }
                        break;
                    case XmlNodeType.Text:
                        if (printNextValue)
                        {
                            printNextValue = false;
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
