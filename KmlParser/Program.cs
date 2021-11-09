using KmlParser;

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

CsvBuilder.Build(Parser.Parse(kmlFile.FullName),
    Path.Combine(kmlFile.Parent?.FullName ?? string.Empty, "Zinj.csv"));

return 0;