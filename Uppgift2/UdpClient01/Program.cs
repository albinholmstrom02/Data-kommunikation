using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using UdpClient01.Classes;

class Program
{
	static void Main()
	{
		// Skapa en UDP-klient och definiera serverns IP-adress och portnummer
		using UdpClient udpClient = new UdpClient();
		IPAddress serverIP = IPAddress.Parse("127.0.0.1");
		int serverPort = 5000;

		// Skapa ett användarobjekt som ska skickas till servern
		User user = new User { Username = "Ola111", Password = "abc123" };

		try
		{
			// Serialisera användarobjektet till JSON
			var json = JsonConvert.SerializeObject(user);

			// Konvertera JSON till byte-array
			byte[] data = Encoding.UTF8.GetBytes(json);

			// Skicka data till servern via UDP
			udpClient.Send(data, data.Length, new IPEndPoint(serverIP, serverPort));

			Console.WriteLine("Data skickades till servern: " + json);
		}
		catch (Exception e)
		{
			Console.WriteLine("Fel vid sändning: " + e.Message);
		}
		finally
		{
			udpClient.Close();
		}
	}
}