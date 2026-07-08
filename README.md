# Locker Bank Configurator

Take-home solution: a locker-bank configurator built with Unity 2022.3.25f1 and UI Toolkit.
The project is under `ATS/`.

## How to run it

1. Open `ATS/` as a project in Unity Hub (2022.3.25f1, any 2022.3 LTS should be fine).
2. Open `Assets/Scenes/SampleScene.unity`.
3. Press Play.

The `Configurator` GameObject in that scene has a `UIDocument` and a `ConfiguratorController`
on it. On enable it loads `Assets/Resources/LockerCatalog.json` (a copy of the catalog file
from the brief) and builds the whole screen from it.

If you want to point it at a different catalog, drop a `TextAsset` into the `Catalog Json`
slot on the `ConfiguratorController` component instead of using the Resources copy.

### Tests

15 EditMode tests live under `Assets/Scripts/Core/Tests/EditMode`, covering the rule engine,
the pricing calculator, and the engine facade. Open Window > General > Test Runner, switch to
the EditMode tab, run all. Headless:

```
Unity -batchmode -runTests -projectPath ATS -testPlatform EditMode -testResults results.xml
```

## How it's structured

Three assemblies:

- `LockerConfigurator.Core`: the actual configurator logic. Catalog model, JSON parsing,
  rule engine, pricing, selection state.
- `LockerConfigurator.Core.Tests`: EditMode tests against the above.
- `LockerConfigurator.UI`: a MonoBehaviour that builds the UI Toolkit screen and talks to
  Core.

Inside Core:

- `CatalogOption` / `Catalog` / `Rule`: the normalized model. A tier's `basePrice` and every
  other group's `delta` both end up as `CatalogOption.Amount`, since the pricing formula
  treats them the same way. Didn't see a reason to special-case tiers just because the JSON
  key happens to be named differently.
- `CatalogLoader`: parses the JSON into the model above, using Newtonsoft.Json (added via
  the `com.unity.nuget.newtonsoft-json` package). Went with Newtonsoft over `JsonUtility`
  mainly so the raw JSON shape (id/label/delta/basePrice etc.) can live in private DTOs
  separate from the domain model, rather than annotating the domain classes directly.
- `OptionRef` / `RuleEngine`: covered below.
- `PricingCalculator`: the formula from the brief, nothing more to it.
- `ConfiguratorState`: current selection only, tier/door/lock ids, quantity, a set of addon
  ids. Plain mutable data, no logic.
- `ConfiguratorEngine`: what the UI actually calls into. Owns the catalog and state, exposes
  `SetTier` / `SetDoor` / `SetLock` / `SetQuantity` / `SetAddonSelected`, and after each of
  those recomputes pricing and rule violations in one pass and fires a `Changed` event with
  both. Kept it as one combined step on purpose, so price and validation are always computed
  from the same state at the same time and can't end up out of sync with each other.

On the UI side, `ConfiguratorController` reads the catalog and builds a `RadioButtonGroup`
per single-choice group (Tier/Door/Lock), a `Toggle` per add-on, and a `SliderInt` for
quantity, all generated from whatever's in the JSON. It subscribes to
`ConfiguratorEngine.Changed` and re-renders the summary, total, and validation panel from
whatever snapshot comes through.

## The rule engine

Rules don't reference tiers, doors, locks, or add-ons as concepts. They reference
`OptionRef` values, a `(kind, id)` pair like `lock:elec`, `door:phen`, `addon:bench`, parsed
straight from the same `kind:id` token format already used in the catalog JSON. A `Rule` has
a type (`incompatible` or `requires`), one `OptionRef` on the left side (`A`), and a list of
`OptionRef` on the right side (`B`). It's a list because `requires`'s `b` can be an OR-list
(`lock:builtin|lock:elec`), split on `|` when the rule is built.

To check a configuration, `RuleEngine.Evaluate` takes the full set of what's currently
selected as `OptionRef`s (tier, door, lock, one per selected add-on) and runs each rule
against that set:

- `incompatible` fails if the set contains `A` and also contains any entry from `B`.
- `requires` fails if the set contains `A` but contains none of `B`.

That's the entire engine. It has no idea what a "lock" or a "tier" actually is, it just sees
refs and set membership, which is what makes new rules a data-only change. To add one, just
append to the `rules` array in `LockerCatalog.json`:

```json
{ "type": "incompatible", "a": "addon:bench", "b": "tier:t1",
  "msg": "Bench doesn't fit on a single-tier locker." }
```

No new code path, no switch statement to extend. `ConfiguratorEngine` calls
`RuleEngine.Evaluate` against whatever rules the catalog happens to contain, and whichever
`msg` fires gets shown in the UI as-is. The only thing that would need actual code is a
third rule type beyond `incompatible`/`requires`.



# NOTE:
AI was used in developing this project, mostly used for UI code generation and some minor optomizations.

