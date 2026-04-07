using System.Linq;
using BepInEx;
using BepInEx.Logging;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Guid;
using MLPCPCK_utils.Abilities;
using MLPCPCK_utils.SpecialAbilities;

namespace MLPCPCK_utils {
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("cyantist.inscryption.api", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("PonyLib")]

    public class Plugin : BaseUnityPlugin { 
        private readonly Harmony harmony = new Harmony(PluginInfo.PLUGIN_NAME);
        internal static ManualLogSource Log { get; set; }

        public void Awake() {
            Log = Logger;
            harmony.PatchAll();
            Logger.LogInfo($"Patched methods: {harmony.GetPatchedMethods().Count()}");
            Log.LogInfo($"{PluginInfo.PLUGIN_NAME} loaded!");
            CandyCurse.AddAbility();
            ChaosAgent.AddAbility();
            Forgettable.AddAbility();
            HardReset.AddAbility();
            Swarm.AddAbility();
            BlankSpecialBehaviour.AddAbility();
            NictisSpecialBehaviour.AddAbility();
            SurpriseSpecialBehaviour.AddAbility();
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

        public const string PLUGIN_NAME = "MLP:FIM CardPack utils";
        
        public const string PLUGIN_VERSION = "0.5.9";
    }
}