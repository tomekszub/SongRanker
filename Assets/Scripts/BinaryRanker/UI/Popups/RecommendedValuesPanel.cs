using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class RecommendedValuesPanel : MonoBehaviour
{
    [SerializeField] List<TextMeshProUGUI> _RecommendedValuesText;
    [SerializeField] GameObject _NewValueIndicator;

    List<string> _recommendedValues;

    public void SetData(List<string> data)
    {
        _recommendedValues = data;
        _NewValueIndicator.SetActive(false);
    }

    public void Refresh(string sourceText)
    {
        _NewValueIndicator.SetActive(string.IsNullOrEmpty(sourceText) ? false : !_recommendedValues.Any(val => val.Equals(sourceText)));

        _RecommendedValuesText.ForEach(t => t.text = "");

        var potentialValueIndices = new HashSet<int>();

        int recommendedValues = 0;

        for (int i = 0; i < _recommendedValues.Count; i++)
        {
            if (!string.IsNullOrEmpty(_recommendedValues[i]) && _recommendedValues[i].Contains(sourceText, StringComparison.InvariantCultureIgnoreCase))
            {
                _RecommendedValuesText[recommendedValues].text = _recommendedValues[i];
                recommendedValues++;
                potentialValueIndices.Add(i);

                if (recommendedValues >= _RecommendedValuesText.Count)
                    return;
            }
        }

        for (int i = 0; i < _recommendedValues.Count; i++)
        {
            if (potentialValueIndices.Contains(i))
                continue;

            _RecommendedValuesText[recommendedValues].text = _recommendedValues[i];
            recommendedValues++;

            if (recommendedValues >= _RecommendedValuesText.Count)
                return;
        }
    }
}
