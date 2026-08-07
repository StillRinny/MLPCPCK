using System.Collections;
using UnityEngine;
using HarmonyLib;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Triggers;
using PonyLib.CardsHelpers;
using PonyLib.GameHelpers;
using MLPCPCK.Helpers;

namespace MLPCPCK.Abilities {
    public class HardReset : AbilityBehaviour, IModifyDamageTaken, IOnPreScalesChangedRef, IOnBellRung {
        internal static Ability ability;
        public override Ability Ability => ability;

        public static void AddAbility() {
            AbilityInfo abil = AbilityManager.New(nameof(MLPAbility),
                "Hard Reset",
                "The card bearing this sigil is cursed with undeath. Once Twilight takes lethal damage you will be able to choose whether to let her die, or to reset back to when you first placed her.",
                typeof(HardReset),
                "hardresetsigilmlp.png"
            );
            abil.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture("hardresetsigilmlpsmall.png"));
            abil.powerLevel = 4;
            abil.canStack = false;
            abil.opponentUsable = false;
            abil.AddMetaCategories(AbilityMetaCategory.Part1Rulebook);
            abil.SetNotMergeable();
            ability = abil.ability;
            
            _clock = AudioTools.GetAudioClip(PluginInfo.PLUGIN_GUID, $"{nameof(HardReset)}.Clock");
            _clockLooped = AudioTools.GetAudioClip(PluginInfo.PLUGIN_GUID, $"{nameof(HardReset)}.ClockLoop");
            
            ResetItemPrefab.transform.position = new Vector3(-1.429f, 4.071f, -4.716f); // Move the prefab to the right location
            ResetItemPrefab.transform.rotation = Quaternion.Euler(0f, 0f, 0f); // Set the prefab right rotation
            ResetItemPrefab.transform.Find("Anim/model/Collider").gameObject.AddComponent<HardResetButtonColliderBehaviour>(); // Attach components to prefab
        }

        /*/ Board state and flag /*/
        private BoardState _state;
        private bool _needResetFlag; // First Flag to initiate button spawn | Disabled before make Card to Die
        /*/ Reset item and flag /*/
        private static GameObject _resetItem;
        internal static GameObject ResetItemPrefab;
        private bool _resetReady; // Second Flag to check that reset is ready to be activated
        /*/ Return Blocked Damage and make able other abilities to use Killer info /*/
        private int _damageStored = 0;
        private PlayableCard _killer = null;
        /*/ Audio Variables /*/
        private static AudioClip _clock;
        private static AudioClip _clockLooped;
        private static AudioSource _soundLoop;
        /*/ Dialogue /*/
        private static bool _deferredDialogueFlag;
        
        /*/
         *  Preparations
        /*/
        private IEnumerator Start() { // If Ability was added after Card Resolve | Also dialogue events
            if (Card.OnBoard) {
                _state = BoardState.GenerateFromCurrentBoard();
                if (_deferredDialogueFlag) {
                    _deferredDialogueFlag = false;
                    Card.TryGetComponent(out TalkingCard talkingComponent); // Try to play dialogue
                    if (talkingComponent == null) yield break;
                    if (DialogueEventsData.EventIsPlayed($"{PluginInfo.PLUGIN_GUID}.Twilight.{nameof(HardReset)}.{nameof(Resources.DialogueSamples.OnAbilityActivated)}")) 
                        yield return talkingComponent.PlaySoloDialogueEvent($"{PluginInfo.PLUGIN_GUID}.Twilight.{nameof(HardReset)}.{nameof(Resources.DialogueSamples.OnAbilityActivated)}");
                    else {
                        View currentView = ViewManager.Instance.CurrentView;
                        ViewManager.Instance.Controller.LockState = ViewLockState.Locked;
                        ViewManager.Instance.SwitchToView(View.Board);
                            
                        yield return new WaitForSeconds(0.5f);
                        yield return talkingComponent.PlaySoloDialogueEvent($"{PluginInfo.PLUGIN_GUID}.Twilight.{nameof(HardReset)}.{nameof(Resources.DialogueSamples.OnAbilityActivated)}");
                        yield return new WaitForSeconds(0.5f);
                            
                        Singleton<ViewManager>.Instance.SwitchToView(currentView);
                        ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;
                    }
                }
            }
        }

        public override bool RespondsToResolveOnBoard() => true;
        public override IEnumerator OnResolveOnBoard() {
            _state = BoardState.GenerateFromCurrentBoard();
            yield break;
        }
        
        /*/
         *  CleanUp
        /*/
        private void OnDestroy() {
            if (_resetItem || _resetReady) DestroyResetItem();
            if (_soundLoop) UnityEngine.Object.Destroy(_soundLoop, _soundLoop.clip.length - _soundLoop.time);
        }

        /*/
         *  Change Card appearance after lethal damage
        /*/
        public bool RespondsToModifyDamageTaken(PlayableCard target, int damage, PlayableCard attacker, int originalDamage) => 
            target == Card && !Card.HasShield();
        public int OnModifyDamageTaken(PlayableCard target, int damage, PlayableCard attacker, int originalDamage) {
            if (target.Health > damage && (attacker == null || !attacker.HasAbility(Ability.Deathtouch))) return damage;
            if (_needResetFlag) return 0; // just return 0 if reset ready
            
             // visual part
            if (!_soundLoop) _soundLoop = AudioTools.PlayAudioLooped(_clockLooped, 0.05f);
            Card.RenderInfo.hiddenAttack = true;
            Card.RenderInfo.hiddenHealth = true;
            Card.AttackedThisTurn = true; // Prevent Attack
            Card.RenderCard();
            
            _killer = attacker;
            _needResetFlag = true;
            return 0;
        }
        public int TriggerPriority(PlayableCard target, int damage, PlayableCard attacker) => int.MaxValue;
        
        /*/
         *  Reset Button creation or Reset itself on Events | Not Used
        /*/
        // Revert Desk immediately if card die somehow
        /*public override bool RespondsToPreDeathAnimation(bool wasSacrifice) => Singleton<TurnManager>.Instance.PlayerPhase != TurnManager.PlayerTurnPhase.Main; // Emergency
        public override IEnumerator OnPreDeathAnimation(bool _) {
            _damageStored = -10; // to detect it in destroy | ye, i don't want to do another bool
            yield break;
        }*/
        
        // Spawn Item and Show Event Dialogue
        public override bool RespondsToTakeDamage(PlayableCard source) => _needResetFlag && !_resetReady;
        public override IEnumerator OnTakeDamage(PlayableCard source) {
            Card.TryGetComponent(out TalkingCard talkingComponent);
            if (talkingComponent != null) yield return talkingComponent.PlaySoloDialogueEvent($"{PluginInfo.PLUGIN_GUID}.Twilight.{nameof(HardReset)}.{nameof(Resources.DialogueSamples.LethalDamageTaken)}");
            yield return SpawnResetItem();
        }
        
        // Safe Player from lose
        public bool RespondsToPreScalesChangedRef(int damage, int numWeights, bool toPlayer) => toPlayer && _needResetFlag;
        public int CollectPreScalesChangedRef(int damage, ref int numWeights, ref bool toPlayer) {
            _damageStored = damage;
            numWeights = 0;
            return 0;
        }
        
        public bool RespondsToBellRung(bool playerCombatPhase) => playerCombatPhase && _resetReady;
        public IEnumerator OnBellRung(bool playerCombatPhase) => AbilityNotUsed(false);

        public override bool RespondsToOtherCardResolve(PlayableCard card) => card.IsPlayerCard() && _resetReady && Singleton<TurnManager>.Instance.PlayerPhase == TurnManager.PlayerTurnPhase.Main;
        public override IEnumerator OnOtherCardResolve(PlayableCard card) => AbilityNotUsed(true);

        private IEnumerator SpawnResetItem() {
            if (!_resetItem) _resetItem = Instantiate(ResetItemPrefab);
            _resetItem.transform.Find("Anim/model/Collider").GetComponent<HardResetButtonColliderBehaviour>().OnClick = () => StartCoroutine(RevertToSnapshot()); // Action changes even if button already exist
            _resetReady = true;
            yield break;
        }

        private void DestroyResetItem() {
            if (_resetItem) Destroy(_resetItem);
            _resetItem = null;
            _resetReady = false;
        }

        private IEnumerator AbilityNotUsed(bool needToStoreView) {
            View currentView = ViewManager.Instance.CurrentView;
            _needResetFlag = false; // To prevent activation of PreDeathAnimation Event
            
            ViewManager.Instance.Controller.LockState = ViewLockState.Locked;
            ViewManager.Instance.SwitchToView(View.Board);
            yield return new WaitForSeconds(0.5f);
            
            Card.TryGetComponent(out TalkingCard talkingComponent); // Try to play dialogue
            if (talkingComponent != null) yield return talkingComponent.PlaySoloDialogueEvent($"{PluginInfo.PLUGIN_GUID}.Twilight.{nameof(HardReset)}.{nameof(Resources.DialogueSamples.OnAbilityNotUsed)}");
            
            if (_soundLoop && _soundLoop.clip != _clock) {
                _soundLoop.loop = false;
                _soundLoop.clip = _clock;
                yield return new WaitUntil(() => !_soundLoop.isPlaying);
                _soundLoop.Play();
            }
            
            DestroyResetItem();
            yield return Card.Die(false, _killer);
            yield return new WaitForSeconds(0.5f);
            
            if (_damageStored != 0) { // Add damage on scales
                Singleton<ViewManager>.Instance.SwitchToView(View.Scales);
                yield return Singleton<LifeManager>.Instance.ShowDamageSequence(_damageStored, _damageStored, true, 0.25f, null, 0.5f);
            }
            if (needToStoreView) Singleton<ViewManager>.Instance.SwitchToView(currentView);
            ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;
        }
        
        private IEnumerator RevertToSnapshot() {
            _deferredDialogueFlag = true;
            if (_resetItem) DestroyResetItem();
            if (_soundLoop && _soundLoop.clip != _clock) {
                _soundLoop.loop = false;
                _soundLoop.clip = _clock;
                yield return new WaitUntil(() => !_soundLoop.isPlaying);
                _soundLoop.Play();
            }
            
            Singleton<LifeManager>.Instance.SetNumWeightsImmediate(_state.playerDamage, _state.opponentDamage);
            
            foreach (CardSlot allSlot in Singleton<BoardManager>.Instance.AllSlots) {
                if (!allSlot.Card) continue;
                PlayableCard card = allSlot.Card;
                allSlot.Card.UnassignFromSlot();
                UnityEngine.Object.Destroy(card.gameObject);
            }

            var applySlotState = AccessTools.Method(typeof(PhotographerSnapshotManager), "ApplySlotState");
            var obj = new PhotographerSnapshotManager(); // it's okay!

            var slots = _state.playerSlots;
            var actualSlots = Singleton<BoardManager>.Instance.PlayerSlotsCopy;
            for (int i = 0; i < slots.Count; ++i) {
                if (slots[i].card != null) 
                    StartCoroutine((IEnumerator)applySlotState.Invoke(obj, new [] { (object)slots[i], actualSlots[i] }));
            }
            
            slots = _state.opponentSlots;
            actualSlots = Singleton<BoardManager>.Instance.OpponentSlotsCopy;
            for (int i = 0; i < slots.Count; ++i) {
                if (slots[i].card != null) 
                    StartCoroutine((IEnumerator)applySlotState.Invoke(obj, new [] { (object)slots[i], actualSlots[i] }));
            }
            
            //applySlotStates.Invoke(obj, new [] {(object)state.playerSlots, Singleton<BoardManager>.Instance.PlayerSlotsCopy});
            //ApplySlotStates(state.playerSlots, Singleton<BoardManager>.Instance.PlayerSlotsCopy);
            
            //applySlotStates.Invoke(obj, new [] {(object)state.opponentSlots, Singleton<BoardManager>.Instance.OpponentSlotsCopy});
            //ApplySlotStates(state.opponentSlots, Singleton<BoardManager>.Instance.OpponentSlotsCopy);
            
            Singleton<UIManager>.Instance.Effects.GetEffect<ScreenGlitchEffect>().SetIntensity(1f, 0.4f);
            yield return new WaitForSeconds(1f);
        }
    }
}