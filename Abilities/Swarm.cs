using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace MLPCPCK_utils.Abilities
{
    public class Swarm : AbilityBehaviour {
        private static Ability ability;
        public override Ability Ability => ability;
        public static Ability AddAbility() {
            AbilityInfo abil = AbilityManager.New(nameof(MLPAbility),
                "Swarms",
                "This sigil will place a random changeling card on either side of it's bearer once placed.",
                typeof(Swarm),
                "swarmsigilmlp.png"
            );
            abil.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture("swarmsigilmlpsmall.png"));
            abil.powerLevel = 2;
            abil.canStack = false;
            abil.opponentUsable = false;
            abil.AddMetaCategories(AbilityMetaCategory.Part1Rulebook);
            return ability = abil.ability;
        }
        private readonly string[] _cardsToPlace = { "MLPCPCK_Changeling", "MLPCPCK_Changeling_Drone", "MLPCPCK_Changeling_Brute", "MLPCPCK_Changeling_Scout" };

        // OnResolve
        public override bool RespondsToResolveOnBoard() => true; // No simple condition.
        public override IEnumerator OnResolveOnBoard() {
            bool isPlayerCard = Card.IsPlayerCard(); // not fully used
            List<CardSlot> slots = Singleton<BoardManager>.Instance.GetSlots(isPlayerCard);
            int index = slots.IndexOf(Card.Slot);
            if(index < 0) yield break; // can be deleted

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

            if (Singleton<ViewManager>.Instance.CurrentView != View.Board) { // Set correct View.
                yield return new WaitForSeconds(0.2f);
                Singleton<ViewManager>.Instance.SwitchToView(View.Board);
                yield return new WaitForSeconds(0.2f);
            }
            yield return PreSuccessfulTriggerSequence();

            int seed = SaveManager.SaveFile.GetCurrentRandomSeed() + Singleton<BoardManager>.Instance.CardsOnBoard.Count + TurnManager.Instance.TurnNumber;
            CardInfo cardToPlace = CardLoader.GetCardByName(_cardsToPlace[SeededRandom.Range(0, 3, seed)]); // Max value hand-set as top index of _cardsToPlace. Change if needed.
            if (toLeftValid) yield return Singleton<BoardManager>.Instance.CreateCardInSlot(cardToPlace, toLeft);
            cardToPlace = CardLoader.GetCardByName(_cardsToPlace[SeededRandom.Range(0, 3, seed)]); // Max value hand-set as top index of _cardsToPlace. Change if needed.
            if (toRightValid) yield return Singleton<BoardManager>.Instance.CreateCardInSlot(cardToPlace, toRight);
        }
    }
}