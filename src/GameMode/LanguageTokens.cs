using RoR2;

namespace RoR2Maze.GameMode
{
    /// <summary>
    /// Registers English display strings for all mod-defined language tokens.
    /// Hooks Language.GetLocalizedStringByToken so no separate .language file is needed.
    /// Add more languages by checking <c>self.name</c> (e.g. "fr", "de", "pt-BR").
    /// </summary>
    internal static class LanguageTokens
    {
        internal static void Init()
        {
            On.RoR2.Language.GetLocalizedStringByToken += Language_GetLocalizedStringByToken;
        }

        private static string Language_GetLocalizedStringByToken(
            On.RoR2.Language.orig_GetLocalizedStringByToken orig,
            Language self,
            string token)
        {
            return token switch
            {
                "MAZE_RUN_NAME"        => "Maze Mode",
                "MAZE_RUN_DESCRIPTION" => "Survive the dark. Find the exit.",
                _                      => orig(self, token),
            };
        }
    }
}
