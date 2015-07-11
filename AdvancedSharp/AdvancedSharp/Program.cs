using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.SDK.Core.Events;

namespace AdvancedSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            Load.OnLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(object sender, EventArgs e)
        {
            Core.LoadChampion(ObjectManager.Player.ChampionName);
        }
    }
}
