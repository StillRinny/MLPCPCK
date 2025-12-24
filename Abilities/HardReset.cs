using System;
using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using EasyFeedback.APIs;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
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

        public override bool RespondsToResolveOnBoard() => true;
        public override IEnumerator OnResolveOnBoard() {
            state = BoardState.GenerateFromCurrentBoard();
            yield break;
        }
        
        public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer) => !wasSacrifice;
        public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer) {
            RevertToSnapshot();
            Singleton<UIManager>.Instance.Effects.GetEffect<ScreenGlitchEffect>().SetIntensity(1f, 0.4f);
            yield return new WaitForSeconds(1f);
        }
        
        private void RevertToSnapshot() {
            Singleton<LifeManager>.Instance.SetNumWeightsImmediate(state.playerDamage, state.opponentDamage);
            
            foreach (CardSlot allSlot in Singleton<BoardManager>.Instance.AllSlots) {
                if (allSlot.Card == null) continue;
                PlayableCard card = allSlot.Card;
                allSlot.Card.UnassignFromSlot();
                UnityEngine.Object.Destroy(card.gameObject);
            }

            var applySlotStates = AccessTools.Method(typeof(PhotographerSnapshotManager), "ApplySlotStates");
            var obj = new PhotographerSnapshotManager(); // it's okay!
            
            applySlotStates.Invoke(obj, new [] {(object)state.playerSlots, Singleton<BoardManager>.Instance.PlayerSlotsCopy});
            //ApplySlotStates(state.playerSlots, Singleton<BoardManager>.Instance.PlayerSlotsCopy);
            
            applySlotStates.Invoke(obj, new [] {(object)state.opponentSlots, Singleton<BoardManager>.Instance.OpponentSlotsCopy});
            //ApplySlotStates(state.opponentSlots, Singleton<BoardManager>.Instance.OpponentSlotsCopy);
        }

        /*private void ApplySlotStates(List<BoardState.SlotState> slotStates, List<CardSlot> actualSlots)
        {
            for (int index = 0; index < slotStates.Count; ++index)
            {
                if (slotStates[index].card != null)
                    this.StartCoroutine(this.ApplySlotState(slotStates[index], actualSlots[index]));
            }
        }
        
        private IEnumerator ApplySlotState(BoardState.SlotState slotState, CardSlot slot)
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