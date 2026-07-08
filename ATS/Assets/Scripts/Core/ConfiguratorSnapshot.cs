using System.Collections.Generic;

namespace LockerConfigurator.Core
{
    public sealed class ConfiguratorSnapshot
    {
        public ConfiguratorState State { get; }
        public PricingResult Pricing { get; }
        public IReadOnlyList<RuleViolation> Violations { get; }
        public bool IsValid => Violations.Count == 0;

        public ConfiguratorSnapshot(ConfiguratorState state, PricingResult pricing, IReadOnlyList<RuleViolation> violations)
        {
            State = state;
            Pricing = pricing;
            Violations = violations;
        }
    }
}
