using System.Linq;
using BepInEx;
using BepInEx.Logging;
using DiskCardGame;
using HarmonyLib;
using MLPCPCK_utils.Abilities;

namespace MLPCPCK_utils {
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("cyantist.inscryption.api", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("MLP_Sigils")]
    
    public class Plugin : BaseUnityPlugin {
        private readonly Harmony harmony = new Harmony(PluginInfo.PLUGIN_NAME);
        public static ManualLogSource Log { get; private set; }

        public void Awake() {
            Log = Logger;
            harmony.PatchAll();
            Logger.LogInfo($"Patched methods: {harmony.GetPatchedMethods().Count()}");
            Log.LogInfo($"{PluginInfo.PLUGIN_NAME} loaded!");
            
             CandyCurse.AddAbility();
        }
        
    }

    public static class MLPAbilities { // shortcuts
        public static Ability CandyCurse;
    }

    public static class PluginInfo {
        public const string PLUGIN_GUID = "MLPCPCK-U";

        public const string PLUGIN_NAME = "MLP FIM Card Pack utils";

        public const string PLUGIN_VERSION = "0.0.1";
    }
}