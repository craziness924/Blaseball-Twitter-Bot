# Blaseball-Twitter-Bot
This bot tweets about things that happen to a specific team in Blaseball.

# Installing and using:
0. Install TweetInvi on Nuget or whatever. Also get approved for Twitter API.
1. Build and run the bot to get all the config texts. Ignore the OAuth link for now (if it shows up)
2. Close the program and input your desired team in to "targetteam.txt" on line 1.
3. Get your consumerkey (API Key) and your consumer secret key (API Secret Key) and put it in their respective files in the secure directory.
4. Run bot again and authorize it on the account you want to post to. Enter the pin into the console then click enter.
5. Enter yes if you want to wait for the next hour.
6. Should post tweets now.

# Issues/To Be Added
- Bot will do something funky on Siesta days. Adding check soon.
- Bot misses events quite often, though it seeks to only be missing Outings.
- More events
  - Incinerations
  - Possibly custom phrases depending on who scored
  - Blahaj
