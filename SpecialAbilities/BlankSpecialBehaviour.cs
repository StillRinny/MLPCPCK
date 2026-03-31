using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using UnityEngine;

namespace MLPCPCK_utils.SpecialAbilities
{
    public class BlankSpecialBehaviour : SpecialCardBehaviour
    {
        public static SpecialTriggeredAbility ability;
        public static void AddAbility() {
            ability = SpecialTriggeredAbilityManager.Add(nameof(MLPSpecial),
                "BlankFlank",
                typeof(BlankSpecialBehaviour)
            ).Id;
        }
        private static CardInfo cardToDraw = CardLoader.GetCardByName("MLPCPCK_Blank_Flank");
        private static CardModificationInfo costMod = new CardModificationInfo(-1, 0) { bloodCostAdjustment = -3 };
        
        
        public override bool RespondsToOtherCardDie(PlayableCard card, CardSlot deathSlot, bool fromCombat, PlayableCard killer) => killer == PlayableCard && !card.HasTrait(Trait.Terrain);

        public override IEnumerator OnOtherCardDie(PlayableCard card, CardSlot deathSlot, bool fromCombat, PlayableCard killer) {
            yield return CreateDrawnCard();
            Singleton<ViewManager>.Instance.Controller.LockState = ViewLockState.Unlocked;
        }
        
        private IEnumerator CreateDrawnCard() {
            View oldView = Singleton<ViewManager>.Instance.CurrentView;
            Singleton<ViewManager>.Instance.Controller.LockState = ViewLockState.Locked;
            
            if (!PlayableCard.IsPlayerCard()) { // Taken from Resurrector and improved
                Opponent self = TurnManager.Instance.Opponent;
                if (self != null) { // redo slot target logic
                    CardSlot mainQueued = null;
                    CardSlot secondaryQueued = null;
                    List<CardSlot> opposingSlots = Singleton<BoardManager>.Instance.GetSlots(false);
                    foreach (CardSlot opposingSlot in opposingSlots) {
                        if (Singleton<BoardManager>.Instance.GetCardQueuedForSlot(opposingSlot) == null) {
                            if (opposingSlot.Card == null) {
                                mainQueued = opposingSlot;
                                break;
                            }
                            else {
                                secondaryQueued = opposingSlot;
                            }
                        }
                    }
                    if (mainQueued == null && secondaryQueued != null) mainQueued = secondaryQueued;
                    else if (mainQueued == null && secondaryQueued == null) yield break;

                    Singleton<ViewManager>.Instance.SwitchToView(View.OpponentQueue);

                    PlayableCard card = self.CreateCard(cardToDraw);
                        //var method = AccessTools.Method(typeof(Opponent), "CreateCard");
                        //PlayableCard card = (PlayableCard)method.Invoke(self, new object[] { cardToDraw });
                    card.AddTemporaryMod(costMod);
                    Singleton<BoardManager>.Instance.QueueCardForSlot(card, mainQueued, 0.25f);
                    self.Queue.Add(card);
                    
                    yield return new WaitForSeconds(0.8f);
                    Singleton<ViewManager>.Instance.SwitchToView(oldView);
                }
            }
            else {
                yield return new WaitForSeconds(0.2f);
                Singleton<ViewManager>.Instance.SwitchToView(View.Hand);
                yield return new WaitForSeconds(0.2f);
                
                yield return Singleton<CardSpawner>.Instance.SpawnCardToHand(cardToDraw, new List<CardModificationInfo>(1) { costMod });
                
                yield return new WaitForSeconds(0.8f);
                Singleton<ViewManager>.Instance.SwitchToView(oldView);
                yield return new WaitForSeconds(0.2f);
            }
        }
    }
}