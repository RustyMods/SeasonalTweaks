
using HarmonyLib;

namespace SeasonalTweaks.Managers;

public static class FishManager
{
    [HarmonyPatch(typeof(Fish), nameof(Fish.Interact))]
    private static class Fish_Interact_Prefix
    {
        private static bool Prefix(Fish __instance)
        {
            if (SeasonKeys.m_currentSeason is not SeasonKeys.Season.Winter) return true;
            return ConfigManager.m_fishOverride.Value is SeasonalTweaksPlugin.Toggle.On;
        }
    }

    [HarmonyPatch(typeof(Fish), nameof(Fish.GetHoverText))]
    private static class Fish_GetHoverText_Postfix
    {
        private static void Postfix(ref string __result)
        {
            if (SeasonKeys.m_currentSeason is not SeasonKeys.Season.Winter) return;
            if (ConfigManager.m_fishOverride.Value is SeasonalTweaksPlugin.Toggle.Off) return;
            __result += "\n $winter_cannot_pick_fish";

        }
    }
}