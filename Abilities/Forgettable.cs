using DiskCardGame;
using InscryptionAPI.Card;
using PonyLib.CardsHelpers;

namespace MLPCPCK.Abilities {
    public class Forgettable : AbilityBehaviour {
        internal static Ability ability;
        public override Ability Ability => ability;
        
        // Patch in Patches/CanAttackDirectlyPatch.cs
        
        public static void AddAbility() {
            AbilityInfo abil = AbilityManager.New(nameof(MLPAbility),
                "Forgettable",
                "[creature] cannot be attacked unless directly opposed.",
                typeof(Forgettable),
                "forgettablesigilmlp.png"
            );
            abil.AddMetaCategories(AbilityMetaCategory.Part1Rulebook);
            abil.SetNotMergeable();
            abil.powerLevel = -5; // Make zero priority for this Card  
            abil.canStack = false;
            abil.opponentUsable = false;
            ability = abil.ability;
        }
    }
}
