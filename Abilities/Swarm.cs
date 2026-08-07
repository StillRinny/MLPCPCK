using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Triggers;
using PonyLib.CardsHelpers;

namespace MLPCPCK.Abilities
{
    public class Swarm : AbilityBehaviour, IPassiveAttackBuff {
        internal static Ability ability;
        public override Ability Ability => ability;
        public static void AddAbility() {
            AbilityInfo abil = AbilityManager.New(nameof(MLPAbility),
                "Swarm",
                "This sigil will place a random changeling card on either side of it's bearer once placed.",
                typeof(Swarm),
                "swarmsigilmlp.png"
            );
            abil.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture("swarmsigilmlpsmall.png"));
            abil.AddMetaCategories(AbilityMetaCategory.Part1Rulebook);
            abil.SetNotMergeable();
            abil.powerLevel = 2;
            abil.canStack = false;
            abil.opponentUsable = false;
            ability = abil.ability;
        }
        private readonly string[] _cardsToPlace = { "MLPCPCK_Changeling", "MLPCPCK_Changeling_Drone", "MLPCPCK_Changeling_Brute", "MLPCPCK_Changeling_Scout" };

        // Buff allied Changelings attack 
        public int GetPassiveAttackBuff(PlayableCard target) => target.HasTrait(MLPTrait.Changeling) && target.IsPlayerCard() == Card.IsPlayerCard() && Card.OnBoard ? 1 : 0;
        
        // OnResolve
        public override bool RespondsToResolveOnBoard() => true; // No simple condition.
        public override IEnumerator OnResolveOnBoard() {
            bool isPlayerCard = Card.IsPlayerCard(); // not fully used
            yield return CreateAdjacentCards(isPlayerCard);
        }

        private IEnumerator CreateAdjacentCards(bool isPlayerCard) {
            List<CardSlot> slots = Singleton<BoardManager>.Instance.GetSlots(isPlayerCard);
            int index = slots.IndexOf(Card.Slot);
            if(index < 0) yield break;

            // >> Slots takes and validation | start
            int index2 = index - 1; // Left slot index.
            bool toLeftValid = index2 >= 0;
            CardSlot toLeft = toLeftValid ? slots[index2] : null;
            index2 = index + 1; // Right slot index.
            bool toRightValid = index2 < slots.Count;
            CardSlot toRight = toRightValid ? slots[index2] : null;

            toLeftValid = toLeft != null && toLeft.Card == null;
            toRightValid = toRight != null && toRight.Card == null;
            if (!toLeftValid && !toRightValid) yield break;
            // << Slots takes and validation | end

            yield return PreSuccessfulTriggerSequence();

            int seed = GetRandomSeed();
            CardInfo cardToPlace = CardLoader.GetCardByName(_cardsToPlace[SeededRandom.Range(0, 3, seed)]); // Max value hand-set as top index of _cardsToPlace. Change if needed.
            if (toLeftValid) yield return Singleton<BoardManager>.Instance.CreateCardInSlot(cardToPlace, toLeft);
            cardToPlace = CardLoader.GetCardByName(_cardsToPlace[SeededRandom.Range(0, 3, seed + 50)]); // Max value hand-set as top index of _cardsToPlace. Change if needed.
            if (toRightValid) yield return Singleton<BoardManager>.Instance.CreateCardInSlot(cardToPlace, toRight);
        }
    }
}