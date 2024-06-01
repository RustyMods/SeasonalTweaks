using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace SeasonalTweaks.Managers;

public static class PieceManager
{
    private static List<GameObject> m_defaultPieces = new();

    public static void UpdateSeasonalPieces()
    {
        if (!Player.m_localPlayer || !ZNetScene.instance) return;
        if (Player.m_localPlayer.m_currentSeason == null) return;
        Player.m_localPlayer.m_currentSeason.Pieces.Clear();
        Player.m_localPlayer.m_currentSeason.Pieces.AddRange(m_defaultPieces);
        foreach (PieceData piece in ConfigManager.m_config.Pieces)
        {
            if (piece.m_season != SeasonKeys.m_currentSeason) continue;
            var prefab = ZNetScene.instance.GetPrefab(piece.m_prefabName);
            if (!prefab) continue;
            if (Player.m_localPlayer.m_currentSeason.Pieces.Contains(prefab)) continue;
            Player.m_localPlayer.m_currentSeason.Pieces.Add(prefab);
        }
        Player.m_localPlayer.UpdateKnownRecipesList();
    }
    
    [HarmonyPatch(typeof(Player), nameof(Player.SetLocalPlayer))]
    private static class Player_SetLocalPlayer_Postfix
    {
        private static void Postfix(Player __instance)
        {
            if (__instance != Player.m_localPlayer) return;
            if (__instance.m_currentSeason == null)
            {
                var group = ScriptableObject.CreateInstance<SeasonalItemGroup>();
                __instance.m_currentSeason = group;
            }
            m_defaultPieces = __instance.m_currentSeason.Pieces;
            UpdateSeasonalPieces();
        }
    }
}