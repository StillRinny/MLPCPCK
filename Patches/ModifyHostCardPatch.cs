using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;

namespace MLPCPCK_utils.Patches
{
    [HarmonyPatch()]
    public static class ModifyHostCardPatch
    {
        [HarmonyPatch(typeof(CardMergeSequencer), "GetValidCardsForSacrifice", MethodType.Normal)]
        [HarmonyPostfix]
        public static void PostFix(ref List<CardInfo> __result) => __result.RemoveAll(x => x.Abilities.Contains(MLPAbilities.CandyCurse));
    }
}
    