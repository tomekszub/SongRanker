using TMPro;
using UnityEngine;

namespace Immortus.SongRanker
{
    public class EditingTab : MonoBehaviour
    {
        [SerializeField] RankerController _RankerController;
        [Header("UI References")]
        [SerializeField] TextMeshProUGUI _RankingPositionText;
        [SerializeField] TextMeshProUGUI _TitleText;
        [SerializeField] TextMeshProUGUI _ArtistsText;
        [SerializeField] TextMeshProUGUI _GenreText;
        [SerializeField] TextMeshProUGUI _YearText;
        [SerializeField] TextMeshProUGUI _LanguageText;
        [SerializeField] TextMeshProUGUI _AlbumNameText;
        [SerializeField] TextMeshProUGUI _AlbumTrackNumberText;
        [SerializeField] TextMeshProUGUI _DurationText;
        [SerializeField] TextMeshProUGUI _PathText;

        Song _song;
        bool _alreadySet;

        void Start()
        {
            if (_alreadySet)
                return;

            _song = _RankerController.Ranker.Ranking[0][0];
            RefreshUI(1);
        }

        void RefreshUI(int position)
        {
            _RankingPositionText.text = position.ToString();
            _TitleText.text = _song.Name;
            _ArtistsText.text = string.Join(", ", SongManager.GetArtistNamesByIDs(_song.ArtistIds));
            _GenreText.text = SongManager.GetGenreByID(_song.GenreID)?.Name;
            _YearText.text = _song.Year.ToString();
            _AlbumNameText.text = SongManager.GetAlbumByID(_song.AlbumID)?.Name;
            _AlbumTrackNumberText.text = _song.AlbumSongNumber.ToString();
            _DurationText.text = _song.Duration.ToString("mm':'ss");
            _PathText.text = _song.Path.ToString();
        }
    }
}