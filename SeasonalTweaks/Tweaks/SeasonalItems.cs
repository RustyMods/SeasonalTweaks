using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace SeasonalTweaks.Tweaks;

public static class SeasonalItems
{
    public static void UpdateSeasonalPieces()
    {
        if (!ZNetScene.instance) return;
        if (!Player.m_localPlayer) return;
        
        GameObject JackOTurnip = ZNetScene.instance.GetPrefab("piece_jackoturnip");
        GameObject MayPole = ZNetScene.instance.GetPrefab("piece_maypole");
        GameObject XmasCrown = ZNetScene.instance.GetPrefab("piece_xmascrown");
        GameObject XmasGarland = ZNetScene.instance.GetPrefab("piece_xmasgarland");
        GameObject XmasTree = ZNetScene.instance.GetPrefab("piece_xmastree");
        GameObject Gift1 = ZNetScene.instance.GetPrefab("piece_gift1");
        GameObject Gift2 = ZNetScene.instance.GetPrefab("piece_gift2");
        GameObject Gift3 = ZNetScene.instance.GetPrefab("piece_gift3");
        GameObject Mistletoe = ZNetScene.instance.GetPrefab("piece_mistletoe");

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
        
        GameObject Hammer = ZNetScene.instance.GetPrefab("Hammer");
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
                
                Player.m_localPlayer.AddKnownPiece(JackOTurnip_Piece);
                if (Player.m_localPlayer.IsRecipeKnown(MayPole_Piece.m_name)) Player.m_localPlayer.m_knownRecipes.Remove(MayPole_Piece.m_name);
                foreach (Piece piece in WinterPieceScripts)
                {
                    if (Player.m_localPlayer.IsRecipeKnown(piece.m_name))
                    {
                        Player.m_localPlayer.m_knownRecipes.Remove(piece.m_name);
                    }
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

                foreach (Piece piece in WinterPieceScripts)
                {
                    Player.m_localPlayer.AddKnownPiece(piece);
                }
                if (Player.m_localPlayer.IsRecipeKnown(MayPole_Piece.m_name)) Player.m_localPlayer.m_knownRecipes.Remove(MayPole_Piece.m_name);
                if (Player.m_localPlayer.IsRecipeKnown(JackOTurnip_Piece.m_name)) Player.m_localPlayer.m_knownRecipes.Remove(JackOTurnip_Piece.m_name);

            }

            if (SeasonKeys.season is SeasonKeys.Seasons.Summer)
            {
                if (!HammerItemDrop.m_itemData.m_shared.m_buildPieces.m_pieces.Contains(MayPole)) HammerItemDrop.m_itemData.m_shared.m_buildPieces.m_pieces.Add(MayPole);
                
                if (HammerItemDrop.m_itemData.m_shared.m_buildPieces.m_pieces.Contains(JackOTurnip)) HammerItemDrop.m_itemData.m_shared.m_buildPieces.m_pieces.Remove(JackOTurnip);
                foreach (GameObject winterPiece in WinterPieces)
                {
                    if (HammerItemDrop.m_itemData.m_shared.m_buildPieces.m_pieces.Contains(winterPiece)) HammerItemDrop.m_itemData.m_shared.m_buildPieces.m_pieces.Remove(winterPiece);
                }
                Player.m_localPlayer.AddKnownPiece(MayPole_Piece);
                if (Player.m_localPlayer.IsRecipeKnown(JackOTurnip_Piece.m_name)) Player.m_localPlayer.m_knownRecipes.Remove(JackOTurnip_Piece.m_name);
                foreach (Piece piece in WinterPieceScripts)
                {
                    if (Player.m_localPlayer.IsRecipeKnown(piece.m_name))
                    {
                        Player.m_localPlayer.m_knownRecipes.Remove(piece.m_name);
                    }
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
                
                if (Player.m_localPlayer.IsRecipeKnown(MayPole_Piece.m_name)) Player.m_localPlayer.m_knownRecipes.Remove(MayPole_Piece.m_name);
                if (Player.m_localPlayer.IsRecipeKnown(JackOTurnip_Piece.m_name)) Player.m_localPlayer.m_knownRecipes.Remove(JackOTurnip_Piece.m_name);
                foreach (Piece piece in WinterPieceScripts)
                {
                    if (Player.m_localPlayer.IsRecipeKnown(piece.m_name))
                    {
                        Player.m_localPlayer.m_knownRecipes.Remove(piece.m_name);
                    }
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
        
        GameObject HelmetYule = ZNetScene.instance.GetPrefab("HelmetYule");
        GameObject HelmetPointy = ZNetScene.instance.GetPrefab("HelmetPointyHat");
        GameObject HelmetMidsummer = ZNetScene.instance.GetPrefab("HelmetMidsummerCrown");
        
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
    

    public static void ModifyHaldorTrader()
    {
        if (!ZNetScene.instance) return;
        if (!Player.m_localPlayer) return;
        if (!ObjectDB.instance) return;
        
        Debug.LogWarning("modifying haldor");
        
        GameObject HelmetYule = ZNetScene.instance.GetPrefab("HelmetYule");
        HelmetYule.TryGetComponent(out ItemDrop HelmetYule_ItemDrop);

        GameObject Haldor = ZNetScene.instance.GetPrefab("Haldor");
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
            
            UpdateCustomSeasonalItems(Trader);
        };
    }

    private static void UpdateCustomSeasonalItems(Trader trader)
    {
        if (!ZNetScene.instance) return;
        if (!Player.m_localPlayer) return;
        if (!ObjectDB.instance) return;

        GameObject HelmetOdin = ZNetScene.instance.GetPrefab("HelmetOdin");
        GameObject CapeOdin = ZNetScene.instance.GetPrefab("CapeOdin");
        GameObject TankardOdin = ZNetScene.instance.GetPrefab("TankardOdin");

        HelmetOdin.TryGetComponent(out ItemDrop HelmetOdin_ItemDrop);
        CapeOdin.TryGetComponent(out ItemDrop CapeOdin_ItemDrop);
        TankardOdin.TryGetComponent(out ItemDrop TankardOdin_ItemDrop);

        HelmetOdin_ItemDrop.m_itemData.m_shared.m_dlc = "";
        CapeOdin_ItemDrop.m_itemData.m_shared.m_dlc = "";
        TankardOdin_ItemDrop.m_itemData.m_shared.m_dlc = "";

        Recipe? HelmetOdin_Recipe = ObjectDB.instance.GetRecipe(HelmetOdin_ItemDrop.m_itemData);
        Recipe? CapeOdin_Recipe = ObjectDB.instance.GetRecipe(CapeOdin_ItemDrop.m_itemData);
        Recipe? TankardOdin_Recipe = ObjectDB.instance.GetRecipe(TankardOdin_ItemDrop.m_itemData);

        if (HelmetOdin_Recipe) HelmetOdin_Recipe.m_enabled = false;
        if (CapeOdin_Recipe) CapeOdin_Recipe.m_enabled = false;
        if (TankardOdin_Recipe) TankardOdin_Recipe.m_enabled = false;
        
        Trader.TradeItem HelmetOdin_TradeItem = new Trader.TradeItem()
        {
            m_prefab = HelmetOdin_ItemDrop,
            m_stack = 1,
            m_price = 100,
            m_requiredGlobalKey = "season_spring"
        };
        Trader.TradeItem CapeOdin_TradeItem = new Trader.TradeItem()
        {
            m_prefab = CapeOdin_ItemDrop,
            m_stack = 1,
            m_price = 100,
            m_requiredGlobalKey = "season_spring"
        };
        Trader.TradeItem TankardOdin_TradeItem = new Trader.TradeItem()
        {
            m_prefab = TankardOdin_ItemDrop,
            m_stack = 1,
            m_price = 100,
            m_requiredGlobalKey = "season_spring"
        };
        
        if (trader.m_items.Find(x => x.m_prefab == HelmetOdin_TradeItem.m_prefab) == null) trader.m_items.Add(HelmetOdin_TradeItem);
        if (trader.m_items.Find(x => x.m_prefab == CapeOdin_TradeItem.m_prefab) == null) trader.m_items.Add(CapeOdin_TradeItem);
        if (trader.m_items.Find(x => x.m_prefab == TankardOdin_TradeItem.m_prefab) == null) trader.m_items.Add(TankardOdin_TradeItem);
    }
}