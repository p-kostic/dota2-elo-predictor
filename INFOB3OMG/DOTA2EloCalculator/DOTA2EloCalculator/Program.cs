using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Windows.Markup;

namespace DOTA2EloCalculator
{
    class Program
    {
        // A dictionary that stores the Elo rating of a player, using the player as a key.
        static Dictionary<string, int> playerElos = new Dictionary<string, int>();
        static int amountofPlayers = 0;
        static int amountofMatches = 0;

        static void Main(string[] args)
        {
            // Use a streamreader to read te file.
            string path = @"D:\Downloads\matches\filteredAll.json";
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

                    // Initialize the teams and a match
                    Team radiant = new Team();
                    Team dire = new Team();


                    // Since the players are nested in an array, we have to delve
                    // deeper into the tokens to retrieve this data for each individual player.
                    JToken players = data.SelectToken("players");
                    foreach (JToken player in players)
                    {
                        // The account id of a certain player.
                        string accountID = (string)player.SelectToken("account_id");

                        // We try to return to grab the elo rating from the dictionary. If the key
                        // doesn't exist, we make a new entry in the dictionary with a basic elo of 1000.
                        if (!playerElos.ContainsKey(accountID))
                        {
                            playerElos.Add(accountID, 1000);
                            amountofPlayers++;
                        }

                        // The player slot of this specific player. See: https://wiki.teamfortress.com/wiki/WebAPI/GetMatchDetails#Player_Slot for more info on player slots.
                        byte playerSlot = (byte)player.SelectToken("player_slot");

                        // The team the player is on. Since this works in a binary fashion, we have to check if the most significant bit is true.
                        // we then turn the output around, so that true = radiant and false = dire, to avoid confusion with the winning team variable.
                        // We can now check if the player has won if we combine the variables.
                        bool playerTeam = (playerSlot & 128) != 128;
                        bool playerHasWon = (radiantWin && playerTeam) || (!radiantWin && !playerTeam);
                        // int winStatus = Convert.ToInt32(playerHasWon);

                        // Add the player to the corresponding team.
                        if (!playerTeam) // dire
                        {
                            dire.AddPlayer(new Player(accountID, playerElos[accountID]));    // Get the elo from the dictionary
                            dire.Won = playerHasWon;
                        }
                        else            // radiant
                        {
                            radiant.AddPlayer(new Player(accountID, playerElos[accountID])); // Get the elo from the dictionary
                            radiant.Won = playerHasWon;
                        }
                    }
                    amountofMatches++;
                    Match match = new Match(radiant, dire);

                    UpdateElo(match);
                }
            }

            foreach (var entry in playerElos)
            {
                if (entry.Value > 1200) // test om te zien of er wel players zijn die een beetje veel winnen
                Console.WriteLine("id: {0} Elo: {1}", entry.Key, entry.Value);
            }
            Console.WriteLine("Amount of unique players: " + amountofPlayers);
            Console.WriteLine("Amount of matches: " + amountofMatches);
            Console.ReadKey();
        }

        static void UpdateElo(Match match)
        {
            if (match.Radiant.AverageElo == 0.0 || match.Dire.AverageElo == 0.0)
                throw new Exception("Radiant or Dire team do not have an AverageElo assigned, team size not 5");
           
            // Calculate the transformed rating of each team using their average elo
            double transformedRadiant = Math.Pow(10, match.Radiant.AverageElo / 400);
            double transformedDire = Math.Pow(10, match.Dire.AverageElo / 400);

            // Calculate the expected score of each team using their average elo
            double expectedScoreRadiant = transformedRadiant / (transformedRadiant + transformedDire);
            double expectedScoreDire = transformedDire / (transformedRadiant + transformedDire);

            // Calculate the S value for each team
            int s1 = 0;
            int s2 = 0;
            if (match.Radiant.Won && !match.Dire.Won)
                s1 = 1;
            else if (!match.Radiant.Won && match.Dire.Won)
                s2 = 1;
            else throw new Exception("Nobody won?" + match.Dire.Won + match.Radiant.Won);

            // Calculate the updated Elo-rating for each team
            int K = 40;
            double ratingChangeRadiant = K * (s1 - expectedScoreRadiant);
            double ratingChangeDire = K * (s2 - expectedScoreDire);

            // Give each player the rating 
            for (int i = 0; i < 5; i++)
            {
                playerElos[match.Radiant.Players[i].account_id] += (int)ratingChangeRadiant;
                playerElos[match.Dire.Players[i].account_id] += (int)ratingChangeDire;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="a_rating"></param>
        /// <param name="b_rating"></param>
        /// <returns></returns>
        static double WinProb(int a_rating, int b_rating)
        {
            double Ra = Math.Pow(10, a_rating/400);
            double Rb = Math.Pow(10, b_rating/400);
            return Ra/(Ra + Rb);

        }

        /// <summary>
        /// Calculate the standard deviation of the players' ELO rating
        /// TODO: Eigenlijk sample standard deviation gebruiken (1st Answer: http://stackoverflow.com/questions/3141692/standard-deviation-of-generic-list )
        /// </summary>
        /// <returns>The standard deviation of the playerElo Dictionary</returns>
        static double CalculateStandardDeviation()
        {
            double average = CalculateMean();
            return Math.Sqrt(playerElos.Values.Average(v => Math.Pow(v - average, 2)));
        }

        /// <summary>
        /// Calculate the mean (average) ELO of all players
        /// </summary>
        /// <returns>The mean of the playerElo Dictionary</returns>
        static double CalculateMean()
        {
            double average = playerElos.Values.Average();
            return average;
        }

        const string outputfolder = @"D:\Downloads\matches";
        void WriteToFile(string text, string filename)
        {
            string path = string.Format(@"{0}{1}.csv", outputfolder, filename);
            using (StreamWriter sw = new StreamWriter(path, true))
                sw.Write(text);
        }

        // TODO: Accuracy van matches die we gaan predicten uitrekenen
        // TODO: Logaritmic loss: https://www.kaggle.com/wiki/LogLoss ??? en Logarithmic regression voor de paper
        // TODO: Greene, W. H. (1999). Econometric analysis (4th ed.). Upper SaddleRiver, NJ: Prentice Hall.

        // TODO: Welk programma? SPSS? R? Excel? Dafuq
        // TODO: 

        // TODO: Variables voor models: Elo_Diff  The ELO rating difference between the two teams at the start of the match.
        // Calculated by subtracting the away team ELO rating from the home team ELO rating.A
        // positive value means that the home team is deemed stronger, while a negative value
        // means that the away team is deemed stronger


    }
}
