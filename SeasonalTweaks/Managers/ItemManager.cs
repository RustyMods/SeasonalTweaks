using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using HarmonyLib;

namespace SeasonalTweaks.Managers;

public static class ItemManager
{
    private static readonly List<ItemDrop> m_items = new();
    
    [HarmonyPatch(typeof(Trader), nameof(Trader.Start))]
    private static class Trader_Start_Postfix
    {
        private static void Postfix(Trader __instance)
        {
            if (!__instance) return;
            List<Trader.TradeItem> items = new();
            foreach (var kvp in ConfigManager.m_items)
            {
                if (!ConfigManager.m_itemPrices.TryGetValue(kvp.Key, out ConfigEntry<int> price)) continue;
                var drop = ObjectDB.instance.GetItemPrefab(kvp.Key).GetComponent<ItemDrop>();
                m_items.Add(drop);
                items.Add(new Trader.TradeItem()
                {
                    m_prefab = drop,
                    m_stack = 1,
                    m_price = price.Value,
                    m_requiredGlobalKey = kvp.Value.Value switch
                    {
                        SeasonKeys.Season.Spring => "season_spring",
                        SeasonKeys.Season.Summer => "season_summer",
                        SeasonKeys.Season.Fall => "season_Fall",
                        SeasonKeys.Season.Winter => "season_winter",
                        _ => "no_available"
                    }
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
            foreach (Trader.TradeItem? item in __instance.m_items.Where(item => m_items.Contains(item.m_prefab)))
            {
                if (ConfigManager.m_itemPrices.TryGetValue(item.m_prefab.name, out ConfigEntry<int> price))
                {
                    item.m_price = price.Value;
                }

                if (ConfigManager.m_items.TryGetValue(item.m_prefab.name, out ConfigEntry<SeasonKeys.Season> season))
                {
                    item.m_requiredGlobalKey = season.Value switch
                    {
                        SeasonKeys.Season.Spring => "season_spring",
                        SeasonKeys.Season.Summer => "season_summer",
                        SeasonKeys.Season.Fall => "season_Fall",
                        SeasonKeys.Season.Winter => "season_winter",
                        _ => "no_available"
                    };
                }
            }
        }
    }
}