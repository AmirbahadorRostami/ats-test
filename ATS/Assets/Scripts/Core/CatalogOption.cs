namespace LockerConfigurator.Core
{
    // Amount covers both a tier's basePrice and every other group's delta - same slot in the pricing formula
    public sealed class CatalogOption
    {
        public string Id { get; }
        public string Label { get; }
        public int Amount { get; }

        public CatalogOption(string id, string label, int amount)
        {
            Id = id;
            Label = label;
            Amount = amount;
        }
    }
}
