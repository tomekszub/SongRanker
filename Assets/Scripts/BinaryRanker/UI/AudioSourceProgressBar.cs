using System;
using UnityEngine;
using UnityEngine.UI;

public class AudioSourceProgressBar : MonoBehaviour
{
    [SerializeField] Slider _SongProgressSlider;
    [SerializeField] Slider _FakeSlider;

    Action<float> _onProgressManuallyChanged;

    public void Init(Action<float> onSongProgressManuallyChanged)
    {
        _onProgressManuallyChanged = onSongProgressManuallyChanged;
        _FakeSlider.onValueChanged.RemoveAllListeners();
        _FakeSlider.onValueChanged.AddListener(SetAudioTime);
    }

    public void UpdateProgress(float progress)
    {
        _SongProgressSlider.value = progress;
    }

    void SetAudioTime(float val)
    {
        _onProgressManuallyChanged?.Invoke(val);
    }
}
