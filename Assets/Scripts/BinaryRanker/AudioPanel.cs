using UnityEngine;

public class AudioPanel : MonoBehaviour
{
    [SerializeField] AudioSource _AudioSource;
    [SerializeField] AudioSource _AudioSourceToPause;
    [SerializeField] AudioSourceProgressBar _AudioProgressBar;
    [SerializeField] AudioClipLoader _AudioClipLoader;
    [SerializeField] GameObject _UIPanel;

    string _path;
    float _initialTime;
    bool _newSongLoaded;

    void Awake() => _AudioProgressBar.Init(PlayFromTime);

    void Update()
    {
        if (_AudioSource.isPlaying)
            _AudioProgressBar.UpdateProgress(_AudioSource.time / _AudioSource.clip.length);
    }

    public void SetActive(bool val) => _UIPanel.SetActive(val);

    public void ChangeSong(string path)
    {
        if(_path != path)
            _newSongLoaded = false;
        _path = path;
    }

    public void PlayFromTime(float val = 0)
    {
        _initialTime = val;

        if (_AudioSource.clip == null || !_newSongLoaded)
        {
            _AudioClipLoader.LoadSong(_path, LoadAndPlaySong);
            return;
        }

        PlayCurrentClip();
    }

    void PlayCurrentClip()
    {
        if (!_AudioSource.isPlaying)
            _AudioSource.Play();

        if (_AudioSourceToPause.isPlaying)
            _AudioSourceToPause.Stop();

        _AudioSource.time = _initialTime * _AudioSource.clip.length;
    }

    void LoadAndPlaySong(AudioClip audioClip)
    {
        _AudioSource.clip = audioClip;
        if (audioClip != null)
            PlayCurrentClip();

        _newSongLoaded = true;
    }
}
