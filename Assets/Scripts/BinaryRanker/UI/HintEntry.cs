using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HintEntry : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _ValueText;
    [SerializeField] Button _Button;

    string _value;
    int _index;

    public void SetActive(bool active) => gameObject.SetActive(active);

    public void SetData(int index, string value, Action<int, string> onEntryClicked)
    {
        _index = index;
        _value = value;
        _ValueText.text = value;
        _Button.onClick.RemoveAllListeners();
        _Button.onClick.AddListener(() => onEntryClicked?.Invoke(_index, _value));
    }
}
