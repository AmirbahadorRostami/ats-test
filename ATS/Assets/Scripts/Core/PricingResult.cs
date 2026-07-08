using System.Collections.Generic;

namespace LockerConfigurator.Core
{
    public sealed class PricingResult
    {
        public CatalogOption Tier { get; }
        public CatalogOption Door { get; }
        public CatalogOption Lock { get; }
        public IReadOnlyList<CatalogOption> Addons { get; }
        public int Quantity { get; }
        public int UnitPrice { get; } // before quantity is applied
        public int Total { get; }

        public PricingResult(CatalogOption tier, CatalogOption door, CatalogOption @lock,
            IReadOnlyList<CatalogOption> addons, int quantity, int unitPrice, int total)
        {
            Tier = tier;
            Door = door;
            Lock = @lock;
            Addons = addons;
            Quantity = quantity;
            UnitPrice = unitPrice;
            Total = total;
        }
    }
}
