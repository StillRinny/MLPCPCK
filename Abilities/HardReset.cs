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

        private List<PlayableCard> _cards = new List<PlayableCard>();
        private List<Transform> _damageWeights;

        public override bool RespondsToResolveOnBoard() => true;
        public override IEnumerator OnResolveOnBoard() {
            List<CardSlot> slots = Singleton<BoardManager>.Instance.AllSlots; // playerSlots + opponentSlots
            PlayableCard component;
            foreach (var slot in slots) {
                PlayableCard card = slot.Card;
                
                component = UnityEngine.Object.Instantiate<GameObject>(Singleton<CardSpawner>.Instance.PlayableCardPrefab).GetComponent<PlayableCard>();
                component.SetInfo(card.Info);
                component.Slot = slot;
                AccessTools.Field(typeof(PlayableCard), "OpponentCard").SetValue(component, card.OpponentCard); // todo: !
                component.TemporaryMods = new List<CardModificationInfo>(card.TemporaryMods);
                _cards.Add(component);
                
                /*if (i > 3) { // queue
                    card = Singleton<BoardManager>.Instance.GetCardQueuedForSlot(_slots[i]);
                    if (card == null) continue;
                    component = UnityEngine.Object.Instantiate<GameObject>(Singleton<CardSpawner>.Instance.PlayableCardPrefab).GetComponent<PlayableCard>();
                    component.SetInfo(card.Info);
                    component.TemporaryMods = new List<CardModificationInfo>(card.TemporaryMods);
                    component.QueuedSlot = card.QueuedSlot;

                    cards[i+4] = component;
                }*/
            }

            Opponent self = TurnManager.Instance.Opponent;
            self.Queue.ForEach(x => {
                if (x == null) return;
                component = UnityEngine.Object.Instantiate<GameObject>(Singleton<CardSpawner>.Instance.PlayableCardPrefab).GetComponent<PlayableCard>();
                component.SetInfo(x.Info);
                component.QueuedSlot = x.QueuedSlot;
                AccessTools.Field(typeof(PlayableCard), "OpponentCard").SetValue(component, false); // todo: !
                component.TemporaryMods = new List<CardModificationInfo>(x.TemporaryMods);
                _cards.Add(component);
            });
            
            _damageWeights = (List<Transform>)AccessTools.Field(typeof(CombatPhaseManager3D), "damageWeights").GetValue(Singleton<CombatPhaseManager3D>.Instance);
            
            Plugin.Log.LogError($"[{nameof(HardReset)}] {nameof(_cards)}: {_cards.Count}; {nameof(_damageWeights)}: {_damageWeights.Count}.");
            yield break;
        }
    }
}