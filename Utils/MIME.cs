namespace Celerio;

public static class Mime
{
    public static Dictionary<string, Dictionary<string, string>> Types { get; } = new ()
    {
        {"application", new Dictionary<string, string>
        {
            {"json", ""},
            {"javascript", "js"},
            {"pdf", ""},
            {"zip", ""},
            {"xml", ""},
        }},
        {"audio", new Dictionary<string, string>
        {
            {"mpeg", "mp3"},
            {"wav", ""},
            {"ogg", ""}
        }},
        {"image", new Dictionary<string, string>
        {
            {"jpeg", "jpg"},
            {"png", ""},
            {"gif", ""},
            {"svg+xml", "svg"}
        }},
        {"text", new Dictionary<string, string>
        {
            {"html", ""},
            {"css", ""},
            {"plain", "txt"}
        }}
    };

    public static string GetType(string extension)
    {
        foreach (var type in Types)
        {
            foreach (var ext in type.Value)
            {
                var k = ext.Value;
                if(k == "")
                    k = ext.Key;

                if (extension == '.'+k||extension == k)
                    return $"{type.Key}/{ext.Key}";
            }
        }

        return "text/plain";
    }
}