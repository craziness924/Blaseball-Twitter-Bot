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
            string downloadedsimData = "";
            string configname = "targetteam.txt";
            int playfinderindex = 0;
            var client = new WebClient();
            string[] simDataDaySplit = new[] { "" };
            string[] simDataSeasonSplit = new[] { "" };
            int gameday = 0;
            int season = 0;
            //   CreateConfig(configname);
            Thread.Sleep(100); // sleep to make sure the config gets made
            string[] configtext = File.ReadAllLines($"{configname}");
            Console.WriteLine("Press enter to start searching for the inputted team's current game.");
            Console.ReadKey();
            Console.WriteLine("Starting search...");
            // find current day of the season
            downloadedsimData = client.DownloadString("https://www.blaseball.com/database/simulationData");
            simDataDaySplit = downloadedsimData.Split("\"day\":");
            string[] simdaysplit = simDataDaySplit[1].Split(",");
            gameday = int.Parse(simdaysplit[0]);
            // end day getter
            // find current season of the season
            simDataSeasonSplit = downloadedsimData.Split("\"season\":");
            string[] simseasonsplit = simDataSeasonSplit[1].Split(",");
            season = int.Parse(simseasonsplit[0]);

            // end season getter
            Console.WriteLine($"Day: {gameday}\nSeason: {season}");
            Console.ReadKey();
            client.Proxy = null;
            string targeturl = $"https://www.blaseball.com/database/games?day={gameday}&season={season}";
            string walter = client.DownloadString(targeturl);
            string desiredteam = $"{configtext[0].Trim()}";
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
            }


        }
        static void LiveGame(string[] gameid)
        {
            // anotherone:
            string gamebyidlog = "";
            string[] findingteaminfo = new[] { "" };
            string lasteventsimilaritycheck = "";
            string gameinfo = "";
            string[] updatesplit = new[] { "" };
            string[] lastevent = new[] { "" };
            int similaritycounter = 0;
            int gameendchecker = 0;
            string hometeam = "";
            string awayteam = "";
            int homescore = 0;
            int awayscore = 0;
            int currentinning = 0;
            bool gamecomplete = false;
            string targeturl = $"https://www.blaseball.com/database/gameById/" + $"{gameid[0]}";
            var eventclient = new WebClient();
            eventclient.Proxy = null;
        anotherone:
            gamebyidlog = eventclient.DownloadString(targeturl);
            findingteaminfo = gamebyidlog.Split($"\"homeTeamNickname\":");
            gameinfo = findingteaminfo[0];
            updatesplit = gameinfo.Split("\"lastUpdate\":\"");
            lastevent = updatesplit[1].Split("\",\"");
            // find home team
            string[] hometeamsplit = gamebyidlog.Split("\"homeTeamName\":");
            hometeam = hometeamsplit[1].Split(",")[0];
            //home team found, now find away team
            string[] awayteamsplit = gamebyidlog.Split("\"awayTeamName\":");
            awayteam = awayteamsplit[1].Split(",")[0];
            //away team found
            // find home score
            string[] homescoresplit = gamebyidlog.Split("\"homeScore\":");
            homescore = int.Parse(homescoresplit[1].Split(",")[0]);
            // home score found, now find away score
            string[] awayscoresplit = gamebyidlog.Split("\"awayScore\":");
            awayscore = int.Parse(awayscoresplit[1].Split(",")[0]);
            // away score found
            //inning number 
            string[] inningsplit = gamebyidlog.Split("\"inning\":");
            currentinning = int.Parse(inningsplit[1].Split(",")[0]) + 1;
            //inning number found
            // checking game completon
            string[] gamecompletesplit = gamebyidlog.Split("\"gameComplete\":"); //temporairly commented out until live games appear
            gamecomplete = bool.Parse(gamecompletesplit[1].Split(",")[0]);
            // gamecomplete = true; // fake completon state for testing reasons
            // game completion checked
            Console.WriteLine($"Home: {hometeam}\nAway: {awayteam}\nHome Score: {homescore}\nAway Score: {awayscore}");
            Console.ReadKey();
            string endtimequestion = Console.ReadLine().ToLower();
            if (!gamecomplete)
            {
                if (lastevent[0].ToLower().Equals(lasteventsimilaritycheck.ToLower()))
                {
                    Console.WriteLine("Last event and current event are the same! Skipping.");
                    //  similaritycounter++;
                    //   goto skipprint;

                }
                else
                {
                    lasteventsimilaritycheck = lastevent[0];
                    Console.WriteLine($"{lastevent[0]}");
                    Tweeter(currentinning, lastevent[0], hometeam, awayteam, homescore, awayscore, gamecomplete);
                    goto anotherone;
                }
            }
            else
            {
                if (endtimequestion.Contains("end"))
                {
                    Console.WriteLine("Returning to start.");
                    Console.WriteLine();
                    Main();
                }
                Tweeter(currentinning, lastevent[0], hometeam, awayteam, homescore, awayscore, gamecomplete);
                goto anotherone;
            }





            /*    if (lastevent[0].ToLower().Equals(lasteventsimilaritycheck.ToLower()) && !gamecomplete)
                {
                    Console.WriteLine("Last event and current event are the same! Skipping.");
                  //  similaritycounter++;
                    goto skipprint;

                }
                else
                {
                    lasteventsimilaritycheck = lastevent[0];
                    Console.WriteLine($"{lastevent[0]}");
                    EventHandler(currentinning, lastevent[0], hometeam, awayteam, homescore, awayscore);
                }
            skipprint:
                if (endtimequestion.Contains("end"))
                {
                    Console.WriteLine("Returning to start.");
                    Console.WriteLine();
                    Main();
                } */
            /*     else if (similaritycounter > 4) commented out now to test gameCOmpleted bool
              /  {
                //     similaritycounter = 0;
                     Console.WriteLine("Too many similar events, game may have ended. Now only checking for new events every 20 seconds.");
                     goto similarityroutine;
               }
                 else
                 {
                     Thread.Sleep(5000);
                     goto anotherone; 
                 } *?

          /*   similarityroutine:
                 Thread.Sleep(2000);
                 gamebyidlog = eventclient.DownloadString(targeturl);
                 findingteaminfo = gamebyidlog.Split($"\"homeTeamNickname\":");
                 gameinfo = findingteaminfo[0];
                 updatesplit = gameinfo.Split("\"lastUpdate\":\"");
                 lastevent = updatesplit[1].Split("\",\"");

                 if (lastevent[0].ToLower() == lasteventsimilaritycheck.ToLower() && gameendchecker < 2)
                 {
                     Console.WriteLine("New event not found...");
                     gameendchecker++;
                     goto similarityroutine;
                 }
                 else if (gameendchecker >= 2)
                 {
                     Console.WriteLine("Game presumably ended, returning to start.");
                     Console.WriteLine();
                     Main();
                     Thread.Sleep(Timeout.Infinite);
                 }
                 else
                 {
                     goto anotherone;
                 } */
        }

        static void Tweeter(int inning, string lastevent, string hometeam, string awayteam, int homescore, int awayscore, bool gamecomplete)
        {
            string tweet = "";
            if (!gamecomplete)
            {
                if (lastevent.ToLower().Contains(","))
                {
                    if (homescore > awayscore)
                    {
                        tweet = $"Inning {inning} is now an outing. {hometeam} leads {awayteam} by {homescore - awayscore}!";
                    }
                    else if (awayscore > homescore)
                    {
                        tweet = $"Inning {inning} is now an outing. {awayteam} leads {hometeam} by {awayscore - homescore}";
                    }
                    else
                    {
                        tweet = $"Inning {inning} is now an outing. {awayteam} and {hometeam} tied at {awayscore}!";
                    }
                }
                else if (lastevent.Contains("whatever")) ;
                {

                }
            }
            else
            {
                if (awayscore > homescore)
                {
                    tweet = $"{awayteam} win over {hometeam} with a final of {awayscore} to {homescore}!";
                }
                else if (homescore > awayscore)
                {
                    tweet = $"{hometeam} win over {awayteam} with a final score of {homescore} to {awayscore}!";
                }
                else
                {
                    tweet = $"[ Bot attempted to post a game that ended in a tie so it's posting this tweet instead. ]";
                }
            }
            Console.WriteLine(tweet);
            Console.WriteLine(gamecomplete);
        }
        /*
        static void CreateConfig(string configname)
        {
            if (!File.Exists(configname)) ;
            {
                Console.WriteLine("Config file not found or incorrect text. Creating new config file...");
                File.WriteAllText(configname, "Mexico City Wild Wings");
                Console.WriteLine();
                Console.WriteLine("Config file created. Please enter the exact team name of the team you want to find on the first line.");
            }

        } */
    }
}

