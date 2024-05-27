using HarmonyLib;

namespace SeasonalTweaks.Managers;

public static class PieceManager
{
    [HarmonyPatch(typeof(Player), nameof(Player.GetBuildPieces))]
    private static class Player_GetBuildPieces_Postfix
    {
        private static void Prefix(Player __instance)
        {
            foreach (var kvp in ConfigManager.m_pieces)
            {
                var prefab = ZNetScene.instance.GetPrefab(kvp.Key);
                if (!prefab) continue;
                if (!prefab.TryGetComponent(out Piece piece)) continue;
                piece.enabled = kvp.Value.Value == SeasonKeys.m_currentSeason;
            }
        }
    }
}