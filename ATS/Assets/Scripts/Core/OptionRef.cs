using System;

namespace LockerConfigurator.Core
{
    public enum OptionKind
    {
        Tier,
        Door,
        Lock,
        Addon
    }

    // "kind:id", e.g. "lock:elec" or "door:phen" - matches the token format used in the catalog rules
    public readonly struct OptionRef : IEquatable<OptionRef>
    {
        public OptionKind Kind { get; }
        public string Id { get; }

        public OptionRef(OptionKind kind, string id)
        {
            Kind = kind;
            Id = id ?? throw new ArgumentNullException(nameof(id));
        }

        public static OptionRef Parse(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new FormatException("Option reference cannot be empty.");

            var parts = token.Split(new[] { ':' }, 2);
            if (parts.Length != 2 || parts[1].Length == 0)
                throw new FormatException($"Invalid option reference '{token}'. Expected format 'kind:id'.");

            if (!Enum.TryParse(parts[0], true, out OptionKind kind))
                throw new FormatException($"Unknown option kind '{parts[0]}' in reference '{token}'.");

            return new OptionRef(kind, parts[1]);
        }

        public bool Equals(OptionRef other) => Kind == other.Kind && Id == other.Id;

        public override bool Equals(object obj) => obj is OptionRef other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + Kind.GetHashCode();
                hash = hash * 31 + (Id?.GetHashCode() ?? 0);
                return hash;
            }
        }

        public override string ToString() => $"{Kind.ToString().ToLowerInvariant()}:{Id}";
    }
}
