using System.Collections.Generic;
using System.Text.RegularExpressions;
using HarmonyLib;
using static SeasonalTweaks.SeasonalTweaksPlugin;

namespace SeasonalTweaks.Tweaks;

public static class Destructible
{
    private enum DestructibleTypes 
    {
        None,
        GuckSackSmall,
        GuckSack
    }

    private static DestructibleTypes GetDestructibleType(string prefabName)
    {
        string normalizedName = Regex.Replace(prefabName, @"\(.*?\)", "");
        return (normalizedName) switch
        {
            "GuckSack_small" => DestructibleTypes.GuckSackSmall,
            "GuckSack" => DestructibleTypes.GuckSack,
            _ => DestructibleTypes.None,
        };
    }

    private static readonly Dictionary<DestructibleTypes, int[]> DestructibleDefaultValues = new();
    
    [HarmonyPatch(typeof(DropOnDestroyed), nameof(DropOnDestroyed.Awake))]
    static class SaveDefaultDropOnDestroyedValues
    {
        private static void Prefix(DropOnDestroyed __instance)
        {
            if (!__instance) return;
            if (_ModEnabled.Value is Toggle.Off) return;
            DestructibleTypes type = GetDestructibleType(__instance.name);

            if (type is DestructibleTypes.None) return;

            int min = __instance.m_dropWhenDestroyed.m_dropMin;
            int max = __instance.m_dropWhenDestroyed.m_dropMax;

            DestructibleDefaultValues[type] = new[] { min, max };
        }
    }

    [HarmonyPatch(typeof(DropOnDestroyed), nameof(DropOnDestroyed.OnDestroyed))]
    static class ModifyDropOnDestroyValues
    {
        private static void Prefix(DropOnDestroyed __instance)
        {
            if (!__instance) return;
            if (_ModEnabled.Value is Toggle.Off) return;
            DestructibleTypes type = GetDestructibleType(__instance.name);

            if (type is DestructibleTypes.None) return;
            
            SetDestructibleValues(type, __instance);
        }
    }
    
    private static void SetDestructibleValues(DestructibleTypes type, DropOnDestroyed __instance)
    {
        int defaultMin = DestructibleDefaultValues[type][0];
        int defaultMax = DestructibleDefaultValues[type][1];
        bool enabled = _ModEnabled.Value is Toggle.On && _TweakDestructibleValues.Value is Toggle.On;
        switch (type)
        {
            case DestructibleTypes.GuckSack:
                switch (SeasonKeys.season)
                {
                    case SeasonKeys.Seasons.Spring:
                    __instance.m_dropWhenDestroyed.m_dropMin = enabled && (int)_GuckSackMinAmount.Value.x != 0 
                        ? (int)_GuckSackMinAmount.Value.x : defaultMin;
                    __instance.m_dropWhenDestroyed.m_dropMax = enabled && (int)_GuckSackMaxAmount.Value.x != 0 
                        ? (int)_GuckSackMaxAmount.Value.x : defaultMax;
                    break;
                    case SeasonKeys.Seasons.Summer:
                        __instance.m_dropWhenDestroyed.m_dropMin = enabled && (int)_GuckSackMinAmount.Value.y != 0 
                            ? (int)_GuckSackMinAmount.Value.y : defaultMin;
                        __instance.m_dropWhenDestroyed.m_dropMax = enabled && (int)_GuckSackMaxAmount.Value.y != 0 
                            ? (int)_GuckSackMaxAmount.Value.y : defaultMax;
                        break;
                    case SeasonKeys.Seasons.Fall:
                        __instance.m_dropWhenDestroyed.m_dropMin = enabled && (int)_GuckSackMinAmount.Value.z != 0 
                            ? (int)_GuckSackMinAmount.Value.z : defaultMin;
                        __instance.m_dropWhenDestroyed.m_dropMax = enabled && (int)_GuckSackMaxAmount.Value.z != 0 
                            ? (int)_GuckSackMaxAmount.Value.z : defaultMax;
                        break;
                    case SeasonKeys.Seasons.Winter:
                        __instance.m_dropWhenDestroyed.m_dropMin = enabled && (int)_GuckSackMinAmount.Value.w != 0 
                            ? (int)_GuckSackMinAmount.Value.w : defaultMin;
                        __instance.m_dropWhenDestroyed.m_dropMax = enabled && (int)_GuckSackMaxAmount.Value.w != 0 
                            ? (int)_GuckSackMaxAmount.Value.w : defaultMax;
                        break;
                }
                break;
            case DestructibleTypes.GuckSackSmall:
                switch (SeasonKeys.season)
                {
                    case SeasonKeys.Seasons.Spring:
                        __instance.m_dropWhenDestroyed.m_dropMin = enabled && (int)_GuckSackSmallMinAmount.Value.x != 0 ? (int)_GuckSackSmallMinAmount.Value.x : defaultMin;
                        __instance.m_dropWhenDestroyed.m_dropMax = enabled && (int)_GuckSackSmallMaxAmount.Value.x != 0 ? (int)_GuckSackSmallMaxAmount.Value.x : defaultMax;
                        break;
                    case SeasonKeys.Seasons.Summer:
                        __instance.m_dropWhenDestroyed.m_dropMin = enabled && (int)_GuckSackSmallMinAmount.Value.y != 0 ? (int)_GuckSackSmallMinAmount.Value.y : defaultMin;
                        __instance.m_dropWhenDestroyed.m_dropMax = enabled && (int)_GuckSackSmallMaxAmount.Value.y != 0 ? (int)_GuckSackSmallMaxAmount.Value.y : defaultMax;
                        break;
                    case SeasonKeys.Seasons.Fall:
                        __instance.m_dropWhenDestroyed.m_dropMin = enabled && (int)_GuckSackSmallMinAmount.Value.z != 0 ? (int)_GuckSackSmallMinAmount.Value.z : defaultMin;
                        __instance.m_dropWhenDestroyed.m_dropMax = enabled && (int)_GuckSackSmallMaxAmount.Value.z != 0 ? (int)_GuckSackSmallMaxAmount.Value.z : defaultMax;
                        break;
                    case SeasonKeys.Seasons.Winter:
                        __instance.m_dropWhenDestroyed.m_dropMin = enabled && (int)_GuckSackSmallMinAmount.Value.w != 0 ? (int)_GuckSackSmallMinAmount.Value.w : defaultMin;
                        __instance.m_dropWhenDestroyed.m_dropMax = enabled && (int)_GuckSackSmallMaxAmount.Value.w != 0 ? (int)_GuckSackSmallMaxAmount.Value.w : defaultMax;
                        break;
                }
                break;
        }
    }
}