using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KmlParser
{
    public class CsvBuilder
    {
        public void Build(List<string> parsedData, string path)
        {
            using var writeStream = File.CreateText(path);

            var count = 0;
            var buildings = new List<Building>();
            while (count < parsedData.Count)
            {
                var name = parsedData[count];

                var type = BuildingType.Unspecified;
                switch (parsedData[count + 1])
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
                var coordPairStrings = parsedData[count + 2].Split(" ").ToList();
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

                buildings.Add(new Building
                {
                    Name = name.Trim(),
                    Type = type,
                    Latitude = lats.Sum() / lats.Count,
                    Longitude = logs.Sum() / logs.Count
                });

                count += 3;
            }

            writeStream.WriteLine("Name,Type,Latitude,Logitude");
            buildings.ForEach(b => writeStream.WriteLine($"{b.Name},{Enum.GetName(typeof(BuildingType), b.Type)},{b.Latitude},{b.Longitude}"));

            writeStream.Flush();
            writeStream.Close();
            writeStream.Dispose();
        }
    }
}
