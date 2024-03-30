using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditPopup : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _TitleText;
    [SerializeField] TextMeshProUGUI _CurrentValueText;
    [SerializeField] TMP_InputField _InputField;
    [SerializeField] Button _SaveButton;
    [SerializeField] List<TextMeshProUGUI> _ReccomendedValuesText;
    [SerializeField] GameObject _NewValueIndicator;

    List<string> _recommendedValues;

    public void SetContent(string title, string currentValue, Action<string> onSave, List<string> recommendedValues)
    {
        _TitleText.text = title;
        _CurrentValueText.text = currentValue;

        _recommendedValues = recommendedValues;
        _NewValueIndicator.SetActive(false);

        _SaveButton.onClick.RemoveAllListeners();
        _SaveButton.onClick.AddListener(() => onSave.Invoke(_InputField.text));

        _InputField.onValueChanged.RemoveAllListeners();
        _InputField.onValueChanged.AddListener(InputValueChanged);

        UpdateRecommendedValuesUI(_InputField.text);
    }

    public void UseRecommendedValue(TextMeshProUGUI source) => _InputField.text = source.text;

    void InputValueChanged(string value)
    {
        UpdateRecommendedValuesUI(value);
    }

    void UpdateRecommendedValuesUI(string sourceText)
    {
        _NewValueIndicator.SetActive(string.IsNullOrEmpty(sourceText) ? false : !_recommendedValues.Any(val => val.Equals(sourceText)));

        _ReccomendedValuesText.ForEach(t => t.text = "");

        var potentialValueIndices = new HashSet<int>();

        int recommendedValues = 0;

        for (int i = 0; i < _recommendedValues.Count; i++)
        {
            if(_recommendedValues[i].Contains(sourceText, StringComparison.InvariantCultureIgnoreCase))
            {
                _ReccomendedValuesText[recommendedValues].text = _recommendedValues[i];
                recommendedValues++;
                potentialValueIndices.Add(i);

                if (recommendedValues >= _ReccomendedValuesText.Count)
                    return;
            }
        }

        for (int i = 0; i < _recommendedValues.Count; i++)
        {
            if (potentialValueIndices.Contains(i))
                continue;

            _ReccomendedValuesText[recommendedValues].text = _recommendedValues[i];
            recommendedValues++;

            if (recommendedValues >= _ReccomendedValuesText.Count)
                return;
        }
    }
}
