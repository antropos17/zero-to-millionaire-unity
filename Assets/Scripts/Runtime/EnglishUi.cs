namespace ZeroToMillionaire
{
    // Presentation text only. Stable quest IDs, prices and save data are unchanged.
    public static class EnglishUi
    {
        public const string RewardHint = "1 matching symbol = 1 part";
        public const string Help = "Eight upgrades, one small room. Spins give coins and parts for your current task. Extra symbols become coins.\n\nCollect BOTH the parts and coins shown, then press BUY to install the upgrade.\n\nA spin costs 5 coins and gives at least 12. Your first 3 spins are free. If you have fewer than 5 coins, FREE SPIN keeps you going.";
        static readonly string[] Titles = {
            "Patch floor", "Repair walls", "Wallpaper", "Repair door",
            "Repair window", "Add mattress", "Add lamp", "Add stool"
        };
        public static string QuestTitle(int index) => Titles[index];
        public static string Coins(int value) => value == 1 ? "coin" : "coins";
        public static string SaveNotice(string notice, bool failed) {
            if (notice == SaveStore.RecoveryNotice) return "Save unavailable. Original files are protected.";
            if (failed) return "Could not save. Please retry the action.";
            if (notice == "Save upgraded: all progress kept.") return "Save updated. All progress kept.";
            if (notice == "Recovered the previous local save.") return "Previous local save recovered.";
            if (notice == "Local save could not be read. Starting a new room.") return "Save unreadable. A new room was started.";
            if (notice == "Reset saved; backup update pending.") return "Reset saved. Backup update pending.";
            return "Saved on this computer · offline";
        }
    }
}
