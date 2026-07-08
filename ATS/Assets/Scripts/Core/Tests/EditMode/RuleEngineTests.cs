using System.Collections.Generic;
using LockerConfigurator.Core;
using NUnit.Framework;

namespace LockerConfigurator.Core.Tests
{
    public class RuleEngineTests
    {
        [Test]
        public void Incompatible_BothSelected_ProducesViolation()
        {
            var rule = new Rule(RuleType.Incompatible, "lock:elec", "tier:t3", "Electronic locks aren't available on triple-tier.");
            var selected = new HashSet<OptionRef>
            {
                new OptionRef(OptionKind.Lock, "elec"),
                new OptionRef(OptionKind.Tier, "t3"),
            };

            var violations = RuleEngine.Evaluate(selected, new[] { rule });

            Assert.AreEqual(1, violations.Count);
            Assert.AreEqual(rule.Message, violations[0].Message);
        }

        [Test]
        public void Incompatible_OnlyOneSideSelected_NoViolation()
        {
            var rule = new Rule(RuleType.Incompatible, "lock:elec", "tier:t3", "msg");
            var selected = new HashSet<OptionRef> { new OptionRef(OptionKind.Lock, "elec"), new OptionRef(OptionKind.Tier, "t1") };

            var violations = RuleEngine.Evaluate(selected, new[] { rule });

            Assert.IsEmpty(violations);
        }

        [Test]
        public void Requires_OrListSatisfiedByEitherAlternative_NoViolation()
        {
            var rule = new Rule(RuleType.Requires, "door:phen", "lock:builtin|lock:elec", "Phenolic doors require a built-in or electronic lock.");

            var withBuiltin = new HashSet<OptionRef> { new OptionRef(OptionKind.Door, "phen"), new OptionRef(OptionKind.Lock, "builtin") };
            var withElectronic = new HashSet<OptionRef> { new OptionRef(OptionKind.Door, "phen"), new OptionRef(OptionKind.Lock, "elec") };

            Assert.IsEmpty(RuleEngine.Evaluate(withBuiltin, new[] { rule }));
            Assert.IsEmpty(RuleEngine.Evaluate(withElectronic, new[] { rule }));
        }

        [Test]
        public void Requires_NeitherAlternativePresent_ProducesViolation()
        {
            var rule = new Rule(RuleType.Requires, "door:phen", "lock:builtin|lock:elec", "Phenolic doors require a built-in or electronic lock.");
            var selected = new HashSet<OptionRef> { new OptionRef(OptionKind.Door, "phen"), new OptionRef(OptionKind.Lock, "pad") };

            var violations = RuleEngine.Evaluate(selected, new[] { rule });

            Assert.AreEqual(1, violations.Count);
        }

        [Test]
        public void Requires_ConditionNotSelected_NoViolation()
        {
            var rule = new Rule(RuleType.Requires, "door:phen", "lock:builtin|lock:elec", "msg");
            var selected = new HashSet<OptionRef> { new OptionRef(OptionKind.Door, "metal"), new OptionRef(OptionKind.Lock, "pad") };

            Assert.IsEmpty(RuleEngine.Evaluate(selected, new[] { rule }));
        }

        [Test]
        public void MultipleRules_AllViolationsAreReported()
        {
            var rules = new[]
            {
                new Rule(RuleType.Incompatible, "addon:slopetop", "addon:bench", "Sloped top and bench can't be combined."),
                new Rule(RuleType.Incompatible, "lock:elec", "tier:t3", "Electronic locks aren't available on triple-tier."),
            };
            var selected = new HashSet<OptionRef>
            {
                new OptionRef(OptionKind.Addon, "slopetop"),
                new OptionRef(OptionKind.Addon, "bench"),
                new OptionRef(OptionKind.Lock, "elec"),
                new OptionRef(OptionKind.Tier, "t3"),
            };

            var violations = RuleEngine.Evaluate(selected, rules);

            Assert.AreEqual(2, violations.Count);
        }
    }
}
