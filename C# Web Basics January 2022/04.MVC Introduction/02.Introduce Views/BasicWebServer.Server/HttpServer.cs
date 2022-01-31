﻿using BasicWebServer.Server.HTTP;
using BasicWebServer.Server.Routing;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace BasicWebServer.Server
{
    public class HttpServer
    {
        private readonly IPAddress ipAddress;
        private readonly int port;
        private readonly TcpListener serverListener;

        private readonly RoutingTable routingTable;

        public HttpServer(string ipAdress, int port, Action<IRoutingTable> routingTableConfiguration)
        {
            this.ipAddress = IPAddress.Parse(ipAdress);
            this.port = port;

            serverListener = new TcpListener(this.ipAddress, this.port);

            routingTableConfiguration(this.routingTable = new RoutingTable());
        }

        public HttpServer(int port, Action<IRoutingTable> routingTable)
            : this("127.0.0.1", port, routingTable)
        {
        }

        public HttpServer(Action<IRoutingTable> routingTable)
            : this(8080, routingTable)
        {
        }

        private async Task<string> ReadRequest(NetworkStream networkStream)
        {
            var bufferLength = 1024;
            var buffer = new byte[bufferLength];

            var totalBytes = 0;

            var requestBuilder = new StringBuilder();

            do
            {
                var bytesRead = await networkStream.ReadAsync(buffer, 0, bufferLength);

                totalBytes += bytesRead;

                if (totalBytes > 10 * 1024)
                {
                    throw new InvalidOperationException("Request is too large.");
                }

                requestBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
            }

            while (networkStream.DataAvailable);
            
            return requestBuilder.ToString();
            
        }

        public async Task Start()
        {
            serverListener.Start();

            Console.WriteLine($"Server started on port {port}");
            Console.WriteLine("Listening for request...");

            while (true)
            {
                var connection = await serverListener.AcceptTcpClientAsync();

                _ = Task.Run(async () =>
                {
                    var networkStream = connection.GetStream();

                    var requestText = await this.ReadRequest(networkStream);

                    Console.WriteLine(requestText);

                    var request = Request.Parse(requestText);

                    var response = this.routingTable.MatchRequest(request);

                    AddSession(request, response);

                    await WriteResponse(networkStream, response);

                    connection.Close();
                });            
            }
        }

        public static void AddSession(Request request, Response response)
        {
            var sessionExists = request.Session
                .ContainsKey(Session.SessionCurrentDateKey);

            if (!sessionExists)
            {
                request.Session[Session.SessionCurrentDateKey]
                    = DateTime.Now.ToString();
                response.Cookies
                    .Add(Session.SessionCookieName, request.Session.Id);
            }
        }

        private async Task WriteResponse(NetworkStream networkStream, Response response)
        { 
            var resposeBytes = Encoding.UTF8.GetBytes(response.ToString());

            await networkStream.WriteAsync(resposeBytes);
        }
    }
}