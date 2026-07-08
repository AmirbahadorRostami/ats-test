using System.Collections.Generic;
using System.Linq;

namespace LockerConfigurator.Core
{
    public sealed class Catalog
    {
        public IReadOnlyList<CatalogOption> Tiers { get; }
        public IReadOnlyList<CatalogOption> Doors { get; }
        public IReadOnlyList<CatalogOption> Locks { get; }
        public IReadOnlyList<CatalogOption> Addons { get; }
        public IReadOnlyList<Rule> Rules { get; }

        public Catalog(
            IReadOnlyList<CatalogOption> tiers,
            IReadOnlyList<CatalogOption> doors,
            IReadOnlyList<CatalogOption> locks,
            IReadOnlyList<CatalogOption> addons,
            IReadOnlyList<Rule> rules)
        {
            Tiers = tiers;
            Doors = doors;
            Locks = locks;
            Addons = addons;
            Rules = rules;
        }

        public IReadOnlyList<CatalogOption> GetGroup(OptionKind kind)
        {
            switch (kind)
            {
                case OptionKind.Tier: return Tiers;
                case OptionKind.Door: return Doors;
                case OptionKind.Lock: return Locks;
                case OptionKind.Addon: return Addons;
                default: return null;
            }
        }

        public CatalogOption Find(OptionKind kind, string id) =>
            id == null ? null : GetGroup(kind)?.FirstOrDefault(o => o.Id == id);

        public CatalogOption FindTier(string id) => Find(OptionKind.Tier, id);
        public CatalogOption FindDoor(string id) => Find(OptionKind.Door, id);
        public CatalogOption FindLock(string id) => Find(OptionKind.Lock, id);
        public CatalogOption FindAddon(string id) => Find(OptionKind.Addon, id);
    }
}
