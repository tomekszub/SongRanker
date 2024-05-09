using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressPopup : MonoBehaviour
{
    [SerializeField] Slider _ProgressSlider;
    [SerializeField] TextMeshProUGUI _Text;

    public void Show() => gameObject.SetActive(true);

    public void Hide() => gameObject.SetActive(false);

    public void SetProgress(float percent, string text)
    {
        _ProgressSlider.value = percent;
        _Text.text = text;
    }
}
