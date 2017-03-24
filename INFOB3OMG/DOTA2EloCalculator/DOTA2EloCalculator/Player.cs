using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOTA2EloCalculator
{
    public class Player
    {
        public string account_id;
        public int elo;

        public Player(string account_id, int elo)
        {
            this.account_id = account_id;
            this.elo = elo;
        }
    }
}
