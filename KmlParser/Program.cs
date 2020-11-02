using System.IO;

namespace KmlParser
{
    public class Program
    {
        static void Main()
        {
            var kmlPath = Path.Combine(Directory.GetCurrentDirectory(), "Zinj.kml");
            var csvPath = Path.Combine(Directory.GetCurrentDirectory(), "Zinj.csv");

            var parsedData = new KmlParser().Parse(kmlPath);

            new CsvBuilder().Build(parsedData, csvPath);
        }
    }
}
