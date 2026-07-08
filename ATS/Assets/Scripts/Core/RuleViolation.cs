namespace LockerConfigurator.Core
{
    public sealed class RuleViolation
    {
        public Rule Rule { get; }
        public string Message => Rule.Message;

        public RuleViolation(Rule rule)
        {
            Rule = rule;
        }
    }
}
