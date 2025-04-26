using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;
using System;

namespace Immortus.SongRanker
{
    public class RankingTab : MonoBehaviour
    {
        const string ARTISTS = "Artists";
        const string GENRES = "Genres";
        const string LANGUAGE = "Language";
        const string ALBUM = "Album";
        const string ARTIST_SONGS_COUNT_RANKING = "ArtistSongCountRanking";
        const string GENRE_SONGS_COUNT_RANKING = "GenreSongCountRanking";
        const string LANGUAGE_SONGS_COUNT_RANKING = "LanguageSongCountRanking";
        const string ALBUM_SONGS_COUNT_RANKING = "AlbumSongCountRanking";
        const string ARTIST_CUSTOM_RATING_RANKING = "ArtistCustomRatingRanking";
        const string GENRE_CUSTOM_RATING_RANKING = "GenreCustomRatingRanking";
        const string LANGUAGE_CUSTOM_RATING_RANKING = "LanguageCustomRatingRanking";
        const string ALBUM_CUSTOM_RATING_RANKING = "AlbumCustomRatingRanking";

        [SerializeField] TextMeshProUGUI _RankingText;
        [SerializeField] RankerTab _RankerTab;
        [SerializeField] EditingTab _EditingTab;

        Dictionary<string, string> _cachedRankingOutputs = new();
        List<RankingObject<Artist>> _artistRanking = new();
        List<RankingObject<Genre>> _genreRanking = new();
        List<RankingObject<Language>> _languageRanking = new();
        List<RankingObject<Album>> _albumRanking = new();
        bool _isRankingEstablished = false;
        HashSet<string> _rankingDirtyLists = new();
        Dictionary<string, Action> _listToRefreshFunction = new();
        string _currList = ARTISTS;

        void Awake()
        {
            // TODO: in no universe ranker tab should know about other tabs, move this events to some other place
            _RankerTab.OnRankingChangedEvent += SetDirty;
            _RankerTab.OnRankingChangedEvent -= SetDirty;
            _EditingTab.OnChangeDone -= SetDirty;
            _EditingTab.OnChangeDone += SetDirty;
            SongManager.OnLanguageToSongChanged -= RefreshLanguageList;
            SongManager.OnLanguageToSongChanged += RefreshLanguageList;
            SongManager.OnArtistToSongChanged -= RefreshArtistList;
            SongManager.OnArtistToSongChanged += RefreshArtistList;
            SongManager.OnAlbumToSongChanged -= RefreshAlbumList;
            SongManager.OnAlbumToSongChanged += RefreshAlbumList;
            SongManager.OnGenreToSongChanged -= RefreshGenreList;
            SongManager.OnGenreToSongChanged += RefreshGenreList;
            
            _listToRefreshFunction.Add(ARTISTS, RefreshArtistList);
            _listToRefreshFunction.Add(GENRES, RefreshGenreList);
            _listToRefreshFunction.Add(LANGUAGE, RefreshLanguageList);
            _listToRefreshFunction.Add(ALBUM, RefreshAlbumList);
            ResetDirtyLists();

            void SetDirty()
            {
                _isRankingEstablished = false;
                ResetDirtyLists();
                _cachedRankingOutputs.Clear();
            }

            void ResetDirtyLists()
            {
                _rankingDirtyLists.Add(ARTISTS);
                _rankingDirtyLists.Add(GENRES);
                _rankingDirtyLists.Add(LANGUAGE);
                _rankingDirtyLists.Add(ALBUM);
            }
        }

        void OnEnable() => RefreshCurrentListIfNeeded();

        void RefreshCurrentListIfNeeded()
        {
            if (!_isRankingEstablished)
            {
                _RankerTab.EstablishRating();
                _isRankingEstablished = true;
            }

            if (_rankingDirtyLists.Contains(_currList))
                _listToRefreshFunction[_currList].Invoke();
        }

        void RefreshArtistList()
        {
            _artistRanking = new();
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

            _rankingDirtyLists.Remove(ARTISTS);
            _cachedRankingOutputs.Remove(ARTIST_SONGS_COUNT_RANKING);
            _cachedRankingOutputs.Remove(ARTIST_CUSTOM_RATING_RANKING);
        }

        void RefreshGenreList()
        {
            _genreRanking = new();
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

            _rankingDirtyLists.Remove(GENRES);
            _cachedRankingOutputs.Remove(GENRE_SONGS_COUNT_RANKING);
            _cachedRankingOutputs.Remove(GENRE_CUSTOM_RATING_RANKING);
        }

        void RefreshLanguageList()
        {
            _languageRanking = new();
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

            _rankingDirtyLists.Remove(LANGUAGE);
            _cachedRankingOutputs.Remove(LANGUAGE_SONGS_COUNT_RANKING);
            _cachedRankingOutputs.Remove(LANGUAGE_CUSTOM_RATING_RANKING);
        }

        void RefreshAlbumList()
        {
            _albumRanking = new();
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

            _rankingDirtyLists.Remove(ALBUM);
            _cachedRankingOutputs.Remove(ALBUM_SONGS_COUNT_RANKING);
            _cachedRankingOutputs.Remove(ALBUM_CUSTOM_RATING_RANKING);
        }

        public void ShowArtistSongCountRanking()
        {
            ShowCustomRanking(ARTISTS, ARTIST_SONGS_COUNT_RANKING, ref _artistRanking, a => a.SongCount);
        }

        public void ShowArtistCustomRatingRanking()
        {
            ShowCustomRanking(ARTISTS, ARTIST_CUSTOM_RATING_RANKING, ref _artistRanking, a => a.CustomRating);
        }

        public void ShowGenreSongCountRanking()
        {
            ShowCustomRanking(GENRES, GENRE_SONGS_COUNT_RANKING, ref _genreRanking, g => g.SongCount);
        }

        public void ShowGenreCustomRatingRanking()
        {
            ShowCustomRanking(GENRES, GENRE_CUSTOM_RATING_RANKING, ref _genreRanking, g => g.CustomRating);
        }

        public void ShowLanguageSongCountRanking()
        {
            ShowCustomRanking(LANGUAGE, LANGUAGE_SONGS_COUNT_RANKING, ref _languageRanking, g => g.SongCount);
        }

        public void ShowLanguageCustomRatingRanking()
        {
            ShowCustomRanking(LANGUAGE, LANGUAGE_CUSTOM_RATING_RANKING, ref _languageRanking, g => g.CustomRating);
        }

        public void ShowAlbumSongCountRanking()
        {
            ShowCustomRanking(ALBUM, ALBUM_SONGS_COUNT_RANKING, ref _albumRanking, g => g.SongCount);
        }

        public void ShowAlbumCustomRatingRanking()
        {
            ShowCustomRanking(ALBUM, ALBUM_CUSTOM_RATING_RANKING, ref _albumRanking, g => g.CustomRating);
        }

        void ShowCustomRanking<T>(string listName, string rankingKey, ref List<RankingObject<T>> ranking, Func<RankingObject<T>, double> orderFunc) where T : IRankable
        {
            _currList = listName;

            if (_cachedRankingOutputs.TryGetValue(rankingKey, out string cachedData))
            {
                _RankingText.text = cachedData;
                return;
            }

            RefreshCurrentListIfNeeded();

            var sortedRanking = ranking.OrderByDescending(orderFunc);

            StringBuilder sb = new();

            int nameLength;
            int songCountLength;
            string avgRatingString;

            foreach (var item in sortedRanking)
            {
                var name = item.Value.GetDisplayName();
                nameLength = name.Length;
                songCountLength = item.SongCount.ToString().Length;
                sb.Append($"{name}");
                for (int i = 0; i < 35 - nameLength; i++)
                    sb.Append(" ");
                sb.Append($"{item.SongCount}");
                for (int i = 0; i < 6 - songCountLength; i++)
                    sb.Append(" ");
                avgRatingString = $"{Math.Round(item.AvgRating, 2)}";
                sb.Append(avgRatingString);
                for (int i = 0; i < 10 - avgRatingString.Length; i++)
                    sb.Append(" ");
                sb.Append($"{Math.Round(item.CustomRating, 2)}");
                sb.AppendLine();
            }

            _cachedRankingOutputs.Add(rankingKey, sb.ToString());
            _RankingText.text = _cachedRankingOutputs[rankingKey];
        }
    }

    class RankingObject<T> where T : IRankable
    {
        public T Value;
        public int SongCount;
        public float AvgRating;
        public float CustomRating;

        public RankingObject(T value, int songCount, float avgRating, float customRating)
        {
            Value = value;
            SongCount = songCount;
            AvgRating = avgRating;
            CustomRating = customRating;
        }
    }
}
