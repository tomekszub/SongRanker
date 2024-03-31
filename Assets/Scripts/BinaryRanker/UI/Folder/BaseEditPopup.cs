using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseEditPopup<T> : MonoBehaviour
{
    [SerializeField] protected Button _SaveButton;
    [SerializeField] TextMeshProUGUI _TitleText;

    protected T _currentValue;
    protected T _oldValue;
    protected Func<T, bool> _validationFunction;

    public virtual void SetContent(string title, T oldValue, Action<T> onSave, Func<T, bool> onValidate)
    {
        _TitleText.text = title;
        _oldValue = oldValue;

        RefreshCurrentUIData();

        _validationFunction = onValidate;

        _SaveButton.onClick.RemoveAllListeners();
        _SaveButton.onClick.AddListener(() => onSave.Invoke(GetCurrentData()));

        Validate(GetCurrentData());
    }

    protected abstract T GetCurrentData();

    protected abstract void RefreshCurrentUIData();

    protected abstract void Validate(T source);
}
