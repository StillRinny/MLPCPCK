using DiskCardGame;
using UnityEngine;

namespace MLPCPCK_utils.Tests
{
    public class HardResetProp : HighlightedInteractable {
        private Animator anim;
        public override CursorType CursorType => !Pressable ? CursorType.Default : CursorType.Slap;

        public void Awake() {
            ChangePosition();
        }

        private void ChangePosition() { // Test
            transform.SetPositionAndRotation(Singleton<BoardManager3D>.Instance.Bell.transform.position, Singleton<BoardManager3D>.Instance.Bell.transform.rotation);
            //transform.SetPositionAndRotation(position: new Vector3(1f, 1f, 1f), rotation: new Quaternion(1f, 1f, 1f, 1f));
            // >> transform.gameObject.GetComponent() << Model?
        }
        
        protected bool Pressable {
            get => PressingAllowed() && _isPressable;
            set {
                _isPressable = value;
                if (!(Singleton<InteractionCursor>.Instance.CurrentInteractable == this))
                    return;
                if (value)
                    OnCursorEnter();
                else
                    OnCursorExit();
            }
        }
        private bool _isPressable;
        private static readonly int Active = Animator.StringToHash("active");

        public override void ManagedUpdate() {
            bool flag = Singleton<TurnManager>.Instance.PlayerCanInitiateCombat && !Singleton<PlayerHand>.Instance.ChoosingSlot;
            if (flag != Pressable)
            {
                Pressable = flag;
                OnPressableChanged();
            }
            if (!InputButtons.GetButtonDown(Button.EndTurn))
                return;
            OnCursorSelectStart();
        }

        protected bool PressingAllowed() { // Pressing condition
            if (Singleton<TextDisplayer>.Instance.PlayingEvent && Singleton<TextDisplayer>.Instance.CurrentAdvanceMode == TextDisplayer.MessageAdvanceMode.Input 
                || !Enabled
                || Singleton<GlobalTriggerHandler>.Instance.StackSize != 0)
                return false; // ^ When false
            return Singleton<ItemsManager>.Instance == null || !Singleton<ItemsManager>.Instance.ActivatingItem;
        }
        
        protected void OnPressableChanged() {
            anim.SetBool(Active, Pressable);
        }
        
        protected override void OnCursorSelectStart() { // Pressed
            
        }

        protected override void OnCursorEnter() { // Hover
            Plugin.Log.LogError("Cum!");
            ShowState(HighlightedInteractable.State.Highlighted);
        }

        protected override void OnCursorExit() { // Hover over
            ShowState(HighlightedInteractable.State.Interactable);
        }
    }
}