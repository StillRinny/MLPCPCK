using DiskCardGame;
using InscryptionAPI.Card;

namespace MLPCPCK_utils.Abilities {
    public class Forgettable : AbilityBehaviour {
        private static Ability ability;
        public override Ability Ability => ability;
        
        // Patch in Patches/CanAttackDirectlyPatch.cs
        
        public static Ability AddAbility() {
            AbilityInfo abil = AbilityManager.New(nameof(MLPAbility),
                "Forgettable",
                "[creature] cannot be attacked unless directly opposed.",
                typeof(Forgettable),
                "forgettablesigilmlp.png"
            );
            abil.powerLevel = 1;
            abil.canStack = false;
            abil.opponentUsable = false;
            ability = abil.ability;
            return ability = abil.ability;
        }
    }
}
