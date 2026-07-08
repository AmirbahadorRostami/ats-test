using System.Collections.Generic;
using LockerConfigurator.Core;
using NUnit.Framework;

namespace LockerConfigurator.Core.Tests
{
    public class PricingCalculatorTests
    {
        private static Catalog BuildCatalog() => new Catalog(
            tiers: new List<CatalogOption> { new CatalogOption("t1", "Single", 120), new CatalogOption("t3", "Triple", 190) },
            doors: new List<CatalogOption> { new CatalogOption("metal", "Metal", 0), new CatalogOption("phen", "Phenolic", 70) },
            locks: new List<CatalogOption> { new CatalogOption("pad", "Padlock hasp", 0), new CatalogOption("elec", "Electronic", 80) },
            addons: new List<CatalogOption> { new CatalogOption("bench", "Bench", 90), new CatalogOption("numbers", "Number plates", 8) },
            rules: new List<Rule>());

        [Test]
        public void Total_MatchesSpecFormula()
        {
            var catalog = BuildCatalog();
            var state = new ConfiguratorState { TierId = "t3", DoorId = "phen", LockId = "elec", Quantity = 2 };
            state.AddonIds.Add("bench");
            state.AddonIds.Add("numbers");

            var result = PricingCalculator.Calculate(catalog, state);

            // (190 + 70 + 80 + (90 + 8)) * 2 = 438 * 2 = 876
            Assert.AreEqual(438, result.UnitPrice);
            Assert.AreEqual(876, result.Total);
            Assert.AreEqual(2, result.Addons.Count);
        }

        [Test]
        public void Total_WithNoAddons_OnlyBaseTierDoorLock()
        {
            var catalog = BuildCatalog();
            var state = new ConfiguratorState { TierId = "t1", DoorId = "metal", LockId = "pad", Quantity = 1 };

            var result = PricingCalculator.Calculate(catalog, state);

            Assert.AreEqual(120, result.UnitPrice);
            Assert.AreEqual(120, result.Total);
            Assert.IsEmpty(result.Addons);
        }

        [Test]
        public void Quantity_OutOfRange_IsClamped()
        {
            var catalog = BuildCatalog();
            var state = new ConfiguratorState { TierId = "t1", DoorId = "metal", LockId = "pad", Quantity = 999 };

            var result = PricingCalculator.Calculate(catalog, state);

            Assert.AreEqual(ConfiguratorState.MaxQuantity, result.Quantity);
        }

        [Test]
        public void AddonOrder_FollowsCatalogDeclarationOrder_NotSelectionOrder()
        {
            var catalog = BuildCatalog();
            var state = new ConfiguratorState { TierId = "t1", DoorId = "metal", LockId = "pad", Quantity = 1 };
            state.AddonIds.Add("numbers");
            state.AddonIds.Add("bench");

            var result = PricingCalculator.Calculate(catalog, state);

            Assert.AreEqual("bench", result.Addons[0].Id);
            Assert.AreEqual("numbers", result.Addons[1].Id);
        }
    }
}
