﻿using System.Reflection;

namespace Celerio;

public class EndpointManager
{
    public void Map(string method, string route, Delegate action)
    {
        var ep = new Endpoint(method, route, action);
        ep.Arguments = Arguments.GetArguments(ep);
        _endpoints.Add(ep);
    }

    public Endpoint? GetEndpoint(HttpRequest request, out string[] pathParameters)
    {
        foreach (var ep in _endpoints)
        {
            if (ep.HttpMethod != request.Method)
                continue;
            if (Endpoint.RoutePattern.Match(ep.Route, request.URI, out pathParameters))
            {
                return ep;
            }
        }

        pathParameters = [];
        return null;
    }

    public static HttpResponse CallEndpoint(Context context) => EndpointInvoker.CallEndpoint(context);
    
    private readonly List<Endpoint> _endpoints = [];

    internal void MapStatic()
    {
        Logging.Log("Searching for endpoints...");
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var t in asm.GetTypes())
            {
                if(!t.IsClass)
                    continue;
                foreach (var method in t.GetMethods())
                {
                    if (!method.IsStatic)
                        continue;
                    var attr = method.GetCustomAttribute<RouteAttribute>();
                    if (attr == null)
                        continue;
                    
                    Logging.Log($"Found endpoint: {attr.Method} {attr.Pattern}");

                    var ep = new Endpoint(attr.Method, attr.Pattern, method);
                    ep.Arguments = Arguments.GetArguments(ep);
                    _endpoints.Add(ep);
                }
            }
        }
    }
}