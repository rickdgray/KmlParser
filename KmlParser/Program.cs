using System;
using System.IO;
using System.Linq;

namespace KmlParser
{
    public class Program
    {
        static int Main(string[] args)
        {
            var path = args.FirstOrDefault();
            if (path == null)
            {
                Console.WriteLine("Drop the .kml file onto the .exe to initiate.");
                Console.ReadKey();
                return 1;
            }

            var kmlFile = new DirectoryInfo(path);

            if (!kmlFile.Extension.Equals(".kml"))
            {
                Console.WriteLine("The provided file is not a .kml.");
                Console.ReadKey();
                return 2;
            }

            var csvFile = Path.Combine(kmlFile.Parent.FullName, "Zinj.csv");

            var parsedData = new KmlParser().Parse(kmlFile.FullName);

            new CsvBuilder().Build(parsedData, csvFile);

            return 0;
        }
    }
}
