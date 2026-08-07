using System.Collections;
using DiskCardGame;
using HarmonyLib;
using MLPCPCK.SpecialAbilities;

namespace MLPCPCK.Patches
{
    [HarmonyPatch]
    public class TurnManagerPatch {
        [HarmonyPatch(typeof(TurnManager), "PlayerTurn")]
        [HarmonyPatch(typeof(TurnManager), "OpponentTurn")]
        [HarmonyPostfix]
        public static IEnumerator APostfix(IEnumerator enumerator, TurnManager __instance) {
            yield return enumerator;
            yield return SurpriseSpecialBehaviour.TryToReturnOld(__instance);
        }

        [HarmonyPatch(typeof(TurnManager), "CleanupPhase")]
        [HarmonyPostfix]
        public static void CleanupPhasePatch() => SurpriseSpecialBehaviour.ClearSurprises();
    }

    /*[HarmonyPatch]
    public class CombatPhaseManagerPatch {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(CombatPhaseManager), "DoCombatPhase", MethodType.Enumerator)]
        public static IEnumerable<CodeInstruction> CombatPhaseManager_DoCombatPhase(IEnumerable<CodeInstruction> instructions) {
            List<CodeInstruction> codeInstructionList = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codeInstructionList.Count; i++) { // extractor
                Plugin.Log.LogError($"[{i}]" + codeInstructionList[i].opcode.ToString());
                if (i > 500) break;
            }*/
            
            /*/
             * IL_024a: call         instance void DiskCardGame.CombatPhaseManager/'<DoCombatPhase>d__4'::'<>m__Finally1'()
             * IL_024f: ldarg.0      // this
             * IL_0250: ldflda       valuetype [netstandard]System.Collections.Generic.List`1/Enumerator<class DiskCardGame.CardSlot> DiskCardGame.CombatPhaseManager/'<DoCombatPhase>d__4'::'<>7__wrap4'
             * IL_0255: initobj      valuetype [netstandard]System.Collections.Generic.List`1/Enumerator<class DiskCardGame.CardSlot>
        
             * todo: > NEW INSTRUCTIONS
             * ldarg.0      // this
             * call         tryToReturn | class [netstandard]System.Collections.IEnumerator MLPCPCK.SpecialAbilities.SurpriseSpecialBehaviour::TryToReturn(class ['Assembly-CSharp']DiskCardGame.CombatPhaseManager)
             * stfld        current | object DiskCardGame.CombatPhaseManager/'<DoCombatPhase>d__4'::'<>2__current'
             * ldarg.0      // this
             * ldc.i4.1
             * stfld        state | int32 DiskCardGame.CombatPhaseManager/'<DoCombatPhase>d__4'::'<>1__state'
             * ldc.i4.1
             * stloc.0      // V_0
             * leave        leave | IL_064d
             *
             * ldarg.0      // this
             * ldc.i4.m1
             * stfld        state | int32 DiskCardGame.CombatPhaseManager/'<DoCombatPhase>d__4'::'<>1__state'
             * todo: < NEW INSTRUCTIONS
             *
             * // [50 9 - 50 99]
             * IL_025b: ldarg.0      // this
             * IL_025c: ldfld        class DiskCardGame.SpecialBattleSequencer DiskCardGame.CombatPhaseManager/'<DoCombatPhase>d__4'::specialSequencer
             * IL_0261: ldnull
             * IL_0262: call         
            /*/
            
            /*/ todo: index ~8+
                IL_0000: ldarg.0      // this
                IL_0001: ldfld        int32 DiskCardGame.CombatPhaseManager/'<DoCombatPhase>d__4'::'<>1__state'     todo: state !!!
                IL_0006: stloc.1      // V_1
                IL_0007: ldarg.0      // this
                IL_0008: ldfld        class DiskCardGame.CombatPhaseManager DiskCardGame.CombatPhaseManager/'<DoCombatPhase>d__4'::'<>4__this'
                IL_000d: stloc.2      // V_2
                IL_000e: ldloc.1      // V_1
                IL_000f: switch       (IL_005f, IL_00f6, IL_0132, IL_015a, IL_0234, IL_028f, IL_02c0, IL_02e7, IL_03cd, IL_0415, IL_0490, IL_04d5, IL_04fb, IL_0580, IL_05da, IL_0600, IL_063b)
                IL_0058: ldc.i4.0
                IL_0059: stloc.0      // V_0
                IL_005a: leave        IL_064d        todo: leave ref !!!
                ...
                IL_00de: callvirt     instance class [netstandard]System.Collections.IEnumerator DiskCardGame.CombatPhaseManager::InitializePhase(class [netstandard]System.Collections.Generic.List`1<class DiskCardGame.CardSlot>, bool)
                IL_00e3: stfld        object DiskCardGame.CombatPhaseManager/'<DoCombatPhase>d__4'::'<>2__current' todo: index ~20+ - current !!!
                IL_00e8: ldarg.0      // this
            /*/
            
            //Singleton<TurnManager>.Instance.CombatPhaseManager; todo: try as __instance
            /*MethodInfo tryToReturn = SymbolExtensions.GetMethodInfo(() => SurpriseSpecialBehaviour.TryToReturn()); // todo: можно вообще убрать параметр
            
            // todo: found enumerator base:
            int index = 1;
            var state = codeInstructionList[index].operand; // todo: found <>1__state
            index = 49;
            var current = codeInstructionList[index].operand; // todo: found <>2__current
            index = 10;
            var leave = codeInstructionList[index].operand; // todo: found leave reference

            index = 178;
            _lb1:
            if (codeInstructionList[index].opcode == OpCodes.Ldarg_0 && codeInstructionList[index - 1].opcode == OpCodes.Initobj) {
                codeInstructionList.Insert(index, new CodeInstruction(OpCodes.Ldarg_0));
                codeInstructionList.Insert(index + 1, new CodeInstruction(OpCodes.Call, tryToReturn));
                codeInstructionList.Insert(index + 2, new CodeInstruction(OpCodes.Stfld, current));
                codeInstructionList.Insert(index + 3, new CodeInstruction(OpCodes.Ldarg_0));
                codeInstructionList.Insert(index + 4, new CodeInstruction(OpCodes.Ldc_I4_5));
                codeInstructionList.Insert(index + 5, new CodeInstruction(OpCodes.Stfld, state));
                codeInstructionList.Insert(index + 6, new CodeInstruction(OpCodes.Ldc_I4_1));
                codeInstructionList.Insert(index + 7, new CodeInstruction(OpCodes.Stloc_0));
                codeInstructionList.Insert(index + 8, new CodeInstruction(OpCodes.Leave, leave));
                codeInstructionList.Insert(index + 9, new CodeInstruction(OpCodes.Ldarg_0));
                codeInstructionList.Insert(index + 10, new CodeInstruction(OpCodes.Ldc_I4_M1));
                codeInstructionList.Insert(index + 11, new CodeInstruction(OpCodes.Stfld, state));
            }
            else {
                index++;
                goto _lb1;
            }
            return codeInstructionList;
        }
    }*/
}