using System;
using System.Collections.Generic;
using BepInEx.Logging;
using HarmonyLib;

namespace SeasonalTweaks.Tweaks;

public static class SeasonKeys
{
    [Flags]
    public enum Seasons
    {
        None = 0, 
        Spring = 1, 
        Summer = 2, 
        Fall = 4, 
        Winter = 8
    }
    public static Seasons season;

    [HarmonyPatch(typeof(Player), nameof(Player.UpdateAwake))]
    static class GetSeasonalKeys
    {
        private static void Postfix(Player __instance)
        {
            if (!__instance) return;
            if (!Player.m_localPlayer) return;
            if (__instance != Player.m_localPlayer) return;
            if (!ZoneSystem.instance) return;
            
            List<string>? currentKeys = ZoneSystem.instance.GetGlobalKeys();
            string key = currentKeys.Find(x => x.StartsWith("season"));
            switch (key)
            {
                case "season_winter": season = Seasons.Winter; break;
                case "season_summer": season = Seasons.Summer; break;
                case "season_spring": season = Seasons.Spring; break;
                case "season_fall": season = Seasons.Fall; break;
            }
        }
    }
}