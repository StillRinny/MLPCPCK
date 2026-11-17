using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using InscryptionAPI.Sound;
using InscryptionAPI.TalkingCards;
using MLPCPCK.Abilities;
using MLPCPCK.Helpers;
using MLPCPCK.SpecialAbilities;
using PonyLib.GameHelpers;
using Sirenix.Utilities;
using UnityEngine;

namespace MLPCPCK {
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("cyantist.inscryption.api", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("PonyLib-S")]

    public class Plugin : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony(PluginInfo.PLUGIN_NAME);
        internal static ManualLogSource Log { get; private set; }

        public void Awake() {
            Log = Logger;
            harmony.PatchAll();
            AddDialogs();
            PreloadAudio();
            AssetBundleHelper.TryGet(ERTools.ExtractEmbeddedResource($"{nameof(MLPCPCK)}.{nameof(Resources)}.Models.HardReset_Button", Assembly.GetExecutingAssembly()), "HardResetButton", out HardReset.ResetItemPrefab);
            RegisterAbilities();
        }
        
        private static void AddDialogs() {
            Assembly assembly = Assembly.GetExecutingAssembly();
            var dialogueEvents = assembly.GetTypes().Where(t => t.Namespace == $"{nameof(MLPCPCK)}.{nameof(Resources)}").ToList().Find(t => t.Name == "DialogueSamples");
            var dialogues = dialogueEvents.GetFields();
            int count = 0;
            foreach (FieldInfo field in dialogues) {
                var dialogueData = (PonyDialogueData)field.GetValue(null);
                if (dialogueData == null) continue;
                ++count;
                dialogueData.CreateDialogueEvent(PluginInfo.PLUGIN_GUID);
            }
            Log.LogInfo($"{count} Event Dialogues registered");
        }
        
        private static void PreloadAudio() {
            AudioTools.StoreAudioClip(PluginInfo.PLUGIN_GUID, $"{nameof(HardReset)}.Clock", SoundManager.LoadAudioClip("HardReset_Clock.ogg"));
            AudioTools.StoreAudioClip(PluginInfo.PLUGIN_GUID,$"{nameof(HardReset)}.ClockLoop", SoundManager.LoadAudioClip("HardReset_ClockLoop.ogg"));
        }
        
        private static void RegisterAbilities() {
            Assembly assembly = Assembly.GetExecutingAssembly();
            var abilityTypes = assembly.GetTypes().Where(t => 
                t.Namespace == $"{nameof(MLPCPCK)}.{nameof(Abilities)}" 
                || t.Namespace == $"{nameof(MLPCPCK)}.{nameof(SpecialAbilities)}").ToList();
            
            int count = 0;
            foreach (Type type in abilityTypes) {
                try {
                    MethodInfo addAbilityMethod = type.GetMethod("AddAbility", BindingFlags.Public | BindingFlags.Static);
                    if (addAbilityMethod == null) continue;
                    ++count;
                    addAbilityMethod.Invoke(null, null);
                } catch (Exception e) {
                    Log.LogError($"[{nameof(RegisterAbilities)}] Failed to register \"{type.Name}\" ability! Exception message:\n{e}\n");
                }
            }
            Log.LogInfo($"{count} Abilities registered");
        }
    }

    public abstract class MLPAbility {
        public static Ability CandyCurse => Abilities.CandyCurse.ability; // JLDR reference: MLPAbility.Candy Curse
        public static Ability ChaosAgent => Abilities.ChaosAgent.ability; // JLDR reference: MLPAbility.Chaos Agent
        public static Ability Forgettable => Abilities.Forgettable.ability; // JLDR reference: MLPAbility.Forgettable
        public static Ability HardReset => Abilities.HardReset.ability; // JLDR reference: MLPAbility.Hard Reset
        public static Ability Swarm = Abilities.Swarm.ability; // JLDR reference: MLPAbility.Swarm
    }

    public abstract class MLPSpecial {
        public static SpecialTriggeredAbility BlackFlank => BlankSpecialBehaviour.ability; // JLDR reference: MLPSpecial_BlankFlank
        public static SpecialTriggeredAbility Infiltrator => NictisSpecialBehaviour.ability; // JLDR reference: MLPSpecial_Infiltrator
        public static SpecialTriggeredAbility Surprise => SurpriseSpecialBehaviour.ability; // JLDR reference: MLPSpecial_Surprise
    }

    public abstract class MLPTrait {
        public static readonly Trait Changeling = GuidManager.GetEnumValue<Trait>(nameof(MLPTrait), nameof(Changeling)); // JLDR reference: MLPTrait_Changeling
    }
    
    public static class PluginInfo {
        public const string PLUGIN_GUID = "MLPCPCK-U";

        public const string PLUGIN_NAME = "MLP:FIM CardPack";
        
        public const string PLUGIN_VERSION = "0.6.7";
    }
}