using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.SDK.Core.UI.INotifications;

namespace AdvancedSharp
{
    class Core
    {
        public static void LoadChampion(String Name)
        {
            Console.WriteLine(ObjectManager.Player.ChampionName);
            switch (Name)
            {

                case "Cassiopeia":
                    new Instance.Cassiopeia();
                    break;
                    /*
                default:
                    new Instance.Orbwalker();
                    break;
                     */
            }
        }

        /*
        public static void LoadWelcomeMessage(bool orbwalker = false)
        {
            if (orbwalker)
                Notifications.AddNotification("Adv#: Orbwalker", 500);
            else Notifications.AddNotification("Adv#: " + ObjectManager.Player.ChampionName, 500);
        }
         */
    }
}
