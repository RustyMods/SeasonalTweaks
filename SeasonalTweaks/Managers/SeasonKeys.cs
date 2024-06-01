using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace SeasonalTweaks.Managers;

public static class SeasonKeys
{
    public enum Season
    {
        None, Spring, Summer, Fall, Winter
    }

    public static Season m_currentSeason = Season.None;
    
    private static float m_timer;
    public static void UpdateSeasonKeys(float dt)
    {
        if (!ZoneSystem.instance) return;
        
        m_timer += dt;
        if (m_timer < 5f) return;
        m_timer = 0.0f;

        Season season = Season.None;
        foreach (var key in ZoneSystem.instance.GetGlobalKeys().Where(key => key.StartsWith("season_")))
        {
            season = key switch
            {
                "season_spring" => Season.Spring,
                "season_summer" => Season.Summer,
                "season_fall" => Season.Fall,
                "season_winter" => Season.Winter,
                _ => Season.Spring,
            };
            break;
        }

        if (season == m_currentSeason) return;
        m_currentSeason = season;
        PieceManager.UpdateSeasonalPieces();
    }
}