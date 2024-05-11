using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SongLoadingResultPopup : MonoBehaviour
{
    [SerializeField] GameObject _ErrorPanel;
    [SerializeField] Image _ResultIcon;
    [SerializeField] TextMeshProUGUI _ResultText;
    [SerializeField] TextMeshProUGUI _ErrorText;
    [SerializeField] Sprite _SuccessIcon;
    [SerializeField] Sprite _FailureIcon;

    public void Show(int loadedSongs, string[] errors)
    {
        gameObject.SetActive(true);

        _ResultText.text = $"Successfully loaded {loadedSongs} songs";

        bool anyErrors = errors.Length > 0;

        _ErrorPanel.SetActive(anyErrors);
        _ResultIcon.sprite = anyErrors ? _FailureIcon : _SuccessIcon;
        _ErrorText.text = string.Join("\n", errors);
    }
}
