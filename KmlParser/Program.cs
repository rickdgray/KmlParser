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
            var zinjKmlPath = Path.Combine(Directory.GetCurrentDirectory(), "Zinj.kml");
            var zinjTxtPath = Path.Combine(Directory.GetCurrentDirectory(), "Zinj.txt");
            var zinjCsvPath = Path.Combine(Directory.GetCurrentDirectory(), "Zinj.csv");

            using var readFileStream = File.OpenRead(zinjKmlPath);
            using var writeFileStream = File.CreateText(zinjTxtPath);

            var parser = new Parser();
            parser.Parse(readFileStream, writeFileStream);

            writeFileStream.Flush();
            writeFileStream.Close();

            readFileStream.Close();

            readFileStream.Dispose();
            writeFileStream.Dispose();

            //TODO: don't write to immediate read
            //turn into memory stream probably

            using var readFileStreamTxt = File.OpenRead(zinjTxtPath);
            using var streamReader = new StreamReader(readFileStreamTxt);

            var column = 0;
            string name;
            var pois = new List<PointOfInterest>();
            while ((name = streamReader.ReadLine()) != null)
            {
                var type = BuildingType.Unspecified;
                switch (streamReader.ReadLine())
                {
                    case "Autre":
                        type = BuildingType.Autre;
                        break;
                    case "Autre/Ecole":
                        type = BuildingType.AutreEcole;
                        break;
                    case "Autre/Eglise":
                        type = BuildingType.AutreEglise;
                        break;
                    case "Autre/Parking":
                        type = BuildingType.AutreParking;
                        break;
                    case "church":
                        type = BuildingType.Church;
                        break;
                    case "collective_house":
                        type = BuildingType.CollectiveHouse;
                        break;
                    case "commercial_building":
                        type = BuildingType.CommercialBuilding;
                        break;
                    case "commercial_building sportive":
                        type = BuildingType.CommercialBuildingSportive;
                        break;
                    case "garage":
                        type = BuildingType.Garage;
                        break;
                    case "hospital":
                        type = BuildingType.Hospital;
                        break;
                    case "light_building":
                        type = BuildingType.LightBuilding;
                        break;
                    case "school":
                        type = BuildingType.School;
                        break;
                    case "single_house":
                        type = BuildingType.SingleHouse;
                        break;
                    case "sport_building":
                        type = BuildingType.SportBuilding;
                        break;
                }

                var lats = new List<double>();
                var logs = new List<double>();
                var coordsString = streamReader.ReadLine();
                var coordPairStrings = coordsString.Split(" ").ToList();
                foreach (var coordPairString in coordPairStrings)
                {
                    var coordPair = coordPairString.Split(",").ToList();

                    if (double.TryParse(coordPair.First(), out var tmpLat))
                    {
                        lats.Add(tmpLat);
                    }

                    if (double.TryParse(coordPair.First(), out var tmpLog))
                    {
                        logs.Add(tmpLog);
                    }
                }

                pois.Add(new PointOfInterest
                {
                    Name = name.Trim(),
                    Type = type,
                    Latitude = lats.Sum() / lats.Count,
                    Longitude = logs.Sum() / logs.Count
                });

                column++;
            }

            using var writeFileStreamCsv = File.CreateText(zinjCsvPath);
            writeFileStreamCsv.WriteLine("Name,Type,Latitude,Logitude");
            pois.ForEach(p => writeFileStreamCsv.WriteLine($"{p.Name},{Enum.GetName(typeof(BuildingType), p.Type)},{p.Latitude},{p.Longitude}"));

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
                        }
                        break;
                    case XmlNodeType.CDATA:
                        var cdata = ParseCdata(reader.Value);
                        writeFileStream.WriteLine(cdata);
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

    public class PointOfInterest
    {
        public string Name { get; set; }
        public BuildingType Type { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public enum BuildingType
    {
        Unspecified,
        Autre,
        AutreEcole,
        AutreEglise,
        AutreParking,
        Church,
        CollectiveHouse,
        CommercialBuilding,
        CommercialBuildingSportive,
        Garage,
        Hospital,
        LightBuilding,
        School,
        SingleHouse,
        SportBuilding
    }
}
