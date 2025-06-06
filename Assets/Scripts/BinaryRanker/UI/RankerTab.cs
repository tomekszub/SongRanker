using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;
using SM = Immortus.SongRanker.SongManager;
using PrimeTween;
using System;
using UnityEngine.Serialization;

namespace Immortus.SongRanker
{
    public class RankerTab : MonoBehaviour
    {
        public event Action OnRankingChangedEvent;

        [SerializeField] RankerController _RankerController;
        [SerializeField] AudioClipLoader _AudioClipLoader;
        [SerializeField] RecyclableVerticalScrollView _RankingView;
        [SerializeField] TextMeshProUGUI _DebugText;
        [SerializeField] GameObject _RatingPanel;
        [SerializeField] GameObject _RatingHint;
        [SerializeField] ConfirmationPopup _ConfirmationPopup;
        [Header("Compared To Element")]
        [SerializeField] TextMeshProUGUI _ComparedToElementText;
        [SerializeField] TextMeshProUGUI _ComparedToElementArtistText;
        [SerializeField] TextMeshProUGUI _ComparedToElementRankingText;
        [SerializeField] AudioPanelController _ComparedToSongAudioPanelController;
        [Header("New Element")]
        [SerializeField] TextMeshProUGUI _NewElementText;
        [SerializeField] TextMeshProUGUI _NewElementArtistText;
        [SerializeField] AudioPanelController _NewSongAudioPanelController;

        Ranker<Song> _ranker;
        List<Song> _options;
        Song _currentlyComparedSong, _newSong;

        public Ranker<Song> Ranker => _ranker;

        void Start()
        {

            Init();

            _ranker.OnRankingChanged -= OnRankingChanged;
            _ranker.OnRankingChanged += OnRankingChanged;
            _ranker.OnCurrentlyComapredElementChanged -= OnCurrentlyComparedSongChanged;
            _ranker.OnCurrentlyComapredElementChanged += OnCurrentlyComparedSongChanged;

            Debug.Log("Init done...");
        }

        // Update is called once per frame
        void Update()
        {
            if (RightOptionChosen())
                SetChoice(ComparisonResult.Worse);
            else if (LeftOptionChosen())
                SetChoice(ComparisonResult.Better);
            else if (MiddleOptionChosen())
                SetChoice(ComparisonResult.Same);
            else if (Input.GetKeyUp(KeyCode.Space))
                NextOption();

            if(Input.GetKeyUp(KeyCode.Backspace))
                _ranker.Undo();
        }

        public void OnChoiceButtonClicked(int choiceIndex) => SetChoice((ComparisonResult)choiceIndex);

        public void EstablishRating()
        {
            var ranking = _ranker.Ranking;
            float interval = 100.0f / ranking.Count;
            interval -= interval / 10;

            int i = 0;

            foreach (var songs in ranking)
            {
                float rating = 100.0f - (i * interval);
                foreach(var song in songs)
                    song.Rating = rating;
                i++;
            }
        }

        void SetChoice(ComparisonResult result) => _ranker.SetComparisonResult(result);

        bool LeftOptionChosen() => Input.GetKeyUp(KeyCode.LeftArrow);

        bool RightOptionChosen() => Input.GetKeyUp(KeyCode.RightArrow);

        bool MiddleOptionChosen() => Input.GetKeyUp(KeyCode.UpArrow);

        void OnRankingChanged()
        {
            _options.RemoveAt(0);
            _RankerController.SaveRanking(true);

            _RankerController.RefreshSongRankingLUT();

            RefreshUI();
            OnRankingChangedEvent?.Invoke();
        }

        public void NextOption()
        {
            if (!NextElementAvailable())
                return;

            if (!_ranker.AddElement(_options[0]))
                Debug.LogError("Updating ranking in progress...");

            _RatingHint.SetActive(false);
            _RatingPanel.SetActive(true);
        }

        public void TryRemoveNewSong()
        {
            _ConfirmationPopup.Show("Are you sure you want to delete this song?<br>Note that this will not remove it from disk!", OnConfirmed, null);

            void OnConfirmed()
            {
                SM.RemoveSong(_newSong);
                _options.RemoveAt(0);
                RefreshUI();
                _ranker.Stop();
            }
        }
        
        void RefreshUI()
        {
            DisplayCurrentRanking();
            SetCurrentlyComparedSongUI(null);
            _RatingPanel.SetActive(false);

            if (_options.Count == 0)
                _NewElementText.text = "Done";
            else
            {
                _RatingHint.SetActive(true);
                ChangeNewSong(_options[0]);
            }
        }

        void DisplayCurrentRanking()
        {
            List<string> ranking = new();
            int index = 1;
            foreach (var item in _ranker.Ranking)
            {
                var positionString = $"<color=\"yellow\">{index}.</color>";

                for(var i = 0; i < item.Count; i++)
                {
                    var song = item[i];
                    
                    if(i == 0)
                        ranking.Add($"{positionString} {song.Name}");
                    else
                        ranking.Add(song.Name);
                }

                index++;
            }
            _DebugText.text = $"{_ranker.Ranking.Count}/{ranking.Count}/{_options.Count}";
            _RankingView.RefreshData(ranking);
        }

        bool NextElementAvailable() => _options.Count > 0;

        void OnCurrentlyComparedSongChanged(List<Song> currSongs)
        {
            _currentlyComparedSong = currSongs[0];
            _ComparedToSongAudioPanelController.ChangeSong(currSongs[0].Path);
            SetCurrentlyComparedSongUI(currSongs[0]);
        }

        void ChangeNewSong(Song song)
        {
            _newSong = song;
            _NewSongAudioPanelController.ChangeSong(song.Path, true);
            SetNewSongUI(song);
        }

        void SetCurrentlyComparedSongUI(Song song)
        {
            if(song == null)
            {
                ResetComparedToUI();
                return;
            }

            _ComparedToElementText.text = song.Name;

            _ComparedToElementArtistText.text = string.Join(", ", SM.GetArtistNamesByIDs(song.ArtistIds));

            _ComparedToSongAudioPanelController.SetActive(true);

            var ranking = _ranker.Ranking;
            for (int i = 0; i < _ranker.Ranking.Count; i++)
            {
                foreach(var songFromRanking in ranking[i])
                {
                    if(songFromRanking.ID == song.ID)
                    {
                        _ComparedToElementRankingText.text = $"{i + 1}";
                        return;
                    }
                }
            }
        }

        void SetNewSongUI(Song song)
        {
            _NewElementText.text = song.Name;

            _NewElementArtistText.text = string.Join(", ", SM.GetArtistNamesByIDs(song.ArtistIds));
        }

        void ResetComparedToUI()
        {
            _ComparedToElementText.text = "-";
            _ComparedToElementArtistText.text = "";
            _ComparedToElementRankingText.text = "";
            _ComparedToSongAudioPanelController.SetActive(false);
        }

        void Init()
        {
            _ranker = _RankerController.Ranker;

            _options = SM.GetAllSongs().ToList();

            if (_ranker.Ranking.Count > 0)
            {
                foreach (var rank in _ranker.Ranking)
                {
                    foreach (var song in rank)
                    {
                        _options.Remove(song);
                    }
                }
                _RankerController.RefreshSongRankingLUT();
                RefreshUI();
            }
            else
            {
                NextOption();
                NextOption();
            }

            _NewSongAudioPanelController.Init(_ComparedToSongAudioPanelController);
            _ComparedToSongAudioPanelController.Init(_NewSongAudioPanelController);
        }
    }
}
