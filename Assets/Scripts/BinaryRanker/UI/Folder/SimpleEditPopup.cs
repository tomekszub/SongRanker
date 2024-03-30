using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SimpleEditPopup : MonoBehaviour
{
    [SerializeField] protected TMP_InputField _InputField;
    [SerializeField] TextMeshProUGUI _TitleText;
    [SerializeField] TextMeshProUGUI _CurrentValueText;
    [SerializeField] Button _SaveButton;

    protected string _currentValue;

    public void SetContent(string title, string currentValue, Action<string> onSave)
    {
        _TitleText.text = title;
        _CurrentValueText.text = currentValue;

        _currentValue = currentValue;

        _SaveButton.onClick.RemoveAllListeners();
        _SaveButton.onClick.AddListener(() => onSave.Invoke(_InputField.text));

        _InputField.onValueChanged.RemoveAllListeners();
        _InputField.onValueChanged.AddListener(InputValueChanged);

        UpdateRecommendedValuesUI(_InputField.text);
    }

    void InputValueChanged(string value)
    {
        UpdateRecommendedValuesUI(value);
    }

    protected virtual void UpdateRecommendedValuesUI(string sourceText)
    {
        _SaveButton.gameObject.SetActive(!string.IsNullOrEmpty(sourceText) && !sourceText.Equals(_currentValue));
    }
}
