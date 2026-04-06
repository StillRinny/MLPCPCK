using System;
using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using UnityEngine;

namespace MLPCPCK_utils.SpecialAbilities
{
    public class NictisSpecialBehaviour : SpecialCardBehaviour
    {
        public static SpecialTriggeredAbility ability;
        public static void AddAbility() {
            ability = SpecialTriggeredAbilityManager.Add(nameof(MLPSpecial),
                "Infiltrator",
                typeof(NictisSpecialBehaviour)
            ).Id;
        }
        /*/
         * Base Game "Shapeshifter" Special
         * with simple changes.
        /*/
        private const string ID = "-ci.bh";
        private const string BaseForm = "MLPCPCK_Nictis";
        private const string DefaultForm = "MLPCPCK_Mare";
        private static readonly Color FrozenSkyColor = new Color(0.12f, 0.68f, 0.92f);

        public override bool RespondsToDrawn() => true;
        public override IEnumerator OnDrawn() {
          DisguiseInBattle();
          yield break;
        }

        public override bool RespondsToResolveOnBoard() => true;
        public override IEnumerator OnResolveOnBoard() {
          yield return RevealInBattle();
        }

        public override IEnumerator OnShownForCardSelect(bool forPositiveEffect) {
          DisguiseOutOfBattle();
          yield break;
        }

        public override IEnumerator OnSelectedForDeckTrial() {
          DisguiseOutOfBattle();
          yield break;
        }

        public override void OnShownInDeckReview() {
          DisguiseOutOfBattle();
          ShowBlueEyes();
        }

        public override void OnShownForCardChoiceNode() => DisguiseAsCardChoice();

        private IEnumerator RevealInBattle() {
          yield return new WaitForSeconds(0.25f);
          AudioController.Instance.PlaySound3D("trial_cave_outro#1", MixerGroup.TableObjectsSFX, transform.position);
          
          yield return PlayableCard.TransformIntoCard(CardLoader.GetCardByName(BaseForm));
        }

        private void DisguiseInBattle() {
          List<CardInfo> cardInfoList = new List<CardInfo>(Singleton<CardDrawPiles>.Instance.Deck.Cards);
          cardInfoList.RemoveAll(x => x.name == "!STATIC!GLITCH");
          DisguiseAsCard(cardInfoList.Count <= 0 ? CardLoader.GetCardByName(DefaultForm) : cardInfoList[SeededRandom.Range(0, cardInfoList.Count, SaveManager.SaveFile.GetCurrentRandomSeed())]);
          PlayableCard.AddPermanentBehaviour<NictisSpecialBehaviour>();
        }

        private void DisguiseOutOfBattle() {
          List<CardInfo> cardInfoList = new List<CardInfo>(RunState.Run.playerDeck.Cards);
          cardInfoList.RemoveAll(x => x.name == BaseForm || x.name == "!STATIC!GLITCH");
          DisguiseAsCard(cardInfoList.Count <= 0 ? CardLoader.GetCardByName(DefaultForm) : cardInfoList[UnityEngine.Random.Range(0, cardInfoList.Count)]);
        }

        private void DisguiseAsCardChoice() {
          CardInfo unlockedRareCard = CardLoader.GetRandomUnlockedRareCard(Environment.TickCount);
          while (unlockedRareCard.name == BaseForm)
            unlockedRareCard = CardLoader.GetRandomUnlockedRareCard(Environment.TickCount);
          unlockedRareCard.Mods.Add(new CardModificationInfo() {
            singletonId = ID,
            nameReplacement = string.Format(Localization.Translate("{0}?"), unlockedRareCard.DisplayedNameLocalized)
          });
          DisguiseAsCard(unlockedRareCard);
          ShowBlueEyes();
        }

        private void ShowBlueEyes() {
          Card.RenderInfo.forceEmissivePortrait = true;
          Card.StatsLayer.SetEmissionColor(FrozenSkyColor);
          Card.RenderCard();
        }

        private void DisguiseAsCard(CardInfo disguise) {
          Card.ClearAppearanceBehaviours();
          Card.SetInfo(disguise);
        }
    }
}