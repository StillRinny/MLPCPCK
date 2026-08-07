using System.Collections.Generic;
using InscryptionAPI.Dialogue;
using MLPCPCK.Abilities;
using PonyLib.GameHelpers;

namespace MLPCPCK.Resources
{
    internal static class DialogueSamples {
        private static string _eventPrefix = $"{PluginInfo.PLUGIN_GUID}.Twilight.{nameof(HardReset)}.";

        public static PonyDialogueData LethalDamageTaken = new PonyDialogueData {
            Name = _eventPrefix + nameof(LethalDamageTaken),
            MainLines = new List<CustomLine> {
                "[e:5][w:1][e:1][w:0.6]Shouldn't..[w:0.7]",
                "[e:2]I be dead?[w:1.3]",
                "This is really strange[w:0.5]",
                "Is this what the icon[0.3]",
                "under my portrait does?[w:1]",
                "What does that[w:0.3]",
                "new thing on the board do?"
            },
            RepeatLines = new List<List<CustomLine>> {
                new List<CustomLine> { "[e:1]Ugh. The loop doesn't work the same here" },
                new List<CustomLine> { "Being in a state of alive and dead?[w:0.3]", "[e:2]This feels horrible, please decide already" },
                new List<CustomLine> { "[e:3]More corpses" },
                new List<CustomLine> { "Oh my Celestia[e:1][w:1]", "[e:2] This is fucking awful" },
                new List<CustomLine> { "[e:3] Again.. and again.." },
                new List<CustomLine> { "Please don't" },
                new List<CustomLine> { "Can I finally rest?" },
                new List<CustomLine> { "I'm so tired" },
                new List<CustomLine> { "[e:1]I'm in your hooves.." },
                new List<CustomLine> { "[e:4] This really hurts you know" },
                new List<CustomLine> { "[e:3]I hope you get to feel this someday" },
                new List<CustomLine> { "I'm just a passenger of fate" },
                new List<CustomLine> { "Is this amusing to you?" },
                new List<CustomLine> { "Hurry up." },
                new List<CustomLine> { "I hate you." },
                new List<CustomLine> { "Don't mind me, just casually dying." },
                new List<CustomLine> { "Having no agency sucks." },
                new List<CustomLine> { "Leave me alone." },
                new List<CustomLine> { "[e:1]It never fucking ends." },
                new List<CustomLine> { "I don't even know how it feels[w:0.5]", "[e:1]To be mortal anymore." },
                new List<CustomLine> { "[e:1]Let me die." }
            }
        };
        
        public static PonyDialogueData OnAbilityActivated = new PonyDialogueData {
            Name = _eventPrefix + nameof(OnAbilityActivated),
            MainLines = new List<CustomLine> {
                "I see[w:0.5]",
                "So the board was reset[w:1]",
                "[e:3]So I really am just here as a pawn[w:0.3]",
                "[e:3]for someone else's amusement, huh?[w:0.8]",
                "[e:1][w:0.2][shake:1]How utterly sickening,[w:0.2] honestly"
            },
            RepeatLines = new List<List<CustomLine>> {
                new List<CustomLine> { "[e:1]Back again" },
                new List<CustomLine> { "[e:1]Lovely" },
                new List<CustomLine> { "[e:1]Why do you keep doing this to me?" },
                new List<CustomLine> { "Anything to win, right?" },
                new List<CustomLine> { "You think this is just a game don't you?" },
                new List<CustomLine> { "Do you think what I feel isn't real?" },
                new List<CustomLine> { "[e:1]Whatever" },
                new List<CustomLine> { "[e:3]I swear.." },
                new List<CustomLine> { "[e:1]Having fun?" },
                new List<CustomLine> { "[e:1]...." },
                new List<CustomLine> { "[e:1]Why not,[e:2] I guess?[e:3]" },
                new List<CustomLine> { "[e:3]Bye bye board." },
                new List<CustomLine> { "At least it doesn't hurt now." },
                new List<CustomLine> { "The pain is over at least." }
            }
        };

        public static PonyDialogueData OnAbilityNotUsed = new PonyDialogueData {
            Name = _eventPrefix + nameof(OnAbilityNotUsed),
            MainLines = new List<CustomLine> {
                "This is it for now huh?[w:0.7]",
                "[e:4]Thank [w:0.4]you[e:5]"
            },
            RepeatLines = new List<List<CustomLine>> {
                new List<CustomLine> { "[e:4]Hell yeah.." },
                new List<CustomLine> { "..[w:1][e:4][w:1][e:3]" },
                new List<CustomLine> { "[e:4]Into oblivion.." },
                new List<CustomLine> { "[e:3][shake:1].[shake:1][w:0.5].[shake:1][w:0.5]" },
                new List<CustomLine> { "[e:3]The end of me.. finally." },
                new List<CustomLine> { "[e:4]Seeya never." },
                new List<CustomLine> { "I better not wake up after this." }
            }
        };
    }
}