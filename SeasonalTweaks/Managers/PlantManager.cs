using System.Collections.Generic;
using BepInEx.Configuration;
using HarmonyLib;
using static SeasonalTweaks.Managers.SeasonKeys;

namespace SeasonalTweaks.Managers;

public static class PlantManager
{
    [HarmonyPatch(typeof(Plant), nameof(Plant.Grow))]
    private static class Plant_Grow_Prefix
    {
        private static bool Prefix(Plant __instance)
        {
            if (!ConfigManager.m_plants.TryGetValue(__instance.name.Replace("(Clone)",string.Empty),
                    out Dictionary<Season, ConfigEntry<SeasonalTweaksPlugin.Toggle>> configs)) return true;
            if (!configs.TryGetValue(m_currentSeason, out ConfigEntry<SeasonalTweaksPlugin.Toggle> config))
                return true;

            if (SeasonalTweaksPlugin.ForagingLoaded)
            {
                if (!ConfigManager.m_plantForagingOverride.TryGetValue(__instance.name.Replace("(Clone)",string.Empty),
                        out ConfigEntry<int> foragingOverride)) return true;
                if (foragingOverride.Value < SkillManager.GetForagingSkillLevel()) return true;
            }

            if (SeasonalTweaksPlugin.FarmingLoaded)
            {
                if (!ConfigManager.m_plantFarmingOverride.TryGetValue(__instance.name.Replace("(Clone)",string.Empty),
                        out ConfigEntry<int> farmingOverride)) return true;
                if (farmingOverride.Value < SkillManager.GetFarmingSkillLevel()) return true;
            }

            return config.Value is SeasonalTweaksPlugin.Toggle.On;
        }
    }

    [HarmonyPatch(typeof(Plant), nameof(Plant.GetStatus))]
    private static class Plant_GetStatus_Postfix
    {
        private static void Postfix(Plant __instance, ref Plant.Status __result)
        {
            if (!ConfigManager.m_plants.TryGetValue(__instance.name.Replace("(Clone)",string.Empty),
                    out Dictionary<Season, ConfigEntry<SeasonalTweaksPlugin.Toggle>> configs)) return;
            if (!configs.TryGetValue(m_currentSeason, out ConfigEntry<SeasonalTweaksPlugin.Toggle> config))
                return;
            
            if (SeasonalTweaksPlugin.ForagingLoaded)
            {
                if (!ConfigManager.m_plantForagingOverride.TryGetValue(__instance.name.Replace("(Clone)",string.Empty),
                        out ConfigEntry<int> foragingOverride)) return;
                if (foragingOverride.Value < SkillManager.GetForagingSkillLevel()) return;
            }

            if (SeasonalTweaksPlugin.FarmingLoaded)
            {
                if (!ConfigManager.m_plantFarmingOverride.TryGetValue(__instance.name.Replace("(Clone)",string.Empty),
                        out ConfigEntry<int> farmingOverride)) return;
                if (farmingOverride.Value < SkillManager.GetFarmingSkillLevel()) return;
            }

            if (config.Value is SeasonalTweaksPlugin.Toggle.Off)
            {
                switch (m_currentSeason)
                {
                    case Season.Spring:
                        __result = Plant.Status.NoSpace;
                        break;
                    case Season.Summer:
                        __result = Plant.Status.TooHot;
                        break;
                    case Season.Fall:
                        __result = Plant.Status.NotCultivated;
                        break;
                    case Season.Winter:
                        __result = Plant.Status.TooCold;
                        break;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Plant), nameof(Plant.GetHoverText))]
    private static class Plant_GetHoverText_Postfix
    {
        private static void Postfix(Plant __instance, ref string __result)
        {
            if (!ConfigManager.m_plants.TryGetValue(__instance.name.Replace("(Clone)",string.Empty),
                    out Dictionary<Season, ConfigEntry<SeasonalTweaksPlugin.Toggle>> configs)) return;
            if (!configs.TryGetValue(m_currentSeason, out ConfigEntry<SeasonalTweaksPlugin.Toggle> config))
                return;
            
            if (SeasonalTweaksPlugin.ForagingLoaded)
            {
                if (!ConfigManager.m_plantForagingOverride.TryGetValue(__instance.name.Replace("(Clone)",string.Empty),
                        out ConfigEntry<int> foragingOverride)) return;
                if (foragingOverride.Value < SkillManager.GetForagingSkillLevel()) return;
            }

            if (SeasonalTweaksPlugin.FarmingLoaded)
            {
                if (!ConfigManager.m_plantFarmingOverride.TryGetValue(__instance.name.Replace("(Clone)",string.Empty),
                        out ConfigEntry<int> farmingOverride)) return;
                if (farmingOverride.Value < SkillManager.GetFarmingSkillLevel()) return;
            }

            if (config.Value is SeasonalTweaksPlugin.Toggle.Off)
            {
                __result += Localization.instance.Localize(m_currentSeason switch
                {
                    Season.Spring => "\n <color=red>$spring_cannot_grow",
                    Season.Summer => "\n <color=red>$summer_cannot_grow",
                    Season.Fall => "\n <color=red>$fall_cannot_grow",
                    Season.Winter => "\n <color=red>$winter_cannot_grow",
                    _ => ""

                });
            }
        }
    }

    [HarmonyPatch(typeof(Plant), nameof(Plant.Awake))]
    private static class Plant_Awake_Postfix
    {
        private static void Postfix(Plant __instance)
        {
            if (ConfigManager.m_plantMaxScale.TryGetValue(__instance.name.Replace("(Clone)", string.Empty),
                    out Dictionary<Season, ConfigEntry<float>> maxScales))
            {
                if (maxScales.TryGetValue(m_currentSeason, out ConfigEntry<float> max))
                {
                    __instance.m_maxScale = max.Value;
                }
            }

            if (ConfigManager.m_plantMinScale.TryGetValue(__instance.name.Replace("(Clone)", string.Empty),
                    out Dictionary<Season, ConfigEntry<float>> minScales))
            {
                if (minScales.TryGetValue(m_currentSeason, out ConfigEntry<float> min))
                {
                    __instance.m_minScale = min.Value;
                }
            }

            if (ConfigManager.m_plantGrowthTime.TryGetValue(__instance.name.Replace("(Clone)", string.Empty),
                    out Dictionary<Season, ConfigEntry<float>> times))
            {
                if (times.TryGetValue(m_currentSeason, out ConfigEntry<float> time))
                {
                    __instance.m_growTime = time.Value;
                }
            }

            if (ConfigManager.m_plantGrowMax.TryGetValue(__instance.name.Replace("(Clone)", string.Empty),
                    out Dictionary<Season, ConfigEntry<float>> growMax))
            {
                if (growMax.TryGetValue(m_currentSeason, out ConfigEntry<float> max))
                {
                    __instance.m_growTimeMax = max.Value;
                }
            }
        }
    }
}