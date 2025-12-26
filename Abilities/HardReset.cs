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
    public class HardReset : AbilityBehaviour, IModifyDamageTaken, IOnPreScalesChanged, IOnPreScalesChangedRef {
        private static Ability ability;
        public override Ability Ability => ability;

        public static Ability AddAbility() {
            AbilityInfo abil = AbilityManager.New(nameof(MLPAbility),
                "Hard Reset",
                "* Awesome Description *",
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

        private BoardState _state;
        private bool _resetFlag;

        private void Start() { // If Ability added after Card Resolve
            if (Card.OnBoard) _state = BoardState.GenerateFromCurrentBoard();
        }
        
        public override bool RespondsToResolveOnBoard() => true;
        public override IEnumerator OnResolveOnBoard() {
            _state = BoardState.GenerateFromCurrentBoard();
            yield break;
        }
        
        public bool RespondsToModifyDamageTaken(PlayableCard target, int damage, PlayableCard attacker, int originalDamage) => 
            target == Card && !Card.HasShield();
        public int OnModifyDamageTaken(PlayableCard target, int damage, PlayableCard attacker, int originalDamage) {
            if (target.Health > damage && (attacker == null || !attacker.HasAbility(Ability.Deathtouch))) return damage;
            _resetFlag = true;
            Card.RenderInfo.hiddenAttack = true;
            Card.RenderInfo.hiddenHealth = true;
            Card.AttackedThisTurn = true; // Prevent Attack
            Card.RenderCard();
            return 0;
        }
        public int TriggerPriority(PlayableCard target, int damage, PlayableCard attacker) => int.MaxValue;
        
        public override bool RespondsToPreDeathAnimation(bool wasSacrifice) => !wasSacrifice; // Emergency
        public override IEnumerator OnPreDeathAnimation(bool _) => RevertToSnapshot();
        
        public bool RespondsToPreScalesChangedRef(int _, int __, bool toPlayer) => toPlayer && _resetFlag;
        public int CollectPreScalesChangedRef(int _, ref int __, ref bool toPlayer) => 0;
        
        public bool RespondsToPreScalesChanged(int _, bool toPlayer, int __, bool ___) => toPlayer && _resetFlag;
        public IEnumerator OnPreScalesChanged(int _, bool toPlayer, int __, bool ___) => RevertToSnapshot();
        
        private IEnumerator RevertToSnapshot() {
            Singleton<LifeManager>.Instance.SetNumWeightsImmediate(_state.playerDamage, _state.opponentDamage);
            
            foreach (CardSlot allSlot in Singleton<BoardManager>.Instance.AllSlots) {
                if (allSlot.Card == null) continue;
                PlayableCard card = allSlot.Card;
                allSlot.Card.UnassignFromSlot();
                UnityEngine.Object.Destroy(card.gameObject);
            }

            var applySlotState = AccessTools.Method(typeof(PhotographerSnapshotManager), "ApplySlotState");
            var obj = new PhotographerSnapshotManager(); // it's okay!

            var slots = _state.playerSlots;
            var actualSlots = Singleton<BoardManager>.Instance.PlayerSlotsCopy;
            for (int index = 0; index < slots.Count; ++index) {
                if (slots[index].card != null) 
                    StartCoroutine((IEnumerator)applySlotState.Invoke(obj, new [] { (object)slots[index], actualSlots[index] }));
            }
            
            slots = _state.opponentSlots;
            actualSlots = Singleton<BoardManager>.Instance.OpponentSlotsCopy;
            for (int index = 0; index < slots.Count; ++index) {
                if (slots[index].card != null) 
                    StartCoroutine((IEnumerator)applySlotState.Invoke(obj, new [] { (object)slots[index], actualSlots[index] }));
            }
            
            //applySlotStates.Invoke(obj, new [] {(object)state.playerSlots, Singleton<BoardManager>.Instance.PlayerSlotsCopy});
            //ApplySlotStates(state.playerSlots, Singleton<BoardManager>.Instance.PlayerSlotsCopy);
            
            //applySlotStates.Invoke(obj, new [] {(object)state.opponentSlots, Singleton<BoardManager>.Instance.OpponentSlotsCopy});
            //ApplySlotStates(state.opponentSlots, Singleton<BoardManager>.Instance.OpponentSlotsCopy);
            
            Singleton<UIManager>.Instance.Effects.GetEffect<ScreenGlitchEffect>().SetIntensity(1f, 0.4f);
            yield return new WaitForSeconds(1f);
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