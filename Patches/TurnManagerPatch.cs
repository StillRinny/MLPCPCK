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
            
            // Base checks
            if(!SurpriseSpecialBehaviour.IsSurpriseExist) yield break;
            var surpriseDictionary = SurpriseSpecialBehaviour.SurpriseCards;
            if (surpriseDictionary.Count == 0) {
                SurpriseSpecialBehaviour.IsSurpriseExist = false;
                yield break;
            }
            if (__instance.TurnNumber < 3) CleanupPhasePatch(); // issue: Player exit combat via menu when Surprise disappeared

            bool isPlayerTurn = __instance.IsPlayerTurn;
            
            // Found cards
            List<PlayableCard> surpriseCards = surpriseDictionary.Keys.ToList();
            var cards = surpriseCards.FindAll(c => c.IsPlayerCard() == isPlayerTurn && surpriseDictionary[c] != __instance.TurnNumber); // todo: do .Find(), if .FindAll() no sense
            if (cards.IsNullOrEmpty()) yield break;
            
            foreach (var card in cards) { // todo: Make Single execution, if no sense
                // Attack now?
                int seed = SaveManager.SaveFile.GetCurrentRandomSeed() * 10 + __instance.TurnNumber * 50 + RandomHelper.GetAdder();
                if (SeededRandom.Range(0, 2, seed) != 0) continue;

                // Has empty slot?
                List<CardSlot> emptySlots = Singleton<BoardManager>.Instance.GetSlots(isPlayerTurn)
                    .Where(s => s.Card == null).ToList();
                if (emptySlots.IsNullOrEmpty()) continue;

                var slot = emptySlots[SeededRandom.Range(0, emptySlots.Count - 1, seed)];
                yield return Singleton<BoardManager>.Instance.AssignCardToSlot(card, slot);
                card.gameObject.SetActive(true);
                int attackCount = __instance.TurnNumber - surpriseDictionary[card]; // Count of attacks
                surpriseDictionary.Remove(card);
                if (slot.Card == null) continue; // Card is Dead for some reason

                // Show special Text Line on a Card
                TalkingCard talkingCard = card.GetComponent<TalkingCard>();
                if (talkingCard != null) {
                    yield return new WaitForSeconds(0.1f);
                    Singleton<ViewManager>.Instance.SwitchToView(View.Board, true, true);
                    yield return new WaitForSeconds(0.1f);

                    DialogueEvent.Line line = new DialogueEvent.Line
                    {
                        text = SurpriseSpecialBehaviour.SurpriseLines[
                            SeededRandom.Range(0, SurpriseSpecialBehaviour.SurpriseLines.Count - 1, seed)]
                    };
                    yield return talkingCard.PlayLine(line);
                }

                AccessTools.Property(typeof(CombatPhaseManager), "DamageDealtThisPhase").SetValue(__instance.CombatPhaseManager, 0); // must be 0
                var method = AccessTools.Method(typeof(CombatPhaseManager), "SlotAttackSequence"); // todo: DELETE!!!
                // Make Card attack slots "x" times
                for (int i = 0; i < attackCount; i++)
                {
                    if (slot.Card == null) continue;
                    yield return method.Invoke(__instance.CombatPhaseManager, new[] { slot });
                    yield return new WaitForSeconds(0.1f);
                }
                
                if (__instance.CombatPhaseManager.DamageDealtThisPhase > 0) {
                    var combatPhaseManager = __instance.CombatPhaseManager;
                    var playerIsAttacker = __instance.IsPlayerTurn;
                    var specialSequencer = __instance.SpecialSequencer;
                    
                    yield return new WaitForSeconds(0.4f);
                    yield return AccessTools.Method(typeof(CombatPhaseManager), "VisualizeDamageMovingToScales").Invoke(combatPhaseManager, new object[] { playerIsAttacker });
                    int excessDamage = 0;
                    if (playerIsAttacker) {
                        excessDamage = Singleton<LifeManager>.Instance.Balance + combatPhaseManager.DamageDealtThisPhase - 5;
                        excessDamage = Mathf.Max(0, excessDamage);
                    }
                    int damage = combatPhaseManager.DamageDealtThisPhase - excessDamage;
                    AscensionStatsData.TryIncreaseStat(AscensionStat.Type.MostDamageDealt, combatPhaseManager.DamageDealtThisPhase);
                    
                    
                    if (!(specialSequencer != null) || !specialSequencer.PreventDamageAddedToScales)
                        yield return  Singleton<LifeManager>.Instance.ShowDamageSequence(damage, damage, !playerIsAttacker, 0.0f);
                    if (specialSequencer != null)
                        yield return  specialSequencer.DamageAddedToScale(damage + excessDamage, playerIsAttacker);
                    if ((!(specialSequencer != null) || !specialSequencer.PreventDamageAddedToScales) && excessDamage > 0 && Singleton<TurnManager>.Instance.Opponent.NumLives == 1 && Singleton<TurnManager>.Instance.Opponent.GiveCurrencyOnDefeat) 
                    {
                        yield return Singleton<TurnManager>.Instance.Opponent.TryRevokeSurrender();
                        RunState.Run.currency += excessDamage;
                        yield return AccessTools.Method(typeof(CombatPhaseManager), "VisualizeExcessLethalDamage").Invoke(combatPhaseManager, new object[] { excessDamage, specialSequencer });
                    }
                }
            }
            Singleton<ViewManager>.Instance.Controller.LockState = ViewLockState.Unlocked;
        }
        
        [HarmonyPatch(typeof(TurnManager), "CleanupPhase")]
        [HarmonyPostfix]
        public static void CleanupPhasePatch() {
            foreach (var surpriseCard in SurpriseSpecialBehaviour.SurpriseCards.Keys) {
                Object.Destroy(surpriseCard);
            }
            SurpriseSpecialBehaviour.SurpriseCards.Clear();
            SurpriseSpecialBehaviour.IsSurpriseExist = false;
        }
    }
}