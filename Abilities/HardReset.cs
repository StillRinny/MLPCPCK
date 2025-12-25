using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using DiskCardGame;
using EasyFeedback.APIs;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using InscryptionAPI.Triggers;
using Mono.Cecil;
using MonoMod.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MLPCPCK_utils.Abilities
{
    public class HardReset : AbilityBehaviour {
        private static Ability ability;
        public override Ability Ability => ability;

        public static Ability AddAbility() {
            AbilityInfo abil = AbilityManager.New(nameof(MLPAbility),
                "Hard Reset",
                "",
                typeof(HardReset),
                "hardresetsigilmlp.png"
            );
            abil.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture("hardresetsigilmlpsmall.png"));
            abil.powerLevel = 4;
            abil.canStack = false;
            abil.opponentUsable = false;
            abil.AddMetaCategories(AbilityMetaCategory.Part1Rulebook);
            return ability = abil.ability;
        }

        private BoardState state;
        private bool flag;

        private void Start() {
            if (Card.OnBoard) state = BoardState.GenerateFromCurrentBoard();
        }
        
        public bool RespondsToModifyDamageTaken(PlayableCard target, int damage, PlayableCard attacker, int originalDamage) => 
            target == Card && !Card.HasShield();
        public int OnModifyDamageTaken(PlayableCard target, int damage, PlayableCard attacker, int originalDamage)
        {
            if ((!attacker && (target.Health <= damage || attacker.HasAbility(Ability.Deathtouch)) &&
                 !attacker.HasSpecialAbility(SpecialTriggeredAbility.GiantCard))
                || target.Health <= damage)
            {
                flag = true;
                Card.RenderInfo.hiddenHealth = true;
                Card.RenderCard();
                return 0;
            }
            else return damage;
        }
        public int TriggerPriority(PlayableCard target, int damage, PlayableCard attacker) => int.MaxValue;

        public override bool RespondsToResolveOnBoard() => true;
        public override IEnumerator OnResolveOnBoard() {
            state = BoardState.GenerateFromCurrentBoard();
            yield break;
        }
        
        public override bool RespondsToPreDeathAnimation(bool wasSacrifice) => !wasSacrifice; // Emergency
        public override IEnumerator OnPreDeathAnimation(bool wasSacrifice) {
            RevertToSnapshot();
            Singleton<UIManager>.Instance.Effects.GetEffect<ScreenGlitchEffect>().SetIntensity(1f, 0.4f);
            yield return new WaitForSeconds(1f);
        }
        
        public override bool RespondsToTurnEnd(bool isPlayerTurn) => !isPlayerTurn && flag;

        public override IEnumerator OnTurnEnd(bool isPlayerTurn) {
            RevertToSnapshot();
            Singleton<UIManager>.Instance.Effects.GetEffect<ScreenGlitchEffect>().SetIntensity(1f, 0.4f);
            yield break;
        }
        
        private void RevertToSnapshot() {
            Singleton<LifeManager>.Instance.SetNumWeightsImmediate(state.playerDamage, state.opponentDamage);
            
            foreach (CardSlot allSlot in Singleton<BoardManager>.Instance.AllSlots) {
                if (allSlot.Card == null) continue;
                PlayableCard card = allSlot.Card;
                allSlot.Card.UnassignFromSlot();
                UnityEngine.Object.Destroy(card.gameObject);
            }

            var applySlotState = AccessTools.Method(typeof(PhotographerSnapshotManager), "ApplySlotState");
            var obj = new PhotographerSnapshotManager(); // it's okay!

            var slots = state.playerSlots;
            var actualSlots = Singleton<BoardManager>.Instance.PlayerSlotsCopy;
            for (int index = 0; index < slots.Count; ++index) {
                if (slots[index].card != null) 
                    StartCoroutine((IEnumerator)applySlotState.Invoke(obj, new [] { (object)slots[index], actualSlots[index] }));
            }
            
            slots = state.opponentSlots;
            actualSlots = Singleton<BoardManager>.Instance.OpponentSlotsCopy;
            for (int index = 0; index < slots.Count; ++index) {
                if (slots[index].card != null) 
                    StartCoroutine((IEnumerator)applySlotState.Invoke(obj, new [] { (object)slots[index], actualSlots[index] }));
            }
            
            //applySlotStates.Invoke(obj, new [] {(object)state.playerSlots, Singleton<BoardManager>.Instance.PlayerSlotsCopy});
            //ApplySlotStates(state.playerSlots, Singleton<BoardManager>.Instance.PlayerSlotsCopy);
            
            //applySlotStates.Invoke(obj, new [] {(object)state.opponentSlots, Singleton<BoardManager>.Instance.OpponentSlotsCopy});
            //ApplySlotStates(state.opponentSlots, Singleton<BoardManager>.Instance.OpponentSlotsCopy);
        }

        /*private void ApplySlotStates(List<BoardState.SlotState> slotStates, List<CardSlot> actualSlots) {
            for (int index = 0; index < slotStates.Count; ++index) {
                if (slotStates[index].card != null)
                    this.StartCoroutine(this.ApplySlotState(slotStates[index], actualSlots[index]));
            }
        }*/
        
        /*private IEnumerator ApplySlotState(BoardState.SlotState slotState, CardSlot slot)
        {
            yield return (object) Singleton<BoardManager>.Instance.CreateCardInSlot(slotState.card.info, slot, 0.0f, false);
            PlayableCard card = slot.Card;
            (card.Anim as DiskCardAnimationController).Expand(true);
            card.TemporaryMods = new List<CardModificationInfo>((IEnumerable<CardModificationInfo>) slotState.card.temporaryMods);
            card.Status = new PlayableCardStatus(slotState.card.status);
            card.OnStatsChanged();
            Singleton<ResourcesManager>.Instance.ForceGemsUpdate();
        }*/
    }
}