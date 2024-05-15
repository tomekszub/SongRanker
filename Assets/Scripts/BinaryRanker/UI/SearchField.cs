using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Immortus.SongRanker
{
    public class SearchField : MonoBehaviour
    {
        [SerializeField] List<HintEntry> _Hints;
        [SerializeField] GameObject _HintsParent;
        [SerializeField] RectTransform _RootRectTransform;
        [SerializeField] TMP_InputField _InputField;

        List<string> _recommendedValues;
        Action<int> _onResult;

        public void SetData(List<string> data, Action<int> onResult)
        {
            _recommendedValues = data;
            _onResult = onResult;
        }

        public void Refresh(string sourceText)
        {
            int chosenValues = 0;

            for (int i = 0; i < _recommendedValues.Count; i++)
            {
                if (!string.IsNullOrEmpty(_recommendedValues[i]) && _recommendedValues[i].Contains(sourceText, StringComparison.InvariantCultureIgnoreCase))
                {
                    _Hints[chosenValues].SetData(i, _recommendedValues[i], SearchEnded);
                    _Hints[chosenValues].SetActive(true);
                    chosenValues++;

                    if (chosenValues >= _Hints.Count)
                        break;
                }
            }

            for (int i = chosenValues; i < _Hints.Count; i++)
            {
                _Hints[i].SetActive(false);
            }

            _HintsParent.SetActive(chosenValues != 0);

            if(chosenValues != 0)
                LayoutRebuilder.ForceRebuildLayoutImmediate(_RootRectTransform);
        }

        public void SearchEnded(string s)
        {
            for (int i = 0; i < _recommendedValues.Count; i++)
            {
                if (_recommendedValues[i] == s)
                {
                    _onResult?.Invoke(i);
                    return;
                }
            }
        }

        void SearchEnded(int index, string val)
        {
            _HintsParent.SetActive(false);
            _InputField.SetTextWithoutNotify(val);
            _onResult?.Invoke(index);
        }
    }
}
