using System.Collections.Generic;
using BepInEx.Configuration;
using HarmonyLib;

namespace SeasonalTweaks.Managers;

public static class PickableManager
{
    [HarmonyPatch(typeof(Pickable), nameof(Pickable.Interact))]
    private static class Pickable_Interact_Prefix
    {
        private static bool Prefix(Pickable __instance)
        {
            if (!ConfigManager.m_pickable.TryGetValue(__instance.name.Replace("(Clone)", string.Empty),
                    out Dictionary<SeasonKeys.Season, ConfigEntry<SeasonalTweaksPlugin.Toggle>> configs)) return true;
            if (!configs.TryGetValue(SeasonKeys.m_currentSeason, out ConfigEntry<SeasonalTweaksPlugin.Toggle> config))
                return true;

            if (SeasonalTweaksPlugin.ForagingLoaded)
            {
                if (!ConfigManager.m_pickableForagingOverride.TryGetValue(__instance.name.Replace("(Clone)",string.Empty), out ConfigEntry<int> foragingOverride)) return true;

                if (SkillManager.GetForagingSkillLevel() > foragingOverride.Value) return true;
            }

            if (SeasonalTweaksPlugin.FarmingLoaded)
            {
                if (!ConfigManager.m_pickableFarmingOverride.TryGetValue(__instance.name.Replace("(Clone)", string.Empty),
                        out ConfigEntry<int> farmingOverride)) return true;
                if (SkillManager.GetFarmingSkillLevel() > farmingOverride.Value) return true;
            }

            if (config.Value is SeasonalTweaksPlugin.Toggle.Off) return false;

            if (ConfigManager.m_pickableAmounts.TryGetValue(__instance.name.Replace("(Clone)", string.Empty),
                    out Dictionary<SeasonKeys.Season, ConfigEntry<int>> amounts))
            {
                if (amounts.TryGetValue(SeasonKeys.m_currentSeason, out ConfigEntry<int> amount))
                {
                    __instance.m_amount = amount.Value;
                }
            }
            return true;

        }
    }

    [HarmonyPatch(typeof(Pickable), nameof(Pickable.GetHoverText))]
    private static class Pickable_GetHoverText_Postfix
    {
        private static void Postfix(Pickable __instance, ref string __result)
        {
            if (!ConfigManager.m_pickable.TryGetValue(__instance.name.Replace("(Clone)", string.Empty),
                    out Dictionary<SeasonKeys.Season, ConfigEntry<SeasonalTweaksPlugin.Toggle>> configs)) return;
            if (!configs.TryGetValue(SeasonKeys.m_currentSeason, out ConfigEntry<SeasonalTweaksPlugin.Toggle> config))
                return;
            
            if (SeasonalTweaksPlugin.ForagingLoaded)
            {
                if (!ConfigManager.m_pickableForagingOverride.TryGetValue(__instance.name.Replace("(Clone)",string.Empty), out ConfigEntry<int> foragingOverride)) return;

                if (SkillManager.GetForagingSkillLevel() > foragingOverride.Value) return;
            }

            if (SeasonalTweaksPlugin.FarmingLoaded)
            {
                if (!ConfigManager.m_pickableFarmingOverride.TryGetValue(__instance.name.Replace("(Clone)", string.Empty),
                        out ConfigEntry<int> farmingOverride)) return;
                if (SkillManager.GetFarmingSkillLevel() > farmingOverride.Value) return;
            }

            if (config.Value is SeasonalTweaksPlugin.Toggle.Off)
            {
                __result += Localization.instance.Localize(SeasonKeys.m_currentSeason switch
                {
                    SeasonKeys.Season.Spring => "\n <color=red>$spring_cannot_pick",
                    SeasonKeys.Season.Summer => "\n <color=red>$summer_cannot_pick",
                    SeasonKeys.Season.Fall => "\n <color=red>$fall_cannot_pick",
                    SeasonKeys.Season.Winter => "\n <color=red>$winter_cannot_pick",
                    _ => ""

                });
            }
        }
    }
}