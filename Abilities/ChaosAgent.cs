using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.RuleBook;
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
                    "\nThis sigil permanently negates Flying and Submerge sigils.",
                behavior: typeof(ChaosAgent),
                pathToArt: "chaossigilmlp.png"
            );
            abil.SetAbilityLearnedDialogue(new DialogueEvent.Line { text = "Madness...", speakerIndex = 1, emotion = Emotion.Neutral });
            abil.SetAbilityRedirect("Flying", Ability.Flying, GameColors.Instance.seafoam);
            abil.SetAbilityRedirect("Submerge", Ability.Submerge, GameColors.Instance.seafoam);
            abil.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture("chaossigilmlpsmall.png"));
            abil.AddMetaCategories(AbilityMetaCategory.Part1Rulebook);
            abil.powerLevel = 1;
            abil.canStack = false;
            abil.opponentUsable = false;
            ability = abil.ability;
        }
        
        private const string ID = "-ch.ab"; // ID needed to prevent secondary mod adding
        private readonly CardModificationInfo _chaosStrategy = new CardModificationInfo() {
            singletonId = ID, 
            abilities = new List<Ability>(1) { Ability.None }, 
            negateAbilities = new List<Ability>(2) { Ability.Flying, Ability.Submerge }
        };
        
        private enum Strategy {
            Flying,
            Swimming,
            Dogging,
            None
        }
        private Strategy _currentStrategy = Strategy.None;

        private void Start() { // Fast mod injection
            Card.AddTemporaryMod(_chaosStrategy);
        }

        public override bool RespondsToResolveOnBoard() => true;
        public override IEnumerator OnResolveOnBoard() {
            ChangeStrategy();
            if (!HasLearned) yield return LearnAbility(0.5f);
            else yield return new WaitForSeconds(0.15f);
        }

        public override bool RespondsToUpkeep(bool playerUpkeep) => playerUpkeep == Card.IsPlayerCard();
        public override IEnumerator OnUpkeep(bool _) {
            ChangeStrategy();
            if (!HasLearned) yield return LearnAbility(0.5f);
            else yield return new WaitForSeconds(0.15f);
        }

        private void ChangeStrategy() {
            // CleanUp previous Strategy
            var abil = _chaosStrategy.abilities[0];
            if (abil != Ability.None) {
                _chaosStrategy.negateAbilities[abil == Ability.Flying ? 0 : 1] = abil;
                if (abil == Ability.Submerge) Card.TriggerHandler.RemoveAbility(abil);
                Card.Status.hiddenAbilities.Remove(abil);
            }
            
            // Take new Strategy
            List<Strategy> strategies = new List<Strategy> { Strategy.Flying, Strategy.Swimming, Strategy.Dogging };
            strategies.Remove(_currentStrategy);
            switch (strategies[SeededRandom.Range(0, strategies.Count - 1, GetRandomSeed())]) {
                case Strategy.Flying:
                    _currentStrategy = Strategy.Flying;
                    _chaosStrategy.attackAdjustment = Card.Attack > 2 ? -2 : 0; // todo: Debuff might be erased
                    _chaosStrategy.abilities[0] = Ability.Flying;
                    _chaosStrategy.negateAbilities[0] = Ability.None;
                    Card.Status.hiddenAbilities.Add(Ability.Flying);
                    break;
                case Strategy.Swimming:
                    _currentStrategy = Strategy.Swimming;
                    _chaosStrategy.attackAdjustment = 0;
                    _chaosStrategy.abilities[0] = Ability.Submerge;
                    _chaosStrategy.negateAbilities[1] = Ability.None;
                    Card.Status.hiddenAbilities.Add(Ability.Submerge);
                    Card.TriggerHandler.AddAbility(Ability.Submerge);
                    break;
                case Strategy.Dogging:
                    _currentStrategy = Strategy.Dogging;
                    _chaosStrategy.attackAdjustment = 2;
                    _chaosStrategy.abilities[0] = Ability.None;
                    break;
            }
            UpdateDisplay();
        }
        
        private void UpdateDisplay() {
            Card.Anim.LightNegationEffect();
            Card.RenderInfo.OverrideAbilityIcon(MLPAbility.ChaosAgent, TextureHelper.GetImageAsTexture($"ChaosAgent.{_currentStrategy.ToString()}.sigilmlp.png"));
            Card.OnStatsChanged();;
        }
    }
}