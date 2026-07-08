using System.Collections.Generic;
using LockerConfigurator.Core;
using NUnit.Framework;

namespace LockerConfigurator.Core.Tests
{
    public class ConfiguratorEngineTests
    {
        private static Catalog BuildCatalog() => new Catalog(
            tiers: new List<CatalogOption> { new CatalogOption("t1", "Single", 120), new CatalogOption("t3", "Triple", 190) },
            doors: new List<CatalogOption> { new CatalogOption("metal", "Metal", 0), new CatalogOption("phen", "Phenolic", 70) },
            locks: new List<CatalogOption> { new CatalogOption("pad", "Padlock hasp", 0), new CatalogOption("elec", "Electronic", 80) },
            addons: new List<CatalogOption> { new CatalogOption("bench", "Bench", 90) },
            rules: new List<Rule>
            {
                new Rule(RuleType.Incompatible, "lock:elec", "tier:t3", "Electronic locks aren't available on triple-tier."),
                new Rule(RuleType.Requires, "door:phen", "lock:elec", "Phenolic doors require an electronic lock."),
            });

        [Test]
        public void DefaultState_SelectsFirstOptionInEachGroup()
        {
            var engine = new ConfiguratorEngine(BuildCatalog());

            Assert.AreEqual("t1", engine.State.TierId);
            Assert.AreEqual("metal", engine.State.DoorId);
            Assert.AreEqual("pad", engine.State.LockId);
            Assert.AreEqual(1, engine.State.Quantity);
        }

        [Test]
        public void Changed_FiresWithUpdatedSnapshot_OnEverySetter()
        {
            var engine = new ConfiguratorEngine(BuildCatalog());
            var received = new List<ConfiguratorSnapshot>();
            engine.Changed += s => received.Add(s);

            engine.SetTier("t3");
            engine.SetDoor("phen");
            engine.SetLock("elec");
            engine.SetQuantity(4);
            engine.SetAddonSelected("bench", true);

            Assert.AreEqual(5, received.Count);
            var last = received[received.Count - 1];
            Assert.AreEqual(4, last.Pricing.Quantity);
            Assert.IsTrue(last.State.AddonIds.Contains("bench"));
        }

        [Test]
        public void Snapshot_BecomesInvalid_WhenSelectionViolatesRule()
        {
            var engine = new ConfiguratorEngine(BuildCatalog());

            engine.SetTier("t3");
            engine.SetLock("elec");

            Assert.IsFalse(engine.Snapshot.IsValid);
            Assert.AreEqual(1, engine.Snapshot.Violations.Count);
        }

        [Test]
        public void Snapshot_BecomesValidAgain_AfterResolvingViolation()
        {
            var engine = new ConfiguratorEngine(BuildCatalog());
            engine.SetTier("t3");
            engine.SetLock("elec");
            Assert.IsFalse(engine.Snapshot.IsValid);

            engine.SetLock("pad");

            Assert.IsTrue(engine.Snapshot.IsValid);
        }

        [Test]
        public void ToggleAddonOff_RemovesItFromState()
        {
            var engine = new ConfiguratorEngine(BuildCatalog());

            engine.SetAddonSelected("bench", true);
            Assert.IsTrue(engine.State.AddonIds.Contains("bench"));

            engine.SetAddonSelected("bench", false);
            Assert.IsFalse(engine.State.AddonIds.Contains("bench"));
        }
    }
}
