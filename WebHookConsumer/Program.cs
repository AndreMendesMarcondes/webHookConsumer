﻿using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace WebHookConsumer
{
    class Program
    {
        private static UTF8Encoding encoding = new UTF8Encoding();

        public static List<Quote> QuotesList { get; set; }

        static void Main(string[] args)
        {
            QuotesList = new List<Quote>();
            Connect("ws://localhost:8080/quotes").Wait();
            Console.ReadKey();
        }

        public static async Task Connect(string uri)
        {
            Thread.Sleep(1000);

            ClientWebSocket webSocket = null;
            try
            {
                webSocket = new ClientWebSocket();
                await webSocket.ConnectAsync(new Uri(uri), CancellationToken.None);
                await Task.WhenAll(Receive(webSocket));
            }
            catch (Exception ex)
            {
            }
            finally
            {
                if (webSocket != null)
                    webSocket.Dispose();
                Console.WriteLine();
                Console.WriteLine("WebSocket closed.");
            }
        }

        private static async Task Receive(ClientWebSocket webSocket)
        {
            byte[] buffer = new byte[1024];
            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
                else
                {
                    var message = Encoding.UTF8.GetString(buffer).TrimEnd('\0');
                    Quote quote = new Quote(message);
                    QuotesList.Add(quote);
                    Console.WriteLine(new JavaScriptSerializer().Serialize(quote));
                }
            }
        }
    }
}


