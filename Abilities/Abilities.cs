using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace MLPCPCK_utils.Abilities
{
    public class CandyCurse : AbilityBehaviour
    {
        private static Ability ability;
        public override Ability Ability => ability;
        
        public static void AddAbility() {
            AbilityInfo abil = AbilityManager.New(nameof(MLPAbilities),
            "Candy Curse",
        "This sigil permanently grants 1 attack per card killed by it's bearer. " +
                        "Upon the card's death it will permanently lose 2 attack. " +
                        "Upon reaching 4 or above attack the sigil's bearer will gain Made of Stone. " +
                        "Upon reaching 7 or above attack, the card will gain Omnistrike. " +
                        "This sigil may not obey cards that are not The Candy Mare.",
                typeof(CandyCurse),
                "candisigilmlp.png"
            );
            abil.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture("candisigilmlpsmall.png"));
            abil.powerLevel = 4;
            abil.canStack = false;
            abil.opponentUsable = false;
            abil.AddMetaCategories(AbilityMetaCategory.Part1Rulebook);
            MLPAbilities.CandyCurse = ability = abil.ability; // Shortcut to use | Check definition in Plugin.cs
        }

        /*/
         * Merge prevention patch: ModifyHostCardPatch.cs 
        /*/
        
        private const string ID = "-cc.ab"; // ID needed to found CandyCurse's modInfo
        private CardModificationInfo _curse; // CandyCurse's modInfo, including all changes take from this ability
        
        private void Start() { // init > _curse init
            _curse = Card.Info.Mods.Find(m => m.singletonId.Equals(ID)); // Pop _curse modInfo from Card
            if (_curse == null) { // Create modInfo and push it to Card Mods, if it's not on Card yet
                _curse = new CardModificationInfo { singletonId = ID };
                RunState.Run.playerDeck.ModifyCard(Card.Info, _curse);
            }
            UpdateAbilities();
        }
        
        // OnKill
        public override bool RespondsToOtherCardDie(PlayableCard card, CardSlot deathSlot, bool fromCombat, PlayableCard killer) => fromCombat && killer == Card;
        public override IEnumerator OnOtherCardDie(PlayableCard card, CardSlot deathSlot, bool fromCombat, PlayableCard killer) {
            _curse.attackAdjustment++;
            Card.OnStatsChanged(); // Update stats
            Card.Anim.LightNegationEffect();
            yield break;
        }
        
        // OnDie
        public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer) => true;
        public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer) {
            int atk = _curse.attackAdjustment;
            atk -= 2;
            _curse.attackAdjustment = atk > 0 ? atk : 0;
            yield break;
        }

        // OnAttackEnded
        public override bool RespondsToAttackEnded() => Card.OnBoard;
        public override IEnumerator OnAttackEnded() {
            UpdateAbilities(); // Updating abilities taken from Curse
            yield break;
        }

        private void UpdateAbilities() {
            bool upgradeHappened = false;
            int val = _curse.attackAdjustment;
            var abil = _curse.abilities = new List<Ability>(2);

            if (val >= 4) {
                abil.Add(Ability.MadeOfStone);
                upgradeHappened = true;
            }
            if (val >= 7) {
                abil.Add(Ability.AllStrike);
                upgradeHappened = true;
            }
            if (upgradeHappened) Card.Anim.PlayTransformAnimation();
            Card.OnStatsChanged();
        }
    }
}