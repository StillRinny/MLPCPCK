using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using MLPCPCK.Resources;
using UnityEngine;
using PonyLib.CardsHelpers;

namespace MLPCPCK.SpecialAbilities
{
    public class BlankSpecialBehaviour : SpecialCardBehaviour
    {
        internal static SpecialTriggeredAbility ability;
        
        public static void AddAbility() {
            ability = SpecialTriggeredAbilityManager.Add(nameof(MLPSpecial),
                "BlankFlank",
                typeof(BlankSpecialBehaviour)
            ).Id;
        }
        
        private static readonly CardInfo CardToDraw = CardLoader.GetCardByName("MLPCPCK_Blank_Flank").MakeChangedCopy(CardSamples.BlankFlankDebuffed);
        
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

                    PlayableCard card = self.CreateCard(CardToDraw);
                        //var method = AccessTools.Method(typeof(Opponent), "CreateCard");
                        //PlayableCard card = (PlayableCard)method.Invoke(self, new object[] { cardToDraw });
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
                
                yield return Singleton<CardSpawner>.Instance.SpawnCardToHand(CardToDraw);
                
                yield return new WaitForSeconds(0.8f);
                Singleton<ViewManager>.Instance.SwitchToView(oldView);
                yield return new WaitForSeconds(0.2f);
            }
        }
    }
}