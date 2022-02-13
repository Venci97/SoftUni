using BasicWebServer.Server.HTTP;

namespace BasicWebServer.Server.Routing
{
    public interface IRoutingTable
    {
        IRoutingTable Map(Method method,
            string path,
            Func<Request, Response> responsFunction);
        IRoutingTable MapGet(string path, Func<Request, Response> responsFunction);
        IRoutingTable MapPost(string path, Func<Request, Response> responsFunction);
    }
}
