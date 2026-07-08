using System.Collections.Generic;

namespace LockerConfigurator.Core
{
    public sealed class ConfiguratorState
    {
        public const int MinQuantity = 1;
        public const int MaxQuantity = 20;

        public string TierId { get; set; }
        public string DoorId { get; set; }
        public string LockId { get; set; }
        public int Quantity { get; set; } = MinQuantity;
        public HashSet<string> AddonIds { get; } = new HashSet<string>();

        public static int ClampQuantity(int quantity)
        {
            if (quantity < MinQuantity) return MinQuantity;
            if (quantity > MaxQuantity) return MaxQuantity;
            return quantity;
        }

        public ConfiguratorState Clone()
        {
            var clone = new ConfiguratorState
            {
                TierId = TierId,
                DoorId = DoorId,
                LockId = LockId,
                Quantity = Quantity
            };
            foreach (var id in AddonIds) clone.AddonIds.Add(id);
            return clone;
        }
    }
}
