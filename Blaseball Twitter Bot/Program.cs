using System;
using System.Net;
using System.IO;
using System.Threading;
using Tweetinvi;
using System.Threading.Tasks;
using System.Diagnostics;
using Tweetinvi.Exceptions;

namespace Blaseball_Twitter_Bot
{// finding the right game seems very broken
    class Program
    {
        static async Task Main()
        {
            GameSetup(); // temporary so I dont auth every test session
           // await TwitterAuth();
        }

        static async Task GameSetup()
        {
            string downloadedsimData = "";
            string configname = "targetteam.txt";
            int playfinderindex = 0;
            var client = new WebClient();
            string[] simDataDaySplit = new[] { "" };
            string[] simDataSeasonSplit = new[] { "" };
            int gameday = 13;
            int season = 13;

            Thread.Sleep(100); // sleep to make sure the config gets made
            string[] configtext = File.ReadAllLines($"{configname}");
            Console.WriteLine("Press enter to start searching for the inputted team's current game.");
            Console.ReadKey();
            Console.WriteLine("Starting search...");
            // find current day of the season
            downloadedsimData = client.DownloadString("https://www.blaseball.com/database/simulationData");
            /*      simDataDaySplit = downloadedsimData.Split("\"day\":");
                  string[] simdaysplit = simDataDaySplit[1].Split(",");
                  gameday = int.Parse(simdaysplit[0]);
                  // end day getter
                  // find current season of the season
                  simDataSeasonSplit = downloadedsimData.Split("\"season\":");
                  string[] simseasonsplit = simDataSeasonSplit[1].Split(",");
                  season = int.Parse(simseasonsplit[0]); */
            // end season getter
            Console.WriteLine($"Day: {gameday}\nSeason: {season}");
            client.Proxy = null;
            string targeturl = $"https://www.blaseball.com/database/games?day={gameday}&season={season}";
            string walter = client.DownloadString(targeturl);
            string desiredteam = $"{configtext[0].Trim()}";
            string[] findingteaminfo = walter.Split($"\"homeTeamNickname\":");
               string gameinfo = findingteaminfo[0];
            string[] findingid = findingteaminfo[0].Split("\"id\":\"");
            findingid = findingteaminfo[0].Split("\"isTitleMatch\"");
        tryidagain:
            
            if (playfinderindex < findingid.Length - 1) // tries to find the string that contains the team name as well as the game id. probably not necessary but it doesn't break anything
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
            //     int similaritycounter = 0;
            //   int gameendchecker = 0;
            string hometeam = "";
            string awayteam = "";
            float homescore = 0f;
            float awayscore = 0f;
            int currentinning = 0;
            bool gamecomplete = false;
            string targeturl = $"https://www.blaseball.com/database/gameById/" + $"{gameid[0]}";
            var eventclient = new WebClient();
            string homePitcher = "";
            string awayPitcher = "";
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
            hometeam = hometeam.Split("\"")[1];
            //home team found, now find away team
            string[] awayteamsplit = gamebyidlog.Split("\"awayTeamName\":");
            awayteam = awayteamsplit[1].Split(",")[0];
            awayteam = awayteam.Split("\"")[1];
            //away team found
            // find home score
            string[] homescoresplit = gamebyidlog.Split("\"homeScore\":");
            homescore = int.Parse(homescoresplit[1].Split(",")[0]);
            homescore = (float)Math.Round((decimal)homescore, 1);
            // home score found, now find away score
            string[] awayscoresplit = gamebyidlog.Split("\"awayScore\":");
            awayscore = int.Parse(awayscoresplit[1].Split(",")[0]);
            awayscore = (float)Math.Round((decimal)awayscore, 1);
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
            // start pitching finders
            string[] homePitcherSplit = gamebyidlog.Split("\"homePitcherName\":");
            homePitcher = homePitcherSplit[1].Split(",")[0];
            homePitcher = homePitcher.Split("\"")[1];

            string[] awayPitcherSplit = gamebyidlog.Split("\"awayPitcherName\":");
            awayPitcher = awayPitcherSplit[1].Split(",")[0];
            awayPitcher = awayPitcher.Split("\"")[1];
            // end pitching finders
            Console.WriteLine($"Home: {hometeam}\nAway: {awayteam}\nHome Score: {homescore}\nAway Score: {awayscore}\nHome Pitcher: {homePitcher}\nAway Pitcher: {awayPitcher}");

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
                    StringMaker(currentinning, lastevent[0], hometeam, awayteam, homescore, awayscore, gamecomplete, awayPitcher, homePitcher);
                    goto anotherone;
                }
            }
            else
            {
                /*          if (endtimequestion.Contains("end"))
                          {
                              Console.WriteLine("Returning to start.");
                              Console.WriteLine();
                              Main();
                          } */
                StringMaker(currentinning, lastevent[0], hometeam, awayteam, homescore, awayscore, gamecomplete, awayPitcher, homePitcher);
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

        static void StringMaker(int inning, string lastevent, string hometeam, string awayteam, float homescore, float awayscore, bool gamecomplete, string awaypitcher, string homepitcher)
        {
            string tweet = "";
            string desiredteam = File.ReadAllText($"targetteam.txt").Trim();
            bool isDesiredTeamHome = false;
            lastevent = "outing";
            if (desiredteam == hometeam)
            {
                isDesiredTeamHome = true;
            }
            if (!gamecomplete)
            {
                if (lastevent.ToLower().Contains("outing"))
                {
                    if (desiredteam.Contains("Mexico City Wild Wings")) // special stuff for wings Twitter
                    {
                        if (isDesiredTeamHome)
                        {
                            if (homescore > awayscore) // if wings are home and winning
                            {
                                tweet = $"Inning {inning} is now an outing. Wings lead the {awayteam} by {homescore - awayscore}!";
                            }
                            else if (awayscore > homescore) // if wings are home and losing
                            {
                                tweet = $"Inning {inning} is now an outing. {awayteam} lead the Wings by {awayscore - homescore}.";
                            }
                            else if (awayscore == homescore && isDesiredTeamHome) // if wings home and tied
                            {
                                tweet = $"Inning {inning} is now an outing. Wings and {awayteam} tied at {awayscore}!";
                            }
                        }
                        else if (!isDesiredTeamHome)
                        {
                            if (awayscore > homescore) // if wings are away and winning
                            {
                                tweet = $"Inning {inning} is now an outing. Wings lead the {hometeam} by {awayscore - homescore}.";
                            }
                            else if (homescore > awayscore) // if wings are away and losing
                            {
                                tweet = $"Inning {inning} is now an outing. Wings trail the {hometeam} by {homescore - awayscore}.";
                            }
                            else if (awayscore == homescore) // if wings away and tied
                            {
                                tweet = $"Inning {inning} is now an outing. Wings and {hometeam} tied at {awayscore}!";
                            }
                        }
                    }
                    else // outings for non wings teams
                    {
                        if (homescore > awayscore)
                        {
                            tweet = $"Inning {inning} is now an outing. {hometeam} leads {awayteam} by {homescore - awayscore}!";
                        }
                        else if (awayscore > homescore)
                        {
                            tweet = $"Inning {inning} is now an outing. {awayteam} leads {hometeam} by {awayscore - homescore}!";
                        }
                        else
                        {
                            tweet = $"Inning {inning} is now an outing. {awayteam} and {hometeam} tied at {awayscore}!";
                        }
                    }
                }
                else if (lastevent.ToLower().Contains("")) // template for moar events
                {

                }

            }
            else // if game is complete, does this stuff
            {
                if (inning == 9)
                {
                    if (desiredteam.Contains("Mexico City Wild Wings")) // special case for wings Bot, feel free to customize
                    {
                        if (isDesiredTeamHome)
                        {
                            if (homescore > awayscore) // if wings win and are home
                            {
                                tweet = $"WINGS WIN!\nFinal score:\nWings ({homepitcher}): {homescore}\n{awayteam} ({awaypitcher}): {awayscore}"; // could add how good our pitcher did or something here
                            }
                            else if (homescore < awayscore) // if wings lose and are home
                            {
                                tweet = $"Wings lose to the {awayteam}. \nFinal score:\nWings ({homepitcher}): {homescore}\n{awayteam} ({awaypitcher}): {awayscore}";
                            }
                        }
                        else if (!isDesiredTeamHome)
                        {
                            if (awayscore > homescore) // if wings win while away
                            {
                                tweet = $"WINGS WIN! \nFinal score:\nWings ({awaypitcher}): {awayscore}\n{hometeam} ({homepitcher}): {homescore}";
                            }
                            else if (awayscore < homescore) // if wings lose while away
                            {
                                tweet = $"Wings lose to the {hometeam}. \nFinal score:\nWings ({awaypitcher}): {awayscore}\n{hometeam} ({homepitcher}):{homescore}";
                            }
                        }
                    }
                    else
                    {
                        if (awayscore > homescore)
                        {
                            tweet = $"{awayteam} win over {hometeam} with a final score of {awayscore} to {homescore}!";
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

                }
                else if (inning > 9)
                {
                    if (isDesiredTeamHome)
                    {
                        if (homescore > awayscore) // wings win at home during extra innings
                        {
                            tweet = $"WINGS WINNN in {inning} innings over the {awayteam}. \nFinal score:\nWings:{homescore}\n{awayteam}:{awayscore}";
                        }
                        else if (homescore < awayscore) // wings lose at home during extra innings
                        {
                            tweet = $"Wings lose to the {awayteam} in {inning} innings. \nFinal score:\nWings:{homescore}\n{awayteam}:{awayscore}";
                        }
                    }
                    else if (!isDesiredTeamHome)
                    {
                        if (awayscore > homescore) // wings win as away team in extra innings
                        {
                            tweet = $"WINGS WIN in {inning} innings over the {hometeam}. \nFinal score:\nWings:{awayscore}\n{hometeam}:{homescore}";
                        }
                        else if (awayscore < homescore) // wings lose as away in extra innings
                        {
                            tweet = $"Wings lose to the {awayteam} in {inning} innings. \nFinal score:\nWings:{awayscore}\n{hometeam}:{homescore}";
                        }
                    }
                    else // general OT win messages if not a wings team
                    {
                        if (awayscore > homescore)
                        {
                            tweet = $"{awayteam} win over {hometeam} in {inning} innings with a final score of {awayscore} to {homescore}!";
                        }
                        else if (homescore > awayscore)
                        {
                            tweet = $"{hometeam} win over {awayteam} in {inning} innings with a final score of {homescore} to {awayscore}!";
                        }
                        else
                        {
                            tweet = $"[ Bot attempted to post a game that ended in a tie so it's posting this tweet instead. ]";
                        }

                    }

                } 
            }
            if (!tweet.Equals(""))
            {
                Tweeter(tweet);
            }
            if (gamecomplete)
            {
                AwaitTopOfHourAfterGame();
            }
            Console.WriteLine(tweet);
            Console.WriteLine(gamecomplete);
        }

        string GetPlayersByID(string id)
        {

            return "";
        }

        public static void AwaitTopOfHourAfterGame()
        {
            Console.WriteLine("Awaiting top of the hour...");
        anotherone:
            DateTime time = DateTime.Now;
            int currentminute = time.Minute;
            if (currentminute != 0)
            {
                goto anotherone;
            }
            GameSetup();
        }

        static async Task TwitterAuth()
        {
                string consumerkey = File.ReadAllText("secure/consumerkey.txt").Trim();
                string consumerkeysecret = File.ReadAllText("secure/consumersecretkey.txt").Trim();
                var appClient = new TwitterClient($"{consumerkey}", $"{consumerkeysecret}");
                var authenticationRequest = await appClient.Auth.RequestAuthenticationUrlAsync();
                Process.Start(new ProcessStartInfo(authenticationRequest.AuthorizationURL)
                {
                    UseShellExecute = true
                });
                Console.WriteLine("Please enter the PIN from the URL on the target account.");
            File.WriteAllText("secure/pin.txt", Console.ReadLine());
             string pinCode = File.ReadAllText("secure/pin.txt");
                var userCredentials = await appClient.Auth.RequestCredentialsFromVerifierCodeAsync(pinCode, authenticationRequest);
                File.WriteAllText("secure/usercredentials.txt", $"{userCredentials.AccessToken}\n{userCredentials.AccessTokenSecret}");
            GameSetup();
        }
        static async Task Tweeter(string tweet)
        {
            string consumerkey = File.ReadAllText("secure/consumerkey.txt").Trim();
            string consumerkeysecret = File.ReadAllText("secure/consumersecretkey.txt").Trim();
            string token = File.ReadAllText("secure/token.txt").Trim();
            string tokensecret = File.ReadAllText("secure/tokensecret.txt").Trim();
            Console.WriteLine("Prolly about to post tweet wanna do that?");
            if (Console.ReadLine().ToLower().Contains("yes"))
            {
                string pinCode = "";
                // Create a client for your app
                var appClient = new TwitterClient($"{consumerkey}", $"{consumerkeysecret}");

                // Start the authentication process
                var authenticationRequest = await appClient.Auth.RequestAuthenticationUrlAsync();


                // Go to the URL so that Twitter authenticates the user and gives him a PIN code.
                /*      if (File.Exists("secure/pin.txt"))
                      {
                          if (1 == 1)
                          {
                              Process.Start(new ProcessStartInfo(authenticationRequest.AuthorizationURL)
                              {
                                  UseShellExecute = true
                              });
                              Console.WriteLine("Please enter the code and press enter.");
                              pinCode = Console.ReadLine();
                              File.WriteAllText("secure/pin.txt", pinCode);
                          }
                          /*       else
                                 {
                                     pinCode = File.ReadAllText("secure/pin.txt");
                                 }

                             }
                             else
                             {
                                 Process.Start(new ProcessStartInfo(authenticationRequest.AuthorizationURL)
                                 {
                                     UseShellExecute = true
                                 });
                                 Console.WriteLine("Please enter pin code and hit enter.");
                                 pinCode = Console.ReadLine();
                                 File.WriteAllText("secure/pin.txt", $"{pinCode}");
                             } */
                pinCode = File.ReadAllText("secure/pin.txt");
                token = File.ReadAllLines("secure/usercredentials.txt")[0];
                tokensecret = File.ReadAllLines("secure/usercredentials.txt")[1];
                var userClient = new TwitterClient($"{consumerkey}", $"{consumerkeysecret}", $"{token}", $"{tokensecret}");
                var user = await userClient.Users.GetAuthenticatedUserAsync();

                Console.WriteLine("Congratulation you have authenticated the user: " + user);

                try
                {
                    await userClient.Tweets.PublishTweetAsync($"{tweet}");
                }
                catch (TwitterException e)
                {
                    Console.WriteLine(e.ToString());
                    goto continueanyways;
                }
            continueanyways:;
            }
        }
        static void CreateConfig(string configname)
        {
            if (!File.Exists(configname))
            {
                Console.WriteLine("Config file not found. Creating new config file...");
                File.WriteAllText(configname, "Mexico City Wild Wings");
                Console.WriteLine();
                Console.WriteLine("Config file created. Please enter the exact team name of the team you want to find on the first line.");
            }
            if (!File.Exists("secure/consumerkey.txt"))
            {
                Directory.CreateDirectory("secure");
                File.Create("secure/consumerkey.txt").Close();
                Console.WriteLine();
                Console.WriteLine("Twitter consumer key config file created. Please enter the consumer key of the Twitter app.");
            }
            if (!File.Exists("secure/consumersecretkey.txt"))
            {
                Directory.CreateDirectory("secure");
                File.Create("secure/consumersecretkey.txt").Close();
                Console.WriteLine();
                Console.WriteLine("Twitter secret consumer key config file created. Please enter the secret consumer key of the Twitter app.");
            }
            if (!File.Exists("secure/token.txt"))
            {
                Directory.CreateDirectory("secure");
                File.Create("secure/token.txt").Close();
                Console.WriteLine();
                Console.WriteLine("Twitter token config file created. prolly dont need to touch this");
            }
            if (!File.Exists("secure/tokensecret.txt"))
            {
                Directory.CreateDirectory("secure");
                File.Create("secure/tokensecret.txt").Close();
                Console.WriteLine();
                Console.WriteLine("Twitter secret token config file created. prolly dont need to touch this");
            }
            if (!File.Exists("secure/pin.txt"))
            {
                Directory.CreateDirectory("secure");
                File.Create("secure/pin.txt").Close();
                Console.WriteLine();
                Console.WriteLine("OAuth Pin config file created. There is no need to edit this file as it'll be overwritten.");
            }
        }
    }
}



