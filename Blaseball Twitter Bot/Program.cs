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
            Console.WriteLine("Press enter to start searching for the inputted team's current game.");
            Console.WriteLine("Starting search...");
            Console.ReadKey();
            var client = new WebClient();
            client.Proxy = null;
            string targeturl = "https://www.blaseball.com/events/streamData";
            string walter = client.DownloadString(targeturl);
            string[] configtext = File.ReadAllLines($"{configname}");
            string desiredteam = $"{configtext[0]}";
            string[] findingteaminfo = walter.Split($"\"homeTeamNickname\":");
            string gameinfo = findingteaminfo[0];
            string[] findingid = gameinfo.Split("\"id\":\"");
            tryidagain:
            if (playfinderindex < findingid.Length - 1) // tries to find the string that contains the team name as well as a supposed game ID. it should work but idk how blaseball works
            {
                if (!findingid[playfinderindex].Contains($"{desiredteam.ToLower()}"))
                {
                    playfinderindex++;
                    Console.WriteLine($"{playfinderindex}");
                    goto tryidagain;
                }
                else
                {
                    goto IDfound;
                }
            }
           
            IDfound:
            string[] gameid = findingid[playfinderindex].Split("\",");

            Console.WriteLine($"Id: {gameid[0]}\nTarget Team: {desiredteam}");
            Console.WriteLine($"Start finding latest events from game {gameid[0]}? Type 'end' when the game is over.");
            string startquestion = Console.ReadLine().ToLower();
            if (startquestion.Contains("yes"))
            {
                LiveGame(gameid);
                Console.WriteLine("Press enter to start event outputter.");
                Thread.Sleep(Timeout.Infinite);
            }
            else
            {
                Console.WriteLine("Press any key to exit..");
                Console.ReadKey();
            }

            
        }
        static void LiveGame(string[] gameid)
        {
        anotherone:
            string gamebyidlog = "";
            string[] findingteaminfo = new[] {""};
          //  string lasteventsimilaritycheck = "";
            string gameinfo = "";
            string[] updatesplit = new[] { "" };
            string[] lastevent = new[] { "" };


            string targeturl = $"https://www.blaseball.com/database/gameById/" + $"{gameid[0]}";
            var eventclient = new WebClient();
            gamebyidlog = eventclient.DownloadString(targeturl);
            findingteaminfo = gamebyidlog.Split($"\"homeTeamNickname\":");
            gameinfo = findingteaminfo[0];
            updatesplit = gameinfo.Split("\"lastUpdate\":\"");
            lastevent = updatesplit[1].Split("\",\"");
            string endtimequestion = Console.ReadLine().ToLower();
            /*    if (lastevent[0].ToLower().Equals(lasteventsimilaritycheck.ToLower()))
                {
                    Console.WriteLine("Last event and current event are the same! Skipping.");
                    goto skipprint;
                } 
                else
                {
                    Console.WriteLine($"{lastevent[0]}");
                } */
            Console.WriteLine($"{lastevent[0]}");
        skipprint:
            if (endtimequestion.Contains("end"))
            {
                Console.WriteLine("Returning to start.");
                Console.WriteLine();
                Main();
            }
            else
            {
                Thread.Sleep(5000);
                goto anotherone;
            }
            

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
