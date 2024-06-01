using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace SeasonalTweaks.Managers;

public static class ItemManager
{
    
    [HarmonyPatch(typeof(Trader), nameof(Trader.Start))]
    private static class Trader_Start_Postfix
    {
        private static void Postfix(Trader __instance)
        {
            if (ConfigManager.m_enabled.Value is SeasonalTweaksPlugin.Toggle.Off) return;
            if (!__instance) return;
            List<Trader.TradeItem> items = new();

            foreach (var item in ConfigManager.m_config.Items)
            {
                var prefab = ObjectDB.instance.GetItemPrefab(item.m_prefabName);
                if (!prefab) continue;
                if (!prefab.TryGetComponent(out ItemDrop drop)) continue;
                items.Add(new Trader.TradeItem()
                {
                    m_prefab = drop,
                    m_stack = item.m_stack,
                    m_price = item.m_price,
                    m_requiredGlobalKey = GetGlobalKey(item.m_season)
                });
            }
            __instance.m_items.AddRange(items);
        }
    }
    
    [HarmonyPatch(typeof(Trader), nameof(Trader.Interact))]
    private static class Trader_Interact_Prefix
    {
        private static void Prefix(Trader __instance)
        {
            if (ConfigManager.m_enabled.Value is SeasonalTweaksPlugin.Toggle.Off) return;
            foreach (Trader.TradeItem? item in __instance.m_items)
            {
                if (!HasConfigs(item.m_prefab.name)) continue;
                var configs = GetConfigs(item.m_prefab.name);
                item.m_price = GetPrice(configs);
                item.m_stack = GetStack(configs);
                item.m_requiredGlobalKey = GetGlobalKey(configs.m_season);
            }
        }
    }
    
    private static string GetGlobalKey(SeasonKeys.Season season)
    {
        return season switch
        {
            SeasonKeys.Season.Spring => "season_spring",
            SeasonKeys.Season.Summer => "season_summer",
            SeasonKeys.Season.Fall => "season_Fall",
            SeasonKeys.Season.Winter => "season_winter",
            _ => "no_available"
        };
    }

    private static bool HasConfigs(string prefabName) =>
        ConfigManager.m_config.Items.Exists(x => x.m_prefabName == prefabName);
    private static ItemData GetConfigs(string prefabName) => ConfigManager.m_config.Items.First(x => x.m_prefabName == prefabName);
    
    private static int GetPrice(ItemData data) => data.m_price;
    private static int GetStack(ItemData data) => data.m_stack;
}