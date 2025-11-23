using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Immortus.SongRanker
{
    public class EditingTabSong : ContextBaseEditingTabContainer<Song>
    {
        const string TEXT_MISSING_PROPERTY = "-";
        const string SAVE_TAG_DESC = "Are you sure you want to save mp3 tags for this song? This action will override existing tags!";
        const string SAVE_ALL_TAGS_DESC = "Are you sure you want to save mp3 tags for all songs? This action will override existing tags!";
        
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

        public override void Init(RankerController rankerController, Action onChangeDone)
        {
            base.Init(rankerController, onChangeDone);
            
            _rankerController.Ranker.Ranking.ForEach(r => _context.AddRange(r));
        }

        public override List<string> GetSearchData() => SongManager.AllSongNames.ToList();

        public override void ShowDataWithIndex(int index)
        {
            var songID = SongManager.GetAllSongs().ToList()[index].ID;

            for (int i = 0; i < _context.Count; i++)
            {
                if (_context[i].ID == songID)
                {
                    _currIndex = i;
                    OnItemChanged();
                }
            }
        }
        
        public void SaveTags()
        {
            PopupController.ShowConfirmationPopup(SAVE_ALL_TAGS_DESC, LaunchTagSaving);

            void LaunchTagSaving() => StartCoroutine(SaveTagsCoroutine());
        }
        
        public void SaveTag() => PopupController.ShowConfirmationPopup(SAVE_TAG_DESC, () => SaveTagInternal(_currentElement));
        
        IEnumerator SaveTagsCoroutine()
        {
            PopupController.ShowProgressPopup();

            for (int i = 0; i < _context.Count; i++)
            {
                Song song = _context[i];
                SaveTagInternal(song);
                yield return null;
                PopupController.SetProgressPopupContent((i + 1.0f) / _context.Count, song.Name);
            }

            PopupController.HideProgressPopup();
        }

        void SaveTagInternal(Song song) => SongManager.SaveTag(song.Path, song);

        protected override void OnItemChanged()
        {
            base.OnItemChanged();
            _Player.Init(_currentElement);
        }

        protected override void Refresh()
        {
            _RankingPositionField.text = _rankerController.GetRankingPosition(_currentElement.ID).ToString();

            bool propertyIsValid = PropertiesValidator.ValidateName(_currentElement.Name);
            _TitleField.SetContent(_currentElement.Name, !propertyIsValid, OpenTitleEditPopup);

            var artists = SongManager.GetArtistNamesByIDs(_currentElement.ArtistIds);
            propertyIsValid = PropertiesValidator.ValidateArtists(artists);
            _ArtistsField.SetContent(string.Join(", ", artists), !propertyIsValid, OpenArtistsEditPopup);

            var genre = SongManager.GetGenreByID(_currentElement.GenreID);
            propertyIsValid = PropertiesValidator.ValidateGenre(genre);
            _GenreField.SetContent(!propertyIsValid ? TEXT_MISSING_PROPERTY : genre.Name, !propertyIsValid, OpenGenreEditPopup);

            propertyIsValid = PropertiesValidator.ValidateYear(_currentElement.Year);
            _YearField.SetContent(!propertyIsValid ? TEXT_MISSING_PROPERTY : _currentElement.Year.ToString(), !propertyIsValid, OpenYearEditPopup);

            var language = SongManager.GetLanguageByID(_currentElement.LanguageID);
            propertyIsValid = PropertiesValidator.ValidateLanguage(language);
            _LanguageField.SetContent(!propertyIsValid ? TEXT_MISSING_PROPERTY : language.Name, !propertyIsValid, OpenLanguageEditPopup);

            var album = SongManager.GetAlbumByID(_currentElement.AlbumID);
            propertyIsValid = PropertiesValidator.ValidateAlbum(album);
            var trackNumber = _currentElement.AlbumSongNumber;
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

            _DurationField.SetContent(_currentElement.Duration.ToString("mm':'ss"));
            _PathField.SetContent(_currentElement.Path.ToString());
        }

        void OpenGenreEditPopup()
        {
            var genre = SongManager.GetGenreByID(_currentElement.GenreID);
            PopupController.ShowHintEditPopup("Genre", genre != null ? genre.Name : TEXT_MISSING_PROPERTY, Save, null, SongManager.AllGenreNames.ToList());

            void Save(string value)
            {
                _currentElement.GenreID = SongManager.GetGenreIDByName(value, true);
                // TODO: ideally should not be here, changing genre should be inside song manager which would handle refreshing luts
                SongManager.RefreshGenresLUT();
                TriggerOnChangeEvents();
                Refresh();
            }
        }

        void OpenTitleEditPopup()
        {
            var songName = _currentElement.Name;
            PopupController.ShowSimpleEditPopup("Title", songName, Save, null);

            void Save(string value)
            {
                _currentElement.Name = value;
                TriggerOnChangeEvents();
                Refresh();
            }
        }

        void OpenYearEditPopup()
        {
            var year = _currentElement.Year.ToString();
            PopupController.ShowSimpleEditPopup("Year", year, Save, Validate);

            bool Validate(string value)
            {
                if(!int.TryParse(value, out int parsedYear))
                    return false;

                return PropertiesValidator.ValidateYear(parsedYear);
            }

            void Save(string value)
            {
                _currentElement.Year = int.Parse(value);
                TriggerOnChangeEvents();
                Refresh();
            }
        }

        void OpenLanguageEditPopup()
        {
            var language = SongManager.GetLanguageByID(_currentElement.LanguageID);
            PopupController.ShowHintEditPopup("Language", language != null ? language.Name : TEXT_MISSING_PROPERTY, Save, null, SongManager.AllLanguageNames.ToList());

            void Save(string value)
            {
                _currentElement.LanguageID = SongManager.GetLanguageIDByName(value, true);
                SongManager.RefreshLanguageLUT();
                TriggerOnChangeEvents();
                Refresh();
            }
        }

        void OpenTrackNumberEditPopup()
        {
            var trackNumber = _currentElement.AlbumSongNumber.ToString();
            PopupController.ShowSimpleEditPopup("Track number", trackNumber, Save, Validate);

            bool Validate(string value)
            {
                if (!int.TryParse(value, out int parsedNumber))
                    return false;

                return PropertiesValidator.ValidateAlbumTrackNumber(parsedNumber);
            }

            void Save(string value)
            {
                _currentElement.AlbumSongNumber = int.Parse(value);
                TriggerOnChangeEvents();
                Refresh();
            }
        }

        void OpenAlbumEditPopup()
        {
            var album = SongManager.GetAlbumByID(_currentElement.AlbumID);
            var originalAlbumName = album != null ? album.Name : "";
            PopupController.ShowHintEditPopup("Album", originalAlbumName, Save, null, SongManager.AllAlbumNames.ToList());

            void Save(string albumName)
            {
                PopupController.ShowCreateAlbumPopup(albumName, OnSuccess, OnCancel);

                void OnSuccess(int albumArtistID)
                {
                    _currentElement.AlbumID = SongManager.GetAlbumIDByNameAndAuthor(albumName, albumArtistID, true);
                    SongManager.RefreshAlbumLUT();
                    TriggerOnChangeEvents();
                    Refresh();
                }

                void OnCancel() => PopupController.HideHintEditPopup();
            }
        }

        void OpenArtistsEditPopup()
        {
            List<string> artistNames = SongManager.GetArtistNamesByIDs(_currentElement.ArtistIds).ToList();
            PopupController.ShowMultipleEditPopup("Artists", artistNames, Save, null, SongManager.AllArtistNames.ToList());

            void Save(List<string> values)
            {
                var artistIds = SongManager.GetArtistIDsByNames(values);
                _currentElement.ArtistIds = artistIds.ToArray();
                // TODO: ideally should not be here, changing genre should be inside song manager which would handle refreshing luts
                SongManager.RefreshArtistsLUT();
                TriggerOnChangeEvents();
                Refresh();
            }
        }
    }
}