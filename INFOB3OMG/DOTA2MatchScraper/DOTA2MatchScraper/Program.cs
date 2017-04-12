using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace DOTA2MatchScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            // Counter for the total amount of matches and the amount of rejected matches.
            ulong numberMatches = 0;
            ulong numberRejectedMatches = 0;

            // An array to store the allowed gamemodes of a match.
            int[] allowedGamemodes = new int[7] { 1, 2, 3, 4, 5, 12, 16 };

            // Use a streamreader to read te file.
            using (FileStream fs = File.Open(@"D:\Downloads\matches\dumpAll.json",
                FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (BufferedStream bs = new BufferedStream(fs))
            using (StreamReader sr = new StreamReader(bs))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    // Up the number of matches present in the file.
                    numberMatches++;

                    // Return the data from the string by parsing it into a JToken type.
                    // This is useful, since we don't have to deserialize the whole json
                    // string to work with the variables we need.
                    JToken data = JObject.Parse(line);

                    #region CheckAmountPLayers
                    // Get the amount of human players in a game, and make sure
                    // there are 10 human players in a match.
                    int amountPlayers = (int)data.SelectToken("human_players");
                    if (amountPlayers != 10)
                    {
                        numberRejectedMatches++;
                        continue;
                    }
                    #endregion

                    #region CheckGameMode
                    // Check if the gamemode is allowed or not.
                    // If not, reject the match.
                    int gameMode = (int)data.SelectToken("game_mode");
                    if (!allowedGamemodes.Contains(gameMode))
                    {
                        numberRejectedMatches++;
                        continue;
                    }
                    #endregion

                    // A boolean to end looping if a player condition is violated.
                    bool flag = true;

                    // Since the players are nested in an array, we have to delve
                    // deeper into the tokens to retrieve this data for each individual player.
                    JToken players = data.SelectToken("players");
                    foreach (JToken player in players)
                    {
                        #region Check leavestatus OR anonymous player
                        // The leave status of a player.
                        // We make sure that no player has left the match in any circumstance.
                        int leaverStatus = (int)player.SelectToken("leaver_status");

                        // The account id of a certain player. If the player has chosen to be anonymous,
                        // It will have a special id: "4294967295". We will use this to get a median of anonymous
                        // players, that will serve as a measuring bar to non anonymous players.
                        long accountID = (long)player.SelectToken("account_id");

                        if (leaverStatus != 0 || accountID == 4294967295)
                        {
                            numberRejectedMatches++;
                            flag = false;
                            break;
                        }
                        #endregion
                    }

                    // We check if the flag has been set on false.
                    // If so, we don't take this match into account.
                    if (!flag)
                        continue;

                    // If the match passed all the tests, write it to the output file.
                    using (System.IO.StreamWriter file =
                        new System.IO.StreamWriter(@"D:\Downloads\matches\filteredAll.json", true))
                    {
                        file.WriteLine(line);
                    }
                }
            }

            // Write the output to the console.s
            Console.WriteLine("The total amount of matches is: " + numberMatches);
            Console.WriteLine("The amount of rejected matches is: " + numberRejectedMatches);
            Console.ReadKey();
        }
    }
}
