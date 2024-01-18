using System;
using System.Collections.Generic;
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
    
    private static bool CheckCustomPrefabs(Plant instance)
    {
        try
        {
            string normalizedName = Regex.Replace(instance.name, @"\(.*?\)", "");
            if (GetSkillLevel.GetFarmingSkillLevel() >= _LevelByPass.Value)
                return true; // Ignore logic if level is higher than threshold
            if (!YamlConfigurations.CustomData.TryGetValue(currentSeason, out List<string> currentSeasonalData))
                return true;

            if (!currentSeasonalData.Contains(normalizedName)) return true;
            instance.m_status = Plant.Status.WrongBiome;
            return false;
        }
        catch (Exception)
        {
            return true;
        }
    }
    
    private static bool CheckCustomPrefabs(Piece instance)
    {
        try
        {
            string normalizedName = Regex.Replace(instance.name, @"\(.*?\)", "");
            if (GetSkillLevel.GetFarmingSkillLevel() >= _LevelByPass.Value)
                return true; // Ignore logic if level is higher than threshold
            if (!YamlConfigurations.CustomData.TryGetValue(currentSeason, out List<string> currentSeasonalData))
                return true;

            if (!currentSeasonalData.Contains(normalizedName)) return true;
            return false;
        }
        catch (Exception)
        {
            return true;
        }
    }

    [HarmonyPatch(typeof(Plant), nameof(Plant.UpdateHealth))]
    static class SeasonPlantUpdateHealth
    {
        private static bool Prefix(Plant __instance)
        {
            if (!__instance) return false;
            if (_ModEnabled.Value is Toggle.Off || _TweakFarming.Value is Toggle.Off) return true;
            PlantTypes type = GetPlantType(__instance.name);
            if (type is PlantTypes.None)
            {
                return CheckCustomPrefabs(__instance);
            }

            float farmingLevel = GetSkillLevel.GetFarmingSkillLevel();

            switch (season)
            {
                case Seasons.Spring:
                    if (_FarmingSpring.Value.HasFlagFast(type) || farmingLevel >= _LevelByPass.Value) return true;
                    __instance.m_status = Plant.Status.WrongBiome;
                    return false;
                case Seasons.Summer:
                    if (_FarmingSummer.Value.HasFlagFast(type) || farmingLevel >= _LevelByPass.Value) return true;
                    __instance.m_status = Plant.Status.WrongBiome;
                    return false;
                case Seasons.Fall:
                    if (_FarmingFall.Value.HasFlagFast(type) || farmingLevel >= _LevelByPass.Value) return true;
                    __instance.m_status = Plant.Status.WrongBiome;
                    return false;
                case Seasons.Winter:
                    if (_FarmingWinter.Value.HasFlagFast(type) || farmingLevel >= _LevelByPass.Value) return true;
                    __instance.m_status = Plant.Status.WrongBiome;
                    return false;
            }
            
            return true;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.PlacePiece))]
    private static class PlayerPlacePiecePatch
    {
        private static bool Prefix(Player __instance, Piece piece)
        {
            if (!__instance || !piece) return false;
            if (_ModEnabled.Value is Toggle.Off || _TweakFarming.Value is Toggle.Off) return true;
            PlantTypes type = GetPlantType(piece.name);
            if (type is PlantTypes.None)
            {
                bool flag = CheckCustomPrefabs(piece);
                if (flag) return true;
                __instance.Message(MessageHud.MessageType.Center, _PlantDeniedText.Value);
                return false;
            }
            float farmingLevel = GetSkillLevel.GetFarmingSkillLevel();
            switch (season)
            {
                case Seasons.Spring:
                    if (_FarmingSpring.Value.HasFlagFast(type) || farmingLevel >= _LevelByPass.Value) return true;
                    __instance.Message(MessageHud.MessageType.Center, _PlantDeniedText.Value);
                    return false;
                case Seasons.Summer:
                    if (_FarmingSummer.Value.HasFlagFast(type) || farmingLevel >= _LevelByPass.Value) return true;
                    __instance.Message(MessageHud.MessageType.Center, _PlantDeniedText.Value);
                    return false;
                case Seasons.Fall:
                    if (_FarmingFall.Value.HasFlagFast(type) || farmingLevel >= _LevelByPass.Value) return true;
                    __instance.Message(MessageHud.MessageType.Center, _PlantDeniedText.Value);
                    return false;
                case Seasons.Winter:
                    if (_FarmingWinter.Value.HasFlagFast(type) || farmingLevel >= _LevelByPass.Value) return true;
                    __instance.Message(MessageHud.MessageType.Center, _PlantDeniedText.Value);
                    return false;
            }
            return true;
        }
    }
}