using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.RuleBook;
using PonyLib.CardsHelpers;

namespace MLPCPCK_utils.Abilities
{
    public class CandyCurse : AbilityBehaviour
    {
        internal static Ability ability;
        public override Ability Ability => ability;

        public static void AddAbility() {
            AbilityInfo abil = AbilityManager.New(nameof(MLPAbility),
                rulebookName: "Candy Curse",
                rulebookDescription: "This sigil permanently grants 1 attack per card killed by it's bearer. " +
                    "Upon the card's death it will permanently lose 2 attack. " +
                    "Upon reaching 4 or above attack the sigil's bearer will gain Made of Stone. " +
                    "Upon reaching 7 or above attack, the card will gain Omnistrike. " +
                    "This sigil may not obey cards that are not The Candy Mare.",
                behavior: typeof(CandyCurse),
                pathToArt: "candisigilmlp.png"
            );
            abil.SetAbilityRedirect("Made of Stone", Ability.MadeOfStone, GameColors.Instance.seafoam);
            abil.SetAbilityRedirect("Omnistrike", Ability.AllStrike, GameColors.Instance.seafoam);
            abil.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture("candisigilmlpsmall.png"));
            abil.AddMetaCategories(AbilityMetaCategory.Part1Rulebook);
            abil.SetNotMergeable();
            abil.powerLevel = 4;
            abil.canStack = false;
            abil.opponentUsable = false;
            ability = abil.ability;
        }

        private const string ID = "-cc.ab"; // ID needed to found CandyCurse's modInfo
        private CardModificationInfo _curse; // CandyCurse's modInfo, including all changes take from this ability

        private void Start() { // init > _curse init
            var mods = Card.Info.Mods;
            if (mods.Count == 0) return;
            
            _curse = mods.Find(m => m.singletonId != null && m.singletonId.Equals(ID)); // Pop _curse modInfo from Card
            if (_curse == null) { // Create modInfo and push it to Card Mods, if it's not on Card yet
                _curse = new CardModificationInfo { singletonId = ID };
                RunState.Run.playerDeck.ModifyCard(Card.Info, _curse);
            }
            //UpdateAbilities(); // Needed only if other abilities added. AllStrike and MadeOfStone are Passive
        }

        // OnKill
        public override bool RespondsToOtherCardDie(PlayableCard card, CardSlot deathSlot, bool fromCombat, PlayableCard killer) => fromCombat && killer == Card;
        public override IEnumerator OnOtherCardDie(PlayableCard card, CardSlot deathSlot, bool fromCombat, PlayableCard killer) {
            _curse.attackAdjustment++;
            Card.OnStatsChanged(); // Update stats
            Card.Anim.LightNegationEffect();
            yield return PreSuccessfulTriggerSequence();
        }

        // OnDie
        public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer) => true;
        public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer) {
            int atk = _curse.attackAdjustment;

            if (atk <= 8) { // Base attack debuff
                atk -= 2;
                _curse.attackAdjustment = atk > 0 ? atk : 0;
            }
            else { // Progressive attack debuff
                double st = (double)atk / 2;
                double nd = 8 / (double)atk;
                nd = 2 / nd;
                st -= nd;
                _curse.attackAdjustment -= (int)st;
            }
            yield break;
        }

        // OnAttackEnded
        public override bool RespondsToAttackEnded() => Card.OnBoard;
        public override IEnumerator OnAttackEnded() {
            //UpdateAbilities(); // Needed only if other abilities added. AllStrike and MadeOfStone are Passive
            yield break;
        }

        private void UpdateAbilities() {
            int val = _curse.attackAdjustment;
            var sabil = _curse.abilities;
            var abil = new List<Ability>(2);

            if (val > 3) abil.Add(Ability.MadeOfStone); // >= 4
            if (val > 6) abil.Add(Ability.AllStrike); // >= 7
            if (sabil.Count == abil.Count) return;

            _curse.abilities = abil;
            //Card.UpdateAbilities(abil); // Needed only if other abilities added. AllStrike and MadeOfStone are Passive
            Card.OnStatsChanged();
            Card.Anim.PlayTransformAnimation();
        }
    }
}