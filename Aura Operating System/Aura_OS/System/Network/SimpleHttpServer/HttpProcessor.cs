﻿/*
* PROJECT:          Aura Operating System Development
* CONTENT:          HttpProcessor class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   David Jeske
*                   Barend Erasmus
* LICENSE:          LICENSES\SimpleHttpServer\LICENSE.md
*/

using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.TCP;
using SimpleHttpServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleHttpServer
{
    //used because Event with return type is not supported by Cosmos.
    public class HttpDiscussion
    {
        public HttpRequest Request;
        public HttpResponse Response;
    }

    public class HttpProcessor
    {
        #region Fields

        private static int MAX_POST_SIZE = 10 * 1024 * 1024; // 10MB

        private List<Route> Routes = new List<Route>();

        #endregion

        #region Constructors

        public HttpProcessor()
        {

        }

        #endregion

        #region Public Methods

        public void HandleClient(TcpClient tcpClient)
        {
            HttpRequest request = GetRequest(tcpClient);

            // route and handle the request...
            HttpResponse response = RouteRequest(tcpClient, request);      
          
            Console.WriteLine("{0} {1}", response.StatusCode, request.Url);

            // build a default response for errors
            if (response.Content == null)
            {
                if (response.StatusCode != "200")
                {
                    response.ContentAsUTF8 = string.Format("{0} {1} <p> {2}", response.StatusCode, request.Url, response.ReasonPhrase);
                }
            }

            WriteResponse(tcpClient, response);
        }

        // this formats the HTTP response...
        private static void WriteResponse(TcpClient client, HttpResponse response)
        {            
            if (response.Content == null)
            {           
                response.Content = new byte[]{};
            }
            
            // default to text/html content type
            if (!response.Headers.ContainsKey("Content-Type"))
            {
                response.Headers["Content-Type"] = "text/html";
            }

            response.Headers["Content-Length"] = response.Content.Length.ToString();

            //make response
            var sb = new StringBuilder();
            sb.Append(string.Format("HTTP/1.0 {0} {1}\r\n", response.StatusCode, response.ReasonPhrase));
            foreach (var header in response.Headers)
            {
                sb.Append(header.Key + ": " + header.Value + "\r\n");
            }
            sb.Append("\r\n");
            sb.Append(Encoding.ASCII.GetString(response.Content));

            client.Send(Encoding.ASCII.GetBytes(sb.ToString()));     
        }

        public void AddRoute(Route route)
        {
            Routes.Add(route);
        }

        #endregion

        #region Private Methods

        protected virtual HttpResponse RouteRequest(TcpClient client, HttpRequest request)
        {
            Route route = null;

            if (!Routes.Any())
            {
                return HttpBuilder.NotFound();
            }   

            foreach (var xroute in Routes)
            {
                if (xroute.Method == request.Method)
                {
                    route = xroute;
                }
            }

            if (route == null)
            {
                return new HttpResponse()
                {
                    ReasonPhrase = "Method Not Allowed",
                    StatusCode = "405",
                };
            }

            // trigger the route handler...
            request.Route = route;
            try
            {
                var discussion = new HttpDiscussion() { Request = request, Response = null };
                route.Callable(discussion);
                return discussion.Response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return HttpBuilder.InternalServerError();
            }
        }

        private HttpRequest GetRequest(TcpClient client)
        {
            var ep = new EndPoint(Address.Zero, 0);

            //Read Request Line
            string request = Encoding.ASCII.GetString(client.Receive(ref ep));

            var lines = request.Split("\r\n");

            string[] tokens = lines[0].Split(' ');

            if (tokens.Length != 3)
            {
                throw new Exception("invalid http request line");
            }

            string method = tokens[0].ToUpper();
            string url = tokens[1];
            string protocolVersion = tokens[2];

            //Read Headers
            Dictionary<string, string> headers = new Dictionary<string, string>();

            for (int i = 1; i < lines.Length; i++)
            {
                if (lines[i].Equals(""))
                {
                    break;
                }

                int separator = lines[i].IndexOf(':');
                if (separator == -1)
                {
                    throw new Exception("invalid http header line: " + lines[i]);
                }
                string name = lines[i].Substring(0, separator);
                int pos = separator + 1;
                while ((pos < lines[i].Length) && (lines[i][pos] == ' '))
                {
                    pos++;
                }

                string value = lines[i].Substring(pos, lines[i].Length - pos);
                headers.Add(name, value);
            }

            /*

            string content = null;
            if (headers.ContainsKey("Content-Length"))
            {
                int totalBytes = Convert.ToInt32(headers["Content-Length"]);
                int bytesLeft = totalBytes;
                byte[] bytes = new byte[totalBytes];
               
                while(bytesLeft > 0)
                {
                    byte[] buffer = new byte[bytesLeft > 1024? 1024 : bytesLeft];
                    int n = inputStream.Read(buffer, 0, buffer.Length);
                    buffer.CopyTo(bytes, totalBytes - bytesLeft);

                    bytesLeft -= n;
                }

                content = Encoding.ASCII.GetString(bytes);
            }*/  

            return new HttpRequest()
            {
                Method = method,
                Url = url,
                Headers = headers,
                Content = "content" //TODO: add content
            };
        }

        #endregion

    }
}
