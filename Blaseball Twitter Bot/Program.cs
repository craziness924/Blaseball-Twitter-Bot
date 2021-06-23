using System;
using System.Net;
using System.IO;
using System.Threading;
using Tweetinvi;
using System.Threading.Tasks;
using System.Diagnostics;
using Tweetinvi.Exceptions;

namespace Blaseball_Twitter_Bot
{
    class Program
    {
        static async Task Main()
        {
            string configname = "targetteam.txt";
            CreateConfig($"{configname}");
            // GameSetup();
            await TwitterAuth();
        }

        static async Task GameSetup()
        {
            string downloadedsimData = "";
            string configname = "targetteam.txt";
            int playfinderindex = 0;
            var client = new WebClient();
            string[] simDataDaySplit = new[] { "" };
            string[] simDataSeasonSplit = new[] { "" };
            int gameday = 0;
            int season = 0;

            Thread.Sleep(100); // sleep to make sure the config gets made
            string[] configtext = File.ReadAllLines($"{configname}");
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
            client.Proxy = null;
            string targeturl = $"https://www.blaseball.com/database/games?day={gameday}&season={season}";
            string walter = client.DownloadString(targeturl);
            string desiredteam = $"{configtext[0].Trim()}";
            //  tryidagain:
            string[] findingid = walter.Split("\"id\":\"");
            playfinderindex = FindIDOfGame(findingid, desiredteam);

            //        IDfound:

            string[] gameid = findingid[playfinderindex].Split("\",");

            Console.WriteLine($"Id: {gameid[0]}\nTarget Team: {desiredteam}");
            //     Console.WriteLine($"Start finding latest events from game {gameid[0]}? Type 'end' when the game is over.");
            //     string startquestion = Console.ReadLine().ToLower();
            //     if (startquestion.Contains("yes"))
            //   {
            //  }
            /*       else
                   {
                       Console.WriteLine("Press any key to exit..");
                   } */
            LiveGame(gameid);
        }
        static int FindIDOfGame(string[] findingid, string desiredteam)
        {
            int index = 0;
        tryagain:
            if (!findingid[index].Contains($"{desiredteam}") && index < findingid.Length - 1)
            {
                Console.WriteLine(index);
                index++;
                goto tryagain;
            }
            else if (findingid[index].Contains($"{desiredteam}"))
            {
                goto good;
            }
        good:
            return index;
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
            string scorer = "";
            string desiredteam = File.ReadAllText($"targetteam.txt").Trim();
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
            string[] scoreUpdate = gamebyidlog.Split("\"scoreUpdate\":");
            scoreUpdate = scoreUpdate[1].Split(",");
            bool scoreOnPlay = false;
            if (scoreUpdate[0].ToLower().Contains("Run".ToLower()))
            {
                scoreOnPlay = true;
                scorer = lastevent[0].Split("hits")[0];
            }
            if (hometeam.ToLower().Contains($"{desiredteam}".ToLower()) || awayteam.ToLower().Contains($"{desiredteam}".ToLower()))
            {
                if (!gamecomplete)
                {
                    if (lastevent[0].ToLower().Equals(lasteventsimilaritycheck.ToLower()))
                    {
                        // Console.WriteLine("Last event and current event are the same! Skipping.");
                        //  similaritycounter++;
                        Thread.Sleep(1800);

                        goto anotherone;

                    }
                    else
                    {

                        lasteventsimilaritycheck = lastevent[0];
                        Console.WriteLine($"{lastevent[0]}");
                        StringMaker(currentinning, lastevent[0], hometeam, awayteam, homescore, awayscore, gamecomplete, awayPitcher, homePitcher, scoreOnPlay, scorer);
                        Thread.Sleep(2300);
                        goto anotherone;
                    }
                }
                if (gamecomplete)
                {
                    StringMaker(currentinning, lastevent[0], hometeam, awayteam, homescore, awayscore, gamecomplete, awayPitcher, homePitcher, scoreOnPlay, scorer);
                }
            }
            else
            {
                Console.WriteLine("uhhhh cant find the team for the given day, sleeping until next hour.");
                AwaitTopOfHourAfterGame();
            }
        }

        static void StringMaker(int inning, string lastevent, string hometeam, string awayteam, float homescore, float awayscore, bool gamecomplete, string awaypitcher, string homepitcher, bool scoreOnPlay, string scorer)
        {
            string tweet = "";
            string desiredteam = File.ReadAllText($"targetteam.txt").Trim();
            bool isDesiredTeamHome = false;
            if (desiredteam == hometeam)
            {
                isDesiredTeamHome = true;
            }
            else
            {
                isDesiredTeamHome = false;
            }

            if (!gamecomplete)
            {
                if (lastevent.ToLower().Contains("Outing".ToLower()))
                {
                    if (desiredteam.ToLower().Contains("Mexico City Wild Wings".ToLower())) // special stuff for wings Twitter
                    {
                        if (isDesiredTeamHome)
                        {
                            if (homescore > awayscore) // if wings are home and winning
                            {
                                tweet = $"Inning {inning} is now an Outing. Wings lead the {awayteam} by {homescore - awayscore}! {homescore}-{awayscore}";
                                goto tweet;
                            }
                            else if (awayscore > homescore) // if wings are home and losing
                            {
                                tweet = $"Inning {inning} is now an Outing. Wings trail the {awayteam} by {awayscore - homescore}. {homescore}-{awayscore}";
                                goto tweet;
                            }
                            else if (awayscore == homescore) // if wings home and tied
                            {
                                tweet = $"Inning {inning} is now an Outing. Wings and {awayteam} tied at {awayscore}!";
                                goto tweet;
                            }
                        }
                        else if (!isDesiredTeamHome)
                        {
                            if (awayscore > homescore) // if wings are away and winning
                            {
                                tweet = $"Inning {inning} is now an Outing. Wings lead the {hometeam} by {awayscore - homescore}! {awayscore}-{homescore}";
                                goto tweet;
                            }
                            else if (homescore > awayscore) // if wings are away and losing
                            {
                                tweet = $"Inning {inning} is now an Outing. Wings trail the {hometeam} by {homescore - awayscore}. {awayscore}-{homescore}";
                                goto tweet;
                            }
                            else if (awayscore == homescore) // if wings away and tied
                            {
                                tweet = $"Inning {inning} is now an Outing. Wings and {hometeam} tied at {awayscore}!";
                                goto tweet;
                            }
                        }
                    }
                    else // outings for non wings teams
                    {
                        if (homescore > awayscore)
                        {
                            tweet = $"Inning {inning} is now an Outing. {hometeam} leads {awayteam} by {homescore - awayscore}! {homescore}-{awayscore}";
                            goto tweet;
                        }
                        else if (awayscore > homescore)
                        {
                            tweet = $"Inning {inning} is now an Outing. {awayteam} leads {hometeam} by {awayscore - homescore}! {homescore}-{awayscore}";
                            goto tweet;
                        }
                        else
                        {
                            tweet = $"Inning {inning} is now an Outing. {awayteam} and {hometeam} tied at {awayscore}!";
                            goto tweet;
                        }
                    }
                }
                /*    else if (lastevent.ToLower().Contains("")) // template for moar events
                    {

                    } */
                else if (lastevent.ToLower().Contains("consumers")) 
                {
                    Random rng = new Random();
                    tweet = $"{lastevent}";
                    int blahajcheck = rng.Next(20);
                    if (blahajcheck == 1)
                    {
                        tweet = $"spinny https://media1.tenor.com/images/08ba0788f89138946d58a7544a9c192c/tenor.gif?itemid=18118200";
                    }
                    else
                    {
                        tweet = $"{lastevent}";
                    }
                    goto tweet;
                }
                else if (lastevent.ToLower().Contains("incinerated"))
                {
                    tweet = $"{lastevent} RIV.";
                    goto tweet;
                }
                else if (lastevent.ToLower().Contains("observed"))
                {
                    tweet = $"{lastevent}";
                    goto tweet;
                }
                if (scoreOnPlay) // do these things if there is a run scored
                {
                    if (desiredteam.ToLower().Contains("Mexico City Wild Wings".ToLower())) // Feel free to customize for other teams
                    {
                        if (isDesiredTeamHome)
                        {
                            tweet = $"{lastevent} \n\nWings: {homescore}\n{awayteam}: {awayscore}";
                            goto tweet;
                        }
                        else if (!isDesiredTeamHome)
                        {
                            tweet = $"{lastevent} \n\nWings: {awayscore}\n{hometeam}: {homescore}";
                            goto tweet;
                        }
                    }
                    else
                    {
                        if (isDesiredTeamHome)
                        {
                            tweet = $"{lastevent} \n\n{hometeam}: {homescore}\n{awayteam}: {awayscore}";
                            goto tweet;
                        }
                        else
                        {

                            tweet = $"{lastevent} \n\n{awayteam}: {awayscore}\n{hometeam}: {homescore}";
                            goto tweet;
                        }
                    }


                }
                else
                {
                    goto done;
                }
            }
            if (gamecomplete) // if game is complete, does this stuff
            {
                if (inning == 9)
                {
                    if (desiredteam.ToLower().Contains("Mexico City Wild Wings".ToLower())) // special case for wings Bot, feel free to customize
                    {
                        if (isDesiredTeamHome)
                        {
                            if (homescore > awayscore) // if wings win and are home
                            {
                                tweet = $"WINGS WIN!\n\nFinal score:\n\nWings ({homepitcher}): {homescore}\n{awayteam} ({awaypitcher}): {awayscore}"; // could add how good our pitcher did or something here
                            }
                            else if (homescore < awayscore) // if wings lose and are home
                            {
                                tweet = $"Wings lose to the {awayteam}. \n\nFinal score:\n\nWings ({homepitcher}): {homescore}\n{awayteam} ({awaypitcher}): {awayscore}";
                            }
                        }
                        else if (!isDesiredTeamHome)
                        {
                            if (awayscore > homescore) // if wings win while away
                            {
                                tweet = $"WINGS WIN! \n\nFinal score:\n\nWings ({awaypitcher}): {awayscore}\n{hometeam} ({homepitcher}): {homescore}";
                            }
                            else if (awayscore < homescore) // if wings lose while away
                            {
                                tweet = $"Wings lose to the {hometeam}. \n\nFinal score:\n\nWings ({awaypitcher}): {awayscore}\n{hometeam} ({homepitcher}): {homescore}";
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
                            tweet = $"WINGS WIN in {inning} innings over the {awayteam}! \n\nFinal score:\n\nWings: {homescore}\n{awayteam}: {awayscore}";
                        }
                        else if (homescore < awayscore) // wings lose at home during extra innings
                        {
                            tweet = $"Wings lose to the {awayteam} in {inning} innings. \n\nFinal score:\n\nWings: {homescore}\n{awayteam}: {awayscore}";
                        }
                    }
                    else if (!isDesiredTeamHome)
                    {
                        if (awayscore > homescore) // wings win as away team in extra innings
                        {
                            tweet = $"WINGS WIN in {inning} innings over the {hometeam}! \n\nFinal score:\nWings: {awayscore}\n{hometeam}: {homescore}";
                        }
                        else if (awayscore < homescore) // wings lose as away in extra innings
                        {
                            tweet = $"Wings lose to the {awayteam} in {inning} innings. \n\nFinal score:\nWings: {awayscore}\n{hometeam}: {homescore}";
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
        tweet:
            Thread.Sleep(100);
            if (!tweet.Equals(""))
            {
                tweet = tweet.Replace("\\n", " ");
                Tweeter(tweet);
            }
            if (gamecomplete)
            {
                AwaitTopOfHourAfterGame();
            }
        done:;
            Thread.Sleep(1000);
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
            int currentsecond = time.Second;
            Thread.Sleep(500);
            if (!(currentminute == 0))
            {
                Thread.Sleep(5000);
                goto anotherone;
            }
            else
            {
                GameSetup();
            }
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
            Console.Clear();
            Thread.Sleep(5);
            Console.WriteLine("Would you like to wait until the next hour? Type yes if you would like to.");
            if (Console.ReadLine().ToLower().Contains("yes"))
            {
                AwaitTopOfHourAfterGame();
            }
            else
            {
                GameSetup();
            }

        }
        static async Task Tweeter(string tweet)
        {
            string consumerkey = File.ReadAllText("secure/consumerkey.txt").Trim();
            string consumerkeysecret = File.ReadAllText("secure/consumersecretkey.txt").Trim();
            string token = File.ReadAllText("secure/token.txt").Trim();
            string tokensecret = File.ReadAllText("secure/tokensecret.txt").Trim();
            string pinCode = "";
            // Create a client for your app
            var appClient = new TwitterClient($"{consumerkey}", $"{consumerkeysecret}");

            // Start the authentication process
            var authenticationRequest = await appClient.Auth.RequestAuthenticationUrlAsync();
            pinCode = File.ReadAllText("secure/pin.txt");
            token = File.ReadAllLines("secure/usercredentials.txt")[0];
            tokensecret = File.ReadAllLines("secure/usercredentials.txt")[1];
            var userClient = new TwitterClient($"{consumerkey}", $"{consumerkeysecret}", $"{token}", $"{tokensecret}");
            Console.WriteLine($"\nSending following tweet: {tweet}");
            var user = await userClient.Users.GetAuthenticatedUserAsync();
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



