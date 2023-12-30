using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HarmonyLib;
using UnityEngine;
using static SeasonalTweaks.SeasonalTweaksPlugin;

namespace SeasonalTweaks.Tweaks;

public static class Pickables
{
    [Flags]
    public enum PickableTypes
    {
        None = 0,
        BarleyWild = 1 << 0, 
        FlaxWild = 1 << 1,
        Mushroom = 1 << 2,
        MushroomBlue = 1 << 3,
        MushroomYellow = 1 << 4,
        JotunPuffs = 1 << 5,
        Magecap = 1 << 6,
        RoyalJelly = 1 << 7,
        Thistle = 1 << 8,
        Dandelion = 1 << 9,
        SeedCarrot = 1 << 10,
        Carrot = 1 << 11,
        SeedTurnip = 1 << 12,
        Turnip = 1 << 13,
        SeedOnion = 1 << 14,
        Onion = 1 << 15,
        Barley = 1 << 16,
        Flax = 1 << 17,
        RaspberryBush = 1 << 18,
        BlueberryBush = 1 << 19,
        CloudberryBush = 1 << 20
    }

    private static PickableTypes GetPickableType(string prefabName)
    {
        string normalizedName = Regex.Replace(prefabName, @"\(.*?\)", "");
        return (normalizedName) switch
        {
            "Pickable_Barley_Wild" => PickableTypes.BarleyWild,
            "Pickable_Flax_Wild" => PickableTypes.FlaxWild,
            "Pickable_Mushroom" => PickableTypes.Mushroom,
            "Pickable_Mushroom_blue" => PickableTypes.MushroomBlue,
            "Pickable_Mushroom_yellow" => PickableTypes.MushroomYellow,
            "Pickable_Mushroom_JotunPuffs" => PickableTypes.JotunPuffs,
            "Pickable_Mushroom_Magecap" => PickableTypes.Magecap,
            "Pickable_RoyalJelly" => PickableTypes.RoyalJelly,
            "Pickable_Thistle" => PickableTypes.Thistle,
            "Pickable_Dandelion" => PickableTypes.Dandelion,
            "Pickable_Carrot" => PickableTypes.Carrot,
            "Pickable_SeedCarrot" => PickableTypes.SeedCarrot,
            "Pickable_Turnip" => PickableTypes.Turnip,
            "Pickable_SeedTurnip" => PickableTypes.SeedTurnip,
            "Pickable_Onion" => PickableTypes.Onion,
            "Pickable_SeedOnion" => PickableTypes.SeedOnion,
            "Pickable_Barley" => PickableTypes.Barley,
            "Pickable_Flax" => PickableTypes.Flax,
            "RaspberryBush" => PickableTypes.RaspberryBush,
            "BlueberryBush" => PickableTypes.BlueberryBush,
            "CloudberryBush" => PickableTypes.CloudberryBush,
            _ => PickableTypes.None
        };
    }

    private static readonly Dictionary<PickableTypes, int[]> DefaultPickableValues = new();

    [HarmonyPatch(typeof(Pickable), nameof(Pickable.Awake))]
    static class GetDefaultPickableValues
    {
        private static void Postfix(Pickable __instance)
        {
            if (!__instance) return;
            PickableTypes type = GetPickableType(__instance.name);
            if (type is PickableTypes.None) return;

            int amount = __instance.m_amount;
            int respawnTime = __instance.m_respawnTimeMinutes;

            DefaultPickableValues[type] = new[] { amount, respawnTime };
        }
    }

    private static void SetPickableValues(Pickable __instance, int amount, int respawnTime, PickableTypes type)
    {
        __instance.m_amount = _TweakPickableValues.Value is Toggle.On && amount != 0 ? amount : DefaultPickableValues[type][0];
        __instance.m_respawnTimeMinutes = _TweakPickableValues.Value is Toggle.On && respawnTime != 0 ? respawnTime : DefaultPickableValues[type][1];
    }

    [HarmonyPatch(typeof(Pickable), nameof(Pickable.Interact))]
    static class SeasonPickableInteract
    {
        private static bool Prefix(Pickable __instance, Humanoid character)
        {
            if (!__instance.m_nview.IsValid()) return false;
            if (_ModEnabled.Value is Toggle.Off) return true;
            PickableTypes type = GetPickableType(__instance.name);
            
            float farmingLevel = GetSkillLevel.GetFarmingSkillLevel();
            float foragingLevel = GetSkillLevel.GetForagingSkillLevel();

            if (type is PickableTypes.None)
            {
                return CheckCustomData(__instance, character);
            }
            
            switch (SeasonKeys.season)
            {
                case SeasonKeys.Seasons.Spring:
                    if (_PickSpring.Value.HasFlagFast(type) || farmingLevel >= _LevelByPass.Value || foragingLevel >= _LevelByPass.Value) return SetValueByType(type, __instance, SeasonKeys.season);
                    character.Message(MessageHud.MessageType.Center, _PickSpringMessage.Value);
                    return false;
                case SeasonKeys.Seasons.Summer:
                    if (_PickSummer.Value.HasFlagFast(type) || farmingLevel >= _LevelByPass.Value || foragingLevel >= _LevelByPass.Value) return SetValueByType(type, __instance, SeasonKeys.season);
                    character.Message(MessageHud.MessageType.Center, _PickSummerMessage.Value);
                    return false;
                case SeasonKeys.Seasons.Fall:
                    if (_PickFall.Value.HasFlagFast(type) || farmingLevel >= _LevelByPass.Value || foragingLevel >= _LevelByPass.Value) return SetValueByType(type, __instance, SeasonKeys.season);
                    character.Message(MessageHud.MessageType.Center, _PickFallMessage.Value);
                    return false;
                case SeasonKeys.Seasons.Winter:
                    if (_PickWinter.Value.HasFlagFast(type) || farmingLevel >= _LevelByPass.Value || foragingLevel >= _LevelByPass.Value) return SetValueByType(type, __instance, SeasonKeys.season);
                    character.Message(MessageHud.MessageType.Center, _PickWinterMessage.Value);
                    return false;
            }
            
            return true;
        }

        private static bool CheckCustomData(Pickable instance, Humanoid character)
        {
            string normalizedName = Regex.Replace(instance.name, @"\(.*?\)", "");
            if (GetSkillLevel.GetFarmingSkillLevel() >= _LevelByPass.Value) return true; // Ignore logic if level is higher than threshold
            if (GetSkillLevel.GetForagingSkillLevel() >= _LevelByPass.Value) return true;
            
            if (!YamlConfigurations.CustomData.TryGetValue(SeasonKeys.currentSeason, out List<string> currentSeasonalData)) return true;
            if (!currentSeasonalData.Contains(normalizedName)) return true;
            switch (SeasonKeys.season)
            {
                case SeasonKeys.Seasons.Spring:
                    character.Message(MessageHud.MessageType.Center, _PickSpringMessage.Value);
                    break;
                case SeasonKeys.Seasons.Summer:
                    character.Message(MessageHud.MessageType.Center, _PickSummerMessage.Value);
                    break;
                case SeasonKeys.Seasons.Fall:
                    character.Message(MessageHud.MessageType.Center, _PickFallMessage.Value);
                    break;
                case SeasonKeys.Seasons.Winter:
                    character.Message(MessageHud.MessageType.Center, _PickWinterMessage.Value);
                    break;
            }
            return false;
        }
        private static bool SetValueByType(PickableTypes type, Pickable __instance, SeasonKeys.Seasons season)
        {
            switch (type)
            {
                case PickableTypes.BarleyWild:
                    switch (season)
                    {
                        case SeasonKeys.Seasons.Spring:
                            SetPickableValues(__instance, (int)_BarleyWildAmount.Value.x, (int)_BarleyWildRespawn.Value.x, type);
                            break;
                        case SeasonKeys.Seasons.Summer:
                            SetPickableValues(__instance, (int)_BarleyWildAmount.Value.y, (int)_BarleyWildRespawn.Value.y, type);
                            break;
                        case SeasonKeys.Seasons.Fall:
                            SetPickableValues(__instance, (int)_BarleyWildAmount.Value.z, (int)_BarleyWildRespawn.Value.z, type);
                            break;
                        case SeasonKeys.Seasons.Winter:
                            SetPickableValues(__instance, (int)_BarleyWildAmount.Value.w, (int)_BarleyWildRespawn.Value.w, type);
                            break;
                    }
                    break;
                case PickableTypes.Barley:
                    switch (season)
                    {
                        case SeasonKeys.Seasons.Spring:
                            SetPickableValues(__instance, (int)_BarleyAmount.Value.x, (int)_BarleyRespawn.Value.x, type);
                            break;
                        case SeasonKeys.Seasons.Summer:
                            SetPickableValues(__instance, (int)_BarleyAmount.Value.y, (int)_BarleyRespawn.Value.y, type);
                            break;
                        case SeasonKeys.Seasons.Fall:
                            SetPickableValues(__instance, (int)_BarleyAmount.Value.z, (int)_BarleyRespawn.Value.z, type);
                            break;
                        case SeasonKeys.Seasons.Winter:
                            SetPickableValues(__instance, (int)_BarleyAmount.Value.w, (int)_BarleyRespawn.Value.w, type);
                            break;
                    }
                    break;
                case PickableTypes.Flax:
                    switch (season)
                    {
                        case SeasonKeys.Seasons.Spring:
                            SetPickableValues(__instance, (int)_FlaxAmount.Value.x, (int)_FlaxRespawn.Value.x, type);
                            break;
                        case SeasonKeys.Seasons.Summer:
                            SetPickableValues(__instance, (int)_FlaxAmount.Value.y, (int)_FlaxRespawn.Value.y, type);
                            break;
                        case SeasonKeys.Seasons.Fall:
                            SetPickableValues(__instance, (int)_FlaxAmount.Value.z, (int)_FlaxRespawn.Value.z, type);
                            break;
                        case SeasonKeys.Seasons.Winter:
                            SetPickableValues(__instance, (int)_FlaxAmount.Value.w, (int)_FlaxRespawn.Value.w, type);
                            break;
                    }
                    break;
                case PickableTypes.FlaxWild:
                    switch (season)
                    {
                        case SeasonKeys.Seasons.Spring:
                            SetPickableValues(__instance, (int)_FlaxWildAmount.Value.x, (int)_FlaxWildRespawn.Value.x, type);
                            break;
                        case SeasonKeys.Seasons.Summer:
                            SetPickableValues(__instance, (int)_FlaxWildAmount.Value.y, (int)_FlaxWildRespawn.Value.y, type);
                            break;
                        case SeasonKeys.Seasons.Fall:
                            SetPickableValues(__instance, (int)_FlaxWildAmount.Value.z, (int)_FlaxWildRespawn.Value.z, type);
                            break;
                        case SeasonKeys.Seasons.Winter:
                            SetPickableValues(__instance, (int)_FlaxWildAmount.Value.w, (int)_FlaxWildRespawn.Value.w, type);
                            break;
                    }
                    break;
                case PickableTypes.Mushroom:
                    switch (season)
                    {
                        case SeasonKeys.Seasons.Spring:
                            SetPickableValues(__instance, (int)_MushroomAmount.Value.x, (int)_MushroomRespawn.Value.x, type);
                            break;
                        case SeasonKeys.Seasons.Summer:
                            SetPickableValues(__instance, (int)_MushroomAmount.Value.y, (int)_MushroomRespawn.Value.y, type);
                            break;
                        case SeasonKeys.Seasons.Fall:
                            SetPickableValues(__instance, (int)_MushroomAmount.Value.z, (int)_MushroomRespawn.Value.z, type);
                            break;
                        case SeasonKeys.Seasons.Winter:
                            SetPickableValues(__instance, (int)_MushroomAmount.Value.w, (int)_MushroomRespawn.Value.w, type);
                            break;
                    }
                    break;
                case PickableTypes.MushroomBlue:
                    switch (season)
                    {
                        case SeasonKeys.Seasons.Spring:
                            SetPickableValues(__instance, (int)_MushroomBlueAmount.Value.x, (int)_MushroomBlueRespawn.Value.x, type);
                            break;
                        case SeasonKeys.Seasons.Summer:
                            SetPickableValues(__instance, (int)_MushroomBlueAmount.Value.y, (int)_MushroomBlueRespawn.Value.y, type);
                            break;
                        case SeasonKeys.Seasons.Fall:
                            SetPickableValues(__instance, (int)_MushroomBlueAmount.Value.z, (int)_MushroomBlueRespawn.Value.z, type);
                            break;
                        case SeasonKeys.Seasons.Winter:
                            SetPickableValues(__instance, (int)_MushroomBlueAmount.Value.w, (int)_MushroomBlueRespawn.Value.w, type);
                            break;
                    }
                    break;
                case PickableTypes.JotunPuffs:
                    switch (season)
                    {
                        case SeasonKeys.Seasons.Spring:
                            SetPickableValues(__instance, (int)_JotunPuffsAmount.Value.x, (int)_JotunPuffsRespawn.Value.x, type);
                            break;
                        case SeasonKeys.Seasons.Summer:
                            SetPickableValues(__instance, (int)_JotunPuffsAmount.Value.y, (int)_JotunPuffsRespawn.Value.y, type);
                            break;
                        case SeasonKeys.Seasons.Fall:
                            SetPickableValues(__instance, (int)_JotunPuffsAmount.Value.z, (int)_JotunPuffsRespawn.Value.z, type);
                            break;
                        case SeasonKeys.Seasons.Winter:
                            SetPickableValues(__instance, (int)_JotunPuffsAmount.Value.w, (int)_JotunPuffsRespawn.Value.w, type);
                            break;
                    }
                    break;
                case PickableTypes.MushroomYellow:
                    switch (season)
                    {
                        case SeasonKeys.Seasons.Spring:
                            SetPickableValues(__instance, (int)_MushroomYellowAmount.Value.x, (int)_MushroomYellowRespawn.Value.x, type);
                            break;
                        case SeasonKeys.Seasons.Summer:
                            SetPickableValues(__instance, (int)_MushroomYellowAmount.Value.y, (int)_MushroomYellowRespawn.Value.y, type);
                            break;
                        case SeasonKeys.Seasons.Fall:
                            SetPickableValues(__instance, (int)_MushroomYellowAmount.Value.z, (int)_MushroomYellowRespawn.Value.z, type);
                            break;
                        case SeasonKeys.Seasons.Winter:
                            SetPickableValues(__instance, (int)_MushroomYellowAmount.Value.w, (int)_MushroomYellowRespawn.Value.w, type);
                            break;
                    }
                    break;
                case PickableTypes.Magecap:
                    switch (season)
                    {
                        case SeasonKeys.Seasons.Spring:
                            SetPickableValues(__instance, (int)_MagecapAmount.Value.x, (int)_MagecapRespawn.Value.x, type);
                            break;
                        case SeasonKeys.Seasons.Summer:
                            SetPickableValues(__instance, (int)_MagecapAmount.Value.y, (int)_MagecapRespawn.Value.y, type);
                            break;
                        case SeasonKeys.Seasons.Fall:
                            SetPickableValues(__instance, (int)_MagecapAmount.Value.z, (int)_MagecapRespawn.Value.z, type);
                            break;
                        case SeasonKeys.Seasons.Winter:
                            SetPickableValues(__instance, (int)_MagecapAmount.Value.w, (int)_MagecapRespawn.Value.w, type);
                            break;
                    }
                    break;
                case PickableTypes.RoyalJelly:
                    switch (season)
                    {
                        case SeasonKeys.Seasons.Spring:
                            SetPickableValues(__instance, (int)_RoyalJellyAmount.Value.x, (int)_RoyalJellyRespawn.Value.x, type);
                            break;
                        case SeasonKeys.Seasons.Summer:
                            SetPickableValues(__instance, (int)_RoyalJellyAmount.Value.y, (int)_RoyalJellyRespawn.Value.y, type);
                            break;
                        case SeasonKeys.Seasons.Fall:
                            SetPickableValues(__instance, (int)_RoyalJellyAmount.Value.z, (int)_RoyalJellyRespawn.Value.z, type);
                            break;
                        case SeasonKeys.Seasons.Winter:
                            SetPickableValues(__instance, (int)_RoyalJellyAmount.Value.w, (int)_RoyalJellyRespawn.Value.w, type);
                            break;
                    }
                    break;
                case PickableTypes.Thistle:
                    switch (season)
                    {
                        case SeasonKeys.Seasons.Spring:
                            SetPickableValues(__instance, (int)_ThistleAmount.Value.x, (int)_ThistleRespawn.Value.x, type);
                            break;
                        case SeasonKeys.Seasons.Summer:
                            SetPickableValues(__instance, (int)_ThistleAmount.Value.y, (int)_ThistleRespawn.Value.y, type);
                            break;
                        case SeasonKeys.Seasons.Fall:
                            SetPickableValues(__instance, (int)_ThistleAmount.Value.z, (int)_ThistleRespawn.Value.z, type);
                            break;
                        case SeasonKeys.Seasons.Winter:
                            SetPickableValues(__instance, (int)_ThistleAmount.Value.w, (int)_ThistleRespawn.Value.w, type);
                            break;
                    }
                    break;
                case PickableTypes.Dandelion:
                    switch (season)
                    {
                        case SeasonKeys.Seasons.Spring:
                            SetPickableValues(__instance, (int)_DandelionAmount.Value.x, (int)_DandelionRespawn.Value.x, type);
                            break;
                        case SeasonKeys.Seasons.Summer:
                            SetPickableValues(__instance, (int)_DandelionAmount.Value.y, (int)_DandelionRespawn.Value.y, type);
                            break;
                        case SeasonKeys.Seasons.Fall:
                            SetPickableValues(__instance, (int)_DandelionAmount.Value.z, (int)_DandelionRespawn.Value.z, type);
                            break;
                        case SeasonKeys.Seasons.Winter:
                            SetPickableValues(__instance, (int)_DandelionAmount.Value.w, (int)_DandelionRespawn.Value.w, type);
                            break;
                    }
                    break;
                case PickableTypes.SeedCarrot:
                    switch (season)
                    {
                        case SeasonKeys.Seasons.Spring:
                            SetPickableValues(__instance, (int)_SeedCarrotAmount.Value.x, (int)_SeedCarrotRespawn.Value.x, type);
                            break;
                        case SeasonKeys.Seasons.Summer:
                            SetPickableValues(__instance, (int)_SeedCarrotAmount.Value.y, (int)_SeedCarrotRespawn.Value.y, type);
                            break;
                        case SeasonKeys.Seasons.Fall:
                            SetPickableValues(__instance, (int)_SeedCarrotAmount.Value.z, (int)_SeedCarrotRespawn.Value.z, type);
                            break;
                        case SeasonKeys.Seasons.Winter:
                            SetPickableValues(__instance, (int)_SeedCarrotAmount.Value.w, (int)_SeedCarrotRespawn.Value.w, type);
                            break;
                    }
                    break;
                case PickableTypes.Carrot:
                    switch (season)
                    {
                        case SeasonKeys.Seasons.Spring:
                            SetPickableValues(__instance, (int)_CarrotAmount.Value.x, (int)_CarrotRespawn.Value.x, type);
                            break;
                        case SeasonKeys.Seasons.Summer:
                            SetPickableValues(__instance, (int)_CarrotAmount.Value.y, (int)_CarrotRespawn.Value.y, type);
                            break;
                        case SeasonKeys.Seasons.Fall:
                            SetPickableValues(__instance, (int)_CarrotAmount.Value.z, (int)_CarrotRespawn.Value.z, type);
                            break;
                        case SeasonKeys.Seasons.Winter:
                            SetPickableValues(__instance, (int)_CarrotAmount.Value.w, (int)_CarrotRespawn.Value.w, type);
                            break;
                    }
                    break;
                case PickableTypes.SeedTurnip:
                    switch (season)
                    {
                        case SeasonKeys.Seasons.Spring:
                            SetPickableValues(__instance, (int)_SeedTurnipAmount.Value.x, (int)_SeedTurnipRespawn.Value.x, type);
                            break;
                        case SeasonKeys.Seasons.Summer:
                            SetPickableValues(__instance, (int)_SeedTurnipAmount.Value.y, (int)_SeedTurnipRespawn.Value.y, type);
                            break;
                        case SeasonKeys.Seasons.Fall:
                            SetPickableValues(__instance, (int)_SeedTurnipAmount.Value.z, (int)_SeedTurnipRespawn.Value.z, type);
                            break;
                        case SeasonKeys.Seasons.Winter:
                            SetPickableValues(__instance, (int)_SeedTurnipAmount.Value.w, (int)_SeedTurnipRespawn.Value.w, type);
                            break;
                    }
                    break;
                case PickableTypes.Turnip:
                    switch (season)
                    {
                        case SeasonKeys.Seasons.Spring:
                            SetPickableValues(__instance, (int)_TurnipAmount.Value.x, (int)_TurnipRespawn.Value.x, type);
                            break;
                        case SeasonKeys.Seasons.Summer:
                            SetPickableValues(__instance, (int)_TurnipAmount.Value.y, (int)_TurnipRespawn.Value.y, type);
                            break;
                        case SeasonKeys.Seasons.Fall:
                            SetPickableValues(__instance, (int)_TurnipAmount.Value.z, (int)_TurnipRespawn.Value.z, type);
                            break;
                        case SeasonKeys.Seasons.Winter:
                            SetPickableValues(__instance, (int)_TurnipAmount.Value.w, (int)_TurnipRespawn.Value.w, type);
                            break;
                    }
                    break;
                case PickableTypes.SeedOnion:
                    switch (season)
                    {
                        case SeasonKeys.Seasons.Spring:
                            SetPickableValues(__instance, (int)_SeedOnionAmount.Value.x, (int)_SeedOnionRespawn.Value.x, type);
                            break;
                        case SeasonKeys.Seasons.Summer:
                            SetPickableValues(__instance, (int)_SeedOnionAmount.Value.y, (int)_SeedOnionRespawn.Value.y, type);
                            break;
                        case SeasonKeys.Seasons.Fall:
                            SetPickableValues(__instance, (int)_SeedOnionAmount.Value.z, (int)_SeedOnionRespawn.Value.z, type);
                            break;
                        case SeasonKeys.Seasons.Winter:
                            SetPickableValues(__instance, (int)_SeedOnionAmount.Value.w, (int)_SeedOnionRespawn.Value.w, type);
                            break;
                    }
                    break;
                case PickableTypes.Onion:
                    switch (season)
                    {
                        case SeasonKeys.Seasons.Spring:
                            SetPickableValues(__instance, (int)_OnionAmount.Value.x, (int)_OnionRespawn.Value.x, type);
                            break;
                        case SeasonKeys.Seasons.Summer:
                            SetPickableValues(__instance, (int)_OnionAmount.Value.y, (int)_OnionRespawn.Value.y, type);
                            break;
                        case SeasonKeys.Seasons.Fall:
                            SetPickableValues(__instance, (int)_OnionAmount.Value.z, (int)_OnionRespawn.Value.z, type);
                            break;
                        case SeasonKeys.Seasons.Winter:
                            SetPickableValues(__instance, (int)_OnionAmount.Value.w, (int)_OnionRespawn.Value.w, type);
                            break;
                    }
                    break;
                case PickableTypes.RaspberryBush:
                    switch (season)
                    {
                        case SeasonKeys.Seasons.Spring:
                            SetPickableValues(__instance, (int)_RaspberryBushAmount.Value.x, (int)_RaspberryBushRespawn.Value.x, type);
                            break;
                        case SeasonKeys.Seasons.Summer:
                            SetPickableValues(__instance, (int)_RaspberryBushAmount.Value.y, (int)_RaspberryBushRespawn.Value.y, type);
                            break;
                        case SeasonKeys.Seasons.Fall:
                            SetPickableValues(__instance, (int)_RaspberryBushAmount.Value.z, (int)_RaspberryBushRespawn.Value.z, type);
                            break;
                        case SeasonKeys.Seasons.Winter:
                            SetPickableValues(__instance, (int)_RaspberryBushAmount.Value.w, (int)_RaspberryBushRespawn.Value.w, type);
                            break;
                    }
                    break;
                case PickableTypes.BlueberryBush:
                    switch (season)
                    {
                        case SeasonKeys.Seasons.Spring:
                            SetPickableValues(__instance, (int)_BlueberryBushAmount.Value.x, (int)_BlueberryBushRespawn.Value.x, type);
                            break;
                        case SeasonKeys.Seasons.Summer:
                            SetPickableValues(__instance, (int)_BlueberryBushAmount.Value.y, (int)_BlueberryBushRespawn.Value.y, type);
                            break;
                        case SeasonKeys.Seasons.Fall:
                            SetPickableValues(__instance, (int)_BlueberryBushAmount.Value.z, (int)_BlueberryBushRespawn.Value.z, type);
                            break;
                        case SeasonKeys.Seasons.Winter:
                            SetPickableValues(__instance, (int)_BlueberryBushAmount.Value.w, (int)_BlueberryBushRespawn.Value.w, type);
                            break;
                    }
                    break;
                case PickableTypes.CloudberryBush:
                    switch (season)
                    {
                        case SeasonKeys.Seasons.Spring:
                            SetPickableValues(__instance, (int)_CloudberryBushAmount.Value.x, (int)_CloudberryBushRespawn.Value.x, type);
                            break;
                        case SeasonKeys.Seasons.Summer:
                            SetPickableValues(__instance, (int)_CloudberryBushAmount.Value.y, (int)_CloudberryBushRespawn.Value.y, type);
                            break;
                        case SeasonKeys.Seasons.Fall:
                            SetPickableValues(__instance, (int)_CloudberryBushAmount.Value.z, (int)_CloudberryBushRespawn.Value.z, type);
                            break;
                        case SeasonKeys.Seasons.Winter:
                            SetPickableValues(__instance, (int)_CloudberryBushAmount.Value.w, (int)_CloudberryBushRespawn.Value.w, type);
                            break;
                    }
                    break;
                
            };

            return true;
        }
    }
}