using System;

namespace SeasonalTweaks.Managers;

public static class SkillManager
{
    public static float GetFarmingSkillLevel()
    {
        if (!SeasonalTweaksPlugin.FarmingLoaded) return 100.0f;
        if (!Player.m_localPlayer) return 100.0f;
        Skills.SkillType FarmingSkill = (Skills.SkillType)Math.Abs("Farming".GetStableHashCode());
        float level = Player.m_localPlayer.GetSkillLevel(FarmingSkill);
        return level;
    }

    public static float GetForagingSkillLevel()
    {
        if (!SeasonalTweaksPlugin.ForagingLoaded) return 100.0f;
        if (!Player.m_localPlayer) return 100.0f;
        Skills.SkillType ForagingSkill = (Skills.SkillType)Math.Abs("Foraging".GetStableHashCode());
        float level = Player.m_localPlayer.GetSkillLevel(ForagingSkill);
        return level;
    }
}