using System;
using System.Net;
using System.IO;
using System.Threading;

namespace Blaseball_Twitter_Bot
{
    class Program
    {
        static void Main()
        {
            string configname = "targetteam.txt";
            int playfinderindex = 0;
            CreateConfig(configname);
            Console.WriteLine("Press enter to start.");
            Console.ReadKey();
            var client = new WebClient();
            client.Proxy = null;
            string targeturl = "https://www.blaseball.com/events/streamData/";
            string walter = client.DownloadString(targeturl);
            string[] configtext = File.ReadAllLines($"{configname}");
            string desiredteam = $"{configtext[0]}";
            string[] findingteaminfo = walter.Split($"\"homeTeamNickname\":");
            string gameinfo = findingteaminfo[0];
            string[] findinglastplay = gameinfo.Split("\"lastUpdate\":\"");
            
     /*       if (!findinglastplay[1].Contains($"{desiredteam.ToLower()}"))
            {
                playfinderindex++;
                Console.WriteLine($"{playfinderindex}");
                goto tryidagain;
            }
            else
            {
                goto IDfound;
            } */
            IDfound:
            string[] lastevent = findinglastplay[1].Split("\",");

            Console.WriteLine($"Id: {lastevent[0]}\nTarget Team: {desiredteam}");
            Console.ReadKey();
        }
        static void CreateConfig(string configname)
        {
            if (!File.Exists(configname))
            {
                File.Create(configname).Dispose();
                Console.WriteLine("Config file created. Please enter the exact team name of the team you want to find on the first line.");
            }

        }
    }
}
