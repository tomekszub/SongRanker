using System;
using System.Collections.Generic;
using UnityEngine;
using SM = Immortus.SongRanker.SongManager;

namespace Immortus.SongRanker
{
    public class RankerController : MonoBehaviour
    {
        public event Action OnRankingChangedEvent;
        
        Ranker<Song> _ranker;
        RankingsController _rankingsController;
        Dictionary<int, int> _songIdToRankingPosition = new();

        public Ranker<Song> Ranker => _ranker;
        public RankingsController RankingsController => _rankingsController;

        // Start is called before the first frame update
        void Awake()
        {
            SM.Init();

            Init();

            _rankingsController = new(_ranker);
            
            _ranker.OnRankingChanged -= OnRankingChanged;
            _ranker.OnRankingChanged += OnRankingChanged;
        }

        void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S))
                SaveRanking();
        }

        public void RefreshSongRankingLUT()
        {
            int rankingPosition = 0;
            foreach (var rank in _ranker.Ranking)
            {
                rankingPosition++;
                foreach (var song in rank)
                {
                    _songIdToRankingPosition[song.ID] = rankingPosition;
                }
            }
        }

        public int GetRankingPosition(int songID)
        {
            _songIdToRankingPosition.TryGetValue(songID, out int rankingPos);
            return rankingPos;
        }
        
        void Init()
        {
            SM.LoadData();

            SM.SearchForNewMusic(ResultCallback);

            SetupRanker();

            void ResultCallback(int newSongs, string[] errors)
            {
                if (newSongs == 0 && errors.Length == 0)
                    return;

                PopupController.ShowSongLoadingResult(newSongs, errors);
            }
        }

        void SaveRanking(bool autoSave = false)
        {
            List<List<int>> save = new();

            foreach (var rank in _ranker.Ranking)
            {
                var ids = new List<int>();
                foreach (var song in rank)
                    ids.Add(song.ID);
                save.Add(ids);
            }

            FileSaver.SaveCollection(autoSave ? "Ranking_auto" : "Ranking", save);

            if (autoSave)
                return;

            TabController.ShowSavePanel();
        }
        
        void OnRankingChanged()
        {
            SaveRanking(true);
            RefreshSongRankingLUT();
            OnRankingChangedEvent?.Invoke();
        }

        void SetupRanker()
        {
            var loadedData = FileSaver.LoadCollection<List<int>>("Ranking");

            List<List<Song>> ranking = new();

            foreach (var rank in loadedData)
            {
                var songs = new List<Song>();
                foreach (var id in rank)
                {
                    var song = SM.GetSongWithID(id);
                    if (song != null)
                        songs.Add(song);
                }
                ranking.Add(songs);
            }

            _ranker = new(ranking);
        }
    }
}