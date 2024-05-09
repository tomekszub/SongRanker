using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SimpleHintEditPopup : SimpleEditPopup
{
    [SerializeField] RecommendedValuesPanel _RecommendedValuesPanel;

    public void SetContent(string title, string currentValue, Action<string> onSave, Func<string, bool> onValidate, List<string> recommendedValues)
    {
        _RecommendedValuesPanel.SetData(recommendedValues);

        SetContent(title, currentValue, onSave, onValidate);
    }

    public void UseRecommendedValue(TextMeshProUGUI source) => _InputField.text = source.text;

    protected override void Validate(string sourceText)
    {
        base.Validate(sourceText);

        _RecommendedValuesPanel.Refresh(sourceText);
    }
}
