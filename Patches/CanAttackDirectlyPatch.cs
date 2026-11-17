using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;

namespace MLPCPCK.Patches
{
    [HarmonyPatch(typeof(PlayableCard), "CanAttackDirectly", 0)]
    public static class CanAttackDirectlyPatch {
        [HarmonyPostfix]
        public static void Postfix(CardSlot opposingSlot, ref bool __result, ref PlayableCard __instance) {
            if (!__result && __instance.OpposingSlot() != opposingSlot && opposingSlot.Card != null && opposingSlot.Card.HasAbility(MLPAbility.Forgettable)) {
                __result = true;
            }
        }
    }
}