namespace KmlParser
{
    public class CsvBuilder
    {
        public static void Build(List<string> parsedData, string path)
        {
            using var writeStream = File.CreateText(path);

            var count = 0;
            var buildings = new List<Building>();
            while (count < parsedData.Count)
            {
                var name = parsedData[count].Trim();

                var type = parsedData[count + 1] switch
                {
                    "Autre" => BuildingType.Autre,
                    "Autre/Ecole" => BuildingType.AutreEcole,
                    "Autre/Eglise" => BuildingType.AutreEglise,
                    "Autre/Parking" => BuildingType.AutreParking,
                    "church" => BuildingType.Church,
                    "collective_house" => BuildingType.CollectiveHouse,
                    "commercial_building" => BuildingType.CommercialBuilding,
                    "commercial_building sportive" => BuildingType.CommercialBuildingSportive,
                    "garage" => BuildingType.Garage,
                    "hospital" => BuildingType.Hospital,
                    "light_building" => BuildingType.LightBuilding,
                    "school" => BuildingType.School,
                    "single_house" => BuildingType.SingleHouse,
                    "sport_building" => BuildingType.SportBuilding,
                    _ => BuildingType.Unspecified,
                };

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
                    Name = name,
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
