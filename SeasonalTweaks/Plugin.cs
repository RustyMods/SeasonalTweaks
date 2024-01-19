using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using SeasonalTweaks.Tweaks;
using ServerSync;
using UnityEngine;
using UnityEngine.Rendering;
using static SeasonalTweaks.Tweaks.SeasonKeys;

namespace SeasonalTweaks
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [BepInDependency("RustyMods.Seasonality", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("org.bepinex.plugins.foraging", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("org.bepinex.plugins.farming", BepInDependency.DependencyFlags.SoftDependency)]

    public class SeasonalTweaksPlugin : BaseUnityPlugin
    {
        #region Settings
        internal const string ModName = "SeasonalTweaks";
        internal const string ModVersion = "1.0.5";
        internal const string Author = "RustyMods";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource SeasonalTweaksLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);
        public static readonly ConfigSync ConfigSync = new(ModGUID) { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };
        #endregion
        public enum Toggle
        {
            On = 1,
            Off = 0
        }
        public enum WorkingAs { Client, Server, Both }
        public static WorkingAs workingAsType;

        public static bool ForagingLoaded;
        public static bool FarmingLoaded;

        public void Awake()
        {
            _serverConfigLocked = config("1 - General", "Lock Configuration", Toggle.On,
                "If on, the configuration is locked and can be changed by server admins only.");
            _ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);
            
            workingAsType = SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null
                ? WorkingAs.Server
                : WorkingAs.Client;
            
            InitGeneralConfigs();
            InitPickableConfigs();
            InitDestructibleConfigs();
            InitBeeHiveConfig();
            
            YamlConfigurations.InitYamlConfigurations();
            YamlConfigurations.InitCustomFileSystemWatch();

            if (Chainloader.PluginInfos.ContainsKey("org.bepinex.plugins.foraging")) ForagingLoaded = true;
            if (Chainloader.PluginInfos.ContainsKey("org.bepinex.plugins.farming")) FarmingLoaded = true;

            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();
        }

        // public void FixedUpdate()
        // {
        //     UpdateSeasonalKeys();
        //     if (YamlConfigurations.HasRun) return;
        //     YamlConfigurations.UpdateSyncedData();
        // }

        private void OnDestroy()
        {
            Config.Save();
        }

        private void SetupWatcher()
        {
            FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                SeasonalTweaksLogger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                SeasonalTweaksLogger.LogError($"There was an issue loading your {ConfigFileName}");
                SeasonalTweaksLogger.LogError("Please check your config entries for spelling and format!");
            }
        }
        
        #region ConfigOptions

        private static ConfigEntry<Toggle> _serverConfigLocked = null!;
        
        #region Custom Configs

        public static ConfigEntry<Toggle> _ModEnabled = null!;
        public static ConfigEntry<int> _LevelByPass = null!;
        public static ConfigEntry<Toggle> _SeasonalItems = null!;
        public static ConfigEntry<Toggle> _UseYMLCustomValues = null!;

        #region Plantables

        public static ConfigEntry<Toggle> _TweakFarming = null!;
        public static ConfigEntry<String> _PlantDeniedText = null!;

        public static ConfigEntry<Farming.PlantTypes> _FarmingSpring = null!;
        public static ConfigEntry<Farming.PlantTypes> _FarmingSummer = null!;
        public static ConfigEntry<Farming.PlantTypes> _FarmingFall = null!;
        public static ConfigEntry<Farming.PlantTypes> _FarmingWinter = null!;
        #endregion
        #region Pickables

        public static ConfigEntry<Pickables.PickableTypes> _PickSpring = null!;
        public static ConfigEntry<Pickables.PickableTypes> _PickSummer = null!;
        public static ConfigEntry<Pickables.PickableTypes> _PickFall = null!;
        public static ConfigEntry<Pickables.PickableTypes> _PickWinter = null!;

        public static ConfigEntry<Toggle> _FishPickableWinter = null!;
        public static ConfigEntry<string> _FishNotPickableMessage = null!;

        #endregion
        #region Messages

        public static ConfigEntry<string> _PickSpringMessage = null!;
        public static ConfigEntry<string> _PickSummerMessage = null!;
        public static ConfigEntry<string> _PickFallMessage = null!;
        public static ConfigEntry<string> _PickWinterMessage = null!;

        #endregion
        #region Modify Pickable Values

        public static ConfigEntry<Toggle> _TweakPickableValues = null!;
        
        public static ConfigEntry<Vector4> _BarleyWildAmount = null!;
        public static ConfigEntry<Vector4> _BarleyWildRespawn = null!;

        public static ConfigEntry<Vector4> _FlaxWildAmount = null!;
        public static ConfigEntry<Vector4> _FlaxWildRespawn = null!;

        public static ConfigEntry<Vector4> _MushroomAmount = null!;
        public static ConfigEntry<Vector4> _MushroomRespawn = null!;

        public static ConfigEntry<Vector4> _MushroomBlueAmount = null!;
        public static ConfigEntry<Vector4> _MushroomBlueRespawn = null!;

        public static ConfigEntry<Vector4> _MushroomYellowAmount = null!;
        public static ConfigEntry<Vector4> _MushroomYellowRespawn = null!;

        public static ConfigEntry<Vector4> _JotunPuffsAmount = null!;
        public static ConfigEntry<Vector4> _JotunPuffsRespawn = null!;

        public static ConfigEntry<Vector4> _MagecapAmount = null!;
        public static ConfigEntry<Vector4> _MagecapRespawn = null!;

        public static ConfigEntry<Vector4> _RoyalJellyAmount = null!;
        public static ConfigEntry<Vector4> _RoyalJellyRespawn = null!;

        public static ConfigEntry<Vector4> _ThistleAmount = null!;
        public static ConfigEntry<Vector4> _ThistleRespawn = null!;

        public static ConfigEntry<Vector4> _DandelionAmount = null!;
        public static ConfigEntry<Vector4> _DandelionRespawn = null!;

        public static ConfigEntry<Vector4> _SeedCarrotAmount = null!;
        public static ConfigEntry<Vector4> _SeedCarrotRespawn = null!;

        public static ConfigEntry<Vector4> _CarrotAmount = null!;
        public static ConfigEntry<Vector4> _CarrotRespawn = null!;

        public static ConfigEntry<Vector4> _SeedTurnipAmount = null!;
        public static ConfigEntry<Vector4> _SeedTurnipRespawn = null!;

        public static ConfigEntry<Vector4> _TurnipAmount = null!;
        public static ConfigEntry<Vector4> _TurnipRespawn = null!;

        public static ConfigEntry<Vector4> _SeedOnionAmount = null!;
        public static ConfigEntry<Vector4> _SeedOnionRespawn = null!;

        public static ConfigEntry<Vector4> _OnionAmount = null!;
        public static ConfigEntry<Vector4> _OnionRespawn = null!;

        public static ConfigEntry<Vector4> _BarleyAmount = null!;
        public static ConfigEntry<Vector4> _BarleyRespawn = null!;

        public static ConfigEntry<Vector4> _FlaxAmount = null!;
        public static ConfigEntry<Vector4> _FlaxRespawn = null!;

        public static ConfigEntry<Vector4> _RaspberryBushAmount = null!;
        public static ConfigEntry<Vector4> _RaspberryBushRespawn = null!;

        public static ConfigEntry<Vector4> _BlueberryBushAmount = null!;
        public static ConfigEntry<Vector4> _BlueberryBushRespawn = null!;

        public static ConfigEntry<Vector4> _CloudberryBushAmount = null!;
        public static ConfigEntry<Vector4> _CloudberryBushRespawn = null!;

        #endregion
        #region Modify Destructible Values

        public static ConfigEntry<Toggle> _TweakDestructibleValues = null!;
        public static ConfigEntry<Vector4> _GuckSackMinAmount = null!;
        public static ConfigEntry<Vector4> _GuckSackMaxAmount = null!;
        public static ConfigEntry<Vector4> _GuckSackSmallMinAmount = null!;
        public static ConfigEntry<Vector4> _GuckSackSmallMaxAmount = null!;

        #endregion
        #region Bee Hive

        public static ConfigEntry<Toggle> _TweakBeeHive = null!;
        public static ConfigEntry<Seasons> _BeeHiveSeason = null!;
        public static ConfigEntry<string> _BeeHiveMessage = null!;
        #endregion
        #endregion

        private void InitBeeHiveConfig()
        {
            _TweakBeeHive = config("Bee Hive", "1 - Enable/Disable", Toggle.On, "If on, plugin modifies beehive behavior");
            _BeeHiveSeason = config("Bee Hive", "Allowed Season",
                (Seasons)Enum.GetValues(typeof(Seasons)).Cast<int>().Sum() & ~(Seasons.Winter), "Toggle seasons");
            _BeeHiveMessage = config("Bee Hive", "Not Allowed Message", "Bees do not enjoy this environment",
                "Message prompted when invalid season");
            
        }
        private void InitPickableConfigs()
        {
            _TweakPickableValues = config("Pickable Values", "1 - Enabled/Disable", Toggle.On, "If on, plugin modifies pickable amount and respawn time, if any values are set to 0, it uses default vanilla values");
            
            _BarleyWildAmount = config("Pickable Values", "Wild Barley Amount", new Vector4(2, 3, 2, 2), "x: Spring, y: Summer, z: Autumn, w: Winter");
            _BarleyWildRespawn = config("Pickable Values", "Wild Barley Respawn (Minutes)", new Vector4(0, 0, 0, 0), "x: Spring, y: Summer, z: Autumn, w: Winter");
            
            _FlaxWildAmount = config("Pickable Values", "Wild Flax Amount", new Vector4(2, 3, 2, 2), "x: Spring, y: Summer, z: Autumn, w: Winter");
            _FlaxWildRespawn = config("Pickable Values", "Wild Flax Respawn (Minutes)", new Vector4(0, 0, 0, 0), "x: Spring, y: Summer, z: Autumn, w: Winter");

            _MushroomAmount = config("Pickable Values", "Mushroom Amount", new Vector4(1, 1, 2, 1), "x: Spring, y: Summer, z: Autumn, w: Winter");
            _MushroomRespawn = config("Pickable Values", "Mushroom Respawn (Minutes)", new Vector4(0, 0, 0, 0), "x: Spring, y: Summer, z: Autumn, w: Winter");

            _MushroomBlueAmount = config("Pickable Values", "Blue Mushroom Amount", new Vector4(1, 1, 2, 1), "x: Spring, y: Summer, z: Autumn, w: Winter");
            _MushroomBlueRespawn = config("Pickable Values", "Blue Mushroom Respawn (Minutes)", new Vector4(0, 0, 0, 0), "x: Spring, y: Summer, z: Autumn, w: Winter");
            
            _MushroomYellowAmount = config("Pickable Values", "Yellow Mushroom Amount", new Vector4(1, 1, 1, 2), "x: Spring, y: Summer, z: Autumn, w: Winter");
            _MushroomYellowRespawn = config("Pickable Values", "Yellow Mushroom Respawn (Minutes)", new Vector4(0, 0, 0, 0), "x: Spring, y: Summer, z: Autumn, w: Winter");

            _JotunPuffsAmount = config("Pickable Values", "Jotun Puffs Amount", new Vector4(1, 1, 1, 1), "x: Spring, y: Summer, z: Autumn, w: Winter");
            _JotunPuffsRespawn = config("Pickable Values", "Jotun Puffs Respawn (Minutes)", new Vector4(0, 0, 0, 0), "x: Spring, y: Summer, z: Autumn, w: Winter");

            _MagecapAmount = config("Pickable Values", "Magecap Amount", new Vector4(1, 1, 1, 1), "x: Spring, y: Summer, z: Autumn, w: Winter");
            _MagecapRespawn = config("Pickable Values", "Magecap Respawn (Minutes)", new Vector4(0, 0, 0, 0), "x: Spring, y: Summer, z: Autumn, w: Winter");

            _RoyalJellyAmount = config("Pickable Values", "Royal Jelly Amount", new Vector4(5, 5, 5, 1), "x: Spring, y: Summer, z: Autumn, w: Winter");
            _RoyalJellyRespawn = config("Pickable Values", "Royal Jelly Respawn (Minutes)", new Vector4(0, 0, 0, 0), "x: Spring, y: Summer, z: Autumn, w: Winter");

            _ThistleAmount = config("Pickable Values", "Thistle Amount", new Vector4(1, 1, 2, 1), "x: Spring, y: Summer, z: Autumn, w: Winter");
            _ThistleRespawn = config("Pickable Values", "Thistle Respawn (Minutes)", new Vector4(0, 0, 0, 0), "x: Spring, y: Summer, z: Autumn, w: Winter");

            _DandelionAmount = config("Pickable Values", "Dandelion Amount", new Vector4(2, 2, 1, 1), "x: Spring, y: Summer, z: Autumn, w: Winter");
            _DandelionRespawn = config("Pickable Values", "Dandelion Respawn (Minutes)", new Vector4(0, 0, 0, 0), "x: Spring, y: Summer, z: Autumn, w: Winter");

            _SeedCarrotAmount = config("Pickable Values", "Carrot Seed Amount", new Vector4(2, 3, 4, 3), "x: Spring, y: Summer, z: Autumn, w: Winter");
            _SeedCarrotRespawn = config("Pickable Values", "Carrot Seed Respawn (Minutes)", new Vector4(0, 0, 0, 0), "x: Spring, y: Summer, z: Autumn, w: Winter");

            _CarrotAmount = config("Pickable Values", "Carrot Amount", new Vector4(2, 1, 1, 1), "x: Spring, y: Summer, z: Autumn, w: Winter");
            _CarrotRespawn = config("Pickable Values", "Carrot Respawn (Minutes)", new Vector4(0, 0, 0, 0), "x: Spring, y: Summer, z: Autumn, w: Winter");

            _SeedTurnipAmount = config("Pickable Values", "Turnip Seed Amount", new Vector4(2, 3, 4, 2), "x: Spring, y: Summer, z: Autumn, w: Winter");
            _SeedTurnipRespawn = config("Pickable Values", "Turnip Seed Respawn (Minutes)", new Vector4(0, 0, 0, 0), "x: Spring, y: Summer, z: Autumn, w: Winter");

            _TurnipAmount = config("Pickable Values", "Turnip Amount", new Vector4(2, 1, 1, 1), "x: Spring, y: Summer, z: Autumn, w: Winter");
            _TurnipRespawn = config("Pickable Values", "Turnip Respawn (Minutes)", new Vector4(0, 0, 0, 0), "x: Spring, y: Summer, z: Autumn, w: Winter");

            _SeedOnionAmount = config("Pickable Values", "Onion Seed Amount", new Vector4(2, 3, 4, 3), "x: Spring, y: Summer, z: Autumn, w: Winter");
            _SeedOnionRespawn = config("Pickable Values", "Onion Seed Respawn (Minutes)", new Vector4(0, 0, 0, 0), "x: Spring, y: Summer, z: Autumn, w: Winter");

            _OnionAmount = config("Pickable Values", "Onion Amount", new Vector4(2, 1, 1, 1), "x: Spring, y: Summer, z: Autumn, w: Winter");
            _OnionRespawn = config("Pickable Values", "Onion Respawn (Minutes)", new Vector4(0, 0, 0, 0), "x: Spring, y: Summer, z: Autumn, w: Winter");

            _BarleyAmount = config("Pickable Values", "Barley Amount", new Vector4(2, 2, 3, 2), "x: Spring, y: Summer, z: Autumn, w: Winter");
            _BarleyRespawn = config("Pickable Values", "Barley Respawn (Minutes)", new Vector4(0, 0, 0, 0), "x: Spring, y: Summer, z: Autumn, w: Winter");

            _FlaxAmount = config("Pickable Values", "Flax Amount", new Vector4(2, 2, 3, 2), "x: Spring, y: Summer, z: Autumn, w: Winter");
            _FlaxRespawn = config("Pickable Values", "Flax Respawn (Minutes)", new Vector4(0, 0, 0, 0), "x: Spring, y: Summer, z: Autumn, w: Winter");

            _RaspberryBushAmount = config("Pickable Values", "Raspberry Bush Amount", new Vector4(2, 1, 1, 1), "x: Spring, y: Summer, z: Autumn, w: Winter");
            _RaspberryBushRespawn = config("Pickable Values", "Raspberry Bush Respawn (Minutes)", new Vector4(0, 0, 0, 0), "x: Spring, y: Summer, z: Autumn, w: Winter");

            _BlueberryBushAmount = config("Pickable Values", "Blueberry Bush Amount", new Vector4(1, 2, 1, 1), "x: Spring, y: Summer, z: Autumn, w: Winter");
            _BlueberryBushRespawn = config("Pickable Values", "Blueberry Bush Respawn (Minutes)", new Vector4(0, 0, 0, 0), "x: Spring, y: Summer, z: Autumn, w: Winter");

            _CloudberryBushAmount = config("Pickable Values", "Cloudberry Bush Amount", new Vector4(1, 1, 2, 1), "x: Spring, y: Summer, z: Autumn, w: Winter");
            _CloudberryBushRespawn = config("Pickable Values", "Cloudberry Bush Respawn (Minutes)", new Vector4(0, 0, 0, 0), "x: Spring, y: Summer, z: Autumn, w: Winter");

            _FishPickableWinter = config("Fish", "Winter Fish Pickable", Toggle.On, "If on, fish are pickable during winter");
            _FishNotPickableMessage = config("Fish", "Fish Frozen Message", "Fish too cold to pickup",
                "Message displayed when fish not pickable during winter");
        }
        private void InitDestructibleConfigs()
        {
            _TweakDestructibleValues = config("Destructible", "1 - Enable/Disable", Toggle.On,
                "If on, plugin tweaks destructibles, if value is set to 0, it uses default vanilla values");
            _GuckSackMinAmount = config("Destructible", "Guck Sack Min", new Vector4(2, 4, 5, 1),
                "x: Spring, y: Summer, z: Autumn, w: Winter");
            _GuckSackMaxAmount = config("Destructible", "Guck Sack Max", new Vector4(3, 5, 9, 2),
                "x: Spring, y: Summer, z: Autumn, w: Winter");
            _GuckSackSmallMinAmount = config("Destructible", "Guck Sack Small Min", new Vector4(2, 2, 3, 1),
                "x: Spring, y: Summer, z: Autumn, w: Winter");
            _GuckSackSmallMaxAmount = config("Destructible", "Guck Sack Small Max", new Vector4(3, 3, 4, 1),
                "x: Spring, y: Summer, z: Autumn, w: Winter");
        }
        private void InitGeneralConfigs()
        {
            _ModEnabled = config("1 - General", "Plugin Enabled", Toggle.On,"If on, plugin is enabled, if values set to 0, it uses default vanilla values");
            _LevelByPass = config("1 - General", "Season Ignore Level", 50,
                new ConfigDescription("Required farming or foraging skill to ignore seasons", new AcceptableValueRange<int>(0, 100)));

            _UseYMLCustomValues = config("1 - General", "Use YML Custom Values", Toggle.Off,
                "If on, plugin will use yml custom values file to manipulate pickable values");
            _SeasonalItems = config("1 - General", "Seasonal Items", Toggle.Off,
                "If on, plugin modifies behavior of seasonal items such as Midsummer Crown, Pointy hat and pieces such as JackOLantern and Xmas Tree");
            _TweakFarming = config("Farming", "1 - Enable/Disable", Toggle.On, "If on, plugin tweaks farming");
            _PlantDeniedText = config("Farming", "2 - Deny Message", "Not allowed during this season",
                "Message displayed when trying to plant during wrong season");
            _FarmingSpring = config("Farming", "Spring",
                (Farming.PlantTypes)Enum.GetValues(typeof(Farming.PlantTypes)).Cast<int>().Sum(),
                "Toggle allowed plants to grow");
            _FarmingSummer = config("Farming", "Summer",
                (Farming.PlantTypes)Enum.GetValues(typeof(Farming.PlantTypes)).Cast<int>().Sum(),
                "Toggle allowed plants to grow");
            _FarmingFall = config("Farming", "Autumn",
                (Farming.PlantTypes)Enum.GetValues(typeof(Farming.PlantTypes)).Cast<int>().Sum(),
                "Toggle allowed plants to grow");
            _FarmingWinter = config("Farming", "Winter",
                (Farming.PlantTypes)Enum.GetValues(typeof(Farming.PlantTypes)).Cast<int>().Sum()
                & ~(Farming.PlantTypes.Carrot | Farming.PlantTypes.CarrotSeed 
                        | Farming.PlantTypes.Turnip | Farming.PlantTypes.TurnipSeed 
                        | Farming.PlantTypes.Onion | Farming.PlantTypes.OnionSeed
                        | Farming.PlantTypes.Flax | Farming.PlantTypes.Barley
                        | Farming.PlantTypes.Magecap | Farming.PlantTypes.JotunPuff
                    ),
                "Toggle allowed plants to grow");

            _PickSpring = config("Pickable", "Spring",
                (Pickables.PickableTypes)Enum.GetValues(typeof(Pickables.PickableTypes)).Cast<int>().Sum(),
                "Toggle allowed pickables to pick");
            _PickSummer = config("Pickable", "Summer",
                (Pickables.PickableTypes)Enum.GetValues(typeof(Pickables.PickableTypes)).Cast<int>().Sum(),
                "Toggle allowed pickables to pick");
            _PickFall = config("Pickable", "Autumn",
                (Pickables.PickableTypes)Enum.GetValues(typeof(Pickables.PickableTypes)).Cast<int>().Sum(),
                "Toggle allowed pickables to pick");
            _PickWinter = config("Pickable", "Winter",
                (Pickables.PickableTypes)Enum.GetValues(typeof(Pickables.PickableTypes)).Cast<int>().Sum(),
                "Toggle allowed pickables to pick");

            _PickSpringMessage = config("Pickable", "Spring Message", "Too pretty to pick", "Unable to pick message");
            _PickSummerMessage = config("Pickable", "Summer Message", "Too hot to pick", "Unable to pick message");
            _PickFallMessage = config("Pickable", "Autumn Message", "Too wet to pick", "Unable to pick message");
            _PickWinterMessage = config("Pickable", "Winter Message", "Too cold to pick", "Unable to pick message");

        }
        
        private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
            bool synchronizedSetting = true)
        {
            ConfigDescription extendedDescription =
                new(
                    description.Description +
                    (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                    description.AcceptableValues, description.Tags);
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, extendedDescription);
            //var configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, string description,
            bool synchronizedSetting = true)
        {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }

        private class ConfigurationManagerAttributes
        {
            [UsedImplicitly] public int? Order = null!;
            [UsedImplicitly] public bool? Browsable = null!;
            [UsedImplicitly] public string? Category = null!;
            [UsedImplicitly] public Action<ConfigEntryBase>? CustomDrawer = null!;
        }

        // class AcceptableShortcuts : AcceptableValueBase
        // {
        //     public AcceptableShortcuts() : base(typeof(KeyboardShortcut))
        //     {
        //     }
        //
        //     public override object Clamp(object value) => value;
        //     public override bool IsValid(object value) => true;
        //
        //     public override string ToDescriptionString() =>
        //         "# Acceptable values: " + string.Join(", ", UnityInput.Current.SupportedKeyCodes);
        // }

        #endregion
    }
    
    public static class SeasonalFlags
    {
        public static bool HasFlagFast(this Seasons value, Seasons flag)
        {
            return (value & flag) != 0;
        }
    }

    public static class FarmingFlags
    {
        public static bool HasFlagFast(this Farming.PlantTypes value, Farming.PlantTypes flag)
        {
            return (value & flag) != 0;
        }
    }

    public static class PickableFlags
    {
        public static bool HasFlagFast(this Pickables.PickableTypes value, Pickables.PickableTypes flag)
        {
            return (value & flag) != 0;
        }
    }
}