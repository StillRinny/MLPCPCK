using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;

namespace MLPCPCK_utils.Patches
{
    [HarmonyPatch(typeof(CardMergeSequencer), "GetValidCardsForSacrifice")]
    public static class ModifyHostCardPatch
    {
        [HarmonyPrefix]
        public static void PostFix(ref List<CardInfo> __result) => __result.RemoveAll(x => x.Abilities.Contains(MLPAbility.CandyCurse));
    }
}
    