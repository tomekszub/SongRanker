using System;
using UnityEngine;

public class AudioPanelController : MonoBehaviour
{
    public event Action OnPlayStateEnter;

    [SerializeField] AudioSource _AudioSource;
    [SerializeField] AudioClipLoader _AudioClipLoader;
    [SerializeField] AudioSourceProgressBar _AudioProgressBar;
    [Header("UI References")]
    [SerializeField] GameObject _PlayIcon;
    [SerializeField] GameObject _PauseIcon;
    [SerializeField] GameObject _UIPanel;

    bool _clipRefreshed;
    bool _clipLoading;
    string _path;
    float _cachedTime;

    void Awake() => _AudioProgressBar.Init(OnSongTimeChanged);

    void Update()
    {
        if (_AudioSource.isPlaying)
            _AudioProgressBar.UpdateProgress(_AudioSource.time / _AudioSource.clip.length);
    }

    public void Init(AudioPanelController otherController)
    {
        otherController.OnPlayStateEnter -= Pause;
        otherController.OnPlayStateEnter += Pause;
    }

    public void SetActive(bool val) => _UIPanel.SetActive(val);

    public void TogglePlay()
    {
        bool wasPlaying = _AudioSource.isPlaying;

        if (wasPlaying)
            Pause();
        else
            Play();
    }

    public void ChangeSong(string path)
    {
        if (_path == path)
            return;

        _clipRefreshed = false;
        _path = path;

        if(_AudioSource.isPlaying)
            Play();
    }

    void OnSongTimeChanged(float val) => Play(val);

    void Play(float time = -1f)
    {
        if (_clipLoading)
            return;

        if(!_clipRefreshed)
        {
            _clipLoading = true;
            _cachedTime = time;
            _AudioClipLoader.LoadSong(_path, LoadAndPlaySong);
            return;
        }

        OnPlayStateEnter?.Invoke();

        _AudioSource.Play();
        _PlayIcon.SetActive(false);
        _PauseIcon.SetActive(true);

        if (time >= 0)
            _AudioSource.time = time * _AudioSource.clip.length;
    }

    void Pause()
    {
        _AudioSource.Pause();
        _PlayIcon.SetActive(true);
        _PauseIcon.SetActive(false);
    }

    void LoadAndPlaySong(AudioClip audioClip)
    {
        if (audioClip == null)
            Debug.LogError("Something went wrong, audio clip should not be null.");

        _AudioSource.clip = audioClip;
        _clipLoading = false;
        _clipRefreshed = true;

        Play(_cachedTime);
        // TODO: settings if we want to keep progress of current song dont set it to -1
        _cachedTime = -1;
    }
}
