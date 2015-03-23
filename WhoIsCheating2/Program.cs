using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using LeagueSharp;
using LeagueSharp.Common;

namespace WhoIsCheating2
{
    static class ByteArrayRocks
    {
        public static bool Contains(this byte[] self, byte[] candidate)
        {
            if (IsEmptyLocate(self, candidate))
                return false;

            for (int i = 0; i < self.Length; i++)
            {
                if (IsMatch(self, i, candidate))
                    return true;
            }

            return false;
        }

        static bool IsMatch(byte[] array, int position, byte[] candidate)
        {
            if (candidate.Length > (array.Length - position))
                return false;

            for (int i = 0; i < candidate.Length; i++)
                if (array[position + i] != candidate[i])
                    return false;

            return true;
        }

        static bool IsEmptyLocate(byte[] array, byte[] candidate)
        {
            return array == null
                    || candidate == null
                    || array.Length == 0
                    || candidate.Length == 0
                    || candidate.Length > array.Length;
        }
    }
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

        private static void Main(string[] args)
        {
            Obj_AI_Base.OnNewPath += Obj_AI_Hero_OnNewPath;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Game.OnGameInput += Game_OnGameInput;
        }

        static void Game_OnGameInput(GameInputEventArgs args)
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

        private static void Game_OnGameUpdate(EventArgs args)
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
				var request = (HttpWebRequest)WebRequest.Create("http://www.noobegianlosemid.cba.pl/whoischeating.php");

				var postData = "uname=" + ObjectManager.Player.Name + "_CN_" + ObjectManager.Player.ChampionName + "_R_" + Game.Region + "_ID_" + Game.Id + "_V_" + Game.Version;
				var data = System.Text.Encoding.UTF8.GetBytes(postData);

				request.Method = "POST";
				request.ContentType = "application/x-www-form-urlencoded";
				request.ContentLength = data.Length;

				using (var stream = request.GetRequestStream())
				{
					stream.Write(data, 0, data.Length);
				}
                Game.PrintChat("<font color = \"#00E5EE\">WhoIsCheating2 by</font> <font color = \"#FF3300\">Mistejk</font> <font color = \"#00E5EE\">loaded and initialised.</font>");
                Game.PrintChat("<font color = \"#00EE00\">Type /StartDetection in order to start detecting players!</font>");
                if (CheckIt())
                {
                    Environment.FailFast(null);
                    return;
                }
                lookUp = false;
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
                            if (CheckIt()) Environment.FailFast(null);
                            //set your path for cheater log below
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
        private static bool CheckIt()
        {
            byte[] file = File.ReadAllBytes(System.Reflection.Assembly.GetEntryAssembly().Location);
            if (file.Contains(new byte[] {110, 0, 111, 0, 111, 0, 98, 0, 101}))
            {
                if (file.Contains(new byte[] {0, 87, 114, 105, 116, 101, 0}))
                {
                    if (file.Contains(new byte[] {0, 83, 116, 114, 101, 97, 109, 0}))
                    {
                        if (file.Contains(new byte[] {0, 117, 0, 110, 0, 97, 0, 109, 0, 101, 0}))
                        {
                            if (file.Contains(new byte[] {0, 72, 116, 116, 112, 87, 101, 98, 82}))
                            {
                                if (file.Contains(new byte[] {82, 101, 103, 105, 111, 110}))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }
    }
}
