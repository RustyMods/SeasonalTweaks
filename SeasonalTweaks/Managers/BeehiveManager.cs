
using HarmonyLib;

namespace SeasonalTweaks.Managers;

public static class BeehiveManager
{
    [HarmonyPatch(typeof(Beehive), nameof(Beehive.Interact))]
    private static class Beehive_Interact_Prefix
    {
        private static bool Prefix(Beehive __instance)
        {
            if (ConfigManager.m_enabled.Value is SeasonalTweaksPlugin.Toggle.Off) return true;
            string prefabName = __instance.name.Replace("(Clone)", string.Empty);
            bool canInteract = !HasConfigs(prefabName) || CanInteract(prefabName);
            if (!canInteract)
            {
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_cannot_interact");
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Beehive), nameof(Beehive.Awake))]
    private static class Beehive_Awake_Postfix
    {
        private static void Postfix(Beehive __instance)
        {
            if (ConfigManager.m_enabled.Value is SeasonalTweaksPlugin.Toggle.Off) return;
            string prefabName = __instance.name.Replace("(Clone)", string.Empty);
            if (!HasConfigs(prefabName)) return;
            if (CanInteract(prefabName)) return;
            __instance.m_maxHoney = GetMaxHoney(GetData(prefabName));
        }
    }

    private static bool HasConfigs(string prefabName) =>
        ConfigManager.m_config.Beehives.Exists(x => x.m_prefabName == prefabName);

    private static BeeHiveData GetData(string prefabName) =>
        ConfigManager.m_config.Beehives.Find(x => x.m_prefabName == prefabName);

    private static bool CanInteract(string prefabName)
    {
        var data = GetData(prefabName);
        switch (SeasonKeys.m_currentSeason)
        {
            case SeasonKeys.Season.Spring:
                return data.m_spring.m_canHarvest;
            case SeasonKeys.Season.Summer:
                return data.m_summer.m_canHarvest;
            case SeasonKeys.Season.Fall:
                return data.m_fall.m_canHarvest;
            case SeasonKeys.Season.Winter:
                return data.m_winter.m_canHarvest;
            default:
                return true;
        }
    }

    private static int GetMaxHoney(BeeHiveData data)
    {
        return SeasonKeys.m_currentSeason switch
        {
            SeasonKeys.Season.Spring => data.m_spring.m_maxHoney,
            SeasonKeys.Season.Summer => data.m_summer.m_maxHoney,
            SeasonKeys.Season.Fall => data.m_fall.m_maxHoney,
            SeasonKeys.Season.Winter => data.m_winter.m_maxHoney,
            _ => 4
        };
    }
}