using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOTA2EloCalculator
{
    // This class holds the account id, 
    // Elo rating and the amount of matches played for a specific player.
    public class Player
    {
        public string account_id;
        public int elo;
        public int AmountOfMatches;

        public Player(string account_id, int elo)
        {
            this.account_id = account_id;
            this.elo = elo;
            this.AmountOfMatches = 0;
        }
    }
}
