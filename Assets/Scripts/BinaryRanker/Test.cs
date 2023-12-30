using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;
using SM = Immortus.SongRanker.SongManager;
using PrimeTween;
using System;

namespace Immortus.SongRanker
{
    public class Test : MonoBehaviour
    {
        public event Action OnRankingChangedEvent;

        [SerializeField] AudioClipLoader _AudioClipLoader;
        [SerializeField] TextMeshProUGUI _RankingText;
        [SerializeField] TextMeshProUGUI _DebugText;
        [SerializeField] GameObject _RatingPanel;
        [SerializeField] GameObject _RatingHint;
        [SerializeField] RectTransform _SaveInfoPanel;
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
            SM.Init();

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

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S))
                SaveRanking();
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
                    song.SetRating(rating);
                i++;
            }
        }

        void SaveRanking(bool autoSave = false)
        {
            List<List<int>> save = new();

            foreach(var rank in _ranker.Ranking)
            {
                var ids = new List<int>();
                foreach (var song in rank)
                    ids.Add(song.ID);
                save.Add(ids);
            }

            FileSaver.SaveCollection(autoSave ? "Ranking_auto" : "Ranking", save);

            if (autoSave)
                return;

            var seq = Sequence.Create();
            seq.Chain(Tween.UIAnchoredPositionY(_SaveInfoPanel, endValue: -100, duration: 0.5f, ease: Ease.InQuad));
            seq.ChainDelay(0.3f);
            seq.Chain(Tween.UIAnchoredPositionY(_SaveInfoPanel, endValue: 20, duration: 0.5f, ease: Ease.InQuad));
        }

        void LoadRanking()
        {
            var loadedData = FileSaver.LoadCollection<List<int>>("Ranking");

            List<List<Song>> ranking = new();

            foreach (var rank in loadedData)
            {
                var songs = new List<Song>();
                foreach (var id in rank)
                {
                    var song = SM.GetSongWithID(id);
                    if(song != null)
                        songs.Add(song);
                }
                ranking.Add(songs);
            }

            _ranker = new(ranking);
        }

        void SetChoice(ComparisonResult result) => _ranker.SetComparisonResult(result);

        bool LeftOptionChosen() => Input.GetKeyUp(KeyCode.LeftArrow);

        bool RightOptionChosen() => Input.GetKeyUp(KeyCode.RightArrow);

        bool MiddleOptionChosen() => Input.GetKeyUp(KeyCode.UpArrow);

        void OnRankingChanged()
        {
            _options.RemoveAt(0);
            SaveRanking(true);
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
            StringBuilder ranking = new();
            int index = 1;
            int rankingEntries = 0;
            foreach (var item in _ranker.Ranking)
            {
                ranking.Append($"<color=\"yellow\">{index}.</color>");
                foreach (var song in item)
                {
                    ranking.AppendLine(song.Name);
                    rankingEntries++;
                }
                index++;
            }
            _DebugText.text = $"{_ranker.Ranking.Count}/{rankingEntries}/{_options.Count}";
            _RankingText.text = ranking.ToString();
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
            _NewSongAudioPanelController.ChangeSong(song.Path);
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
            if (!SM.LoadData())
            {
                string path = @"D:\Muzyka\Sample Music";
                string[] files = Directory.GetFiles(path, "*.mp3");

                foreach (string file in files)
                {
                    Debug.Log($"Processing {file}");
                    var ret = SM.LoadSong(file);
                    if (ret.Item1 == null)
                        Debug.LogError($"{ret.Item2}");
                }

                SM.SaveDataToFiles();
            }

            LoadRanking();

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
