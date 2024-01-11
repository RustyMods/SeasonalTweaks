// using System.Collections.Generic;
// using BepInEx;
// using HarmonyLib;
// using UnityEngine;
//
// namespace SeasonalTweaks.Tweaks;
//
// public static class SeasonalSpawns
// {
//     [HarmonyPatch(typeof(SpawnSystem), nameof(SpawnSystem.Spawn))]
//     static class ModifySpawnSystemSpawn
//     {
//         static bool Prefix(SpawnSystem __instance, SpawnArea.SpawnData critter)
//         {
//             if (!__instance) return false;
//             if (!critter.m_prefab) return true;
//             Debug.LogWarning("Spawning " + critter.m_prefab.name);
//             return true;
//         }
//     }
//
//     [HarmonyPatch(typeof(SpawnSystem), nameof(SpawnSystem.UpdateSpawnList))]
//     static class ModifySpawnList
//     {
//         static void Postfix(SpawnSystem __instance, List<SpawnSystem.SpawnData> spawners)
//         {
//             if (!__instance) return;
//             List<string> currentKeys = ZoneSystem.instance.GetGlobalKeys();
//             foreach (var spawner in spawners)
//             {
//                 if (spawner.m_name.IsNullOrWhiteSpace()) continue;
//                 Debug.LogWarning(spawner.m_name + " : " + spawner.m_prefab.name + " : " + spawner.m_requiredGlobalKey);
//                 if (spawner.m_requiredGlobalKey.IsNullOrWhiteSpace())
//                 {
//                     
//                 }
//             }
//         }
//     }
//     
// }