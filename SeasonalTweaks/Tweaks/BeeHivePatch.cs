using System.Text.RegularExpressions;
using HarmonyLib;
using static SeasonalTweaks.SeasonalTweaksPlugin;

namespace SeasonalTweaks.Tweaks;

public static class BeeHivePatch
{
    [HarmonyPatch(typeof(Beehive), nameof(Beehive.Interact))]
    static class SeasonBeeHive
    {
        private static bool Prefix(Beehive __instance, Humanoid character)
        {
            if (!__instance) return false;
            string normalizedName = Regex.Replace(__instance.name, @"\(.*?\)", "");
            if (normalizedName != "piece_beehive") return true;
            if (_TweakBeeHive.Value is Toggle.Off) return true;

            if (_BeeHiveSeason.Value.HasFlagFast(SeasonKeys.season)) return true;
            character.Message(MessageHud.MessageType.Center, _BeeHiveMessage.Value);
            return false;
        }
    }

    [HarmonyPatch(typeof(Beehive), nameof(Beehive.UpdateBees))]
    static class UpdateBeesPatch
    {
        private static bool Prefix(Beehive __instance)
        {
            if (!__instance) return false;
            string normalizedName = Regex.Replace(__instance.name, @"\(.*?\)", "");
            if (normalizedName != "piece_beehive") return true;
            return _TweakBeeHive.Value is Toggle.Off || _BeeHiveSeason.Value.HasFlagFast(SeasonKeys.season);
        }
    }
}