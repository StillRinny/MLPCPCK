using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using UnityEngine;

namespace MLPCPCK_utils.Patches
{
    [HarmonyPatch(typeof(CardMergeSequencer), "GetValidCardsForSacrifice")]
    public static class ModifyHostCardPatch
    {
        [HarmonyPrefix]
        public static void PostFix(ref CardInfo host) {
            var stateField = AccessTools.DeclaredField(typeof(List<CardInfo>), "cardsForSacrifice");//AccessTools.Field(typeof(List<CardInfo>), "cardsForSacrifice");

            AccessTools.
            
            var cards = (List<CardInfo>)stateField.GetValue(typeof(List<CardInfo>));
            cards.RemoveAll(x => x.Abilities.Contains(MLPAbilities.CandyCurse));
            stateField.SetValue(typeof(List<CardInfo>), cards);
        }

        /*
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            instructions.

            foreach (var instruction in instructions) {
                if (instruction.opcode == System.Reflection.Emit.OpCodes.) {
                    cardsForSacrifice.RemoveAll((Predicate<CardInfo>)
                        (x => x.Abilities.Contains(MLPAbilities.CandyCurse)));
                }
            }
        }*/
        
        /* Чё заменить: cardsForSacrifice.RemoveAll((Predicate<CardInfo>) (x => x.Mods.Exists((Predicate<CardModificationInfo>) (m => m.fromCardMerge)) || x.SpecialAbilities.Contains(SpecialTriggeredAbility.Shapeshifter)));
                      
                          IL_0014: call         class [netstandard]System.Collections.Generic.List`1<class DiskCardGame.CardInfo> DiskCardGame.RunState::get_DeckList()
                            IL_0019: newobj       instance void class [netstandard]System.Collections.Generic.List`1<class DiskCardGame.CardInfo>::.ctor(class [netstandard]System.Collections.Generic.IEnumerable`1<!0/*class DiskCardGame.CardInfo/>)
                                IL_001e: stloc.1      // cardsForSacrifice
                      
                      
                      [144 63 - 144 270]
                      IL_0000: ldarg.1      // x
                      IL_0001: callvirt     instance class [netstandard]System.Collections.Generic.List`1<class DiskCardGame.CardModificationInfo> DiskCardGame.CardInfo::get_Mods()
                      IL_0006: ldsfld       class [netstandard]System.Predicate`1<class DiskCardGame.CardModificationInfo> DiskCardGame.CardMergeSequencer/'<>c'::'<>9__10_2'
                      IL_000b: dup

                      IL_000c: brtrue.s     IL_0025
                      IL_000e: pop
                      IL_000f: ldsfld       class DiskCardGame.CardMergeSequencer/'<>c' DiskCardGame.CardMergeSequencer/'<>c'::'<>9'
                      IL_0014: ldftn        instance bool DiskCardGame.CardMergeSequencer/'<>c'::'<GetValidCardsForHost>b__10_2'(class DiskCardGame.CardModificationInfo)
                      IL_001a: newobj       instance void class [netstandard]System.Predicate`1<class DiskCardGame.CardModificationInfo>::.ctor(object, native int)
                      IL_001f: dup
                      IL_0020: stsfld       class [netstandard]System.Predicate`1<class DiskCardGame.CardModificationInfo> DiskCardGame.CardMergeSequencer/'<>c'::'<>9__10_2'
                      IL_0025: callvirt     instance bool class [netstandard]System.Collections.Generic.List`1<class DiskCardGame.CardModificationInfo>::Exists(class [netstandard]System.Predicate`1<!0/*class DiskCardGame.CardModificationInfo/>)
                      IL_002a: brtrue.s     IL_0048
                      IL_002c: ldarg.1      // x
                      IL_002d: callvirt     instance class [netstandard]System.Collections.Generic.List`1<valuetype DiskCardGame.SpecialTriggeredAbility> DiskCardGame.CardInfo::get_SpecialAbilities()
                      IL_0032: ldc.i4.7
                      IL_0033: callvirt     instance bool class [netstandard]System.Collections.Generic.List`1<valuetype DiskCardGame.SpecialTriggeredAbility>::Contains(!0/*valuetype DiskCardGame.SpecialTriggeredAbility/)
                      IL_0038: brtrue.s     IL_0048
                      IL_003a: ldarg.1      // x
                      IL_003b: callvirt     instance class [netstandard]System.Collections.Generic.List`1<valuetype DiskCardGame.SpecialTriggeredAbility> DiskCardGame.CardInfo::get_SpecialAbilities()
                      IL_0040: ldc.i4.s     25 // 0x19
                      IL_0042: callvirt     instance bool class [netstandard]System.Collections.Generic.List`1<valuetype DiskCardGame.SpecialTriggeredAbility>::Contains(!0/*valuetype DiskCardGame.SpecialTriggeredAbility/)
                      
                      IL_0143: brtrue.s     IL_0048
                      IL_0144: ldarg.1      // x
                      IL_0145: callvirt     instance class [netstandard]System.Collections.Generic.List`1<valuetype ['Assembly-CSharp']DiskCardGame.Ability> ['Assembly-CSharp']DiskCardGame.CardInfo::get_Abilities()
                      IL_0146: ldsfld       valuetype ['Assembly-CSharp']DiskCardGame.Ability MLPCPCK_utils.MLPAbilities::CandyCurse
                      IL_0147: callvirt     instance bool class [netstandard]System.Collections.Generic.List`1<valuetype ['Assembly-CSharp']DiskCardGame.Ability>::Contains(!0/*valuetype ['Assembly-CSharp']DiskCardGame.Ability/)
                      
                      IL_0047: ret
                      IL_0048: ldc.i4.1
                      IL_0049: ret
                      
                      
                      
                      IL_0000: ldarg.1      // x
                      IL_0001: callvirt     instance class [netstandard]System.Collections.Generic.List`1<class ['Assembly-CSharp']DiskCardGame.CardModificationInfo> ['Assembly-CSharp']DiskCardGame.CardInfo::get_Mods()
                      IL_0006: ldsfld       class [netstandard]System.Predicate`1<class ['Assembly-CSharp']DiskCardGame.CardModificationInfo> MLPCPCK_utils.Abilities.CandyCurse/'<>c'::'<>9__9_3'
                      IL_000b: dup
                      IL_000c: brtrue.s     IL_0025
                      IL_000e: pop
                      IL_000f: ldsfld       class MLPCPCK_utils.Abilities.CandyCurse/'<>c' MLPCPCK_utils.Abilities.CandyCurse/'<>c'::'<>9'
                      IL_0014: ldftn        instance bool MLPCPCK_utils.Abilities.CandyCurse/'<>c'::'<Start>b__9_3'(class ['Assembly-CSharp']DiskCardGame.CardModificationInfo)
                      IL_001a: newobj       instance void class [netstandard]System.Predicate`1<class ['Assembly-CSharp']DiskCardGame.CardModificationInfo>::.ctor(object, native int)
                      IL_001f: dup
                      IL_0020: stsfld       class [netstandard]System.Predicate`1<class ['Assembly-CSharp']DiskCardGame.CardModificationInfo> MLPCPCK_utils.Abilities.CandyCurse/'<>c'::'<>9__9_3'
                      IL_0025: callvirt     instance bool class [netstandard]System.Collections.Generic.List`1<class ['Assembly-CSharp']DiskCardGame.CardModificationInfo>::Exists(class [netstandard]System.Predicate`1<!0/*class ['Assembly-CSharp']DiskCardGame.CardModificationInfo/>)
                      IL_002a: brtrue.s     IL_004d
                      IL_002c: ldarg.1      // x
                      IL_002d: callvirt     instance class [netstandard]System.Collections.Generic.List`1<valuetype ['Assembly-CSharp']DiskCardGame.SpecialTriggeredAbility> ['Assembly-CSharp']DiskCardGame.CardInfo::get_SpecialAbilities()
                      IL_0032: ldc.i4.s     25 // 0x19
                      IL_0034: callvirt     instance bool class [netstandard]System.Collections.Generic.List`1<valuetype ['Assembly-CSharp']DiskCardGame.SpecialTriggeredAbility>::Contains(!0/*valuetype ['Assembly-CSharp']DiskCardGame.SpecialTriggeredAbility/)
                      
                      
                      IL_0039: brtrue.s     IL_004d
                      IL_003b: ldarg.1      // x
                      IL_003c: callvirt     instance class [netstandard]System.Collections.Generic.List`1<valuetype ['Assembly-CSharp']DiskCardGame.Ability> ['Assembly-CSharp']DiskCardGame.CardInfo::get_Abilities()
                      IL_0041: ldsfld       valuetype ['Assembly-CSharp']DiskCardGame.Ability MLPCPCK_utils.MLPAbilities::CandyCurse
                      IL_0046: callvirt     instance bool class [netstandard]System.Collections.Generic.List`1<valuetype ['Assembly-CSharp']DiskCardGame.Ability>::Contains(!0/*valuetype ['Assembly-CSharp']DiskCardGame.Ability/)
                      
                      
                      IL_004b: br.s         IL_004e
                      IL_004d: ldc.i4.1
                      IL_004e: ret
         */
        
        /*
                      IL_0000: ldarg.1      // x
                      IL_0001: callvirt     instance class [netstandard]System.Collections.Generic.List`1<class ['Assembly-CSharp']DiskCardGame.CardModificationInfo> ['Assembly-CSharp']DiskCardGame.CardInfo::get_Mods()
                      IL_0006: ldsfld       class [netstandard]System.Predicate`1<class ['Assembly-CSharp']DiskCardGame.CardModificationInfo> MLPCPCK_utils.Abilities.CandyCurse/'<>c'::'<>9__9_2'
                      IL_000b: dup
                      IL_000c: brtrue.s     IL_0025
                      IL_000e: pop
                      IL_000f: ldsfld       class MLPCPCK_utils.Abilities.CandyCurse/'<>c' MLPCPCK_utils.Abilities.CandyCurse/'<>c'::'<>9'
                      IL_0014: ldftn        instance bool MLPCPCK_utils.Abilities.CandyCurse/'<>c'::'<Start>b__9_2'(class ['Assembly-CSharp']DiskCardGame.CardModificationInfo)
                      IL_001a: newobj       instance void class [netstandard]System.Predicate`1<class ['Assembly-CSharp']DiskCardGame.CardModificationInfo>::.ctor(object, native int)
                      IL_001f: dup
                      IL_0020: stsfld       class [netstandard]System.Predicate`1<class ['Assembly-CSharp']DiskCardGame.CardModificationInfo> MLPCPCK_utils.Abilities.CandyCurse/'<>c'::'<>9__9_2'
                      IL_0025: callvirt     instance bool class [netstandard]System.Collections.Generic.List`1<class ['Assembly-CSharp']DiskCardGame.CardModificationInfo>::Exists(class [netstandard]System.Predicate`1<!0/*class ['Assembly-CSharp']DiskCardGame.CardModificationInfo/>)
                      IL_002a: brtrue.s     IL_003b
                      IL_002c: ldarg.1      // x
                      IL_002d: callvirt     instance class [netstandard]System.Collections.Generic.List`1<valuetype ['Assembly-CSharp']DiskCardGame.SpecialTriggeredAbility> ['Assembly-CSharp']DiskCardGame.CardInfo::get_SpecialAbilities()
                      IL_0032: ldc.i4.s     25 // 0x19
                      IL_0034: callvirt     instance bool class [netstandard]System.Collections.Generic.List`1<valuetype ['Assembly-CSharp']DiskCardGame.SpecialTriggeredAbility>::Contains(!0/*valuetype ['Assembly-CSharp']DiskCardGame.SpecialTriggeredAbility/)
                      IL_0039: br.s         IL_003c
                      IL_003b: ldc.i4.1
                      IL_003c: ret
         */
    }
}
    