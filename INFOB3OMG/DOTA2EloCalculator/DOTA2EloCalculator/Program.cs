using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace DOTA2EloCalculator
{
    class Program
    {
        static void Main(string[] args)
        {
            // Use a streamreader to read te file.
            using (FileStream fs = File.Open("C:\\Users\\Mark Berentsen\\Documents\\School\\Onderzoeksmethoden Gametechnology\\JsonFiles\\sample1k.json",
                FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
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

                    // Since the players are nested in an array, we have to delve
                    // deeper into the tokens to retrieve this data for each individual player.
                    JToken players = data.SelectToken("players");
                    foreach (JToken player in players)
                    {
                        // The account id of a certain player.
                        long accountID = (long)player.SelectToken("account_id");

                        // The player slot of this specific player. See: https://wiki.teamfortress.com/wiki/WebAPI/GetMatchDetails#Player_Slot for more info on player slots.
                        byte playerSlot = (byte)player.SelectToken("player_slot");

                        // The team the player is on. Since this works in a binary fashion, we have to check if the most significant bit is true.
                        // we then turn the output around, so that true = radiant and false = dire, to avoid confusion with the winning team variable.
                        bool playerTeam = (playerSlot & 128) != 128;

                        // We if the player has one combining the two variables.
                        bool playerHasWon = (radiantWin && playerTeam) || (!radiantWin && !playerTeam);
                    }
                }
            }

            Console.ReadKey();
        }
    }
}
