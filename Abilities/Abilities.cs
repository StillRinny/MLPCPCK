using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Logging;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;

namespace MLPCPCK_utils.Abilities
{
    public class CandyCurse : AbilityBehaviour
    {
        private static Ability ability;
        public override Ability Ability => ability;
        
        public static void AddAbility() {
            AbilityInfo val = AbilityManager.New(nameof(MLPAbilities),
                "Candy Curse",
                "This sigil permanently grants 1 attack per card killed by it's bearer. Upon the card's death it will permanently lose 2 attack. Upon reaching 4 or above attack the sigil's bearer will gain Made of Stone. Upon reaching 7 or above attack, the card will gain Omnistrike. This sigil may not obey cards that are not The Candy Mare.",
                typeof(CandyCurse),
                "candisigilmlp.png");
            val.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture("candisigilmlpsmall.png"));
            val.powerLevel = 4;
            val.canStack = false;
            val.opponentUsable = false;
            val.AddMetaCategories(AbilityMetaCategory.Part1Rulebook);
            ability = val.ability;
            
            MLPAbilities.CandyCurse = ability;
        }

        private const string Prefix = "+cc.ab";

        private CardModificationInfo GetCurse {
            get => _curse = Card.Info.Mods.Find(m => m.singletonId.Equals(Prefix));
            set => _curse = value;
        }
        private CardModificationInfo _curse = null;

        private void Start() { //init
            List<CardInfo> cardsForSacrifice = new List<CardInfo>((IEnumerable<CardInfo>) RunState.DeckList);

            cardsForSacrifice.RemoveAll((Predicate<CardInfo>)
                (x => x.Abilities.Exists((Predicate<Ability>)
                    (a => a == MLPAbilities.CandyCurse))));
            
            GetCurse = GetCurse; // Pretty smart
            if (_curse == null) {
                GetCurse = new CardModificationInfo { singletonId = Prefix };
                RunState.Run.playerDeck.ModifyCard(Card.Info, _curse);
            }
        }
        
        public override bool RespondsToOtherCardDie(PlayableCard card, CardSlot deathSlot, bool fromCombat, PlayableCard killer) => fromCombat && killer == Card;

        public override IEnumerator OnOtherCardDie(PlayableCard card, CardSlot deathSlot, bool fromCombat, PlayableCard killer) {
            _curse.attackAdjustment++;
            Card.OnStatsChanged();
            Card.Anim.LightNegationEffect();
            yield break;
        }
        
        public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer) => true;

        public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer) {
            int atk = _curse.attackAdjustment;
            atk -= 2;
            _curse.attackAdjustment = atk >= 0 ? atk : 0;
            yield break;
        }
    }
}