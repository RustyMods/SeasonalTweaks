using System.Collections.Generic;
using BepInEx.Configuration;
using HarmonyLib;

namespace SeasonalTweaks.Managers;

public static class BeehiveManager
{
    [HarmonyPatch(typeof(Beehive), nameof(Beehive.Interact))]
    private static class Beehive_Interact_Prefix
    {
        private static bool Prefix(Beehive __instance)
        {
            if (!ConfigManager.m_beehives.TryGetValue(__instance.name.Replace("(Clone)",string.Empty),
                    out Dictionary<SeasonKeys.Season, ConfigEntry<SeasonalTweaksPlugin.Toggle>> configs)) return true;
            if (!configs.TryGetValue(SeasonKeys.m_currentSeason, out ConfigEntry<SeasonalTweaksPlugin.Toggle> config))
                return true;

            if (config.Value is SeasonalTweaksPlugin.Toggle.Off) return false;
            
            return true;
        }
    }

    [HarmonyPatch(typeof(Beehive), nameof(Beehive.Awake))]
    private static class Beehive_Awake_Postfix
    {
        private static void Postfix(Beehive __instance)
        {
            if (!ConfigManager.m_beehive_maxHoney.TryGetValue(__instance.name.Replace("(Clone)", string.Empty),
                    out Dictionary<SeasonKeys.Season, ConfigEntry<int>> amounts)) return;
            if (!amounts.TryGetValue(SeasonKeys.m_currentSeason, out ConfigEntry<int> amount)) return;
            __instance.m_maxHoney = amount.Value;
        }
    }
}