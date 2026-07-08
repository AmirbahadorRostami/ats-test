using System;
using System.Collections.Generic;
using System.Linq;

namespace LockerConfigurator.Core
{
    // ties catalog + state + rule engine + pricing together - the only thing the UI talks to
    public sealed class ConfiguratorEngine
    {
        public Catalog Catalog { get; }
        public ConfiguratorState State { get; }

        public event Action<ConfiguratorSnapshot> Changed;

        public ConfiguratorEngine(Catalog catalog, ConfiguratorState initialState = null)
        {
            Catalog = catalog;
            State = initialState ?? CreateDefaultState(catalog);
        }

        public static ConfiguratorState CreateDefaultState(Catalog catalog) => new ConfiguratorState
        {
            TierId = catalog.Tiers.FirstOrDefault()?.Id,
            DoorId = catalog.Doors.FirstOrDefault()?.Id,
            LockId = catalog.Locks.FirstOrDefault()?.Id,
            Quantity = ConfiguratorState.MinQuantity
        };

        public ConfiguratorSnapshot Snapshot => BuildSnapshot();

        public void SetTier(string tierId)
        {
            State.TierId = tierId;
            NotifyChanged();
        }

        public void SetDoor(string doorId)
        {
            State.DoorId = doorId;
            NotifyChanged();
        }

        public void SetLock(string lockId)
        {
            State.LockId = lockId;
            NotifyChanged();
        }

        public void SetQuantity(int quantity)
        {
            State.Quantity = ConfiguratorState.ClampQuantity(quantity);
            NotifyChanged();
        }

        public void SetAddonSelected(string addonId, bool selected)
        {
            if (selected) State.AddonIds.Add(addonId);
            else State.AddonIds.Remove(addonId);
            NotifyChanged();
        }

        private void NotifyChanged() => Changed?.Invoke(BuildSnapshot());

        private ConfiguratorSnapshot BuildSnapshot()
        {
            var selected = BuildSelectedRefs();
            var violations = RuleEngine.Evaluate(selected, Catalog.Rules);
            var pricing = PricingCalculator.Calculate(Catalog, State);
            return new ConfiguratorSnapshot(State, pricing, violations);
        }

        private HashSet<OptionRef> BuildSelectedRefs()
        {
            var refs = new HashSet<OptionRef>();
            if (State.TierId != null) refs.Add(new OptionRef(OptionKind.Tier, State.TierId));
            if (State.DoorId != null) refs.Add(new OptionRef(OptionKind.Door, State.DoorId));
            if (State.LockId != null) refs.Add(new OptionRef(OptionKind.Lock, State.LockId));
            foreach (var addonId in State.AddonIds) refs.Add(new OptionRef(OptionKind.Addon, addonId));
            return refs;
        }
    }
}
