using System.Net.WebSockets;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Server.Classes;

class Program
{
	static async Task Main(string[] args)
	{
		HttpListener listener = new HttpListener();
		listener.Prefixes.Add("http://localhost:2000/");
		listener.Start();
		Console.WriteLine("Servern lyssnar på http://localhost:2000/");

		while (true)
		{
			HttpListenerContext context = await listener.GetContextAsync();
			if (context.Request.IsWebSocketRequest)
			{
				HttpListenerWebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null);

				// Starta en ny tråd för att hantera WebSocket-anslutningen
				_ = HandleWebSocketConnection(webSocketContext.WebSocket);
			}
			else
			{
				context.Response.StatusCode = 400;
				context.Response.Close();
			}
		}
	}
	static async Task HandleWebSocketConnection(WebSocket webSocket)
	{
		try
		{
			while (webSocket.State == WebSocketState.Open)
			{
				// Tar emot meddelande från klienten
				WebSocketReceiveResult result;
				byte[] buffer = new byte[1024];
				StringBuilder messageData = new StringBuilder();

				do
				{
					result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
					messageData.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
				}
				while (!result.EndOfMessage);

				if(result.MessageType == WebSocketMessageType.Text)
				{
					// Deserialiserar JSON-meddelande till ett användarobjekt
					string receivedMessage = messageData.ToString();
					User user = JsonConvert.DeserializeObject<User>(receivedMessage)!;

					Console.WriteLine($"Meddelande från klient: Användarnamn: {user.Username}, Lösenord: {user.Password}");

					// Skicka svar tillbaka till klienten
					string responseMessage = JsonConvert.SerializeObject(user);
					buffer = Encoding.UTF8.GetBytes(responseMessage);
					await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
				}
				else if(result.MessageType == WebSocketMessageType.Close)
				{
					Console.WriteLine("Socket closed..");
					break;
				}
				
			}
		}
		catch (Exception e)
		{
			Console.WriteLine($"Fel vid WebSocket-hantering: {e.Message}");
		}
		finally
		{
			webSocket.Dispose();
		}
	}
}
