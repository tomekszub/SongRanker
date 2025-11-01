using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressPopup : BasePopup
{
    [SerializeField] Slider _ProgressSlider;
    [SerializeField] TextMeshProUGUI _Text;

    public void SetProgress(float percent, string text)
    {
        _ProgressSlider.value = percent;
        _Text.text = text;
    }
}
