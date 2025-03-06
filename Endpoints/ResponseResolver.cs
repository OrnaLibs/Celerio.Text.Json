using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace Celerio;

public static class ResponseResolver
{
    public static HttpResponse Resolve(object? respRaw)
    {
        if (respRaw != null && respRaw.GetType() == typeof(HttpResponse))
            return (HttpResponse)respRaw;
        if (respRaw is string r)
            return HttpResponse.Ok(r);
            
        return HttpResponse.Ok(JsonSerializer.Serialize(respRaw, new JsonSerializerOptions
        {
            PropertyNamingPolicy = new CamelCaseNamingPolicy(),
            IncludeFields = true,
            Encoder = JavaScriptEncoder.Create(new TextEncoderSettings(UnicodeRanges.All))
        }));
    }
    
    private sealed class CamelCaseNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            return char.ToLowerInvariant(name[0]) + name[1..];
        }
    }
}
