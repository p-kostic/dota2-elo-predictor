using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;

namespace DOTA2EloCalculator
{
    class Match
    {
        public Team Radiant { get; set; }
        public Team Dire { get; set; }
        public string Winner;

        public Match(Team teamA, Team teamB)
        {
            this.Radiant = teamA;
            this.Dire = teamB;
            if (Radiant.Won)
                Winner = "Radiant";
            else if (Dire.Won)
                Winner = "Dire";
        }
    }
}
