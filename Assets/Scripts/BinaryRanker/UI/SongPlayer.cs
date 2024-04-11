using UnityEngine;
using UnityEngine.UI;

namespace Immortus.SongRanker
{
    public class SongPlayer : MonoBehaviour
    {
        [SerializeField] AudioClipLoader _AudioLoader;
        [SerializeField] AudioSource _AudioSource;
        [SerializeField] Image _PlayButtonIcon;
        [SerializeField] Sprite _PlaySprite;
        [SerializeField] Sprite _PauseSprite;
        [SerializeField] Slider _SongProgressbar;

        bool _isplaying;

        public void Init(Song song)
        {
            _AudioSource.time = 0;
            PausePlaying();
            _SongProgressbar.value = 0;
            _AudioLoader.LoadSong(song.Path, (clip) => _AudioSource.clip = clip);
        }

        public void TogglePlayPause()
        {
            if (_isplaying)
                PausePlaying();
            else
                StartPlaying();
        }

        public void OnVolumeChanged(float val) => _AudioSource.volume = val;

        void Update()
        {
            if (_isplaying)
                _SongProgressbar.value = _AudioSource.time / _AudioSource.clip.length;
        }

        void PausePlaying()
        {
            _AudioSource.Pause();
            _PlayButtonIcon.sprite = _PlaySprite;
            _isplaying = false;
        }

        void StartPlaying()
        {
            _AudioSource.Play();
            _PlayButtonIcon.sprite = _PauseSprite;
            _isplaying = true;
        }
    }
}
