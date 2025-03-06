﻿using System.Text;
using System.Text.RegularExpressions;

namespace Celerio;

public class MultipartData
{
    public class Part
    {
        public string Body { get; init; }
        public byte[] BodyRaw { get; init; }
        public string Name { get; init; }
        public string? Filename { get; init; }
        public HeadersCollection Headers { get; init; }

        public Part(string body, byte[] bodyRaw, string name, string? filename, HeadersCollection headers)
        {
            Body = body;
            BodyRaw = bodyRaw;
            Name = name;
            Filename = filename;
            Headers = headers;
        }
    }
    
    public Part[] Parts { get; }

    public MultipartData(Part[] parts)
    {
        Parts = parts;
    }
    
    
    private static Regex ContentTypeRegex = new (@"multipart\/form-data; *boundary=(.*)", RegexOptions.Compiled);
    private static Regex ContentDispositionRegex = new (@"form-data; name=\""([^""]*)\""(; filename=\""(.*)\"")?", RegexOptions.Compiled);

    public Part? GetPart(string name) => Parts.FirstOrDefault(p => p.Name == name);
    
    public static bool TryParse(HttpRequest request, out MultipartData? data, out string? reason)
    {
        reason = null;
        data = null;

        if (request.Body == null || request.Body.Length == 0)
        {
            reason = "No request body";
            return false;
        }

        if (!request.Headers.TryGet("Content-Type", out List<string> contentType))
        {
            reason = "No Content-Type header";
            return false;
        }

        if (contentType.Count != 1)
        {
            reason = "There should be exactly one Content-Type header";
            return false;
        }

        var contentTypeMatch = ContentTypeRegex.Match(contentType[0]);
        if (!contentTypeMatch.Success)
        {
            reason = "Content-Type header wrong type";
            return false;
        }

        var boundary = "--" + contentTypeMatch.Groups[1].Value;
        var bodyBytes = request.Body!;
        var parts = new List<Part>();
        
        var index = 0;
        while (index < bodyBytes.Length)
        {
            var headerStartIndex = IndexOf(bodyBytes, boundary, index)+boundary.Length+2;
            if (headerStartIndex == boundary.Length+1) break;

            var headerEndIndex = IndexOf(bodyBytes, "\r\n\r\n", headerStartIndex);
            if (headerEndIndex == -1) break;
            
            var headersSpan = bodyBytes.AsSpan(headerStartIndex, headerEndIndex - headerStartIndex);
            var headers = ParseHeaders(headersSpan);
            if (!headers.TryGet("Content-Disposition", out List<string> contentDisposition))
            {
                reason = "Every part should contain Content-Disposition header";
                return false;
            }

            if (contentDisposition.Count != 1)
            {
                reason = "Content-Disposition header should contain exactly one value";
                return false;
            }
            var dispositionMatch = ContentDispositionRegex.Match(contentDisposition[0]);
            var name = dispositionMatch.Groups[1].Value; // получить заголовок имени
            var filename = dispositionMatch.Groups.Count >= 3 ? dispositionMatch.Groups[3].Value : null;

            var bodyStartIndex = headerEndIndex+4;
            
            var bodyEndIndex = IndexOf(bodyBytes, boundary, bodyStartIndex) - 2;
            if (bodyEndIndex == -3) break;
            
            var bodyRaw = bodyBytes.AsSpan(bodyStartIndex, bodyEndIndex - bodyStartIndex).ToArray();
            var body = Encoding.UTF8.GetString(bodyRaw);

            parts.Add(new Part(body, bodyRaw, name, filename, headers));

            index = bodyEndIndex+2;
        }

        data = new MultipartData(parts.ToArray());
        return true;
    }

    private static int IndexOf(byte[] source, string value, int startIndex = 0)
    {
        byte[] target = Encoding.ASCII.GetBytes(value);
        for (int i = startIndex; i <= source.Length - target.Length; i++)
        {
            int j = 0;
            while (j < target.Length && source[i + j] == target[j])
            {
                j++;
            }
            if (j == target.Length)
            {
                return i;
            }
        }
        return -1;
    }

    private static HeadersCollection ParseHeaders(ReadOnlySpan<byte> headersSpan)
    {
        var headers = new HeadersCollection();
        string headersStr = Encoding.UTF8.GetString(headersSpan);

        foreach (var line in headersStr.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            int colonIndex = line.IndexOf(':');
            if (colonIndex > 0)
            {
                string name = line.Substring(0, colonIndex).Trim();
                string value = line.Substring(colonIndex + 1).Trim();
                headers.Add(name, value);
            }
        }

        return headers;
    }
}