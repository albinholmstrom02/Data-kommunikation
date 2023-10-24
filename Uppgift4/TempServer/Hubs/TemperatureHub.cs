using Microsoft.AspNetCore.SignalR;
using System.Security.Cryptography;
using System.Text.Json;
using TempServer.Models;

public class TemperatureHub : Hub
{
    // Hårdkodade krypteringsnyckel och initialiseringsvektor värde (IV).
    private static readonly byte[] EncryptionKey = new byte[]
    {
        0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0,
        0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0
    };
    private static readonly byte[] IV = new byte[16];

    // Loggtjänst som hjälper till med att logga fel och annan information.
    private readonly ILogger<TemperatureHub> _logger;

    // Konstruktor som tar emot loggtjänst.
    public TemperatureHub(ILogger<TemperatureHub> logger)
    {
        _logger = logger;
    }

    // Metoden som kallas av klienter för att sända krypterad temperaturdata.
    public async Task ReceiveTempertureData(string encryptedData)
    {
        try
        {
            // Dekrypterar datan med den givna nyckeln och IV.
            string decryptedData = Decrypt(encryptedData, EncryptionKey, IV);
            // Deserialiserar den dekrypterade datan till ett WeatherForecast objekt.
            WeatherForecast forecast = JsonSerializer.Deserialize<WeatherForecast>(decryptedData)!;
            Console.WriteLine($"Received temperature forecast - Date: {forecast.Date}, Temperature: {forecast.TemperatureC}, Summary: {forecast.Summary}");

            // Sänd den dekrypterade datan till alla anslutna klienter.
            await Clients.All.SendAsync("ReceiveTempertureData", forecast);
        }
        catch (Exception ex)
        {
            // Loggar eventuellt fel.
            _logger.LogError(ex, "Error while processing temperature data.");
            // Informerar den anropande klienten om felet.
            await Clients.Caller.SendAsync("ErrorProcessingData", "An error occurred while processing the data.");
        }
    }

    private string Decrypt(string encryptedText, byte[] key, byte[] iv)
    {
        try
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(encryptedText)))
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (StreamReader sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }
        catch (Exception ex)
        {
            // Logga felet med mer information.
            _logger.LogError(ex, "Error during decryption.");
            // Kasta om felet för att fånga det i den anropande metoden.
            throw;
        }
    }
}
