using System;

namespace ZeroToMillionaire
{
    public enum Symbol { Patches, Surfaces, Wallpaper, Door, Window, Mattress, Lamp, Stool }
    public enum ReelSymbol { Cash, FloorPatch, WallRepair, Wallpaper, Door, Window, Mattress, Lamp, Stool }
    public enum QuestStatus { Locked, Collecting, ReadyToBuy, Completed }

    public sealed class UpgradeDefinition
    {
        public readonly Symbol Symbol;
        public readonly string Title;
        public readonly string Id, ActionTitle;
        public readonly int CashPrice;
        public readonly int FragmentGoal;
        public readonly string Description;
        public readonly ReelSymbol FragmentSymbol;
        public UpgradeDefinition(Symbol symbol, string id, string actionTitle, string title, int cash, int fragments, string description)
        {
            Symbol = symbol; Id=id;ActionTitle=actionTitle;Title = title; CashPrice = cash; FragmentGoal = fragments; Description = description;
            FragmentSymbol = (ReelSymbol)((int)symbol + 1);
        }
    }

    public static class Economy
    {
        public const int Columns = 5, Rows = 3, SymbolCount = 8;
        public const int ReelSymbolCount = 9;
        public const int SpinCost = 5, BasePayout = 12, TripleBonus = 10, StarterFreeSpins = 3;
        public const int SaveVersion = 4;
        public static readonly UpgradeDefinition[] Upgrades = {
            new UpgradeDefinition(Symbol.Patches, "floor-patches", "Patch the floor", "Floor patches", 18, 8, "Fill the worst holes in the floor."),
            new UpgradeDefinition(Symbol.Surfaces, "surface-repair", "Repair the walls", "Repair surfaces", 55, 12, "Restore the floor and damaged walls."),
            new UpgradeDefinition(Symbol.Wallpaper, "cheap-wallpaper", "Paper the walls", "Cheap wallpaper", 80, 14, "Add simple wallpaper to the same walls."),
            new UpgradeDefinition(Symbol.Door, "door-repair", "Repair the door", "Repair door", 65, 12, "Fix the existing door."),
            new UpgradeDefinition(Symbol.Window, "window-repair", "Repair the window", "Repair window", 60, 12, "Fix the existing window."),
            new UpgradeDefinition(Symbol.Mattress, "first-mattress", "Place a mattress", "Mattress", 110, 16, "A first place to sleep, on the floor."),
            new UpgradeDefinition(Symbol.Lamp, "small-lamp", "Install the lamp", "Small lamp", 55, 12, "Add a little warm light."),
            new UpgradeDefinition(Symbol.Stool, "wooden-stool", "Place the stool", "Wooden stool", 45, 12, "One simple seat. Level 1 is complete.")
        };

        public static int ActiveQuest(PlayerState s)
        {for(int i=0;i<SymbolCount;i++)if(!s.purchased[i])return i;return -1;}
        public static int NextQuest(PlayerState s)
        {int current=ActiveQuest(s);for(int i=current+1;current>=0&&i<SymbolCount;i++)if(!s.purchased[i])return i;return -1;}
        public static QuestRecord[] QuestRecords(PlayerState s)
        {
            var records=new QuestRecord[SymbolCount];int current=ActiveQuest(s);
            for(int i=0;i<SymbolCount;i++)records[i]=new QuestRecord{ id=Upgrades[i].Id,
                status=s.purchased[i]?QuestStatus.Completed:i!=current?QuestStatus.Locked:
                    s.fragments[i]>=Upgrades[i].FragmentGoal&&s.cash>=Upgrades[i].CashPrice?QuestStatus.ReadyToBuy:QuestStatus.Collecting};
            return records;
        }

        public static int[] ActivePool(PlayerState s)
        {
            var pool = new System.Collections.Generic.List<int> { (int)ReelSymbol.Cash };
            int current=ActiveQuest(s);
            if(current>=0&&s.fragments[current]<Upgrades[current].FragmentGoal)pool.Add((int)Upgrades[current].FragmentSymbol);
            return pool.ToArray();
        }
    }

    public interface IRandomSource { int Next(int exclusiveMax); }
    public sealed class SeededRandom : IRandomSource
    {
        private readonly Random random;
        public SeededRandom(int seed) { random = new Random(seed); }
        public int Next(int exclusiveMax) { return random.Next(exclusiveMax); }
    }
}
