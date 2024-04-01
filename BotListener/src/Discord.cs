using System;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BotListener
{
    //Moving to async instead
    /*public class Discord
    {
        private ClientWebSocket websocket;

        public List<Dictionary<string, object>> dataList;
        public Dictionary<string, List<Dictionary<string, object>>> outerDictionary;


        public Discord()
        {
            websocket = new ClientWebSocket();
            dataList = new List<Dictionary<string, object>>();
            outerDictionary = new Dictionary<string, List<Dictionary<string, object>>>();
        }

        public void constructData()
        {
            outerDictionary = new Dictionary<string, List<Dictionary<string, object>>>
        {
            { "data", dataList }
        };
        }

        public void addData(Dictionary<string, object> input)
        {
            dataList.Add(input);
        }

        public void clearData() 
        {
            dataList.Clear();
            outerDictionary.Clear();
        }


        public void Connect()
        {
            var task = websocket.ConnectAsync(new Uri("ws://localhost:8080"), CancellationToken.None);
            task.Wait();
            Console.WriteLine("Connected to Discord bot");
            Task.Run(() => ReceiveMessage()); // Start receiving messages asynchronously
        }

        public void SendMessage([Optional]string message)
        {
            constructData();
            string data = JsonSerializer.Serialize(outerDictionary);
            //Console.WriteLine(data);
            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(data));
            var task = websocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
            task.Wait();
            clearData();
        }

        public void ReceiveMessage()
        {
            var buffer = new byte[1024];
            while (websocket.State == WebSocketState.Open)
            {
                var task = websocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                var result = task.Result;
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine("Received message from Discord bot: " + receivedMessage);
                    // Process the message as needed
                }
            }
        }

        public void Close()
        {
            var task = websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            task.Wait();
            websocket.Dispose();
            Console.WriteLine("WebSocket connection closed");
        }
    }
    */

    public class Discord 
    {
        private ClientWebSocket _webSocket = new ClientWebSocket();
        private readonly SemaphoreSlim _receiveSemaphore = new SemaphoreSlim(0);
        private string _receivedData;

        public async Task ConnectAsync(string url)
        {
            await _webSocket.ConnectAsync(new Uri(url), CancellationToken.None);
            Console.WriteLine("Connected to WebSocket server");

            // Start listening for incoming messages in a separate task
            _ = Task.Run(async () => await ReceiveLoopAsync());
        }

        public async Task SendMessageAsync(string message)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            await _webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task<string> RecieveAsync()
        {
            await _receiveSemaphore.WaitAsync();
            return _receivedData;
        }

        private async Task ReceiveLoopAsync()
        {
            while (_webSocket.State == WebSocketState.Open)
            {
                try
                {
                    var buffer = new byte[1024];
                    var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        _receivedData = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        _receiveSemaphore.Release(); // Signal that data has been received
                    }
                }
                catch (WebSocketException ex)
                {
                    Console.WriteLine("WebSocket error: " + ex.Message);
                }
            }
        }

        public async Task DisconnectAsync()
        {
            if (_webSocket.State == WebSocketState.Open)
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing connection", CancellationToken.None);
                Console.WriteLine("Disconnected from WebSocket server");
            }
        }
    }
}
