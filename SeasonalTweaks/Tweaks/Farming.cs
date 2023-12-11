using System;
using System.Text.RegularExpressions;
using HarmonyLib;
using static SeasonalTweaks.SeasonalTweaksPlugin;
using static SeasonalTweaks.Tweaks.SeasonKeys;

namespace SeasonalTweaks.Tweaks;

public static class Farming
{
    [Flags]
    public enum PlantTypes
    {
        None = 0,
        Carrot = 1,
        CarrotSeed = 1 << 1,   // 2
        Turnip = 1 << 2,       // 4
        TurnipSeed = 1 << 3,   // 8
        Onion = 1 << 4,        // 16
        OnionSeed = 1 << 5,    // 32
        Barley = 1 << 6,       // 64
        Flax = 1 << 7,         // 128
        JotunPuff = 1 << 8,    // 256
        Magecap = 1 << 9       // 512
    }
    
    private static PlantTypes GetPlantType(string prefabName)
    {
        string normalizedName = Regex.Replace(prefabName, @"\(.*?\)", "");
        return (normalizedName) switch
        {
            "sapling_seedonion" => PlantTypes.OnionSeed,
            "sapling_onion" => PlantTypes.Onion,
            "sapling_turnip" => PlantTypes.Turnip,
            "sapling_seedturnip" => PlantTypes.TurnipSeed,
            "sapling_carrot" => PlantTypes.Carrot,
            "sapling_seedcarrot" => PlantTypes.CarrotSeed,
            "sapling_barley" => PlantTypes.Barley,
            "sapling_flax" => PlantTypes.Flax,
            "sapling_jotunpuffs" => PlantTypes.JotunPuff,
            "sapling_magecap" => PlantTypes.Magecap,
            _ => PlantTypes.None,
        };
    }

    [HarmonyPatch(typeof(Plant), nameof(Plant.UpdateHealth))]
    static class SeasonPlantUpdateHealth
    {
        private static bool Prefix(Plant __instance)
        {
            if (!__instance) return false;
            if (_ModEnabled.Value is Toggle.Off || _TweakFarming.Value is Toggle.Off) return true;
            PlantTypes type = GetPlantType(__instance.name);
            if (type is PlantTypes.None) return true;

            switch (season)
            {
                case Seasons.Spring:
                    if (_FarmingSpring.Value.HasFlagFast(type)) return true;
                    __instance.m_status = Plant.Status.WrongBiome;
                    return false;
                case Seasons.Summer:
                    if (_FarmingSummer.Value.HasFlagFast(type)) return true;
                    __instance.m_status = Plant.Status.WrongBiome;
                    return false;
                case Seasons.Fall:
                    if (_FarmingFall.Value.HasFlagFast(type)) return true;
                    __instance.m_status = Plant.Status.WrongBiome;
                    return false;
                case Seasons.Winter:
                    if (_FarmingWinter.Value.HasFlagFast(type)) return true;
                    __instance.m_status = Plant.Status.WrongBiome;
                    return false;
            }
            
            return true;
        }
    }
}