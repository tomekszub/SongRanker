using System;
using System.Collections;
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
        [SerializeField] SongPlayer _Player;
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
        [SerializeField] SimpleHintEditPopup _SimpleHintEditPopup;
        [SerializeField] SimpleEditPopup _SimpleEditPopup;
        [SerializeField] MultiHintEditPopup _MultiHintEditPopup;
        [SerializeField] CreateAlbumPopup _CreateAlbumPopup;
        [SerializeField] ConfirmationPopup _ConfirmationPopup;
        [SerializeField] ProgressPopup _ProgressPopup;
        [SerializeField] SearchField _SearchField;

        List<Song> _context;
        Song _song;
        int _currSongIndex;
        bool _alreadySet;

        void Start()
        {
            if (_alreadySet)
                return;

            _SearchField.SetData(SongManager.AllSongNames.ToList(), ShowSong);

            _context = new List<Song>();
            _RankerController.Ranker.Ranking.ForEach(r => _context.AddRange(r));
            _currSongIndex = 0;
            ChangeSong();
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

            if (Input.GetKey(KeyCode.LeftControl))
                _currSongIndex = Mathf.Min(_currSongIndex + 5, _context.Count - 1);
            else if (Input.GetKey(KeyCode.LeftShift))
                _currSongIndex = Mathf.Min(_currSongIndex + 25, _context.Count - 1);
            else
                _currSongIndex++;

            ChangeSong();
        }

        public void Previous()
        {
            if (_currSongIndex <= 0)
                return;

            if (Input.GetKey(KeyCode.LeftControl))
                _currSongIndex = Mathf.Max(_currSongIndex - 5, 0);
            else if (Input.GetKey(KeyCode.LeftShift))
                _currSongIndex = Mathf.Min(_currSongIndex - 25, _context.Count - 1);
            else
                _currSongIndex--;

            ChangeSong();
        }

        public void SaveTags()
        {
            _ConfirmationPopup.Show("Are you sure you want to save mp3 tags? This action will override existing tags!", SaveTags);

            void SaveTags()
            {
                StartCoroutine(SaveTagsCoroutine());
            }
        }

        IEnumerator SaveTagsCoroutine()
        {
            _ProgressPopup.Show();

            for (int i = 0; i < _context.Count; i++)
            {
                Song song = _context[i];
                SongManager.SaveTag(song.Path, song);
                yield return null;
                _ProgressPopup.SetProgress((i + 1.0f) / _context.Count, song.Name);
            }

            _ProgressPopup.Hide();
        }

        void ShowSong(int index)
        {
            var songID = SongManager.GetAllSongs().ToList()[index].ID;

            for (int i = 0; i < _context.Count; i++)
            {
                if (_context[i].ID == songID)
                {
                    _currSongIndex = i;
                    ChangeSong();
                }
            }
        }

        void ChangeSong()
        {
            _song = _context[_currSongIndex];
            RefreshUI();
            _Player.Init(_song);
        }

        void RefreshUI()
        {
            _RankingPositionField.text = _RankerController.GetRankingPosition(_song.ID).ToString();

            bool propertyIsValid = PropertiesValidator.ValidateName(_song.Name);
            _TitleField.SetContent(_song.Name, !propertyIsValid, OpenTitleEditPopup);

            var artists = SongManager.GetArtistNamesByIDs(_song.ArtistIds);
            propertyIsValid = PropertiesValidator.ValidateArtists(artists);
            _ArtistsField.SetContent(string.Join(", ", artists), !propertyIsValid, OpenArtistsEditPopup);

            var genre = SongManager.GetGenreByID(_song.GenreID);
            propertyIsValid = PropertiesValidator.ValidateGenre(genre);
            _GenreField.SetContent(!propertyIsValid ? TEXT_MISSING_PROPERTY : genre.Name, !propertyIsValid, OpenGenreEditPopup);

            propertyIsValid = PropertiesValidator.ValidateYear(_song.Year);
            _YearField.SetContent(!propertyIsValid ? TEXT_MISSING_PROPERTY : _song.Year.ToString(), !propertyIsValid, OpenYearEditPopup);

            var language = SongManager.GetLanguageByID(_song.LanguageID);
            propertyIsValid = PropertiesValidator.ValidateLanguage(language);
            _LanguageField.SetContent(!propertyIsValid ? TEXT_MISSING_PROPERTY : language.Name, !propertyIsValid, OpenLanguageEditPopup);

            var album = SongManager.GetAlbumByID(_song.AlbumID);
            propertyIsValid = PropertiesValidator.ValidateAlbum(album);
            var trackNumber = _song.AlbumSongNumber;
            if (album != null)
            {
                _AlbumNameField.SetContent(album.Name, !propertyIsValid, OpenAlbumEditPopup);
                propertyIsValid = PropertiesValidator.ValidateAlbumTrackNumber(trackNumber);
                _AlbumTrackNumberField.SetContent(!propertyIsValid ? TEXT_MISSING_PROPERTY : trackNumber.ToString(), !propertyIsValid, OpenTrackNumberEditPopup);
            }
            else
            {
                _AlbumNameField.SetContent(TEXT_MISSING_PROPERTY, false, OpenAlbumEditPopup);
                propertyIsValid = PropertiesValidator.ValidateAlbumTrackNumber(trackNumber);
                _AlbumTrackNumberField.SetContent(!propertyIsValid ? TEXT_MISSING_PROPERTY : trackNumber.ToString(), propertyIsValid, OpenTrackNumberEditPopup);
            }

            _DurationField.SetContent(_song.Duration.ToString("mm':'ss"));
            _PathField.SetContent(_song.Path.ToString());
        }

        void OpenGenreEditPopup()
        {
            _SimpleHintEditPopup.gameObject.SetActive(true);
            var genre = SongManager.GetGenreByID(_song.GenreID);
            _SimpleHintEditPopup.SetContent("Genre", genre != null ? genre.Name : TEXT_MISSING_PROPERTY, Save, null, SongManager.AllGenreNames.ToList());

            void Save(string value)
            {
                _song.GenreID = SongManager.GetGenreIDByName(value, true);
                // TODO: ideally should not be here, changing genre should be inside song manager which would handle refreshing luts
                SongManager.RefreshGenresLUT();
                OnChangeDone?.Invoke();
                RefreshUI();
                _SimpleHintEditPopup.gameObject.SetActive(false);
            }
        }

        void OpenTitleEditPopup()
        {
            _SimpleEditPopup.gameObject.SetActive(true);
            var songName = _song.Name;
            _SimpleEditPopup.SetContent("Title", songName, Save, null);

            void Save(string value)
            {
                _song.Name = value;
                OnChangeDone?.Invoke();
                RefreshUI();
                _SimpleEditPopup.gameObject.SetActive(false);
            }
        }

        void OpenYearEditPopup()
        {
            _SimpleEditPopup.gameObject.SetActive(true);
            var year = _song.Year.ToString();
            _SimpleEditPopup.SetContent("Year", year, Save, Validate);

            bool Validate(string value)
            {
                if(!int.TryParse(value, out int parsedYear))
                    return false;

                return PropertiesValidator.ValidateYear(parsedYear);
            }

            void Save(string value)
            {
                _song.Year = int.Parse(value);
                OnChangeDone?.Invoke();
                RefreshUI();
                _SimpleEditPopup.gameObject.SetActive(false);
            }
        }

        void OpenLanguageEditPopup()
        {
            _SimpleHintEditPopup.gameObject.SetActive(true);
            var language = SongManager.GetLanguageByID(_song.LanguageID);
            _SimpleHintEditPopup.SetContent("Language", language != null ? language.Name : TEXT_MISSING_PROPERTY, Save, null, SongManager.AllLanguageNames.ToList());

            void Save(string value)
            {
                _song.LanguageID = SongManager.GetLanguageIDByName(value, true);
                SongManager.RefreshLanguageLUT();
                OnChangeDone?.Invoke();
                RefreshUI();
                _SimpleHintEditPopup.gameObject.SetActive(false);
            }
        }

        void OpenTrackNumberEditPopup()
        {
            _SimpleEditPopup.gameObject.SetActive(true);
            var trackNumber = _song.AlbumSongNumber.ToString();
            _SimpleEditPopup.SetContent("Track number", trackNumber, Save, Validate);

            bool Validate(string value)
            {
                if (!int.TryParse(value, out int parsedNumber))
                    return false;

                return PropertiesValidator.ValidateAlbumTrackNumber(parsedNumber);
            }

            void Save(string value)
            {
                _song.AlbumSongNumber = int.Parse(value);
                OnChangeDone?.Invoke();
                RefreshUI();
                _SimpleEditPopup.gameObject.SetActive(false);
            }
        }

        void OpenAlbumEditPopup()
        {
            _SimpleHintEditPopup.gameObject.SetActive(true);
            var album = SongManager.GetAlbumByID(_song.AlbumID);
            var albumName = album != null ? album.Name : "";
            _SimpleHintEditPopup.SetContent("Album", albumName, Save, null, SongManager.AllAlbumNames.ToList());

            void Save(string albumName)
            {
                var album = SongManager.GetAlbumByID(_song.AlbumID);
                var albumArtistID = album != null ? album.ArtistID : _song.ArtistIds.Length != 0 ? _song.ArtistIds[0] : -1;

                _CreateAlbumPopup.SetData(albumName, OnSuccess, OnCancel);

                void OnSuccess(int albumArtistID)
                {
                    _song.AlbumID = SongManager.GetAlbumIDByNameAndAuthor(albumName, albumArtistID, true);
                    SongManager.RefreshAlbumLUT();
                    OnChangeDone?.Invoke();
                    RefreshUI();
                    _SimpleHintEditPopup.gameObject.SetActive(false);
                }

                void OnCancel() => _SimpleHintEditPopup.gameObject.SetActive(false);
            }
        }

        void OpenArtistsEditPopup()
        {
            _MultiHintEditPopup.gameObject.SetActive(true);
            List<string> artistNames = SongManager.GetArtistNamesByIDs(_song.ArtistIds).ToList();
            _MultiHintEditPopup.SetContent("Artists", artistNames, Save, null, SongManager.AllArtistNames.ToList());

            void Save(List<string> values)
            {
                var artistIds = SongManager.GetArtistIDsByNames(values);
                _song.ArtistIds = artistIds.ToArray();
                // TODO: ideally should not be here, changing genre should be inside song manager which would handle refreshing luts
                SongManager.RefreshArtistsLUT();
                OnChangeDone?.Invoke();
                RefreshUI();
                _MultiHintEditPopup.gameObject.SetActive(false);
            }
        }
    }
}