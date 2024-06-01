
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using ServerSync;
using UnityEngine;
using YamlDotNet.Serialization;
using static SeasonalTweaks.SeasonalTweaksPlugin;

namespace SeasonalTweaks.Managers;

public static class ConfigManager
{
    private static readonly string m_folderPath = Paths.ConfigPath + Path.DirectorySeparatorChar + "SeasonalTweaks";
    private static readonly string m_filePath = m_folderPath + Path.DirectorySeparatorChar + "configurations.yml";

    public static ConfigEntry<float> m_farmingOverride = null!;
    public static ConfigEntry<float> m_foragingOverride = null!;
    public static ConfigEntry<Toggle> m_fishOverride = null!;
    
    public static ConfigEntry<Toggle> m_enabled = null!;
    
    public static Configurations m_config = new();

    private static readonly CustomSyncedValue<string> m_serverSyncConfigs =
        new(SeasonalTweaksPlugin.ConfigSync, "SeasonalTweaks_ServerSync_Configurations", "");

    public static readonly Dictionary<string, SeasonKeys.Season> m_seasonalPieces = new Dictionary<string, SeasonKeys.Season>()
    {
        { "piece_xmascrown", SeasonKeys.Season.Winter },
        { "piece_xmastree", SeasonKeys.Season.Winter },
        { "piece_xmasgarland", SeasonKeys.Season.Winter },
        { "piece_mistletoe", SeasonKeys.Season.Winter },
        { "piece_maypole", SeasonKeys.Season.Summer },
        { "piece_jackoturnip", SeasonKeys.Season.Fall },
        { "piece_gift1", SeasonKeys.Season.Winter},
        { "piece_gift2", SeasonKeys.Season.Winter},
        { "piece_gift3", SeasonKeys.Season.Winter}
    };

    public static void CreateDirectories()
    {
        if (!Directory.Exists(m_folderPath)) Directory.CreateDirectory(m_folderPath);
    }

    public static void InitStaticConfigs()
    {
        m_enabled = _plugin.config("1 - General", "Enabled", Toggle.On, "If on, plugin is enabled");
        m_farmingOverride = _plugin.config("Settings", "Farming Override", 50f,
            new ConfigDescription("Set the level required for seasons to be overridden",
                new AcceptableValueRange<float>(0f, 101f)));
        m_foragingOverride = _plugin.config("Settings", "Foraging Override", 50f,
            new ConfigDescription("Set the level required for seasons to be overridden",
                new AcceptableValueRange<float>(0f, 101f)));
        m_fishOverride = _plugin.config("Fish", "Winter Interactable", Toggle.Off,
            "If on, fish are interactable during winter");
    }

    private static bool m_readFile;

    public static void ReadConfigFile()
    {
        if (!File.Exists(m_filePath)) return;
        SeasonalTweaksLogger.LogDebug("Reading config file");
        try
        {
            var deserializer = new DeserializerBuilder().Build();
            var file = File.ReadAllText(m_filePath);
            var data = deserializer.Deserialize<Configurations>(file);
            m_config = data;
            if (ZNet.instance && ZNet.instance.IsServer()) m_serverSyncConfigs.Value = file;
            m_readFile = true;
        }
        catch
        {
            SeasonalTweaksLogger.LogWarning("Failed to deserialize configurations YML");
        }
    }

    [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
    private static class ObjectDB_Awake_Register_Configs
    {
        private static void Postfix(ObjectDB __instance)
        {
            if (!__instance || !ZNetScene.instance) return;

            var hammer = __instance.GetItemPrefab("Hammer");
            if (hammer)
            {
                if (hammer.TryGetComponent(out ItemDrop component))
                {
                    foreach (var kvp in m_seasonalPieces)
                    {
                        var prefab = ZNetScene.instance.GetPrefab(kvp.Key);
                        if (!prefab) continue;
                        if (component.m_itemData.m_shared.m_buildPieces.m_pieces.Contains(prefab)) continue;
                        component.m_itemData.m_shared.m_buildPieces.m_pieces.Add(prefab);
                    }
                }
                
            }
            
            if (File.Exists(m_filePath) && m_readFile) return;
            SeasonalTweaksLogger.LogDebug("Creating config file");
            var serializer = new SerializerBuilder().Build();
            Configurations configs = new();

            foreach (var prefab in ZNetScene.instance.m_prefabs)
            {
                if (CreatePickableConfigs(prefab, out PickableData pickableData))
                {
                    configs.Pickable.Add(pickableData);
                }
                if (CreatePlantConfigs(prefab, out PlantData plantData))
                {
                    configs.Plants.Add(plantData);
                }
                if (CreateBeehiveConfigs(prefab, out BeeHiveData beehiveData))
                {
                    configs.Beehives.Add(beehiveData);
                }
                if (CreatePieceConfigs(prefab, out PieceData pieceData))
                {
                    configs.Pieces.Add(pieceData);
                }
            }

            foreach (var recipe in __instance.m_recipes.Where(x => x != null && !x.m_enabled && x.m_item != null))
            {
                if (CreateItemConfigs(recipe, out ItemData itemData))
                {
                    configs.Items.Add(itemData);
                }
            }
            configs.Items.Add(new ItemData(){m_prefabName = "QueenBee", m_price = 999, m_season = SeasonKeys.Season.Summer});
            m_config = configs;
            var data = serializer.Serialize(configs);
            if (ZNet.instance && ZNet.instance.IsServer()) m_serverSyncConfigs.Value = data;
            if (!File.Exists(m_filePath)) File.WriteAllText(m_filePath, data);
        }
    }

    private static bool CreatePickableConfigs(GameObject prefab, out PickableData data)
    {
        data = new ();
        if (!prefab.TryGetComponent(out Pickable pickable)) return false;
        if (!pickable.m_itemPrefab) return false;
        if (!pickable.m_itemPrefab.TryGetComponent(out ItemDrop itemDrop)) return false;
        if (itemDrop.m_itemData.m_shared.m_itemType is not ItemDrop.ItemData.ItemType.Consumable) return false;

        data.m_prefabName = pickable.name;
        data.m_spring.m_amount = pickable.m_amount;
        data.m_summer.m_amount = pickable.m_amount;
        data.m_fall.m_amount = pickable.m_amount;
        data.m_winter.m_amount = pickable.m_amount;
        data.m_winter.m_canHarvest = false;
        return true;

    }

    private static bool CreatePlantConfigs(GameObject prefab, out PlantData data)
    {
        data = new();
        if (!prefab.TryGetComponent(out Plant plant)) return false;
        data.m_prefabName = plant.name;

        data.m_spring.m_growTime = plant.m_growTime;
        data.m_spring.m_growTimeMax = plant.m_growTimeMax;
        data.m_spring.m_minScale = plant.m_minScale;
        data.m_spring.m_maxScale = plant.m_maxScale;

        data.m_summer.m_growTime = plant.m_growTime;
        data.m_summer.m_growTimeMax = plant.m_growTimeMax;
        data.m_summer.m_minScale = plant.m_minScale;
        data.m_summer.m_maxScale = plant.m_maxScale;
        
        data.m_fall.m_growTime = plant.m_growTime;
        data.m_fall.m_growTimeMax = plant.m_growTimeMax;
        data.m_fall.m_minScale = plant.m_minScale;
        data.m_fall.m_maxScale = plant.m_maxScale;
        
        data.m_winter.m_growTime = plant.m_growTime;
        data.m_winter.m_growTimeMax = plant.m_growTimeMax;
        data.m_winter.m_minScale = plant.m_minScale;
        data.m_winter.m_maxScale = plant.m_maxScale;
        data.m_winter.m_canHarvest = false;
        
        return true;

    }

    private static bool CreateBeehiveConfigs(GameObject prefab, out BeeHiveData data)
    {
        data = new();
        if (!prefab.TryGetComponent(out Beehive beehive)) return false;
        data.m_prefabName = beehive.name;
        data.m_spring.m_maxHoney = beehive.m_maxHoney;
        data.m_summer.m_maxHoney = beehive.m_maxHoney;
        data.m_fall.m_maxHoney = beehive.m_maxHoney;
        data.m_winter.m_maxHoney = beehive.m_maxHoney;
        data.m_winter.m_canHarvest = false;
        return true;

    }

    private static bool CreatePieceConfigs(GameObject prefab, out PieceData data)
    {
        data = new();
        if (!prefab.TryGetComponent(out Piece piece)) return false;
        if (!m_seasonalPieces.TryGetValue(prefab.name, out SeasonKeys.Season season)) return false;
        data.m_prefabName = piece.name;
        data.m_season = season;
        return true;

    }

    private static bool CreateItemConfigs(Recipe recipe, out ItemData data)
    {
        data = new();
        if (recipe.m_item.name.StartsWith("Armor")) return false;
        if (recipe.m_item.name.StartsWith("HelmetHat")) return false;
        if (recipe.m_item.m_itemData.m_shared.m_itemType 
            is ItemDrop.ItemData.ItemType.Consumable 
            or ItemDrop.ItemData.ItemType.Shield 
            or ItemDrop.ItemData.ItemType.Torch) return false;

        data.m_prefabName = recipe.m_item.name;
        return true;
    }

    public static void StartFileWatch()
    {
        FileSystemWatcher watcher = new FileSystemWatcher(m_folderPath, "*.yml")
        {
            EnableRaisingEvents = true,
            IncludeSubdirectories = false,
            SynchronizingObject = ThreadingHelper.SynchronizingObject
        };
        watcher.Created += OnConfigurationChange;
        watcher.Changed += OnConfigurationChange;
        watcher.Deleted += OnConfigurationChange;
    }

    private static void OnConfigurationChange(object sender, FileSystemEventArgs e)
    {
        if (!ZNet.instance) return;
        if (!File.Exists(m_filePath)) return;
        SeasonalTweaksLogger.LogDebug("Read configurations called");
        if (!ZNet.instance.IsServer())
        {
            SeasonalTweaksLogger.LogDebug("Canceled, user is not server");
            return;
        }
        
        ReadConfigFile();
    }
    public static void StartServerConfigurationWatcher()
    {
        m_serverSyncConfigs.ValueChanged += () =>
        {
            if (m_serverSyncConfigs.Value.IsNullOrWhiteSpace()) return;
            SeasonalTweaksLogger.LogDebug("Server configurations read called");
            var deserializer = new DeserializerBuilder().Build();
            var data = deserializer.Deserialize<Configurations>(m_serverSyncConfigs.Value);
            m_config = data;
        };
    }
    
}