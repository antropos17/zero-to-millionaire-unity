namespace ZeroToMillionaire
{
    // Presentation text only. Stable quest IDs, prices and save data are unchanged.
    public static class EnglishUi
    {
        public const string RewardHint = "1 matching symbol = 1 part";
        public const string StakeRules = "Paid stakes cost 5 / 10 / 15 coins. All coin rewards multiply by 1x / 2x / 3x. Parts stay one per matching icon.\n\nFree starter and recovery spins always cost 0 and give 1x rewards. Choose a lower stake if coins are short.";
        public const string Help = "Eight upgrades, one small room. Collect BOTH coins and matching parts, then BUY to install. SHOP previews every room improvement and its unlock.\n\nTap STAKE for 1x / 2x / 3x: costs 5 / 10 / 15 coins, coin rewards scale, parts stay 1 per matching icon. Free spins always give 1x rewards. Below 5 coins, FREE SPIN keeps you going.";
        static readonly string[] Titles = {
            "Patch floor", "Repair walls", "Wallpaper", "Repair door",
            "Repair window", "Add mattress", "Add lamp", "Add stool"
        };
        public static string QuestTitle(int index) => Titles[index];
        public static string Coins(int value) => value == 1 ? "coin" : "coins";
        public static string SaveNotice(string notice, bool failed) {
            if (notice == SaveStore.RecoveryNotice) return "Save unavailable. Original files are protected.";
            if (notice == SaveStore.MigrationSnapshotError) return "Could not protect the old save. No changes saved.";
            if (failed) return "Could not save. Please retry the action.";
            if (notice == "Save upgraded: all progress kept.") return "Save updated. All progress kept.";
            if (notice == "Recovered the previous local save.") return "Previous local save recovered.";
            if (notice == "Local save could not be read. Starting a new room.") return "Save unreadable. A new room was started.";
            if (notice == "Reset saved; backup update pending.") return "Reset saved. Backup update pending.";
            return "Saved on this computer · offline";
        }
    }
}
