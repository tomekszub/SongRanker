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
        const string ARTIST_SONGS_COUNT_RANKING = "ArtistSongCountRanking";
        const string ARTIST_CUSTOM_RATING_RANKING = "ArtistCustomRatingRanking";
        const string GENRE_SONGS_COUNT_RANKING = "GenreSongCountRanking";
        const string GENRE_CUSTOM_RATING_RANKING = "GenreCustomRatingRanking";

        [SerializeField] TextMeshProUGUI _RankingText;
        [SerializeField] Test _RankerTab;

        Dictionary<string, string> _cachedRankings = new();
        List<(Artist artist, int songCount, float avgRating, float customRating)> _artistRanking;
        List<(Genre genre, int songCount, float avgRating)> _genreRanking;
        bool _isRankingEstablished = false;
        HashSet<string> _rankingDirtyLists = new();
        Dictionary<string, Action> _listToRefreshFunction = new();
        string _currList = ARTISTS;

        void Awake()
        {
            _RankerTab.OnRankingChangedEvent += SetDirty;
            _listToRefreshFunction.Add(ARTISTS, RefreshArtistList);
            _listToRefreshFunction.Add(GENRES, RefreshGenreList);
            ResetDirtyLists();

            void SetDirty()
            {
                _isRankingEstablished = false;
                ResetDirtyLists();
                _cachedRankings.Clear();
            }

            void ResetDirtyLists()
            {
                _rankingDirtyLists.Add(ARTISTS);
                _rankingDirtyLists.Add(GENRES);
            }
        }

        void OnEnable()
        {
            RefreshCurrentListIfNeeded();
        }

        void RefreshCurrentListIfNeeded()
        {
            if (!_isRankingEstablished)
                _RankerTab.EstablishRating();

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
                _artistRanking.Add((artist, ratedSongs, ratingSum / ratedSongs, (ratingSum / ratedSongs) + ((ratedSongs - 1) * 5)));
            }

            _rankingDirtyLists.Remove(ARTISTS);
            _cachedRankings.Remove(ARTIST_SONGS_COUNT_RANKING);
            _cachedRankings.Remove(ARTIST_CUSTOM_RATING_RANKING);
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
                _genreRanking.Add((genre, ratedSongs, ratingSum / ratedSongs));
            }

            _rankingDirtyLists.Remove(GENRES);
            _cachedRankings.Remove(GENRE_SONGS_COUNT_RANKING);
            _cachedRankings.Remove(GENRE_CUSTOM_RATING_RANKING);
        }

        public void ShowArtistCustomRatingRanking()
        {
            _currList = ARTISTS;

            if (_cachedRankings.TryGetValue(ARTIST_CUSTOM_RATING_RANKING, out string cachedData))
            {
                _RankingText.text = cachedData;
                return;
            }

            RefreshCurrentListIfNeeded();

            var sortedRanking = _artistRanking.OrderByDescending(s => s.customRating);

            StringBuilder sb = new();
            int artistNameLength;

            foreach (var (artist, songCount, avgRating, customRating) in sortedRanking)
            {
                artistNameLength = artist.Name.Length;
                sb.Append($"{artist.Name}");
                for (int i = 0; i < 35 - artistNameLength; i++)
                    sb.Append(" ");
                sb.AppendLine($"{songCount}\t\t{Math.Round(avgRating, 2)}\t\t{Math.Round(customRating, 2)}");
            }

            _cachedRankings.Add(ARTIST_CUSTOM_RATING_RANKING, sb.ToString());
            _RankingText.text = _cachedRankings[ARTIST_CUSTOM_RATING_RANKING];
        }

        public void ShowArtistSongCountRanking()
        {
            _currList = ARTISTS;

            if (_cachedRankings.TryGetValue(ARTIST_SONGS_COUNT_RANKING, out string cachedData))
            {
                _RankingText.text = cachedData;
                return;
            }

            RefreshCurrentListIfNeeded();

            var sortedRanking = _artistRanking.OrderByDescending(s => s.songCount);

            StringBuilder sb = new();
            int artistNameLength;

            foreach (var item in sortedRanking)
            {
                artistNameLength = item.artist.Name.Length;
                sb.Append($"{item.artist.Name}");
                for (int i = 0; i < 35 - artistNameLength; i++)
                    sb.Append(" ");
                sb.AppendLine($"{item.songCount}");
            }

            _cachedRankings.Add(ARTIST_SONGS_COUNT_RANKING, sb.ToString());
            _RankingText.text = _cachedRankings[ARTIST_SONGS_COUNT_RANKING];
        }

        public void ShowGenreSongCountRanking()
        {
            _currList = GENRES;

            if (_cachedRankings.TryGetValue(GENRE_SONGS_COUNT_RANKING, out string cachedData))
            {
                _RankingText.text = cachedData;
                return;
            }

            RefreshCurrentListIfNeeded();

            var sortedRanking = _genreRanking.OrderByDescending(g => g.songCount);

            StringBuilder sb = new();
            int genreNameLength;

            foreach (var item in sortedRanking)
            {
                genreNameLength = item.genre.Name.Length;
                sb.Append($"{item.genre.Name}");
                for (int i = 0; i < 35 - genreNameLength; i++)
                    sb.Append(" ");
                sb.AppendLine($"{item.songCount}");
            }

            _cachedRankings.Add(GENRE_SONGS_COUNT_RANKING, sb.ToString());
            _RankingText.text = _cachedRankings[GENRE_SONGS_COUNT_RANKING];
        }

        public void ShowGenreCustomRatingRanking()
        {
            _currList = GENRES;

            if (_cachedRankings.TryGetValue(GENRE_CUSTOM_RATING_RANKING, out string cachedData))
            {
                _RankingText.text = cachedData;
                return;
            }

            RefreshCurrentListIfNeeded();

            var sortedRanking = _genreRanking.OrderByDescending(g => g.avgRating);

            StringBuilder sb = new();
            int artistNameLength;

            foreach (var item in sortedRanking)
            {
                artistNameLength = item.genre.Name.Length;
                sb.Append($"{item.genre.Name}");
                for (int i = 0; i < 35 - artistNameLength; i++)
                    sb.Append(" ");
                sb.AppendLine($"{item.songCount}\t\t{Math.Round(item.avgRating, 2)}");
            }

            _cachedRankings.Add(GENRE_CUSTOM_RATING_RANKING, sb.ToString());
            _RankingText.text = _cachedRankings[GENRE_CUSTOM_RATING_RANKING];
        }
    }
}
