﻿using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using HarmonyLib;
using ServerSync;
using UnityEngine;
using YamlDotNet.Serialization;

namespace SeasonalTweaks.Tweaks;

public static class YamlConfigurations
{
    private static readonly CustomSyncedValue<string> SyncedData = new(SeasonalTweaksPlugin.ConfigSync, "SyncedData", "");
    private static readonly CustomSyncedValue<string> SyncedValues = new(SeasonalTweaksPlugin.ConfigSync, "SyncedValues", "");
    
    private static readonly string folderPath = Paths.ConfigPath + Path.DirectorySeparatorChar + "SeasonalTweaks";
    private static readonly string CustomFilePath = folderPath + Path.DirectorySeparatorChar + "CustomPrefabs.yml";
    private static readonly string CustomValuePath = folderPath + Path.DirectorySeparatorChar + "CustomValues.yml";

    public static Dictionary<string, List<string>> CustomData = new()
    {
        {"season_spring", new List<string>()},
        {"season_summer",new List<string>()},
        {"season_fall",new List<string>()},
        {"season_winter",new List<string>()}
    };

    public static void InitCustomFileSystemWatch()
    {
        if (SeasonalTweaksPlugin.workingAsType is not SeasonalTweaksPlugin.WorkingAs.Server)
        {
            SyncedData.ValueChanged += OnValueChanged;
            SyncedValues.ValueChanged += OnValueChanged;
            return;
        }
        FileSystemWatcher fileWatcher = new FileSystemWatcher(folderPath)
        {
            Filter = "*.yml",
            EnableRaisingEvents = true,
            IncludeSubdirectories = true,
            SynchronizingObject = ThreadingHelper.SynchronizingObject,
            NotifyFilter = NotifyFilters.LastWrite
        };
        fileWatcher.Created += OnChanged;
        fileWatcher.Changed += OnChanged;
        fileWatcher.Deleted += OnDeleted;
    }

    private static void OnValueChanged()
    {
        UpdatedSyncedData = false;
    }

    private static void OnChanged(object sender, FileSystemEventArgs e)
    {
        UpdatedSyncedData = false;
    }

    private static void OnDeleted(object sender, FileSystemEventArgs e)
    {
        if (e.FullPath != CustomValuePath) return;
        if (!ZoneSystem.instance) return;
        GetCustomPlantValues(ZoneSystem.instance);
    }
    public static void InitYamlConfigurations()
    {
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
        WriteTutorial();

        if (!File.Exists(CustomFilePath))
        {
            ISerializer serializer = new SerializerBuilder().Build();

            string data = serializer.Serialize(CustomData);
            File.WriteAllText(CustomFilePath, data);
        }
        
        ReadYamlConfigs();
    }
    private static void GetCustomPlantValues(ZoneSystem instance)
    {
        if (!instance) return;
        if (!File.Exists(CustomValuePath))
        {
            ISerializer serializer = new SerializerBuilder().Build();

            GetCustomPlants();
            string data = serializer.Serialize(customPickableData);
            File.WriteAllText(CustomValuePath, data);
        }
    }

    [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Start))]
    private static class ZoneSystemStartPatch
    {
        private static void Postfix(ZoneSystem __instance)
        {
            if (!__instance) return;
            GetCustomPlantValues(__instance);
        }
    }

    public static bool UpdatedSyncedData = false;
    public static void UpdateSyncedData()
    {
        if (UpdatedSyncedData) return;
        if (SeasonalTweaksPlugin.workingAsType is SeasonalTweaksPlugin.WorkingAs.Server or SeasonalTweaksPlugin.WorkingAs.Both)
        {
            string RawRata = File.ReadAllText(CustomFilePath);
            string RawValues = File.ReadAllText(CustomValuePath);

            if (RawValues.IsNullOrWhiteSpace()) return;
            SyncedData.Value = RawRata;
            SyncedValues.Value = RawValues;
        }
        else
        {
            if (SyncedValues.Value.IsNullOrWhiteSpace()) return;
            IDeserializer deserializer = new DeserializerBuilder().Build();
            CustomData = deserializer.Deserialize<Dictionary<string, List<string>>>(SyncedData.Value);
            customPickableData = deserializer.Deserialize<List<PickableValueConfigurations>>(SyncedValues.Value);
        }
        UpdatedSyncedData = true;
    }
    private static void ReadYamlConfigs()
    {
        string RawData = File.ReadAllText(CustomFilePath);
        
        IDeserializer deserializer = new DeserializerBuilder().Build();
        Dictionary<string, List<string>> FarmingData = deserializer.Deserialize<Dictionary<string, List<string>>>(RawData);
        CustomData = FarmingData;
        
        if (File.Exists(CustomValuePath))
        {
            string RawValues = File.ReadAllText(CustomValuePath);
            customPickableData = deserializer.Deserialize<List<PickableValueConfigurations>>(RawValues);
        }
    }
    private static void WriteTutorial()
    {
        string filePath = folderPath + Path.DirectorySeparatorChar + "README.md";
        if (File.Exists(filePath)) return;
        List<string> tutorial = new()
        {
            "# Yaml Configurations",
            "Use these files to add custom prefabs to control seasonally.",
            "Add the prefab names as strings into the brackets of the season you wish to NOT allow.",
            "#### ",
            "Example: ",
            "```yml",
            "season_spring: ['CottonWoodSapling_RtD','OakSapling_RtD']",
            "season_summer: []",
            "season_fall: []",
            "season_winter:",
            "- BlossomSapling_RtD",
            "- ThinPineSapling_RtD",
            "```",
            "#### ",
            "You will notice that the YML accepts two formats for adding to the list.",
            "Direct string inputs into the brackets or line inputs.",
            "#### ",
            "CustomValues.yml will automatically generate if file is missing once game is loaded.",
            "Once generated, you can open file and tweak the values.",
            "If you ever add new custom pickables, simply delete the file, and let regenerate, then tweak again.",
            "#### Notes",
            "Have fun - Rusty"
        };
        File.WriteAllLines(filePath, tutorial);
    }

    [Serializable]
    public class PickableValueConfigurations
    {
        public string prefab_name = null!;
        public int spring_amount;
        public int summer_amount;
        public int fall_amount;
        public int winter_amount;
        public int spring_respawn_time;
        public int summer_respawn_time;
        public int fall_respawn_time;
        public int winter_respawn_time;
    }

    public static List<PickableValueConfigurations> customPickableData = new();
    
    private static void GetCustomPlants()
    {
        customPickableData.Clear();
        HashSet<string> uniquePrefabNames = new();
        GameObject[] AllObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject prefab in AllObjects)
        {
            if (!prefab.TryGetComponent(out Pickable pickable)) continue;
            if (!pickable.m_itemPrefab) continue;
            if (uniquePrefabNames.Contains(pickable.m_itemPrefab.name)) continue;
            PickableValueConfigurations data = new PickableValueConfigurations()
            {
                prefab_name = pickable.m_itemPrefab.name,
                spring_amount = pickable.m_amount,
                summer_amount = pickable.m_amount,
                fall_amount = pickable.m_amount,
                winter_amount = pickable.m_amount,
                spring_respawn_time = pickable.m_respawnTimeMinutes,
                summer_respawn_time = pickable.m_respawnTimeMinutes,
                fall_respawn_time = pickable.m_respawnTimeMinutes,
                winter_respawn_time = pickable.m_respawnTimeMinutes
            };
            uniquePrefabNames.Add(pickable.m_itemPrefab.name);
            if (!customPickableData.Contains(data)) customPickableData.Add(data);
        }
    }
}