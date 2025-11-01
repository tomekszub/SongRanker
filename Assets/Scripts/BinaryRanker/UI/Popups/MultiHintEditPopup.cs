using System;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MultiHintEditPopup : BaseEditPopup<List<string>>
{
    [SerializeField] RecommendedValuesPanel _RecommendedValuesPanel;
    [SerializeField] protected TMP_InputField _InputField;
    [SerializeField] TextMeshProUGUI _CurrentValueText;
    [SerializeField] TextMeshProUGUI _OldValueText;
    [SerializeField] Button _AddButton;

    public void Show(string title, List<string> oldValue, Action<List<string>> onSave, Func<List<string>, bool> onValidate, List<string> recommendedValues)
    {
        string oldValueJoined = string.Join(",", oldValue.ToArray());
        _OldValueText.text = oldValueJoined;
        _RecommendedValuesPanel.SetData(recommendedValues);

        _InputField.onValueChanged.RemoveAllListeners();
        _InputField.onValueChanged.AddListener(InputValueChanged);

        _AddButton.onClick.RemoveAllListeners();
        _AddButton.onClick.AddListener(AddNewItem);

        InputValueChanged(_InputField.text);

        _currentValue = new();

        base.Show(title, oldValue, onSave, onValidate);
    }

    public void UseRecommendedValue(TextMeshProUGUI source) => _InputField.text = source.text;

    void AddNewItem()
    {
        _currentValue.Add(_InputField.text);
        _CurrentValueText.text = string.Join(',', _currentValue);
        _AddButton.gameObject.SetActive(false);
        Validate(GetCurrentData());
    }

    void InputValueChanged(string value)
    {
        bool valid = !string.IsNullOrEmpty(value) && !_currentValue.Contains(value);
        _AddButton.gameObject.SetActive(valid);
        _RecommendedValuesPanel.Refresh(value);
    }

    protected override void RefreshCurrentUIData()
    {
        _CurrentValueText.text = string.Join(',', _currentValue);
    }

    protected override List<string> GetCurrentData() => _currentValue;

    protected override void Validate(List<string> currentValues)
    {
        bool valid = HelperExtensions.IsNewListValid(currentValues, _oldValue);

        if (valid && _validationFunction != null)
            valid = _validationFunction(currentValues);

        _SaveButton.gameObject.SetActive(valid);
    }
}
