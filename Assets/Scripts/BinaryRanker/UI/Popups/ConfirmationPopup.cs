using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmationPopup : BasePopup
{
    [SerializeField] TextMeshProUGUI _ContentText;
    [SerializeField] Button _ConfirmButton;
    [SerializeField] Button _CancelButton;

    public void Show(string content, Action onConfirm, Action onCancel = null)
    {
        if(gameObject.activeInHierarchy)
            return;

        Show();

        _ContentText.text = content;

        _ConfirmButton.onClick.RemoveAllListeners();
        _ConfirmButton.onClick.AddListener(() => gameObject.SetActive(false));
        _ConfirmButton.onClick.AddListener(() => onConfirm.Invoke());

        _CancelButton.onClick.RemoveAllListeners();
        if(onCancel != null)
            _CancelButton.onClick.AddListener(() => onCancel.Invoke());
        _CancelButton.onClick.AddListener(() => gameObject.SetActive(false));
    }
}
