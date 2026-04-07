using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using Sirenix.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MLPCPCK_utils.SpecialAbilities
{
    public class SurpriseSpecialBehaviour : SpecialCardBehaviour
    {
        public static SpecialTriggeredAbility ability;
        public static void AddAbility() {
            ability = SpecialTriggeredAbilityManager.Add(nameof(MLPSpecial),
                "Surprise",
                typeof(SurpriseSpecialBehaviour)
            ).Id;
        }

        protected static bool IsSurpriseExist = false;
        private static Dictionary<PlayableCard, int> SurpriseCards = new Dictionary<PlayableCard, int> {};
        private static List<string> SurpriseLines = new List<string> { "[e:1]Miss me?!?", "[e:4]Suuurprise!!!!", "I like this game!", "Don't forget about me!" };

        private int turnsInPlay = 0;
        private int turnsUntilDisappearance;

        public void Start() {
            try { // To hide error inside custom deck manager 
                turnsUntilDisappearance = SeededRandom.Range(2, 6, GetRandomSeed());
            } catch (Exception e) {  }
        }

        public override bool RespondsToUpkeep(bool playerUpkeep) => playerUpkeep == PlayableCard.IsPlayerCard();

        public override IEnumerator OnUpkeep(bool playerUpkeep) {
            turnsInPlay++;
            if (turnsInPlay != turnsUntilDisappearance) yield break;
            MakeDisappeared();
        }

        private void MakeDisappeared() {
            IsSurpriseExist = true;
            SurpriseCards.Add(PlayableCard, TurnManager.Instance.TurnNumber);
            PlayableCard.UnassignFromSlot();
            PlayableCard.Slot = null;
            PlayableCard.gameObject.SetActive(false);
        }

        internal static IEnumerator TryToReturn(CombatPhaseManager combatPhaseManager) { // Transpiler edition
            if (!IsSurpriseExist) yield break;
            var surpriseDictionary = SurpriseCards;
            if (surpriseDictionary.Count == 0) {
                IsSurpriseExist = false;
                yield break;
            }

            int turnNumber = Singleton<TurnManager>.Instance.TurnNumber;
            if (turnNumber < 3) ClearSurprises(); // issue: Player exit combat via menu when Surprise disappeared
            
            bool isPlayerTurn = Singleton<TurnManager>.Instance.IsPlayerTurn;
            
            // Found cards
            List<PlayableCard> surpriseCards = surpriseDictionary.Keys.ToList();
            var cards = surpriseCards.FindAll(c => c.IsPlayerCard() == isPlayerTurn && surpriseDictionary[c] != turnNumber); // todo: do .Find(), if .FindAll() no sense
            if (cards.IsNullOrEmpty()) yield break;

            foreach (var card in cards) {// todo: Make Single execution, if no sense
                // Attack now?
                int seed = SaveManager.SaveFile.GetCurrentRandomSeed() + Singleton<GlobalTriggerHandler>.Instance.NumTriggersThisBattle; // in fact GetRandomSeep() but static
                if (SeededRandom.Range(0, 2, seed) != 0) continue;

                // Has empty slot?
                List<CardSlot> emptySlots = Singleton<BoardManager>.Instance.GetSlots(isPlayerTurn).Where(s => s.Card == null).ToList();
                if (emptySlots.IsNullOrEmpty()) break;

                var slot = emptySlots[SeededRandom.Range(0, emptySlots.Count - 1, seed)];
                yield return Singleton<BoardManager>.Instance.AssignCardToSlot(card, slot);
                card.gameObject.SetActive(true);
                int attackCount = turnNumber - surpriseDictionary[card]; // Count of attacks
                surpriseDictionary.Remove(card);
                if (slot.Card == null) continue; // Card is Dead for some reason

                // Show special Text Line on a Card
                TalkingCard talkingCard = card.GetComponent<TalkingCard>();
                if (talkingCard != null)
                {
                    yield return new WaitForSeconds(0.1f);
                    Singleton<ViewManager>.Instance.SwitchToView(View.Board, true, true);
                    yield return new WaitForSeconds(0.1f);

                    DialogueEvent.Line line = new DialogueEvent.Line
                    {
                        text = SurpriseLines[
                            SeededRandom.Range(0, SurpriseLines.Count - 1, seed)]
                    };
                    yield return talkingCard.PlayLine(line);
                }

                //AccessTools.Property(typeof(CombatPhaseManager), "DamageDealtThisPhase").SetValue(combatPhaseManager, 0); // must be 0
                //__instance.CombatPhaseManager.DamageDealtThisPhase = 0; // ^ private set ^ todo: << #1

                // Make Card attack slots "x" times
                for (int i = 0; i < attackCount; i++)
                {
                    if (slot.Card == null) continue;
                    yield return combatPhaseManager.SlotAttackSequence(slot);
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }

        internal static IEnumerator TryToReturnOld(TurnManager turnManager) {
            // Base checks
            if (!IsSurpriseExist) yield break;
            var surpriseDictionary = SurpriseCards;
            if (surpriseDictionary.Count == 0) {
                IsSurpriseExist = false;
                yield break;
            }
            if (turnManager.TurnNumber < 3) ClearSurprises(); // issue: Player exit combat via menu when Surprise disappeared

            bool isPlayerTurn = turnManager.IsPlayerTurn;

            // Found cards
            List<PlayableCard> surpriseCards = surpriseDictionary.Keys.ToList();
            var cards = surpriseCards.FindAll(c => c.IsPlayerCard() == isPlayerTurn && surpriseDictionary[c] != turnManager.TurnNumber); // todo: do .Find(), if .FindAll() no sense
            if (cards.IsNullOrEmpty()) yield break;

            foreach (var card in cards) { // todo: Make Single execution, if no sense
                // Attack now?
                int seed = SaveManager.SaveFile.GetCurrentRandomSeed() + Singleton<GlobalTriggerHandler>.Instance.NumTriggersThisBattle; // in fact GetRandomSeep() but static
                if (SeededRandom.Range(0, 2, seed) != 0) continue;

                // Has empty slot?
                List<CardSlot> emptySlots = Singleton<BoardManager>.Instance.GetSlots(isPlayerTurn)
                    .Where(s => s.Card == null).ToList();
                if (emptySlots.IsNullOrEmpty()) continue;

                var slot = emptySlots[SeededRandom.Range(0, emptySlots.Count - 1, seed)];
                yield return Singleton<BoardManager>.Instance.AssignCardToSlot(card, slot);
                card.gameObject.SetActive(true);
                int attackCount = turnManager.TurnNumber - surpriseDictionary[card]; // Count of attacks
                surpriseDictionary.Remove(card);
                if (slot.Card == null) continue; // Card is Dead for some reason

                // Show special Text Line on a Card
                TalkingCard talkingCard = card.GetComponent<TalkingCard>();
                if (talkingCard != null) {
                    yield return new WaitForSeconds(0.1f);
                    Singleton<ViewManager>.Instance.SwitchToView(View.Board, true, true);
                    yield return new WaitForSeconds(0.1f);

                    DialogueEvent.Line line = new DialogueEvent.Line {
                        text = SurpriseLines[
                            SeededRandom.Range(0, SurpriseLines.Count - 1, seed)]
                    };
                    yield return talkingCard.PlayLine(line);
                }

                AccessTools.Property(typeof(CombatPhaseManager), "DamageDealtThisPhase").SetValue(turnManager.CombatPhaseManager, 0); // must be 0 // todo: вынести до foreach?
                    //__instance.CombatPhaseManager.DamageDealtThisPhase = 0; // ^ private set ^ todo: << #1

                // Make Card attack slots "x" times
                for (int i = 0; i < attackCount; i++) {
                    if (slot.Card == null) continue;
                    yield return turnManager.CombatPhaseManager.SlotAttackSequence(slot);
                    yield return new WaitForSeconds(0.1f);
                }

                if (turnManager.CombatPhaseManager.DamageDealtThisPhase > 0) { // todo: убрать за пределы foreach?
                    var combatPhaseManager = turnManager.CombatPhaseManager;
                    var playerIsAttacker = turnManager.IsPlayerTurn;
                    var specialSequencer = turnManager.SpecialSequencer;

                    yield return new WaitForSeconds(0.4f);
                    yield return combatPhaseManager.VisualizeDamageMovingToScales(playerIsAttacker);
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
                        yield return combatPhaseManager.VisualizeExcessLethalDamage(excessDamage, specialSequencer);
                    }
                }
            }
            Singleton<ViewManager>.Instance.Controller.LockState = ViewLockState.Unlocked;
        }

        internal static void ClearSurprises() {
            foreach (var surpriseCard in SurpriseCards.Keys) {
                Object.Destroy(surpriseCard);
            }
            SurpriseCards.Clear();
            IsSurpriseExist = false;
        }
    }
}