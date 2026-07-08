using System.Linq;

namespace LockerConfigurator.Core
{
    public static class PricingCalculator
    {
        public static PricingResult Calculate(Catalog catalog, ConfiguratorState state)
        {
            var tier = catalog.FindTier(state.TierId);
            var door = catalog.FindDoor(state.DoorId);
            var lockOption = catalog.FindLock(state.LockId);

            // Preserve catalog declaration order rather than selection order.
            var addons = catalog.Addons.Where(a => state.AddonIds.Contains(a.Id)).ToList();

            int quantity = ConfiguratorState.ClampQuantity(state.Quantity);
            int unitPrice = (tier?.Amount ?? 0) + (door?.Amount ?? 0) + (lockOption?.Amount ?? 0)
                            + addons.Sum(a => a.Amount);
            int total = unitPrice * quantity;

            return new PricingResult(tier, door, lockOption, addons, quantity, unitPrice, total);
        }
    }
}
