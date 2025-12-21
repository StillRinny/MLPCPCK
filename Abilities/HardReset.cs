using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;

namespace MLPCPCK_utils.Abilities
{
    public class HardReset : AbilityBehaviour {
        private static Ability ability;
        public override Ability Ability => ability;

        public static Ability AddAbility() {
            AbilityInfo abil = AbilityManager.New(nameof(MLPAbility),
                "Hard Reset",
                "",
                typeof(HardReset),
                "hardresetsigilmlp.png"
            );
            abil.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture("hardresetsigilmlpsmall.png"));
            abil.powerLevel = 4;
            abil.canStack = false;
            abil.opponentUsable = false;
            abil.AddMetaCategories(AbilityMetaCategory.Part1Rulebook);
            return ability = abil.ability;
        }

        public override bool RespondsToResolveOnBoard() => true;
        public override IEnumerator OnResolveOnBoard() {
            var a = Singleton<BoardManager>.Instance.AllSlots;
            var b = Singleton<Scales3D>.Instance.;
            yield break ;
        }
    }
}