﻿namespace Celerio;

public class StaticFiles : ModuleBase
{
    public struct StaticFile
    {
        public string Path { get; set; }
        public string ContentType { get; set; }

        public StaticFile(string path, string contentType)
        {
            Path = path;
            ContentType = contentType;
        }
    }

    public Dictionary<string, StaticFile> Files { get; set; } = new Dictionary<string, StaticFile>();
    

    public StaticFiles(Dictionary<string, StaticFile> files)
    {
        Files = files;
    }

    public StaticFiles()
    {
    }


    public override HttpResponse? AfterRequest(Context context)
    {
        if (context.Request.Method != "GET") 
            return null;
        
        foreach (var file in Files)
        {
            if (file.Key != context.Request.URI)
                continue;

            return HttpResponse.File(new FileStream(file.Value.Path, FileMode.Open, FileAccess.Read), file.Value.ContentType);
        }

        return null;
    }
}