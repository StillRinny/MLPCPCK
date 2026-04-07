using System;
using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.RuleBook;
using Rewired.Utils;
using UnityEngine;

namespace MLPCPCK_utils.Abilities
{
    public class ChaosAgent : AbilityBehaviour {
        internal static Ability ability;
        public override Ability Ability => ability;

        public static void AddAbility() {
            AbilityInfo abil = AbilityManager.New(nameof(MLPAbility),
                rulebookName: "Chaos Agent",
                rulebookDescription: "A shard of Discord's chaotic power imprisoned inside [creature]." +
                    "Under it's influence, behavior of the [creature] will randomly change every turn." +
                    "\nThis sigil permanently negates Flying, Submerge, Morsel and Beehive sigils.",
                behavior: typeof(ChaosAgent),
                pathToArt: "chaossigilmlp.png"
            );
            abil.SetAbilityLearnedDialogue(new DialogueEvent.Line { text = "Madness...", speakerIndex = 1, emotion = Emotion.Neutral });
            abil.SetAbilityRedirect("Flying", Ability.Flying, GameColors.Instance.seafoam);
            abil.SetAbilityRedirect("Submerge", Ability.Submerge, GameColors.Instance.seafoam);
            abil.SetAbilityRedirect("Morsel", Ability.Morsel, GameColors.Instance.seafoam);
            abil.SetAbilityRedirect("Beehive", Ability.BeesOnHit, GameColors.Instance.seafoam);
            abil.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture("chaossigilmlpsmall.png"));
            abil.AddMetaCategories(AbilityMetaCategory.Part1Rulebook);
            abil.powerLevel = 2;
            abil.canStack = false;
            abil.opponentUsable = false;
            ability = abil.ability;
        }
        
        private const string ID = "-ch.ab"; // ID needed to prevent secondary mod adding
        private readonly CardModificationInfo _chaosStrategy = new CardModificationInfo() {
            singletonId = ID, 
            abilities = new List<Ability>(1) { Ability.TriStrike }, // TriStrike is placeholder, cuz Ability.None makes many bugs
            negateAbilities = new List<Ability>(5) { Ability.Flying, Ability.Submerge, Ability.Morsel, Ability.BeesOnHit, Ability.TriStrike }
        };
        
        private enum Strategy { // перенести всю нагрузку на список negateAbilities? 
            Flying,
            Submerge,
            Morsel,
            BeesOnHit,
            Dogging,
            None
        }
        private Strategy _currentStrategy = Strategy.None;

        private static Texture2D[] _icons = {
            TextureHelper.GetImageAsTexture("chaos.flying.sigilmlp.png"),
            TextureHelper.GetImageAsTexture("chaos.submerge.sigilmlp.png"),
            TextureHelper.GetImageAsTexture("chaos.morsel.sigilmlp.png"),
            TextureHelper.GetImageAsTexture("chaos.beehive.sigilmlp.png"),
            TextureHelper.GetImageAsTexture("chaos.dogging.sigilmlp.png"),
        };

        private void Start() { // Fast mod injection
            Card.AddTemporaryMod(_chaosStrategy);
            if (!Card.OnBoard) return;
            ChangeStrategy();
            UpdateDisplay();
        }

        public override bool RespondsToResolveOnBoard() => true;
        public override IEnumerator OnResolveOnBoard() {
            ChangeStrategy();
            yield return new WaitForSeconds(0.15f); // Visual Only 
            UpdateDisplay();
            if (!HasLearned) yield return LearnAbility(0.5f);
            else yield return new WaitForSeconds(0.15f);
        }

        public override bool RespondsToUpkeep(bool playerUpkeep) => playerUpkeep == Card.IsPlayerCard();
        public override IEnumerator OnUpkeep(bool _) => OnResolveOnBoard();

        private void ChangeStrategy() {
            // > CleanUp previous Strategy
            _chaosStrategy.attackAdjustment = 0; // todo: Delete iF Flying and Beehive attack Debuff removed
            if (Card.Health > 2) _chaosStrategy.healthAdjustment = 0; // Remove health Buff if Card can survive it
            
            if (_currentStrategy != Strategy.None && _currentStrategy != Strategy.Dogging) { // Skip if no Abilities taken from Strategy
                var abil = _chaosStrategy.abilities[0];
                _chaosStrategy.negateAbilities[(int)_currentStrategy] = abil; // Return current Ability to negation
                Card.Status.hiddenAbilities.Remove(abil); // Make current Ability visible
                if (abil == Ability.Submerge || abil == Ability.Morsel || abil == Ability.BeesOnHit) // Destroy Handler if Ability have it
                    Card.TriggerHandler.RemoveAbility(abil);
            }
            
            // > Take new Strategy
            List<Strategy> strategies = new List<Strategy> { Strategy.Flying, Strategy.Submerge, Strategy.Morsel, Strategy.BeesOnHit, Strategy.Dogging }; // To not make 300 conversions
            strategies.Remove(_currentStrategy); // To prevent second using 
            _currentStrategy = strategies[SeededRandom.Range(0, strategies.Count, GetRandomSeed())]; // Randomly takes new one
            switch (_currentStrategy) {
                case Strategy.Flying:
                    _chaosStrategy.attackAdjustment = Card.Attack > 2 ? -2 : 0; // todo: Debuff might be erased
                    goto default;
                case Strategy.BeesOnHit:
                    _chaosStrategy.healthAdjustment = 2;
                    goto default;
                case Strategy.Dogging:
                    _chaosStrategy.attackAdjustment = 2;
                    _chaosStrategy.abilities[0] = Ability.TriStrike; // TriStrike is placeholder, cuz Ability.None makes many bugs
                    break;
                default: // Flying, Submerge, Morsel, Beehive
                    int i = (int)_currentStrategy;
                    Ability abil = _chaosStrategy.negateAbilities[i];
                    _chaosStrategy.abilities[0] = abil;
                    _chaosStrategy.negateAbilities[i] = Ability.None;
                    Card.Status.hiddenAbilities.Add(abil);
                    Card.TriggerHandler.AddAbility(abil);
                    break;
            }
        }
        
        private void UpdateDisplay() {
            Card.Anim.LightNegationEffect();
            Card.RenderInfo.OverrideAbilityIcon(MLPAbility.ChaosAgent, _icons[(int)_currentStrategy]);
            Card.OnStatsChanged();;
        }
    }
}