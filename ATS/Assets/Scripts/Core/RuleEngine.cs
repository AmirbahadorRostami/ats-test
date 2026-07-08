using System.Collections.Generic;
using System.Linq;

namespace LockerConfigurator.Core
{
    public static class RuleEngine
    {
        public static IReadOnlyList<RuleViolation> Evaluate(IReadOnlyCollection<OptionRef> selected, IEnumerable<Rule> rules)
        {
            var selectedSet = selected as HashSet<OptionRef> ?? new HashSet<OptionRef>(selected);
            var violations = new List<RuleViolation>();

            foreach (var rule in rules)
            {
                bool hasA = selectedSet.Contains(rule.A);
                bool hasAnyB = rule.B.Any(selectedSet.Contains);

                bool violated = rule.Type == RuleType.Incompatible
                    ? hasA && hasAnyB
                    : hasA && !hasAnyB; // Requires

                if (violated)
                    violations.Add(new RuleViolation(rule));
            }

            return violations;
        }
    }
}
