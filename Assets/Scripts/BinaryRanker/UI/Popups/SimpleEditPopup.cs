using System;
using TMPro;
using UnityEngine;

public class SimpleEditPopup : BaseEditPopup<string>
{
    [SerializeField] protected TMP_InputField _InputField;
    [SerializeField] TextMeshProUGUI _OldValueText;

    public override void Show(string title, string oldValue, Action<string> onSave, Func<string, bool> onValidate)
    {
        _OldValueText.text = oldValue;
        _InputField.text = "";

        _InputField.onValueChanged.RemoveAllListeners();
        _InputField.onValueChanged.AddListener(InputValueChanged);

        base.Show(title, oldValue, onSave, onValidate);

        _InputField.ActivateInputField();
    }

    void InputValueChanged(string value) => Validate(value);

    protected override void Validate(string sourceText)
    {
        bool validated = !string.IsNullOrEmpty(sourceText) && !sourceText.Equals(_oldValue);

        if (validated && _validationFunction != null)
            validated = _validationFunction(sourceText);

        _SaveButton.gameObject.SetActive(validated);
    }

    protected override string GetCurrentData() => _InputField.text;

    protected override void RefreshCurrentUIData() => _InputField.text = _currentValue;
}
