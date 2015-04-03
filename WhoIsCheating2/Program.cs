using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
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
        private static bool isDetecting;
        private static int lastTick;

        private static List<Hero> heroList;
        private static TimeSpan ts;
        private static DateTime start;

        private const string LogFilePath = @"D:\cheaters.txt";
		private bool IsLoggingToFile = (ObjectManager.Player.Name == "Mistejk");

        private static void Main(string[] args)
        {
            Obj_AI_Base.OnNewPath += Obj_AI_Hero_OnNewPath;
            Game.OnUpdate += Game_OnUpdate;
            Game.OnInput += Game_OnInput;
        }

        static void Game_OnInput(GameInputEventArgs args)
        {
            if (args.Input.StartsWith("/StartDetection"))
            {
                isDetecting = true;
                Game.PrintChat("<font color = \"#EEAD0E\">WhoIsCheating2 is now detecting players.</font>");
                args.Process = false;
            }
        }

        private static void DebugStatus(string message, ConsoleColor colour)
        {
            Console.ForegroundColor = colour;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            Check();
        }

        private static void Check()
        {
            if (Environment.TickCount <= lastTick + 200) return;

            if (!lookUp)
            {
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
                Game.PrintChat("<font color = \"#00E5EE\">WhoIsCheating2 by</font> <font color = \"#FF3300\">Mistejk</font> <font color = \"#00E5EE\">loaded and initialised.</font>");
                Game.PrintChat("<font color = \"#00EE00\">Type /StartDetection in order to start detecting players!</font>");
                lookUp = true;
            }
            if (!isDetecting) return;
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
							if (IsLoggingToFile)
                            File.AppendAllText(LogFilePath, String.Format(
                                    "ID: {0}, Count: {1}, Champion: {2}, Detections: {3}, IGN: {4}, {5}/{6}/{7} on level {8}\r\n",
                                    Game.Id, heroList.Find(y => y.NetworkId == hero.NetworkId).Count,
                                    hero.ChampionName, heroList.Find(y => y.NetworkId == hero.NetworkId).Detections,
                                    hero.Name, hero.ChampionsKilled, hero.Deaths, hero.Assists, hero.Level), System.Text.Encoding.UTF8);
                            Game.PrintChat("Cheater detected: <font color = \"#FF0000\">{0} ({3})</font>. Detection {1}. Count {2}.", hero.ChampionName, heroList.Find(y => y.NetworkId == hero.NetworkId).Detections, heroList.Find(y => y.NetworkId == hero.NetworkId).Count, hero.Name);
                        }
                        heroList.Find(y => y.NetworkId == hero.NetworkId).Count = 0;
                    }
                }
            }
            start = DateTime.Now;
        }

        private static void Obj_AI_Hero_OnNewPath(Obj_AI_Base sender, EventArgs args)
        {
            if (!(sender is Obj_AI_Hero) || !lookUp || !isDetecting) return;
            ++heroList.Find(hero => hero.NetworkId == sender.NetworkId).Count;
        }
    }
}
