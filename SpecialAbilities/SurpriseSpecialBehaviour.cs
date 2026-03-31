using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using UnityEngine;

namespace MLPCPCK_utils.SpecialAbilities
{
    public class SurpriseSpecialBehaviour : SpecialCardBehaviour
    {
        public static SpecialTriggeredAbility ability;
        public static void AddAbility() {
            ability = SpecialTriggeredAbilityManager.Add(nameof(MLPSpecial),
                "Surprise",
                typeof(SurpriseSpecialBehaviour)
            ).Id;
        }
        
        public static bool IsSurpriseExist = false;
        public static Dictionary<PlayableCard, int> SurpriseCards = new Dictionary<PlayableCard, int> {};
        public static List<string> SurpriseLines = new List<string> { "[e:1]Miss me?!?", "[e:4]Suuurprise!!!!", "I like this game!", "Don't forget about me!" };
        
        private int turnsInPlay = 0;
        private int turnsUntilDisappearance;

        public void Start() {
            turnsUntilDisappearance = SeededRandom.Range(2, 6, SaveManager.SaveFile.GetCurrentRandomSeed() * 10 + Singleton<TurnManager>.Instance.TurnNumber * 50 + RandomHelper.GetAdder());
        }

        public override bool RespondsToUpkeep(bool playerUpkeep) => playerUpkeep == PlayableCard.IsPlayerCard();

        public override IEnumerator OnUpkeep(bool playerUpkeep) {
            turnsInPlay++;
            if (turnsInPlay != turnsUntilDisappearance) yield break;

            IsSurpriseExist = true;
            SurpriseCards.Add(PlayableCard, TurnManager.Instance.TurnNumber);
            PlayableCard.UnassignFromSlot();
            PlayableCard.Slot = null;
            PlayableCard.gameObject.SetActive(false);
        }
    }
}