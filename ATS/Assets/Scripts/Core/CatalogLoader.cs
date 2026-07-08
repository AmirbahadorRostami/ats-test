using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace LockerConfigurator.Core
{
    public static class CatalogLoader
    {
        public static Catalog Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("Catalog JSON cannot be empty.", nameof(json));

            var raw = JsonConvert.DeserializeObject<CatalogJson>(json)
                      ?? throw new FormatException("Catalog JSON could not be parsed.");

            var tiers = (raw.Tiers ?? new List<TierJson>())
                .Select(t => new CatalogOption(t.Id, t.Label, t.BasePrice)).ToList();
            var doors = ToOptions(raw.Doors);
            var locks = ToOptions(raw.Locks);
            var addons = ToOptions(raw.Addons);
            var rules = (raw.Rules ?? new List<RuleJson>())
                .Select(r => new Rule(ParseRuleType(r.Type), r.A, r.B, r.Msg)).ToList();

            return new Catalog(tiers, doors, locks, addons, rules);
        }

        private static List<CatalogOption> ToOptions(List<OptionJson> raw) =>
            (raw ?? new List<OptionJson>())
            .Select(o => new CatalogOption(o.Id, o.Label, o.Delta))
            .ToList();

        private static RuleType ParseRuleType(string type)
        {
            switch (type)
            {
                case "incompatible": return RuleType.Incompatible;
                case "requires": return RuleType.Requires;
                default:
                    throw new FormatException($"Unknown rule type '{type}'.");
            }
        }

        // mirrors the JSON shape exactly; everything else only sees the normalized model above
        private class CatalogJson
        {
            [JsonProperty("tiers")] public List<TierJson> Tiers;
            [JsonProperty("doors")] public List<OptionJson> Doors;
            [JsonProperty("locks")] public List<OptionJson> Locks;
            [JsonProperty("addons")] public List<OptionJson> Addons;
            [JsonProperty("rules")] public List<RuleJson> Rules;
        }

        private class OptionJson
        {
            [JsonProperty("id")] public string Id;
            [JsonProperty("label")] public string Label;
            [JsonProperty("delta")] public int Delta;
        }

        private class TierJson
        {
            [JsonProperty("id")] public string Id;
            [JsonProperty("label")] public string Label;
            [JsonProperty("basePrice")] public int BasePrice;
        }

        private class RuleJson
        {
            [JsonProperty("type")] public string Type;
            [JsonProperty("a")] public string A;
            [JsonProperty("b")] public string B;
            [JsonProperty("msg")] public string Msg;
        }
    }
}
