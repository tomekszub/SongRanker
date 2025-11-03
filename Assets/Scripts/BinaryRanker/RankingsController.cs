using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Immortus.SongRanker
{
    public class RankingsController
    {
        public enum RankingType
        {
            Artist,
            Genre,
            Language,
            Album
        }
        
        public event Action<RankingType> OnRankingChangedForType;
        
        Ranker<Song> _ranker;
        
        Dictionary<RankingType, Action> _listToRefreshFunction = new();
        Dictionary<RankingType, IEnumerable<IRankingObject<IRankable>>> _rankingPerType = new();
        HashSet<RankingType> _rankingDirtyLists = new();
        List<RankingObject<Artist>> _artistRanking = new();
        List<RankingObject<Genre>> _genreRanking = new();
        List<RankingObject<Language>> _languageRanking = new();
        List<RankingObject<Album>> _albumRanking = new();
        
        bool _isRankingEstablished = false;
        
        public RankingsController(Ranker<Song> ranker)
        {
            _ranker = ranker;
            Init();
        }
        
        void Init()
        {
            _ranker.OnRankingChanged -= SetDirty;
            _ranker.OnRankingChanged += SetDirty;
            EditingTab.OnChangeDone -= SetDirty;
            EditingTab.OnChangeDone += SetDirty;
            
            SongManager.OnLanguageToSongChanged -= RefreshLanguageList;
            SongManager.OnLanguageToSongChanged += RefreshLanguageList;
            SongManager.OnArtistToSongChanged -= RefreshArtistList;
            SongManager.OnArtistToSongChanged += RefreshArtistList;
            SongManager.OnAlbumToSongChanged -= RefreshAlbumList;
            SongManager.OnAlbumToSongChanged += RefreshAlbumList;
            SongManager.OnGenreToSongChanged -= RefreshGenreList;
            SongManager.OnGenreToSongChanged += RefreshGenreList;
            
            _listToRefreshFunction.Add(RankingType.Artist, RefreshArtistList);
            _listToRefreshFunction.Add(RankingType.Genre, RefreshGenreList);
            _listToRefreshFunction.Add(RankingType.Language, RefreshLanguageList);
            _listToRefreshFunction.Add(RankingType.Album, RefreshAlbumList);
            
            _rankingPerType.Add(RankingType.Artist, _artistRanking);
            _rankingPerType.Add(RankingType.Genre, _genreRanking);
            _rankingPerType.Add(RankingType.Language, _languageRanking);
            _rankingPerType.Add(RankingType.Album, _albumRanking);
            
            ResetDirtyLists();

            void SetDirty()
            {
                _isRankingEstablished = false;
                ResetDirtyLists();

                if(OnRankingChangedForType == null) 
                    return;

                OnRankingChangedForType.Invoke(RankingType.Artist);
                OnRankingChangedForType.Invoke(RankingType.Genre);
                OnRankingChangedForType.Invoke(RankingType.Language);
                OnRankingChangedForType.Invoke(RankingType.Album);
            }

            void ResetDirtyLists()
            {
                _rankingDirtyLists.Add(RankingType.Artist);
                _rankingDirtyLists.Add(RankingType.Genre);
                _rankingDirtyLists.Add(RankingType.Language);
                _rankingDirtyLists.Add(RankingType.Album);
            }
        }

        public IEnumerable<IRankingObject<IRankable>> GetCurrentRanking(RankingType type)
        {
            Refresh(type);

            if(_rankingPerType.TryGetValue(type, out var ranking))
                return ranking;
            
            return Enumerable.Empty<IRankingObject<IRankable>>();
        }
        
        public void Refresh(RankingType type)
        {
            if (!_isRankingEstablished)
            {
                EstablishRating();
                _isRankingEstablished = true;
            }

            if (_rankingDirtyLists.Contains(type))
                _listToRefreshFunction[type].Invoke();
        }
        
        void EstablishRating()
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
        
        void RefreshArtistList()
        {
            _artistRanking.Clear();
            var artistToSongs = SongManager.ArtistToSongs;

            foreach (var kvp in artistToSongs)
            {
                float ratingSum = 0;
                int ratedSongs = 0;
                foreach (var song in kvp.Value) 
                {
                    if (song.Rating > 0)
                    {
                        ratingSum += song.Rating;
                        ratedSongs++;
                    }
                }
                var artist = SongManager.GetArtistByID(kvp.Key);
                if (artist == null)
                {
                    Debug.LogError($"Artist with id {kvp.Key} deos not exist or is null!");
                    continue;
                }
                _artistRanking.Add(new(artist, ratedSongs, ratingSum / ratedSongs, (ratingSum / ratedSongs) + ((ratedSongs - 1) * 4.5f)));
            }

            _rankingDirtyLists.Remove(RankingType.Artist);
            OnRankingChangedForType?.Invoke(RankingType.Artist);
        }
        
        void RefreshGenreList()
        {
            _genreRanking.Clear();
            var genresToSongs = SongManager.GenreToSongs;

            foreach (var kvp in genresToSongs)
            {
                float ratingSum = 0;
                int ratedSongs = 0;
                foreach (var song in kvp.Value)
                {
                    if (song.Rating > 0)
                    {
                        ratingSum += song.Rating;
                        ratedSongs++;
                    }
                }
                var genre = SongManager.GetGenreByID(kvp.Key);
                if (genre == null)
                {
                    Debug.LogError($"Genre with id {kvp.Key} deos not exist or is null!");
                    continue;
                }
                _genreRanking.Add(new(genre, ratedSongs, ratingSum / ratedSongs, (ratingSum / ratedSongs) + ((ratedSongs - 1) * 4.5f)));
            }

            _rankingDirtyLists.Remove(RankingType.Genre);
            OnRankingChangedForType?.Invoke(RankingType.Genre);
        }

        void RefreshLanguageList()
        {
            _languageRanking.Clear();
            var languagesToSongs = SongManager.LanguageToSongs;

            foreach (var kvp in languagesToSongs)
            {
                float ratingSum = 0;
                int ratedSongs = 0;
                foreach (var song in kvp.Value)
                {
                    if (song.Rating > 0)
                    {
                        ratingSum += song.Rating;
                        ratedSongs++;
                    }
                }

                var language = SongManager.GetLanguageByID(kvp.Key);
                if (language == null)
                {
                    Debug.LogError($"Language with id {kvp.Key} deos not exist or is null!");
                    continue;
                }
                _languageRanking.Add(new(language, ratedSongs, ratingSum / ratedSongs, (ratingSum / ratedSongs) + ((ratedSongs - 1) * 4.5f)));
            }

            _rankingDirtyLists.Remove(RankingType.Language);
            OnRankingChangedForType?.Invoke(RankingType.Language);
        }

        void RefreshAlbumList()
        {
            _albumRanking.Clear();
            var albumToSongs = SongManager.AlbumToSongs;

            foreach (var kvp in albumToSongs)
            {
                float ratingSum = 0;
                int ratedSongs = 0;
                foreach (var song in kvp.Value)
                {
                    if (song.Rating > 0)
                    {
                        ratingSum += song.Rating;
                        ratedSongs++;
                    }
                }

                var album = SongManager.GetAlbumByID(kvp.Key);
                if (album == null)
                {
                    Debug.LogError($"Album with id {kvp.Key} deos not exist or is null!");
                    continue;
                }
                _albumRanking.Add(new(album, ratedSongs, ratingSum / ratedSongs, (ratingSum / ratedSongs) + ((ratedSongs - 1) * 4.5f)));
            }

            _rankingDirtyLists.Remove(RankingType.Album);
            OnRankingChangedForType?.Invoke(RankingType.Album);
        }
    }
}
