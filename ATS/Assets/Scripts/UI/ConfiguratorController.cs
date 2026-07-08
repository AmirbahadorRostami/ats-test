using System;
using System.Collections.Generic;
using System.Linq;
using LockerConfigurator.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace LockerConfigurator.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class ConfiguratorController : MonoBehaviour
    {
        [SerializeField] private TextAsset catalogJson;
        [SerializeField] private string resourcesCatalogName = "LockerCatalog";

        private ConfiguratorEngine _engine;
        private VisualElement _optionsScroll;
        private VisualElement _lineItems;
        private VisualElement _validationPanel;
        private Label _totalLabel;

        private readonly Dictionary<OptionKind, RadioButtonGroup> _singleChoiceGroups = new Dictionary<OptionKind, RadioButtonGroup>();
        private readonly Dictionary<OptionKind, List<string>> _singleChoiceOptionIds = new Dictionary<OptionKind, List<string>>();

        private void OnEnable()
        {
            var document = GetComponent<UIDocument>();
            var root = document.rootVisualElement;

            var json = catalogJson != null ? catalogJson.text : Resources.Load<TextAsset>(resourcesCatalogName).text;
            var catalog = CatalogLoader.Parse(json);
            _engine = new ConfiguratorEngine(catalog);
            _engine.Changed += OnEngineChanged;

            BuildUI(root, catalog);
            Refresh(_engine.Snapshot);
        }

        private void OnDisable()
        {
            if (_engine != null)
                _engine.Changed -= OnEngineChanged;
        }

        private void BuildUI(VisualElement root, Catalog catalog)
        {
            _optionsScroll = root.Q<VisualElement>("options-scroll");
            _lineItems = root.Q<VisualElement>("line-items");
            _validationPanel = root.Q<VisualElement>("validation-panel");
            _totalLabel = root.Q<Label>("total-label");

            _optionsScroll.Add(BuildSingleChoiceGroup("Tier", OptionKind.Tier, catalog.Tiers, isBase: true, _engine.SetTier));
            _optionsScroll.Add(BuildSingleChoiceGroup("Door", OptionKind.Door, catalog.Doors, isBase: false, _engine.SetDoor));
            _optionsScroll.Add(BuildSingleChoiceGroup("Lock", OptionKind.Lock, catalog.Locks, isBase: false, _engine.SetLock));
            _optionsScroll.Add(BuildQuantityGroup());
            _optionsScroll.Add(BuildAddonGroup(catalog.Addons));
        }

        private VisualElement BuildSingleChoiceGroup(string title, OptionKind kind, IReadOnlyList<CatalogOption> options,
            bool isBase, Action<string> onSelect)
        {
            var container = new VisualElement();
            container.AddToClassList("option-group");

            var header = new Label(title);
            header.AddToClassList("option-group__header");
            container.Add(header);

            var ids = options.Select(o => o.Id).ToList();
            var choices = options.Select(o => FormatOptionLabel(o, isBase)).ToList();

            var group = new RadioButtonGroup(string.Empty, choices);
            group.SetValueWithoutNotify(ResolveInitialIndex(kind, ids));
            group.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue < 0 || evt.newValue >= ids.Count) return;
                onSelect(ids[evt.newValue]);
            });

            container.Add(group);
            _singleChoiceGroups[kind] = group;
            _singleChoiceOptionIds[kind] = ids;
            return container;
        }

        private int ResolveInitialIndex(OptionKind kind, List<string> ids)
        {
            string currentId = kind switch
            {
                OptionKind.Tier => _engine.State.TierId,
                OptionKind.Door => _engine.State.DoorId,
                OptionKind.Lock => _engine.State.LockId,
                _ => null
            };
            int idx = ids.IndexOf(currentId);
            return idx >= 0 ? idx : 0;
        }

        private VisualElement BuildQuantityGroup()
        {
            var container = new VisualElement();
            container.AddToClassList("option-group");

            var header = new Label("Quantity");
            header.AddToClassList("option-group__header");
            container.Add(header);

            var row = new VisualElement();
            row.AddToClassList("quantity-row");

            var slider = new SliderInt(ConfiguratorState.MinQuantity, ConfiguratorState.MaxQuantity)
            {
                value = _engine.State.Quantity,
                showInputField = true
            };
            slider.style.flexGrow = 1;
            slider.RegisterValueChangedCallback(evt => _engine.SetQuantity(evt.newValue));

            row.Add(slider);
            container.Add(row);
            return container;
        }

        private VisualElement BuildAddonGroup(IReadOnlyList<CatalogOption> addons)
        {
            var container = new VisualElement();
            container.AddToClassList("option-group");

            var header = new Label("Add-ons");
            header.AddToClassList("option-group__header");
            container.Add(header);

            foreach (var addon in addons)
            {
                var id = addon.Id;
                var toggle = new Toggle(FormatOptionLabel(addon, isBase: false))
                {
                    value = _engine.State.AddonIds.Contains(id)
                };
                toggle.AddToClassList("addon-toggle");
                toggle.RegisterValueChangedCallback(evt => _engine.SetAddonSelected(id, evt.newValue));
                container.Add(toggle);
            }

            if (addons.Count == 0)
            {
                var hint = new Label("No add-ons available.");
                hint.AddToClassList("option-group__hint");
                container.Add(hint);
            }

            return container;
        }

        private static string FormatOptionLabel(CatalogOption option, bool isBase)
        {
            if (isBase) return $"{option.Label} (${option.Amount})";
            if (option.Amount == 0) return option.Label;
            return option.Amount > 0 ? $"{option.Label} (+${option.Amount})" : $"{option.Label} (-${-option.Amount})";
        }

        private void OnEngineChanged(ConfiguratorSnapshot snapshot) => Refresh(snapshot);

        private void Refresh(ConfiguratorSnapshot snapshot)
        {
            SyncSingleChoice(OptionKind.Tier, snapshot.State.TierId);
            SyncSingleChoice(OptionKind.Door, snapshot.State.DoorId);
            SyncSingleChoice(OptionKind.Lock, snapshot.State.LockId);

            RenderLineItems(snapshot.Pricing);
            RenderValidation(snapshot.Violations);

            _totalLabel.text = $"${snapshot.Pricing.Total}";
            _totalLabel.EnableInClassList("total-label--invalid", !snapshot.IsValid);
        }

        private void SyncSingleChoice(OptionKind kind, string selectedId)
        {
            if (!_singleChoiceGroups.TryGetValue(kind, out var group)) return;
            var ids = _singleChoiceOptionIds[kind];
            int idx = ids.IndexOf(selectedId);
            if (idx >= 0 && group.value != idx)
                group.SetValueWithoutNotify(idx);
        }

        private void RenderLineItems(PricingResult pricing)
        {
            _lineItems.Clear();

            if (pricing.Tier != null) AddLineItem($"Tier: {pricing.Tier.Label}", FormatAmount(pricing.Tier.Amount, isBase: true));
            if (pricing.Door != null) AddLineItem($"Door: {pricing.Door.Label}", FormatAmount(pricing.Door.Amount, isBase: false));
            if (pricing.Lock != null) AddLineItem($"Lock: {pricing.Lock.Label}", FormatAmount(pricing.Lock.Amount, isBase: false));
            foreach (var addon in pricing.Addons)
                AddLineItem($"Add-on: {addon.Label}", FormatAmount(addon.Amount, isBase: false));

            AddLineItem("Unit price", $"${pricing.UnitPrice}");
            AddLineItem("Quantity", $"× {pricing.Quantity}");
        }

        private static string FormatAmount(int amount, bool isBase)
        {
            if (isBase) return $"${amount}";
            return amount >= 0 ? $"+${amount}" : $"-${-amount}";
        }

        private void AddLineItem(string label, string value)
        {
            var row = new VisualElement();
            row.AddToClassList("line-item");

            var labelElement = new Label(label);
            labelElement.AddToClassList("line-item__label");

            var valueElement = new Label(value);
            valueElement.AddToClassList("line-item__value");

            row.Add(labelElement);
            row.Add(valueElement);
            _lineItems.Add(row);
        }

        private void RenderValidation(IReadOnlyList<RuleViolation> violations)
        {
            _validationPanel.Clear();
            bool hasViolations = violations.Count > 0;
            _validationPanel.EnableInClassList("validation-panel--visible", hasViolations);
            if (!hasViolations) return;

            var title = new Label(violations.Count == 1
                ? "This configuration isn't buildable:"
                : $"This configuration isn't buildable ({violations.Count} issues):");
            title.AddToClassList("validation-title");
            _validationPanel.Add(title);

            foreach (var violation in violations)
            {
                var message = new Label("• " + violation.Message);
                message.AddToClassList("validation-message");
                _validationPanel.Add(message);
            }
        }
    }
}
