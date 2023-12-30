using HarmonyLib;
using UnityEngine;

namespace SeasonalTweaks.Tweaks;

public static class SeasonalSpawns
{
    [HarmonyPatch(typeof(SpawnSystem), nameof(SpawnSystem.Spawn))]
    static class ModifySpawnSystemSpawn
    {
        static bool Prefix(SpawnSystem __instance, SpawnArea.SpawnData critter)
        {
            if (!__instance) return false;
            if (!critter.m_prefab) return true;
            Debug.LogWarning("Spawning " + critter.m_prefab.name);
            return true;
        }
    }
    
}