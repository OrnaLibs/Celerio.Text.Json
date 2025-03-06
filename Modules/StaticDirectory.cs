namespace Celerio;

public class StaticDirectory : StaticFiles
{
    public string Route { get; set; }
    public string Directory { get; set; }

    public override void Initialize(Pipeline pipeline)
    {
        Files = new Dictionary<string, StaticFile>();
        foreach (var file in System.IO.Directory.GetFiles(Directory))
        {
            var rel = Path.GetRelativePath(Directory, file);
            string type = Mime.GetType(Path.GetExtension(file));
            Console.WriteLine($"Indexing {Route}{rel} => {file} as {type}");
            Files.Add(Route+rel, new StaticFile(file, type));
        }
    }

    public StaticDirectory(string route, string directory)
    {
        Route = route;
        Directory = directory;
    }
}