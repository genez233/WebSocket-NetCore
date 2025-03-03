// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.WebSockets;
using System.Text;

// WebSocket 服务器地址和端口
var serverUrl = "http://localhost:5000/";
var httpListener = new HttpListener();
httpListener.Prefixes.Add(serverUrl);
httpListener.Start();
Console.WriteLine("WebSocket server Listening on " + serverUrl);

while (true)
{
    // 等待客户端连接
    var httpContext = await httpListener.GetContextAsync();
    
    // 检查是否是 WebSocket 请求
    if (httpContext.Request.IsWebSocketRequest)
    {
        Console.WriteLine("Websocket connection request received.");
        await HandleWebSocketRequest(httpContext);
    }
    else
    {
        httpContext.Response.StatusCode = 400;
        httpContext.Response.Close();
    }
}

async Task HandleWebSocketRequest(HttpListenerContext context)
{
    // 接受 WebSocket 连接
    var webSocket = await context.AcceptWebSocketAsync(null);
    var webSocketStream = webSocket.WebSocket;
    
    var buffer = new byte[1024 * 4];

    try
    {
        while (webSocketStream.State == WebSocketState.Open)
        {
            // 接收消息
            var result = await webSocketStream.ReceiveAsync(buffer, CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                Console.WriteLine("WebSocket connection closed by client.");
                await webSocketStream.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            }
            else
            {
                // 处理接收到的消息
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine($"Received: {message}");

                // 发送响应消息
                var responseMessage = $"Server received: {message}";
                var responseBuffer = Encoding.UTF8.GetBytes(responseMessage);
                await webSocketStream.SendAsync(new ArraySegment<byte>(responseBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
    catch (Exception e)
    {
        Console.WriteLine($"WebSocket error: {e.Message}");
    } finally
    {
        webSocketStream.Dispose();
        Console.WriteLine("WebSocket connection closed.");
    }
}
