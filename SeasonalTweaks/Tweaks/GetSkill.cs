using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using HarmonyLib;
using HarmonyLib.Tools;

namespace SeasonalTweaks.Tweaks;

public static class GetSkillLevel
{
    public static float GetFarmingSkillLevel()
    {
        if (!SeasonalTweaksPlugin.FarmingLoaded) return 0.0f;
        if (!Player.m_localPlayer) return 0.0f;
        Skills.SkillType FarmingSkill = (Skills.SkillType)Math.Abs("Farming".GetStableHashCode());
        float level = Player.m_localPlayer.GetSkillLevel(FarmingSkill);
        return level;
    }

    public static float GetForagingSkillLevel()
    {
        if (!SeasonalTweaksPlugin.ForagingLoaded) return 0.0f;
        if (!Player.m_localPlayer) return 0.0f;
        Skills.SkillType ForagingSkill = (Skills.SkillType)Math.Abs("Foraging".GetStableHashCode());
        float level = Player.m_localPlayer.GetSkillLevel(ForagingSkill);
        return level;
    }
}