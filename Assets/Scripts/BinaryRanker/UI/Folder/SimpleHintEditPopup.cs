using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class SimpleHintEditPopup : SimpleEditPopup
{
    [SerializeField] List<TextMeshProUGUI> _ReccomendedValuesText;
    [SerializeField] GameObject _NewValueIndicator;

    List<string> _recommendedValues;

    public void SetContent(string title, string currentValue, Action<string> onSave, List<string> recommendedValues)
    {
        _recommendedValues = recommendedValues;
        _NewValueIndicator.SetActive(false);

        SetContent(title, currentValue, onSave);
    }

    public void UseRecommendedValue(TextMeshProUGUI source) => _InputField.text = source.text;

    protected override void UpdateRecommendedValuesUI(string sourceText)
    {
        base.UpdateRecommendedValuesUI(sourceText);

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
