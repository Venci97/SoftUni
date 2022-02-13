using BasicWebServer.Demo.Controllers;
using BasicWebServer.Server;
using BasicWebServer.Server.Controllers;

namespace BasicWebServer.Demo
{
    public class StartUp
    {
        public static async Task Main()
        {
            await new HttpServer(routes => routes
                .MapControllers())
                .Start();
        }
    }
}