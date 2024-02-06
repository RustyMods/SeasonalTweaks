using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace SeasonalTweaks.Tweaks;

public static class SeasonalItems
{
    private static GameObject Hammer = null!;
    
    private static GameObject JackOTurnip = null!;
    private static GameObject MayPole = null!;
    private static GameObject XmasCrown = null!;
    private static GameObject XmasGarland = null!;
    private static GameObject XmasTree = null!;
    private static GameObject Gift1 = null!;
    private static GameObject Gift2 = null!;
    private static GameObject Gift3 = null!;
    private static GameObject Mistletoe = null!;
    
    private static GameObject HelmetYule = null!;
    private static GameObject HelmetPointy = null!;
    private static GameObject HelmetMidsummer = null!;
    
    private static GameObject Haldor = null!;
    
    private static GameObject HelmetOdin = null!;
    private static GameObject CapeOdin = null!;
    private static GameObject TankardOdin = null!;
    
    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    static class CacheGameObject
    {
        private static void Postfix(ZNetScene __instance)
        {
            if (!__instance) return;
            Hammer = __instance.GetPrefab("Hammer");
            JackOTurnip = __instance.GetPrefab("piece_jackoturnip");
            MayPole = __instance.GetPrefab("piece_maypole");
            XmasCrown = __instance.GetPrefab("piece_xmascrown");
            XmasGarland = __instance.GetPrefab("piece_xmasgarland");
            XmasTree = __instance.GetPrefab("piece_xmastree");
            Gift1 = __instance.GetPrefab("piece_gift1");
            Gift2 = __instance.GetPrefab("piece_gift2");
            Gift3 = __instance.GetPrefab("piece_gift3");
            Mistletoe = __instance.GetPrefab("piece_mistletoe");
            
            HelmetYule = __instance.GetPrefab("HelmetYule");
            HelmetPointy = __instance.GetPrefab("HelmetPointyHat");
            HelmetMidsummer = __instance.GetPrefab("HelmetMidsummerCrown");
            
            Haldor = __instance.GetPrefab("Haldor");

            HelmetOdin = __instance.GetPrefab("HelmetOdin");
            CapeOdin = __instance.GetPrefab("CapeOdin");
            TankardOdin = __instance.GetPrefab("TankardOdin");
            
            HelmetOdin.TryGetComponent(out ItemDrop HelmetOdin_ItemDrop);
            CapeOdin.TryGetComponent(out ItemDrop CapeOdin_ItemDrop);
            TankardOdin.TryGetComponent(out ItemDrop TankardOdin_ItemDrop);

            HelmetOdin_ItemDrop.m_itemData.m_shared.m_dlc = "";
            CapeOdin_ItemDrop.m_itemData.m_shared.m_dlc = "";
            TankardOdin_ItemDrop.m_itemData.m_shared.m_dlc = "";
        }
    }
    public static void UpdateSeasonalPieces()
    {
        if (!ZNetScene.instance) return;
        if (!Player.m_localPlayer) return;
        
        if (SeasonalTweaksPlugin._SeasonalItems.Value is SeasonalTweaksPlugin.Toggle.Off) return;

        JackOTurnip.TryGetComponent(out Piece JackOTurnip_Piece);
        MayPole.TryGetComponent(out Piece MayPole_Piece);
        XmasCrown.TryGetComponent(out Piece XmasCrown_Piece);
        XmasGarland.TryGetComponent(out Piece XmasGarland_Piece);
        XmasTree.TryGetComponent(out Piece XmasTree_Piece);
        Gift1.TryGetComponent(out Piece Gift1_Piece);
        Gift2.TryGetComponent(out Piece Gift2_Piece);
        Gift3.TryGetComponent(out Piece Gift3_Piece);
        Mistletoe.TryGetComponent(out Piece Mistletoe_Piece);
        List<Piece> allPieces = new() { JackOTurnip_Piece, MayPole_Piece };

        List<GameObject> WinterPieces = new() { XmasCrown, XmasGarland, XmasTree, Gift1, Gift2, Gift3, Mistletoe };
        List<Piece> WinterPieceScripts = new() { XmasCrown_Piece, XmasGarland_Piece, XmasTree_Piece, Gift1_Piece, Gift2_Piece, Gift3_Piece, Mistletoe_Piece };
        
        allPieces.AddRange(WinterPieceScripts);

        foreach (Piece piece in allPieces) piece.m_enabled = true;
        
        
        if (Hammer.TryGetComponent(out ItemDrop HammerItemDrop))
        {
            if (SeasonKeys.season is SeasonKeys.Seasons.Fall)
            {
                if (!HammerItemDrop.m_itemData.m_shared.m_buildPieces.m_pieces.Contains(JackOTurnip)) HammerItemDrop.m_itemData.m_shared.m_buildPieces.m_pieces.Add(JackOTurnip);

                if (HammerItemDrop.m_itemData.m_shared.m_buildPieces.m_pieces.Contains(MayPole)) HammerItemDrop.m_itemData.m_shared.m_buildPieces.m_pieces.Remove(MayPole);
                foreach (GameObject winterPiece in WinterPieces)
                {
                    if (HammerItemDrop.m_itemData.m_shared.m_buildPieces.m_pieces.Contains(winterPiece)) HammerItemDrop.m_itemData.m_shared.m_buildPieces.m_pieces.Remove(winterPiece);
                }
            }

            if (SeasonKeys.season is SeasonKeys.Seasons.Winter)
            {
                foreach (GameObject winterPiece in WinterPieces)
                {
                    if (!HammerItemDrop.m_itemData.m_shared.m_buildPieces.m_pieces.Contains(winterPiece)) HammerItemDrop.m_itemData.m_shared.m_buildPieces.m_pieces.Add(winterPiece);
                }
                if (HammerItemDrop.m_itemData.m_shared.m_buildPieces.m_pieces.Contains(JackOTurnip)) HammerItemDrop.m_itemData.m_shared.m_buildPieces.m_pieces.Remove(JackOTurnip);
                if (HammerItemDrop.m_itemData.m_shared.m_buildPieces.m_pieces.Contains(MayPole)) HammerItemDrop.m_itemData.m_shared.m_buildPieces.m_pieces.Remove(MayPole);
            }

            if (SeasonKeys.season is SeasonKeys.Seasons.Summer)
            {
                if (!HammerItemDrop.m_itemData.m_shared.m_buildPieces.m_pieces.Contains(MayPole)) HammerItemDrop.m_itemData.m_shared.m_buildPieces.m_pieces.Add(MayPole);
                
                if (HammerItemDrop.m_itemData.m_shared.m_buildPieces.m_pieces.Contains(JackOTurnip)) HammerItemDrop.m_itemData.m_shared.m_buildPieces.m_pieces.Remove(JackOTurnip);
                foreach (GameObject winterPiece in WinterPieces)
                {
                    if (HammerItemDrop.m_itemData.m_shared.m_buildPieces.m_pieces.Contains(winterPiece)) HammerItemDrop.m_itemData.m_shared.m_buildPieces.m_pieces.Remove(winterPiece);
                }
            }

            if (SeasonKeys.season is SeasonKeys.Seasons.Spring)
            {
                if (HammerItemDrop.m_itemData.m_shared.m_buildPieces.m_pieces.Contains(JackOTurnip)) HammerItemDrop.m_itemData.m_shared.m_buildPieces.m_pieces.Remove(JackOTurnip);
                if (HammerItemDrop.m_itemData.m_shared.m_buildPieces.m_pieces.Contains(MayPole)) HammerItemDrop.m_itemData.m_shared.m_buildPieces.m_pieces.Remove(MayPole);
                foreach (GameObject winterPiece in WinterPieces)
                {
                    if (HammerItemDrop.m_itemData.m_shared.m_buildPieces.m_pieces.Contains(winterPiece)) HammerItemDrop.m_itemData.m_shared.m_buildPieces.m_pieces.Remove(winterPiece);
                }
            }

            Player.m_localPlayer.UpdateAvailablePiecesList();
        }
    }
    
    [HarmonyWrapSafe]
    public static void UpdateSeasonalItems()
    {
        if (!ZNetScene.instance) return;
        if (!Player.m_localPlayer) return;
        if (!ObjectDB.instance) return;
        
        if (SeasonalTweaksPlugin._SeasonalItems.Value is SeasonalTweaksPlugin.Toggle.Off) return;

        HelmetPointy.TryGetComponent(out ItemDrop HelmetPointy_ItemDrop);
        HelmetMidsummer.TryGetComponent(out ItemDrop HelmetMidsummer_ItemDrop);
        HelmetYule.TryGetComponent(out ItemDrop HelmetYule_ItemDrop);
        
        Recipe? HelmetPointy_Recipe = ObjectDB.instance.GetRecipe(HelmetPointy_ItemDrop.m_itemData);
        Recipe? HelmetMidsummer_Recipe = ObjectDB.instance.GetRecipe(HelmetMidsummer_ItemDrop.m_itemData);
        Recipe? HelmetYule_Recipe = ObjectDB.instance.GetRecipe(HelmetYule_ItemDrop.m_itemData);
        
        switch (SeasonKeys.season)
        {
            case SeasonKeys.Seasons.Fall:
                if (HelmetPointy_Recipe) HelmetPointy_Recipe.m_enabled = true;
                if (HelmetMidsummer_Recipe) HelmetMidsummer_Recipe.m_enabled = false;
                if (HelmetYule_Recipe) HelmetYule_Recipe.m_enabled = false;
                break;
            case SeasonKeys.Seasons.Winter:
                if (HelmetPointy_Recipe) HelmetPointy_Recipe.m_enabled = false;
                if (HelmetMidsummer_Recipe) HelmetMidsummer_Recipe.m_enabled = false;
                if (HelmetYule_Recipe) HelmetYule_Recipe.m_enabled = true;
                break;
            case SeasonKeys.Seasons.Spring:
                if (HelmetPointy_Recipe) HelmetPointy_Recipe.m_enabled = false;
                if (HelmetMidsummer_Recipe) HelmetMidsummer_Recipe.m_enabled = false;
                if (HelmetYule_Recipe) HelmetYule_Recipe.m_enabled = false;
                break;
            case SeasonKeys.Seasons.Summer:
                if (HelmetPointy_Recipe) HelmetPointy_Recipe.m_enabled = false;
                if (HelmetMidsummer_Recipe) HelmetMidsummer_Recipe.m_enabled = true;
                if (HelmetYule_Recipe) HelmetYule_Recipe.m_enabled = false;
                break;
        }
    }

    [HarmonyPatch(typeof(Trader), nameof(Trader.Start))]
    static class TraderStartPatch
    {
        private static void Postfix(Trader __instance)
        {
            if (!__instance) return;
            if (SeasonalTweaksPlugin._SeasonalItems.Value is SeasonalTweaksPlugin.Toggle.Off) return;
            HelmetYule.TryGetComponent(out ItemDrop HelmetYule_ItemDrop);

            Trader.TradeItem? HelmetYule_TradeItem = __instance.m_items.Find(x => x.m_prefab == HelmetYule_ItemDrop);
            if (HelmetYule_TradeItem != null)
            {
                HelmetYule_TradeItem.m_requiredGlobalKey = "season_winter";
            }
            else
            {
                HelmetYule_TradeItem = new Trader.TradeItem()
                {
                    m_prefab = HelmetYule_ItemDrop,
                    m_price = 100,
                    m_stack = 1,
                    m_requiredGlobalKey = "season_winter"
                };
                __instance.m_items.Add(HelmetYule_TradeItem);
            }
        }
    }
    public static void ModifyHaldorTrader()
    {
        if (!ZNetScene.instance) return;
        if (!Player.m_localPlayer) return;
        if (!ObjectDB.instance) return;
        
        if (SeasonalTweaksPlugin._SeasonalItems.Value is SeasonalTweaksPlugin.Toggle.Off) return;
        
        HelmetYule.TryGetComponent(out ItemDrop HelmetYule_ItemDrop);
        if (Haldor.TryGetComponent(out Trader Trader))
        {
            Trader.TradeItem? HelmetYule_TradeItem = Trader.m_items.Find(x => x.m_prefab == HelmetYule_ItemDrop);
            if (HelmetYule_TradeItem != null)
            {
                HelmetYule_TradeItem.m_requiredGlobalKey = "season_winter";
            }
            else
            {
                HelmetYule_TradeItem = new Trader.TradeItem()
                {
                    m_prefab = HelmetYule_ItemDrop,
                    m_price = 100,
                    m_stack = 1,
                    m_requiredGlobalKey = "season_winter"
                };
                Trader.m_items.Add(HelmetYule_TradeItem);
            }
        };
    }
    
}