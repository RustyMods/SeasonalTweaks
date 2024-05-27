using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using static SeasonalTweaks.SeasonalTweaksPlugin;

namespace SeasonalTweaks.Managers;

public static class ConfigManager
{
    public static readonly Dictionary<string, Dictionary<SeasonKeys.Season, ConfigEntry<Toggle>>> m_pickable = new();

    public static readonly Dictionary<string, Dictionary<SeasonKeys.Season, ConfigEntry<int>>>
        m_pickableAmounts = new();

    public static readonly Dictionary<string, Dictionary<SeasonKeys.Season, ConfigEntry<Toggle>>> m_plants = new();

    public static readonly Dictionary<string, Dictionary<SeasonKeys.Season, ConfigEntry<float>>>
        m_plantMaxScale = new();

    public static readonly Dictionary<string, Dictionary<SeasonKeys.Season, ConfigEntry<float>>>
        m_plantMinScale = new();

    public static readonly Dictionary<string, Dictionary<SeasonKeys.Season, ConfigEntry<float>>> m_plantGrowMax = new();

    public static readonly Dictionary<string, Dictionary<SeasonKeys.Season, ConfigEntry<float>>> m_plantGrowthTime =
        new();
    
    public static readonly Dictionary<string, Dictionary<SeasonKeys.Season, ConfigEntry<Toggle>>> m_beehives = new();

    public static readonly Dictionary<string, Dictionary<SeasonKeys.Season, ConfigEntry<int>>> m_beehive_maxHoney =
        new();

    public static readonly Dictionary<string, ConfigEntry<int>> m_pickableForagingOverride = new();
    public static readonly Dictionary<string, ConfigEntry<int>> m_pickableFarmingOverride = new();
    public static readonly Dictionary<string, ConfigEntry<int>> m_plantForagingOverride = new();
    public static readonly Dictionary<string, ConfigEntry<int>> m_plantFarmingOverride = new();

    public static readonly Dictionary<string, ConfigEntry<SeasonKeys.Season>> m_pieces = new();
    public static readonly Dictionary<string, ConfigEntry<SeasonKeys.Season>> m_items = new();
    public static readonly Dictionary<string, ConfigEntry<int>> m_itemPrices = new();

    public static ConfigEntry<Toggle> WinterFish = null!;

    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    private static class Register_Configurations
    {
        private static void Postfix(ZNetScene __instance)
        {
            if (!__instance) return;

            foreach (var prefab in __instance.m_prefabs)
            {
                CreatePickableConfigs(prefab);
                CreatePlantConfigs(prefab);
                CreateBeehiveConfigs(prefab);
                CreatePieceConfigs(prefab);
            }

            WinterFish = _plugin.config("Fish", "Winter Interactable", Toggle.Off,
                "If on, fish are interactable during winter");
        }
    }

    [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
    private static class ObjectDB_Awake_Register_Configs
    {
        private static void Postfix(ObjectDB __instance)
        {
            if (!__instance || !ZNetScene.instance) return;
            CreateItemConfigs(__instance);
        }
    }

    private static void CreatePickableConfigs(GameObject prefab)
    {
        if (prefab.TryGetComponent(out Pickable pickable))
        {
            if (!pickable.m_itemPrefab) return;
            if (!pickable.m_itemPrefab.TryGetComponent(out ItemDrop itemDrop)) return;
            if (itemDrop.m_itemData.m_shared.m_itemType is not ItemDrop.ItemData.ItemType.Consumable) return;

            var map = new Dictionary<SeasonKeys.Season, ConfigEntry<Toggle>>();
            var amounts = new Dictionary<SeasonKeys.Season, ConfigEntry<int>>();
            
            ConfigEntry<int> foragingOverride = _plugin.config(pickable.name, "Foraging Override", 50,
                new ConfigDescription($"Set the level required to override the seasons",
                    new AcceptableValueRange<int>(0, 101)));
            ConfigEntry<int> farmingOverride = _plugin.config(pickable.name, "Farming Override", 50,
                new ConfigDescription($"Set the level required to override the seasons",
                    new AcceptableValueRange<int>(0, 101)));
            m_pickableForagingOverride[pickable.name] = foragingOverride;
            m_pickableFarmingOverride[pickable.name] = farmingOverride;
            foreach (SeasonKeys.Season season in Enum.GetValues(typeof(SeasonKeys.Season)))
            {
                if (season is SeasonKeys.Season.None) continue;
                ConfigEntry<int> amount = _plugin.config(pickable.name, $"{(int)season} - {season} Amount",
                    pickable.m_amount, new ConfigDescription($"Set the amount of items harvested during {season}", new AcceptableValueRange<int>(0, 101)));
                amounts[season] = amount;
                
                ConfigEntry<Toggle> interactable = _plugin.config(pickable.name, $"{(int)season} - {season} Interactable", Toggle.On, $"If on, {pickable.name} is harvestable during {season}");
                map[season] = interactable;
                
            }
            m_pickable[pickable.name] = map;
            m_pickableAmounts[pickable.name] = amounts;
        }
    }

    private static void CreatePlantConfigs(GameObject prefab)
    {
        if (prefab.TryGetComponent(out Plant plant))
        {
            var map = new Dictionary<SeasonKeys.Season, ConfigEntry<Toggle>>();
            var maxScales = new Dictionary<SeasonKeys.Season, ConfigEntry<float>>();
            var minScales = new Dictionary<SeasonKeys.Season, ConfigEntry<float>>();
            var growthChange = new Dictionary<SeasonKeys.Season, ConfigEntry<float>>();
            var growthMax = new Dictionary<SeasonKeys.Season, ConfigEntry<float>>();

            ConfigEntry<int> foragingOverride = _plugin.config(plant.name, "Foraging Override", 50,
                new ConfigDescription("Set the level required to override the seasons",
                    new AcceptableValueRange<int>(0, 101)));
            ConfigEntry<int> farmingOverride = _plugin.config(plant.name, "Farming Override", 50,
                new ConfigDescription("Set the level required to override the seasons",
                    new AcceptableValueRange<int>(0, 101)));
            m_plantForagingOverride[plant.name] = foragingOverride;
            m_plantFarmingOverride[plant.name] = farmingOverride;
            
            ConfigEntry<Toggle> destroyIfCantGrow =
                _plugin.config(plant.name, "Destroy If Cannot Grow",
                    Toggle.Off,
                    $"If on, {plant.name} will be destroyed if it cannot grow");
            destroyIfCantGrow.SettingChanged += OnPlantDestroyChange;

            foreach (SeasonKeys.Season season in Enum.GetValues(typeof(SeasonKeys.Season)))
            {
                if (season is SeasonKeys.Season.None) continue;
                ConfigEntry<float> growTime = _plugin.config(plant.name,
                    $"{(int)season} - {season} Grow Time", plant.m_growTime,
                    new ConfigDescription($"Set the growth time of {plant.name} during {season}",
                        new AcceptableValueRange<float>(0f, 10000f)));
                growthChange[season] = growTime;

                ConfigEntry<float> growMax = _plugin.config(plant.name, $"{(int)season} - {season} Grow Time Max",
                    plant.m_growTimeMax,
                    new ConfigDescription($"Set the max growth time of {plant.name} during {season}",
                        new AcceptableValueRange<float>(0, 10000f)));

                ConfigEntry<Toggle> grows =
                    _plugin.config(plant.name, $"{(int)season} - {season} Grows",
                        Toggle.On, $"If on, {plant.name} is harvestable during {season}");
                map[season] = grows;

                ConfigEntry<float> minScale = _plugin.config(plant.name, $"{(int)season} - {season} Min Scale",
                    plant.m_minScale, new ConfigDescription($"Set the minimum scale of plant during {season}", new AcceptableValueRange<float>(0f, 10f)));
                minScales[season] = minScale;

                ConfigEntry<float> maxScale = _plugin.config(plant.name, $"{(int)season} - {season} Max Scale",
                    plant.m_maxScale, new ConfigDescription($"Set the maximum scale of plant during {season}", new AcceptableValueRange<float>(0f, 10f)));
                maxScales[season] = maxScale;
            }

            m_plants[plant.name] = map;
            m_plantMinScale[plant.name] = minScales;
            m_plantMaxScale[plant.name] = maxScales;
            m_plantGrowthTime[plant.name] = growthChange;
            m_plantGrowMax[plant.name] = growthMax;
        }
    }

    private static void CreateBeehiveConfigs(GameObject prefab)
    {
        if (prefab.TryGetComponent(out Beehive beehive))
        {
            Dictionary<SeasonKeys.Season, ConfigEntry<Toggle>> map = new();
            Dictionary<SeasonKeys.Season, ConfigEntry<int>> honey = new();
                    
            foreach (SeasonKeys.Season season in Enum.GetValues(typeof(SeasonKeys.Season)))
            {
                if (season is SeasonKeys.Season.None) continue;
                ConfigEntry<Toggle> interactable = _plugin.config(beehive.name, $"{(int)season} - {season} Interactable",
                    Toggle.On, $"If on, {beehive.name} is harvestable during {season}");
                map[season] = interactable;

                ConfigEntry<int> maxHoney = _plugin.config(beehive.name, $"{(int)season} - {season} Max Honey",
                    beehive.m_maxHoney,
                    new ConfigDescription($"Set the maximum amount of honey produced during {season}",
                        new AcceptableValueRange<int>(0, 101)));
                honey[season] = maxHoney;
            }

            m_beehives[beehive.name] = map;
            m_beehive_maxHoney[beehive.name] = honey;
        }
    }

    private static void CreatePieceConfigs(GameObject prefab)
    {
        if (prefab.TryGetComponent(out Piece piece))
        {
            if (piece.enabled) return;
            ConfigEntry<SeasonKeys.Season> active = _plugin.config(piece.name, "Available",
                SeasonKeys.Season.Summer, "Set the season which makes the piece available");
            m_pieces[piece.name] = active;
        }
    }

    private static void CreateItemConfigs(ObjectDB __instance)
    {
        foreach (var recipe in __instance.m_recipes.Where(x => x != null && !x.m_enabled && x.m_item != null))
        {
            if (recipe.m_item.name.StartsWith("Armor")) continue;
            if (recipe.m_item.name.StartsWith("HelmetHat")) continue;
            if (recipe.m_item.m_itemData.m_shared.m_itemType is ItemDrop.ItemData.ItemType.Consumable) continue;
            if (recipe.m_item.m_itemData.m_shared.m_itemType is ItemDrop.ItemData.ItemType.Shield) continue;
            if (recipe.m_item.m_itemData.m_shared.m_itemType is ItemDrop.ItemData.ItemType.Torch) continue;
            
            ConfigEntry<SeasonKeys.Season> enabled = _plugin.config(recipe.m_item.name, "Available",
                SeasonKeys.Season.Summer, "Set the season which makes the item available");
            m_items[recipe.m_item.name] = enabled;
            ConfigEntry<int> price = _plugin.config(recipe.m_item.name, "Haldor Price", 1000, "Set price for item");
            m_itemPrices[recipe.m_item.name] = price;
        }

        m_items["QueenBee"] = _plugin.config("QueenBee", "Available", SeasonKeys.Season.Summer,
            "Set the season which makes the item available");
        m_itemPrices["QueenBee"] = _plugin.config("QueenBee", "Haldor Price", 1000, "Set the price for the item");
    }

    private static void OnPlantDestroyChange(object sender, EventArgs e)
    {
        if (!ZNetScene.instance) return;
        if (sender is not ConfigEntry<Toggle> config) return;
        var name = config.Definition.Section;
        var prefab = ZNetScene.instance.GetPrefab(name);
        if (!prefab) return;
        if (!prefab.TryGetComponent(out Plant plant)) return;
        plant.m_destroyIfCantGrow = config.Value is Toggle.On;
    }
    
    
}