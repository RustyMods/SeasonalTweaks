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
            if (ConfigManager.m_enabled.Value is SeasonalTweaksPlugin.Toggle.Off) return true;
            string prefabName = __instance.name.Replace("(Clone)",string.Empty);
            if (!HasConfigs(prefabName)) return true;
            var data = GetData(prefabName);
            if (SkillManager.HasOverrideLevel()) return true;

            return m_currentSeason switch
            {
                Season.Spring => data.m_spring.m_canHarvest,
                Season.Summer => data.m_summer.m_canHarvest,
                Season.Fall => data.m_fall.m_canHarvest,
                Season.Winter => data.m_winter.m_canHarvest,
                _ => true,
            };
        }
    }

    private static bool HasConfigs(string prefabName) =>
        ConfigManager.m_config.Plants.Exists(x => x.m_prefabName == prefabName);
    private static PlantData GetData(string prefabName) => ConfigManager.m_config.Plants.Find(x => x.m_prefabName == prefabName);
    
    [HarmonyPatch(typeof(Plant), nameof(Plant.GetStatus))]
    private static class Plant_GetStatus_Postfix
    {
        private static void Postfix(Plant __instance, ref Plant.Status __result)
        {
            if (ConfigManager.m_enabled.Value is SeasonalTweaksPlugin.Toggle.Off) return;
            string prefabName = __instance.name.Replace("(Clone)",string.Empty);
            if (!HasConfigs(prefabName)) return;
            var data = GetData(prefabName);
            if (SkillManager.HasOverrideLevel()) return;
            
            switch (m_currentSeason)
            {
                case Season.Spring:
                    if (data.m_spring.m_canHarvest) return;
                    __result = Plant.Status.NoSpace;
                    break;
                case Season.Summer:
                    if (data.m_summer.m_canHarvest) return;
                    __result = Plant.Status.TooHot;
                    break;
                case Season.Fall:
                    if (data.m_fall.m_canHarvest) return;
                    __result = Plant.Status.NotCultivated;
                    break;
                case Season.Winter:
                    if (data.m_winter.m_canHarvest) return;
                    __result = Plant.Status.TooCold;
                    break;
            }
        }
    }

    [HarmonyPatch(typeof(Plant), nameof(Plant.GetHoverText))]
    private static class Plant_GetHoverText_Postfix
    {
        private static void Postfix(Plant __instance, ref string __result)
        {
            if (ConfigManager.m_enabled.Value is SeasonalTweaksPlugin.Toggle.Off) return;
            string prefabName = __instance.name.Replace("(Clone)",string.Empty);
            if (!HasConfigs(prefabName)) return;
            PlantData data = GetData(prefabName);
            if (SkillManager.HasOverrideLevel()) return;

            switch (SeasonKeys.m_currentSeason)
            {
                case Season.Spring:
                    if (data.m_spring.m_canHarvest) return;
                    __result += Localization.instance.Localize("\n <color=red>$spring_cannot_grow");
                    break;
                case Season.Summer:
                    if (data.m_summer.m_canHarvest) return;
                    __result += Localization.instance.Localize("\n <color=red>$summer_cannot_grow");
                    break;
                case Season.Fall :
                    if (data.m_fall.m_canHarvest) return;
                    __result += Localization.instance.Localize("\n <color=red>$fall_cannot_grow");
                    break;
                case Season.Winter:
                    if (data.m_winter.m_canHarvest) return;
                    __result += Localization.instance.Localize("\n <color=red>$winter_cannot_grow");
                    break;
            }
        }
    }

    [HarmonyPatch(typeof(Plant), nameof(Plant.Awake))]
    private static class Plant_Awake_Postfix
    {
        private static void Postfix(Plant __instance)
        {
            if (ConfigManager.m_enabled.Value is SeasonalTweaksPlugin.Toggle.Off) return;
            string prefabName = __instance.name.Replace("(Clone)",string.Empty);
            if (!HasConfigs(prefabName)) return;
            PlantData data = GetData(prefabName);
            PlantValues? values = null;
            switch (SeasonKeys.m_currentSeason)
            {
                case Season.Spring:
                    values = data.m_spring;
                    break;
                case Season.Summer:
                    values = data.m_summer;
                    break;
                case Season.Fall:
                    values = data.m_fall;
                    break;
                case Season.Winter:
                    values = data.m_winter;
                    break;
            }

            if (values == null) return;
            
            __instance.m_maxScale = values.m_maxScale;
            __instance.m_minScale = values.m_minScale;
            __instance.m_growTimeMax = values.m_growTimeMax;
            __instance.m_growTime = values.m_growTime;
        }
    }
}