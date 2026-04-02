using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using MLPCPCK_utils.SpecialAbilities;
using Sirenix.Utilities;
using UnityEngine;

namespace MLPCPCK_utils.Patches
{
    [HarmonyPatch]
    public class TurnManagerPatch {
        [HarmonyPatch(typeof(TurnManager), "PlayerTurn")]
        [HarmonyPatch(typeof(TurnManager), "OpponentTurn")]
        [HarmonyPostfix]
        public static IEnumerator APostfix(IEnumerator enumerator, TurnManager __instance) {
            yield return enumerator;
            yield return SurpriseSpecialBehaviour.TryToReturn(__instance);
        }

        [HarmonyPatch(typeof(TurnManager), "CleanupPhase")]
        [HarmonyPostfix]
        public static void CleanupPhasePatch() => SurpriseSpecialBehaviour.ClearSurprises();
    }
}