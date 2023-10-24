using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System.Security.Cryptography;
using TempGenerator.Classes;

class Program
{
    // En statisk variabel som behåller anslutningen mellan SignalR-hubben och console.
    public static HubConnection? connection;

    // Hårdkodade krypteringsnyckel och initialiseringsvektor värde (IV).
    private static byte[] EncryptionKey = new byte[]
    {
        0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0,
        0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0
    };
    private static byte[] iv = new byte[16];

    static async Task Main(string[] args)
    {
        // instans av HubHandler som hanterar kommunikation med SignalR-hub.
        HubHandler handler = new HubHandler();

        // initialiserar anslutningen till hub.
        await handler.InitialyzeAsync();
        // Sänder simulerad tempdata till hub kontinuerligt.
        await handler.SendTempAsync();
    }

    public class HubHandler
    {
        // HubConnection, anslutningen till SignalR-hubben.
        HubConnection hubConn = null!;

        public HubHandler()
        {
            // Bygger anslutningen till SignalR-hubben med en URL och använder WebSockets.
            hubConn = new HubConnectionBuilder().WithUrl("https://localhost:7045/TemperatureHub", HttpTransportType.WebSockets).Build();
        }

        public async Task InitialyzeAsync()
        {
            //ansluter till hub.
            await hubConn.StartAsync();
        }

        public async Task SendTempAsync()
        {
            // Loopar för att regelbundet skicka tempdata.
            while (true)
            {
                var random = new Random();

                // Skapar ett slumpmässigt TemperatureC-värde.
                int temperatureValue = random.Next(-25, 40);

                // Använder funktionen för att bestämma Summary baserat på TemperatureC.
                string summaryValue = GetSummaryBasedOnTemperature(temperatureValue);

                // Skapar ett simulerat väderprognosobjekt.
                WeatherForecast forecast = new()
                {
                    Date = DateTime.Now,
                    TemperatureC = temperatureValue,
                    Summary = summaryValue
                };

                // Serialiserar väderprognosobjektet till en JSON-sträng.
                string json = JsonConvert.SerializeObject(forecast);

                // Krypterar JSON-strängen med hjälp av den givna nyckeln och IV:n.
                string encryptedData = Encrypt(json, EncryptionKey, iv);

                // Sänder den krypterade datan till SignalR-hubben.
                await hubConn.SendAsync("ReceiveTempertureData", encryptedData);

                // Väntar i 7 sekunder innan nästa sändning.
                await Task.Delay(TimeSpan.FromSeconds(7));
            }
        }

        // Funktion för att bestämma Summary-texten baserat på TemperatureC.
        private string GetSummaryBasedOnTemperature(int temperatureC)
        {
            if (temperatureC < 0)
            {
                return "Cold Weather";
            }
            else if (temperatureC < 10)
            {
                return "Chilly Weather";
            }
            else if (temperatureC < 20)
            {
                return "Moderate Weather";
            }
            else if (temperatureC < 30)
            {
                return "Warm Weather";
            }
            else
            {
                return "Hot Weather";
            }
        }


        // En hjälpmetod för att kryptera strängar med Aes-kryptering.
        private static string Encrypt(string temperature, byte[] key, byte[] iv)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        // Skriver in temperaturen i krypteringsströmmen.
                        sw.Write(temperature);
                    }
                    // Returnerar den krypterade datan som en base64-sträng.
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }
    }
}
