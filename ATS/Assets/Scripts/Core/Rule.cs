using System.Collections.Generic;
using System.Linq;

namespace LockerConfigurator.Core
{
    public enum RuleType
    {
        Incompatible,
        Requires
    }

    public sealed class Rule
    {
        public RuleType Type { get; }
        public OptionRef A { get; }
        public IReadOnlyList<OptionRef> B { get; } // OR-list, e.g. "lock:builtin|lock:elec" - any one satisfies it
        public string Message { get; }

        public Rule(RuleType type, string aToken, string bToken, string message)
        {
            Type = type;
            A = OptionRef.Parse(aToken);
            B = bToken.Split('|').Select(OptionRef.Parse).ToList();
            Message = message;
        }
    }
}
