using BepInEx;
using HarmonyLib;

namespace SeasonalTweaks.Tweaks;

public static class Fish
{
    [HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.Pickup))]
    private static class ItemDropPickup
    {
        private static bool Prefix(ItemDrop __instance)
        {
            if (!__instance) return false;

            if (__instance.m_itemData.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Fish) return true;

            if (SeasonalTweaksPlugin._FishPickableWinter.Value is SeasonalTweaksPlugin.Toggle.On) return true;
            
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, SeasonalTweaksPlugin._FishNotPickableMessage.Value);
            return false;
        }
    }
}