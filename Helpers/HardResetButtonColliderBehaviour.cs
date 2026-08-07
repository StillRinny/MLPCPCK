using System;
using DiskCardGame;

namespace MLPCPCK.Helpers {
    public class HardResetButtonColliderBehaviour : MainInputInteractable {
        private static bool ClickAvailable => GameFlowManager.IsCardBattle && Singleton<GlobalTriggerHandler>.Instance.StackSize == 0 && (Singleton<BoardManager>.Instance == null || !Singleton<BoardManager>.Instance.ChoosingSlot && !Singleton<BoardManager>.Instance.ChoosingSacrifices) && Singleton<TurnManager>.Instance.IsPlayerMainPhase && !Singleton<TurnManager>.Instance.GameEnding && !Singleton<ItemsManager>.Instance.ActivatingItem;
        internal Action OnClick;
        
        public override CursorType CursorType => CursorType.Point;

        public override void OnCursorSelectStart() {
            if (!ClickAvailable || OnClick == null) return;
            OnClick();
        }
    }
}
