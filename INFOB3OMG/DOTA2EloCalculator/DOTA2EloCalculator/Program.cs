using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace DOTA2EloCalculator
{
    class Program
    {
        // A dictionary that stores the Elo rating of a player, using the player as a key.
        static Dictionary<string, int> playerElos = new Dictionary<string, int>();

        static void Main(string[] args)
        {
            // Use a streamreader to read te file.
            string path = "C:\\Users\\Mark Berentsen\\Documents\\School\\Onderzoeksmethoden Gametechnology\\JsonFiles\\sample1k.json";
            using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (BufferedStream bs = new BufferedStream(fs))
            using (StreamReader sr = new StreamReader(bs))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    // Return the data from the string by parsing it into a JToken type.
                    // This is useful, since we don't have to deserialize the whole json
                    // string to work with the variables we need.
                    JToken data = JObject.Parse(line);

                    // Which team won. True for radiant, False for Dire.
                    bool radiantWin = (bool)data.SelectToken("radiant_win");

                    // We define a list we can pass to the calculation method so we can update the elo ratings.
                    List<Dictionary<string, int[]>> playerMatchData = new List<Dictionary<string, int[]>>(); 

                    // Since the players are nested in an array, we have to delve
                    // deeper into the tokens to retrieve this data for each individual player.
                    JToken players = data.SelectToken("players");
                    foreach (JToken player in players)
                    {
                        // The account id of a certain player.
                        string accountID = (string)player.SelectToken("account_id");

                        // We try to return to grab the elo rating from the dictionary. If the key
                        // doesn't exist, we make a new entry in the dictionary with a basic elo of 1000.
                        int elo = 0;
                        if (!playerElos.TryGetValue(accountID, out elo))
                        {
                            elo = 1000;
                            playerElos.Add(accountID, elo);
                        }


                        // The player slot of this specific player. See: https://wiki.teamfortress.com/wiki/WebAPI/GetMatchDetails#Player_Slot for more info on player slots.
                        byte playerSlot = (byte)player.SelectToken("player_slot");

                        // The team the player is on. Since this works in a binary fashion, we have to check if the most significant bit is true.
                        // we then turn the output around, so that true = radiant and false = dire, to avoid confusion with the winning team variable.
                        // We can now check if the player has won if we combine the variables.
                        bool playerTeam = (playerSlot & 128) != 128;
                        bool playerHasWon = (radiantWin && playerTeam) || (!radiantWin && !playerTeam);
                        int winStatus = Convert.ToInt32(playerHasWon);

                        Dictionary<string, int[]> playerData = new Dictionary<string, int[]>();
                        playerData.Add(accountID, new int[2] { winStatus, elo });
                        playerMatchData.Add(playerData);
                    }

                    CalculateElo(playerMatchData);
                }
            }

            Console.ReadKey();
        }

        static void CalculateElo(List<Dictionary<string, int[]>> playerData)
        {

        }
    }
}
