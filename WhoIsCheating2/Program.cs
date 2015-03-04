using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using LeagueSharp;
using LeagueSharp.Common;

namespace WhoIsCheating2
{
    class Hero
    {
        public int NetworkId;
        public int Count;
        public int Detections;
    }
    class Program
    {
        private static bool lookUp;
        private static int lastTick;

        private static List<Hero> heroList;
        private static TimeSpan ts;
        private static DateTime start;

        private static void Main(string[] args)
        {
            Obj_AI_Base.OnNewPath += Obj_AI_Hero_OnNewPath;
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        private static void DebugStatus(string message, ConsoleColor colour)
        {
            Console.ForegroundColor = colour;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            Check();
        }

        private static void Check()
        {
            if (Environment.TickCount <= lastTick + 200) return;

            if (!lookUp)
            {
                if (Game.Time < 10) return;
                heroList = new List<Hero>();
                using (IEnumerator<Obj_AI_Hero> enumerator = ObjectManager.Get<Obj_AI_Hero>().GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        Obj_AI_Hero current = enumerator.Current;
                        if (current != null && current.IsValid)
                        {
                            heroList.Add(new Hero { NetworkId = current.NetworkId, Count = 0, Detections = 0 });
                            DebugStatus(String.Format("Added NId: {0}", current.NetworkId), ConsoleColor.White);
                        }
                    }
                }
                Game.PrintChat("<font color = \"#0000FF\">WhoIsCheating2 by</font> <font color = \"#FF3300\">Mistejk</font> <font color = \"#0000FF\">loaded and initialised.</font>");
                lookUp = true;
            }
            ts = DateTime.Now - start;
            if (ts.TotalMilliseconds > 1000.0)
            {
                WhoIsCheatingHuehue();
            }
            lastTick = Environment.TickCount;
        }

        private static void WhoIsCheatingHuehue()
        {
            using (IEnumerator<Obj_AI_Hero> enumerator = ObjectManager.Get<Obj_AI_Hero>().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Obj_AI_Hero hero = enumerator.Current;
                    if (hero != null && hero.IsValid)
                    {
                        if (heroList.Find(y => y.NetworkId == hero.NetworkId).Count >= 10)
                        {
                            ++heroList.Find(y => y.NetworkId == hero.NetworkId).Detections;
                            DebugStatus(
                                String.Format(
                                    "Cheater detected! NId: {0}, Count: {1}, CN: {2}, Detections: {3}",
                                    hero.NetworkId, heroList.Find(y => y.NetworkId == hero.NetworkId).Count,
                                    hero.ChampionName, heroList.Find(y => y.NetworkId == hero.NetworkId).Detections),
                                ConsoleColor.Red);
                            //set your path for cheater log below
                            File.AppendAllText(@"D:\cheaters.txt", String.Format(
                                    "ID: {0}, Count: {1}, Champion: {2}, Detections: {3}, IGN: {4}, {5}/{6}/{7} on level {8}\r\n",
                                    Game.Id, heroList.Find(y => y.NetworkId == hero.NetworkId).Count,
                                    hero.ChampionName, heroList.Find(y => y.NetworkId == hero.NetworkId).Detections,
                                    hero.Name, hero.ChampionsKilled, hero.Deaths, hero.Assists, hero.Level), System.Text.Encoding.UTF8);
                            Game.PrintChat("Cheater detected: <font color = \"#FF0000\">{0}</font>. Detection {1}. Count {2}.", hero.ChampionName, heroList.Find(y => y.NetworkId == hero.NetworkId).Detections, heroList.Find(y => y.NetworkId == hero.NetworkId).Count);
                        }
                        heroList.Find(y => y.NetworkId == hero.NetworkId).Count = 0;
                    }
                }
            }
            start = DateTime.Now;
        }

        private static void Obj_AI_Hero_OnNewPath(Obj_AI_Base sender, EventArgs args)
        {
            if (!(sender is Obj_AI_Hero) || !lookUp) return;
            ++heroList.Find(hero => hero.NetworkId == sender.NetworkId).Count;
//            DebugStatus(String.Format("Increased Count for NId: {0}", sender.NetworkId), ConsoleColor.Yellow);
        }
    }
}
