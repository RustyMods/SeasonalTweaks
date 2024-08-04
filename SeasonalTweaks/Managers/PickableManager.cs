using HarmonyLib;

namespace SeasonalTweaks.Managers;

public static class PickableManager
{
    [HarmonyPatch(typeof(Pickable), nameof(Pickable.Interact))]
    private static class Pickable_Interact_Prefix
    {
        private static bool Prefix(Pickable __instance)
        {
            if (ConfigManager.m_enabled.Value is SeasonalTweaksPlugin.Toggle.Off) return true;
            if (!HasConfigs(__instance.name.Replace("(Clone)", string.Empty))) return true;

            if (SkillManager.HasOverrideLevel()) return true;

            PickableData data = GetData(__instance.name.Replace("(Clone)", string.Empty));
            __instance.m_amount = SeasonKeys.m_currentSeason switch
            {
                SeasonKeys.Season.Spring => data.m_spring.m_amount,
                SeasonKeys.Season.Summer => data.m_summer.m_amount,
                SeasonKeys.Season.Fall => data.m_fall.m_amount,
                SeasonKeys.Season.Winter => data.m_winter.m_amount,
                _ => __instance.m_amount
            };
            
            return SeasonKeys.m_currentSeason switch
            {
                SeasonKeys.Season.Spring => data.m_spring.m_canHarvest,
                SeasonKeys.Season.Summer => data.m_summer.m_canHarvest,
                SeasonKeys.Season.Fall => data.m_fall.m_canHarvest,
                SeasonKeys.Season.Winter => data.m_winter.m_canHarvest,
                _ => true,
            };
        }
    }

    [HarmonyPatch(typeof(Pickable), nameof(Pickable.GetHoverText))]
    private static class Pickable_GetHoverText_Postfix
    {
        private static void Postfix(Pickable __instance, ref string __result)
        {
            if (ConfigManager.m_enabled.Value is SeasonalTweaksPlugin.Toggle.Off) return;
            if (!HasConfigs(__instance.name.Replace("(Clone)", string.Empty))) return;
            if (SkillManager.HasOverrideLevel()) return;

            PickableData data = GetData(__instance.name.Replace("(Clone)", string.Empty));

            switch (SeasonKeys.m_currentSeason)
            {
                case SeasonKeys.Season.Spring:
                    if (data.m_spring.m_canHarvest) return;
                    __result += Localization.instance.Localize("\n <color=red>$spring_cannot_pick");
                    break;
                case SeasonKeys.Season.Summer:
                    if (data.m_summer.m_canHarvest) return;
                    __result += Localization.instance.Localize("\n <color=red>$summer_cannot_pick");
                    break;
                case SeasonKeys.Season.Fall:
                    if (data.m_fall.m_canHarvest) return;
                    __result += Localization.instance.Localize("\n <color=red>$fall_cannot_pick");
                    break;
                case SeasonKeys.Season.Winter:
                    if (data.m_winter.m_canHarvest) return;
                    __result += Localization.instance.Localize("\n <color=red>$winter_cannot_pick");
                    break;
            }
        }
    }

    private static bool HasConfigs(string prefabName) =>
        ConfigManager.m_config.Pickable.Exists(x => x.m_prefabName == prefabName);
    private static PickableData GetData(string prefabName) => ConfigManager.m_config.Pickable.Find(x => x.m_prefabName == prefabName);
    
}