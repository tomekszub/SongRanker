using System;
using UnityEngine;
using UnityEngine.UI;

public class GenericPopup_PopupData : PopupData
{
    public string title;
    public string description;
    public string noButtonText;
    public string yesButtonText;
    public Action onNoButton;
    public Action onYesButton;

    public GenericPopup_PopupData(string title, string description, string noButtonText, string yesButtonText, Action onNoButton, Action onYesButton, Action onClose = null) : base(onClose)
    {
        this.title = title;
        this.description = description;
        this.noButtonText = noButtonText;
        this.yesButtonText = yesButtonText;
        this.onNoButton = onNoButton;
        this.onYesButton = onYesButton;
    }
}

public class GenericPopup : Popup
{
    [SerializeField] Text _Title;
    [SerializeField] Text _Description;
    [SerializeField] Text _NoButtonText;
    [SerializeField] Text _YesButtonText;
    [SerializeField] Button _NoButton;
    [SerializeField] Button _YesButton;

    public override void Show(PopupData popupData)
    {
        base.Show(popupData);

        if (!(popupData is GenericPopup_PopupData myPopupData))
            return;

        _Title.text = myPopupData.title;
        _Description.text = myPopupData.description;
        _NoButtonText.text = myPopupData.noButtonText;
        _YesButtonText.text = myPopupData.yesButtonText;

        _NoButton.onClick.RemoveAllListeners();
        if (myPopupData.onNoButton != null)
            _NoButton.onClick.AddListener(() => myPopupData.onNoButton());

        _YesButton.onClick.RemoveAllListeners();
        if (myPopupData.onYesButton != null)
            _YesButton.onClick.AddListener(() => myPopupData.onYesButton());

        _NoButton.onClick.AddListener(() => Hide());
        _YesButton.onClick.AddListener(() => Hide());
    }
}
