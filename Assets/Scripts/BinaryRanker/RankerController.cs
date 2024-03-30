using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SM = Immortus.SongRanker.SongManager;

namespace Immortus.SongRanker
{
    public class RankerController : MonoBehaviour
    {
        [SerializeField] string _MusicPath = @"D:\Muzyka\Sample Music";

        Ranker<Song> _ranker;

        public Ranker<Song> Ranker => _ranker;

        // Start is called before the first frame update
        void Awake()
        {
            SM.Init();

            Init();
        }

        void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S))
                SaveRanking();
        }

        public void SaveRanking(bool autoSave = false)
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

        void Init()
        {
            if (!SM.LoadData())
            {
                string[] files = Directory.GetFiles(_MusicPath, "*.mp3");

                foreach (string file in files)
                {
                    Debug.Log($"Processing {file}");
                    var ret = SM.LoadSong(file);
                    if (ret.Item1 == null)
                        Debug.LogError($"{ret.Item2} is null!");
                }

                SM.SaveDataToFiles();
            }

            LoadRanking();
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
                    if (song != null)
                        songs.Add(song);
                }
                ranking.Add(songs);
            }

            _ranker = new(ranking);
        }
    }
}