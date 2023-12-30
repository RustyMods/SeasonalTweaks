using System.Collections.Generic;
using System.IO;
using BepInEx;
using UnityEngine;
using YamlDotNet.Core.Tokens;
using YamlDotNet.Serialization;

namespace SeasonalTweaks.Tweaks;

public static class YamlConfigurations
{
    private static readonly string folderPath = Paths.ConfigPath + Path.DirectorySeparatorChar + "SeasonalTweaks";
    private static readonly string CustomFilePath = folderPath + Path.DirectorySeparatorChar + "CustomPrefabs.yml";

    public static Dictionary<string, List<string>> CustomData = new()
    {
        {"season_spring", new List<string>()},
        {"season_summer",new List<string>()},
        {"season_fall",new List<string>()},
        {"season_winter",new List<string>()}
    };
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

    private static void ReadYamlConfigs()
    {
        IDeserializer deserializer = new DeserializerBuilder().Build();
        Dictionary<string, List<string>> FarmingData = deserializer.Deserialize<Dictionary<string, List<string>>>(File.ReadAllText(CustomFilePath));
        CustomData = FarmingData;
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
            "#### Notes",
            "Have fun - Rusty"
        };
        File.WriteAllLines(filePath, tutorial);
    }
}