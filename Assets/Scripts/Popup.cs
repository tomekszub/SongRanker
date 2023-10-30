using System;
using UnityEngine;

public class PopupData
{
    public Action OnClose;

    public PopupData(Action onClose = null) => OnClose = onClose;
}

[Serializable]
public class Popup : MonoBehaviour
{
    [SerializeField] Canvas _Canvas;

    public virtual void Show(PopupData popupData) => _Canvas.enabled = true;

    protected void Hide() => _Canvas.enabled = false;
}
