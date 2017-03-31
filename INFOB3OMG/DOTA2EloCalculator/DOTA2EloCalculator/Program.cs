using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Windows.Markup;

namespace DOTA2EloCalculator
{
    class Program
    {
        // A dictionary that stores information of the player, using the account_id as a key
        static Dictionary<string, Player> playerElos = new Dictionary<string, Player>();

        // Values to track the amount of players, and the amount of matches.
        static int amountofPlayers = 0;
        static int amountofMatches = 0;

        static void Main(string[] args)
        {
            // Use a streamreader to read te file.
            string path = @"D:\Downloads\matches\filteredAll.json";
        
            #region InitializeBacktesting
            // Check the amount of lines, so we can quite reading when needed (used for backtesting).
            long numberOfLines = 0;
            using (FileStream fs1 = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (BufferedStream bs1 = new BufferedStream(fs1))
            using (StreamReader sr = new StreamReader(bs1))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    numberOfLines++;
                }
            }
            #endregion

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
                            playerElos.Add(accountID, new Player(accountID, 1000));
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
                        if (playerTeam) // radiant
                        {
                            radiant.AddPlayer(new Player(accountID, playerElos[accountID].elo)); // Get the elo from the dictionary
                            radiant.Won = playerHasWon;
                        }
                        else           // dire
                        {
                            dire.AddPlayer(new Player(accountID, playerElos[accountID].elo));    // Get the elo from the dictionary
                            dire.Won = playerHasWon;
                        }
                    }

                    // Up the amount of matches played, and add the match info to a new match object.
                    amountofMatches++;
                    Match match = new Match(radiant, dire);

                    // Update the player's amount of matches played
                    UpdatePlayerMatchCount(match);

                    // Write the match info to the logistic.
                    WriteToFileLogistic(match, "statOutcomeFinal");

                    UpdateElo(match);
                }
            }

            foreach (var entry in playerElos)
            {
                if (entry.Value.elo > 1200) // test om te zien of er wel players zijn die een beetje veel winnen
                    Console.WriteLine("id: {0} Elo: {1}", entry.Key, entry.Value.elo);
            }

            Console.WriteLine("Amount of unique players: " + amountofPlayers);
            Console.WriteLine("Amount of matches: " + amountofMatches);
            //double mean = CalculateMean();
            //Console.WriteLine("Mean: " + mean);
            //Console.WriteLine("Variance: " + CalculateVariance(mean));
            //Console.WriteLine("Standard Deviation: " + CalculateStandardDeviation());
            Console.WriteLine("Number of lines: " + numberOfLines);
            //WriteToFileStandardDeviation(CalculateStandardDeviation(), "standardDevTest");
            Console.ReadKey();
        }

        // Calculate and update the elo rating for the teams.
        static void UpdateElo(Match match)
        {
            if (match.Radiant.AverageElo == 0.0 || match.Dire.AverageElo == 0.0)
                throw new Exception("Radiant or Dire team do not have an AverageElo assigned, team size not 5");

            // Calculate the transformed rating of each team using their average elo

            var powerA = (match.Dire.AverageElo - match.Radiant.AverageElo);
            var powerB = (match.Radiant.AverageElo - match.Dire.AverageElo);

            double expectedRadiant = 1 / (1 + Math.Pow(10, powerA / 400));
            double expectedDire = 1 / (1 + Math.Pow(10, powerB / 400));

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
            double ratingChangeRadiant = K * (s1 - expectedRadiant);
            double ratingChangeDire = K * (s2 - expectedDire);

            // Give each player the rating 
            for (int i = 0; i < 5; i++)
            {
                playerElos[match.Radiant.Players[i].account_id].elo += (int)ratingChangeRadiant;
                playerElos[match.Dire.Players[i].account_id].elo += (int)ratingChangeDire;
            }
        }
        /// <summary>
        /// Calculate the mean (average) ELO of all players
        /// </summary>
        /// <returns>The mean of the playerElo Dictionary</returns>
        //static double CalculateMean()
        //{
        //    double average = playerElos.Values.Average();
        //    return average;
        //}

        /// <summary>
        /// Calculate the variance. We use the sample correction for our data.
        /// </summary>
        /// <param name="mean"></param>
        /// <returns></returns>
        //static double CalculateVariance(double mean)
        //{
        //    double sum = playerElos.Values.Sum(v => Math.Pow(v - mean, 2));
        //    double variance = sum / (playerElos.Values.Count - 1);
        //    return variance;
        //}

        /// <summary>
        /// Calculate the standard deviation of the players' ELO rating
        /// TODO: Eigenlijk sample standard deviation gebruiken (1st Answer: http://stackoverflow.com/questions/3141692/standard-deviation-of-generic-list )
        /// </summary>
        /// <returns>The standard deviation of the playerElo Dictionary</returns>
        //static double CalculateStandardDeviation()
        //{
        //    double average = CalculateMean();
        //    double variance = CalculateVariance(average);
        //    double deviation = Math.Sqrt(variance);

        //    return deviation;
        //}

        const string outputfolder = @"D:\Downloads\matches";
        static void WriteToFileLogistic(Match match, string filename)
        {
            // Calculate ELO difference (Radiant - Dire)
            double eloDire = match.Dire.AverageElo;
            double eloRadiant = match.Radiant.AverageElo;
            double eloDifference = eloRadiant - eloDire;

            bool validMatchesPlayed = false;

            // Check if all players in the match have played more than 100 matches, this means it has a significance
            for (int i = 0; i < 5; i++)
            {
                if (playerElos[match.Radiant.Players[i].account_id].AmountOfMatches < 100 && playerElos[match.Dire.Players[i].account_id].AmountOfMatches < 100)
                    validMatchesPlayed = false;
                else validMatchesPlayed = true;
            }

            if (eloDifference != 0 && validMatchesPlayed)
            {
                char outcome = 'X';
                if (match.Radiant.Won)
                    outcome = '1';
                if (match.Dire.Won)
                    outcome = '0';

                string text = string.Format("{0};{1}", eloDifference, outcome);
                string path = string.Format(@"{0}\{1}.csv", outputfolder, filename);
                using (StreamWriter sw = new StreamWriter(path, true))
                    sw.WriteLine(text);
            }
        }

        static void UpdatePlayerMatchCount(Match match)
        {
            for (int i = 0; i < 5; i++)
            {
                playerElos[match.Radiant.Players[i].account_id].AmountOfMatches++;
                playerElos[match.Dire.Players[i].account_id].AmountOfMatches++;
            }
        }

        //static void WriteToFileStandardDeviation(double deviation, string filename)
        //{
        //    double average = CalculateMean();

        //    #region CalculateRanges
        //    double[] ranges = new double[7];
        //    ranges[3] = average;
        //    for (int i = 2; i >= 0; i--)
        //        ranges[i] = ranges[i + 1] - deviation;
        //    for (int i = 4; i <= 6; i++)
        //        ranges[i] = ranges[i - 1] + deviation;
        //    #endregion

        //    int[] population = new int[8];
        //    foreach (int elos in playerElos.Values)
        //    {
        //        if (elos < ranges[0])
        //            population[0] += 1;
        //        else if (elos >= ranges[6])
        //            population[7] += 1;
        //        else
        //        {
        //            for (int i = 0; i < 6; i++)
        //                if (elos >= ranges[i] && elos < ranges[i + 1])
        //                    population[i + 1] += 1;
        //        }
        //    }

        //    double[] percentages = new double[8];

        //    for (int i = 0; i < population.Length; i++)
        //        percentages[i] = (double)population[i] / (double)amountofPlayers * 100;

        //    string path = string.Format(@"{0}\{1}.csv", outputfolder, filename);
        //    using (StreamWriter sw = new StreamWriter(path, true))
        //        for (int i = 0; i < percentages.Length; i++)
        //            sw.WriteLine(string.Format("Current range: {0}, percentage: {1}%", i, percentages[i]));
        //}

        // TODO: Accuracy van matches die we gaan predicten uitrekenen
        // TODO: Logaritmic loss: https://www.kaggle.com/wiki/LogLoss ??? en Logarithmic regression voor de paper

        // TODO: Variables voor models: Elo_Diff  The ELO rating difference between the two teams at the start of the match.
        // Calculated by subtracting the away team ELO rating from the home team ELO rating.A
        // positive value means that the home team is deemed stronger, while a negative value
        // means that the away team is deemed stronger


    }
}
