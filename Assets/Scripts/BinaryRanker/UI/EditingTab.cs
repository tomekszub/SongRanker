using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Immortus.SongRanker
{
    public class EditingTab : MonoBehaviour
    {
        const string TEXT_MISSING_PROPERTY = "-";

        public event Action OnChangeDone;

        [SerializeField] RankerController _RankerController;
        [Header("UI References")]
        [SerializeField] TextMeshProUGUI _RankingPositionField;
        [SerializeField] PropertyField _TitleField;
        [SerializeField] PropertyField _ArtistsField;
        [SerializeField] PropertyField _GenreField;
        [SerializeField] PropertyField _YearField;
        [SerializeField] PropertyField _LanguageField;
        [SerializeField] PropertyField _AlbumNameField;
        [SerializeField] PropertyField _AlbumTrackNumberField;
        [SerializeField] PropertyField _DurationField;
        [SerializeField] PropertyField _PathField;
        [SerializeField] EditPopup _EditPopup;

        List<Song> _context;
        Song _song;
        int _currSongIndex;
        bool _alreadySet;

        void Start()
        {
            if (_alreadySet)
                return;

            _context = new List<Song>();
            _RankerController.Ranker.Ranking.ForEach(r => _context.AddRange(r));
            _currSongIndex = 0;
            _song = _context[_currSongIndex];
            RefreshUI();
        }

        void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S))
            {
                SongManager.SaveDataToFiles();
                TabController.ShowSavePanel();
            }
        }

        public void Next()
        {
            if (_currSongIndex >= _context.Count - 1)
                return;

            _currSongIndex++;
            _song = _context[_currSongIndex];
            RefreshUI();
        }

        public void Previous()
        {
            if (_currSongIndex <= 0)
                return;

            _currSongIndex--;
            _song = _context[_currSongIndex];
            RefreshUI();
        }

        void RefreshUI()
        {
            var position = _currSongIndex + 1;

            _RankingPositionField.text = position.ToString();

            bool propertyIsValid = PropertiesValidator.ValidateName(_song.Name);
            _TitleField.SetContent(_song.Name, !propertyIsValid);

            var artists = SongManager.GetArtistNamesByIDs(_song.ArtistIds);
            propertyIsValid = PropertiesValidator.ValidateArtists(artists);
            _ArtistsField.SetContent(string.Join(", ", artists), !propertyIsValid);

            var genre = SongManager.GetGenreByID(_song.GenreID);
            propertyIsValid = PropertiesValidator.ValidateGenre(genre);
            _GenreField.SetContent(genre == null ? TEXT_MISSING_PROPERTY : genre.Name, !propertyIsValid, OpenGenreEditPopup);

            propertyIsValid = PropertiesValidator.ValidateYear(_song.Year);
            _YearField.SetContent(_song.Year.ToString(), !propertyIsValid);

            var album = SongManager.GetAlbumByID(_song.AlbumID);
            propertyIsValid = PropertiesValidator.ValidateAlbum(album);
            var trackNumber = _song.AlbumSongNumber;
            if (album != null)
            {
                _AlbumNameField.SetContent(album.Name, !propertyIsValid);
                propertyIsValid = PropertiesValidator.ValidateAlbumTrackNumber(trackNumber);
                _AlbumTrackNumberField.SetContent(trackNumber == 0 ? TEXT_MISSING_PROPERTY : trackNumber.ToString(), !propertyIsValid);
            }
            else
            {
                _AlbumNameField.SetContent(TEXT_MISSING_PROPERTY, true);
                propertyIsValid = PropertiesValidator.ValidateAlbumTrackNumber(trackNumber);
                _AlbumTrackNumberField.SetContent(trackNumber == 0 ? TEXT_MISSING_PROPERTY : trackNumber.ToString(), !propertyIsValid);
            }

            _DurationField.SetContent(_song.Duration.ToString("mm':'ss"));
            _PathField.SetContent(_song.Path.ToString());
        }

        void OpenGenreEditPopup()
        {
            _EditPopup.gameObject.SetActive(true);
            var genre = SongManager.GetGenreByID(_song.GenreID);
            _EditPopup.SetContent("Genre", genre != null ? genre.Name : TEXT_MISSING_PROPERTY, Save, SongManager.AllGenreNames.ToList());

            void Save(string value)
            {
                _song.GenreID = SongManager.GetGenreIDByName(value, true);
                // TODO: ideally should not be here, changing genre should be inside song manager which would handle refreshing luts
                SongManager.RefreshGenresLUT();
                OnChangeDone?.Invoke();
                RefreshUI();
                _EditPopup.gameObject.SetActive(false);
            }
        }
    }
}