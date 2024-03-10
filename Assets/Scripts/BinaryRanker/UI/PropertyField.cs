using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PropertyField : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _ContentText;
    [SerializeField] GameObject _ErrorBackground;
    [SerializeField] Button _EditButton;

    public void SetContent(string text, bool enableErrorIndicator = false, Action onEdit = null)
    {
        _ErrorBackground.SetActive(enableErrorIndicator);

        _ContentText.text = text;

        _EditButton.gameObject.SetActive(onEdit != null);
        if (_EditButton != null)
        {
            _EditButton.onClick.RemoveAllListeners();
            _EditButton.onClick.AddListener(() => onEdit.Invoke());
        }
    }
}
