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
            TextureHelper.GetImageAsTexture($"chaos.submerge.sigilmlp.png"),
            TextureHelper.GetImageAsTexture($"chaos.morsel.sigilmlp.png"),
            TextureHelper.GetImageAsTexture($"chaos.beehive.sigilmlp.png"),
            TextureHelper.GetImageAsTexture($"chaos.dogging.sigilmlp.png"),
        };

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
            _chaosStrategy.attackAdjustment = 0; // todo: Delete id Flying and Beehive attack Debuff removed
            
            var abil = _chaosStrategy.abilities[0];
            if (_currentStrategy != Strategy.None && _currentStrategy != Strategy.Dogging) {
                _chaosStrategy.negateAbilities[(int)_currentStrategy] = abil;
                Card.Status.hiddenAbilities.Remove(abil);
                if (abil == Ability.Submerge || abil == Ability.Morsel || abil == Ability.BeesOnHit) 
                    Card.TriggerHandler.RemoveAbility(abil); 
            }
            
            // Take new Strategy
            List<Strategy> strategies = new List<Strategy> { Strategy.Flying, Strategy.Submerge, Strategy.Morsel, Strategy.BeesOnHit, Strategy.Dogging };
            strategies.Remove(_currentStrategy);
            switch (strategies[SeededRandom.Range(0, strategies.Count, GetRandomSeed() + Singleton<TurnManager>.Instance.TurnNumber)]) {
                case Strategy.Flying:
                    _currentStrategy = Strategy.Flying;
                    _chaosStrategy.attackAdjustment = Card.Attack > 2 ? -2 : 0; // todo: Debuff might be erased
                    _chaosStrategy.abilities[0] = Ability.Flying;
                    _chaosStrategy.negateAbilities[0] = Ability.None;
                    Card.Status.hiddenAbilities.Add(Ability.Flying);
                    Card.TriggerHandler.AddAbility(Ability.Flying);
                    break;
                case Strategy.Submerge:
                    _currentStrategy = Strategy.Submerge;
                    _chaosStrategy.abilities[0] = Ability.Submerge;
                    _chaosStrategy.negateAbilities[1] = Ability.None;
                    Card.Status.hiddenAbilities.Add(Ability.Submerge);
                    Card.TriggerHandler.AddAbility(Ability.Submerge);
                    break;
                case Strategy.Morsel:
                    _currentStrategy = Strategy.Morsel;
                    _chaosStrategy.abilities[0] = Ability.Morsel;
                    _chaosStrategy.negateAbilities[2] = Ability.None;
                    Card.Status.hiddenAbilities.Add(Ability.Morsel);
                    Card.TriggerHandler.AddAbility(Ability.Morsel);
                    break;
                case Strategy.BeesOnHit:
                    _currentStrategy = Strategy.BeesOnHit;
                    _chaosStrategy.attackAdjustment = Card.Attack > 2 ? -2 : 0; // todo: Debuff might be erased
                    _chaosStrategy.abilities[0] = Ability.BeesOnHit;
                    _chaosStrategy.negateAbilities[3] = Ability.None;
                    Card.Status.hiddenAbilities.Add(Ability.BeesOnHit);
                    Card.TriggerHandler.AddAbility(Ability.BeesOnHit);
                    break;
                case Strategy.Dogging:
                    _currentStrategy = Strategy.Dogging;
                    _chaosStrategy.attackAdjustment = 2;
                    _chaosStrategy.abilities[0] = Ability.TriStrike; // TriStrike is placeholder, cuz Ability.None makes many bugs
                    break;
            }
            UpdateDisplay();
        }
        
        private void UpdateDisplay() {
            Card.Anim.LightNegationEffect();
            Card.RenderInfo.OverrideAbilityIcon(MLPAbility.ChaosAgent, _icons[(int)_currentStrategy]);
            Card.OnStatsChanged();;
        }
    }
}