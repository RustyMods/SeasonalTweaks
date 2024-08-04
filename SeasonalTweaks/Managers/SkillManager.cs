using System;

namespace SeasonalTweaks.Managers;

public static class SkillManager
{
    public static bool HasOverrideLevel()
    {
        if (SeasonalTweaksPlugin.ForagingLoaded)
        {
            if (HasForagingLevel()) return true;
        }

        if (SeasonalTweaksPlugin.FarmingLoaded)
        {
            if (HasFarmingLevel()) return true;
        }

        return false;
    }

    private static bool HasFarmingLevel() => ConfigManager.m_farmingOverride.Value <= GetFarmingSkillLevel();
    private static bool HasForagingLevel() => ConfigManager.m_foragingOverride.Value <= GetForagingSkillLevel();
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