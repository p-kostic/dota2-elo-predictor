using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using Newtonsoft.Json;

namespace INFOB3OMG
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Allows us to see the console during runtime 
            AllocConsole();

            string url = "https://api.steampowered.com/IDOTA2Match_570/GetMatchHistory/V001/?key=520D2DA46A878CD76A707D6E065EC7E1";
            var json = new WebClient().DownloadString(url);
            var api = JsonConvert.DeserializeObject<JsonWrapper>(json);

            List<string> urls = new List<string>();

            for (int i = 0; i < api.result.matches.Length; i++)
            {
               urls.Add("https://api.steampowered.com/IDOTA2Match_570/GetMatchDetails/V001/?match_id=" + api.result.matches[i].match_id + "&key=520D2DA46A878CD76A707D6E065EC7E1");
            }

            List<JsonMatchDetails> matches = new List<JsonMatchDetails>();

            for (int i = 0; i < urls.Count - 1; i++)
            {
                var jsonMatch = new WebClient().DownloadString(urls[i]);
                matches.Add(JsonConvert.DeserializeObject<JsonMatchDetails>(jsonMatch));
            }

            // test
            for (int i = 0; i < matches.Count; i++)
                Console.WriteLine(matches.Count);
            

            // TODO: Geen duplicates, match data wegschrijven
            // TODO: structuur maken om ELO te berekenen
            // TODO: Elo update functie voor een match 
            // TODO: Prediction model.
            // TODO: Match_id, win/lose, etc. mag niet 0 of null zijn! Unit tests of Exceptions!
            // TODO: Data op een manier weergeven zodat we er iets mee kunnen in onze paper. (C# --> EXCEL?)
        }

        // Allows us to see the console during runtime
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();
    }


    public class JsonWrapper
    {
        public JsonResult result { get; set; }
    }

    public class JsonResult
    {
        public int status { get; set; }
        public int num_results { get; set; }
        public int total_results { get; set; }
        public int results_remaining { get; set; }
        public JsonMatch[] matches { get; set; }
    }

    public class JsonMatch
    {
        public long match_id { get; set; }
        public long match_seq_number { get; set; }
        public long start_time { get; set; }
        public int lobby_type { get; set; }
        public int radiant_team_id { get; set; }
        public int dire_team_id { get; set; }
        public JsonPlayers[] players { get; set; }
    }

    public class JsonPlayers
    {
        public long account_id { get; set; }
        public int player_slot { get; set; }
        public int hero_id { get; set; }
    }

    // -------- MATCH DETAILS -------------
    public class JsonMatchDetails
    {
        public JsonMatchResult result { get; set; }
    }

    public class JsonMatchResult
    {
        public JsonMatchPlayers[] players { get; set; }
        public bool radiant_win { get; set; }
        public int duration { get; set; }
        public int pre_game_duration { get; set; }
        public long start_time { get; set; }
        public long match_id { get; set; }
        public long match_seq_num { get; set; }
        public int tower_status_radiant { get; set; }
        public int tower_status_dire { get; set; }
        public int barracks_status_radiant { get; set; }
        public int barracks_status_dire { get; set; }
        public int cluster { get; set; }
        public int first_blood_time { get; set; }
        public int lobby_type { get; set; }
        public int human_players { get; set; }
        public int leagueid { get; set; }
        public int positive_votes { get; set; }
        public int negative_votes { get; set; }
        public int game_mode { get; set; }
        public int flags { get; set; }
        public int engine { get; set; }
        public int radiant_score { get; set; }
        public int dire_score { get; set; }
    }

    public class JsonMatchPlayers
    {
        public long account_id { get; set; }
        public int player_slot { get; set; }
        public int hero_id { get; set; }
        public int item_0 { get; set; }
        public int item_1 { get; set; }
        public int item_2 { get; set; }
        public int item_3 { get; set; }
        public int item_4 { get; set; }
        public int item_5 { get; set; }
        public int backpack_0 { get; set; }
        public int backpack_1 { get; set; }
        public int backpack_2 { get; set; }
        public int kills { get; set; }
        public int deaths { get; set; }
        public int assists { get; set; }
        public int leaver_status { get; set; }
        public int last_hits { get; set; }
        public int denies { get; set; }
        public int gold_per_min { get; set; }
        public int xp_per_min { get; set; }
        public int level { get; set; }
    }

}
