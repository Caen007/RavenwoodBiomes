namespace Ravenwood.Biomes
{
    public sealed class TreeDropEntry
    {
        public string ItemName;
        public int Min;
        public int Max;
        public float Chance;

        public TreeDropEntry(string itemName, int min, int max, float chance)
        {
            ItemName = itemName;
            Min = min;
            Max = max;
            Chance = chance;
        }
    }
}
