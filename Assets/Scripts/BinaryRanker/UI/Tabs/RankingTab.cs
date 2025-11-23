using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;
using System;
using static Immortus.SongRanker.RankingsController;

namespace Immortus.SongRanker
{
    public class RankingTab : BaseTab
    {
        const string ARTIST_SONGS_COUNT_RANKING = "ArtistSongCountRanking";
        const string GENRE_SONGS_COUNT_RANKING = "GenreSongCountRanking";
        const string LANGUAGE_SONGS_COUNT_RANKING = "LanguageSongCountRanking";
        const string ALBUM_SONGS_COUNT_RANKING = "AlbumSongCountRanking";
        const string ARTIST_CUSTOM_RATING_RANKING = "ArtistCustomRatingRanking";
        const string GENRE_CUSTOM_RATING_RANKING = "GenreCustomRatingRanking";
        const string LANGUAGE_CUSTOM_RATING_RANKING = "LanguageCustomRatingRanking";
        const string ALBUM_CUSTOM_RATING_RANKING = "AlbumCustomRatingRanking";

        static readonly Dictionary<RankingType, List<string>> CACHED_RANKINGS_LUT = new()
        {
            {
                RankingType.Artist,
                new()
                {
                    ARTIST_SONGS_COUNT_RANKING,
                    ARTIST_CUSTOM_RATING_RANKING
                }
            },
            {
                RankingType.Genre,
                new ()
                {
                    GENRE_SONGS_COUNT_RANKING,
                    GENRE_CUSTOM_RATING_RANKING
                }
            },
            {
                RankingType.Language,
                new()
                {
                    LANGUAGE_SONGS_COUNT_RANKING,
                    LANGUAGE_CUSTOM_RATING_RANKING
                }
            },
            {
                RankingType.Album,
                new()
                {
                    ALBUM_SONGS_COUNT_RANKING,
                    ALBUM_CUSTOM_RATING_RANKING
                }
            }
        };
        
        [SerializeField] RecyclableVerticalScrollView _RankingView;
        [SerializeField] RankerController _RankerController;
        [SerializeField] EditingTab _EditingTab;
        
        Dictionary<string, List<IRecyclableData>> _cachedRankingOutputs = new();
        RankingsController _rankingsController;
        RankingType _currRankingType = RankingType.Artist;

        void Start()
        {
            _rankingsController = _RankerController.RankingsController;
            _rankingsController.OnRankingChangedForType -= OnRankingChangedForType;
            _rankingsController.OnRankingChangedForType += OnRankingChangedForType;
            
            EnsureCurrentRankingFreshness();
        }

        void OnEnable() => EnsureCurrentRankingFreshness();

        public void ShowArtistSongCountRanking()
        {
            ShowCustomRanking(RankingType.Artist, ARTIST_SONGS_COUNT_RANKING, a => a.SongCount);
        }

        public void ShowArtistCustomRatingRanking()
        {
            ShowCustomRanking(RankingType.Artist, ARTIST_CUSTOM_RATING_RANKING, a => a.CustomRating);
        }

        public void ShowGenreSongCountRanking()
        {
            ShowCustomRanking(RankingType.Genre, GENRE_SONGS_COUNT_RANKING, g => g.SongCount);
        }

        public void ShowGenreCustomRatingRanking()
        {
            ShowCustomRanking(RankingType.Genre, GENRE_CUSTOM_RATING_RANKING, g => g.CustomRating);
        }

        public void ShowLanguageSongCountRanking()
        {
            ShowCustomRanking(RankingType.Language, LANGUAGE_SONGS_COUNT_RANKING, g => g.SongCount);
        }

        public void ShowLanguageCustomRatingRanking()
        {
            ShowCustomRanking(RankingType.Language, LANGUAGE_CUSTOM_RATING_RANKING, g => g.CustomRating);
        }

        public void ShowAlbumSongCountRanking()
        {
            ShowCustomRanking(RankingType.Album, ALBUM_SONGS_COUNT_RANKING, g => g.SongCount);
        }

        public void ShowAlbumCustomRatingRanking()
        {
            ShowCustomRanking(RankingType.Album, ALBUM_CUSTOM_RATING_RANKING, g => g.CustomRating);
        }

        void EnsureCurrentRankingFreshness() => _rankingsController?.Refresh(_currRankingType);

        void OnRankingChangedForType(RankingType type)
        {
            if(CACHED_RANKINGS_LUT.TryGetValue(type, out var list))
                list.ForEach(rankingName => _cachedRankingOutputs.Remove(rankingName));
        }
        
        void ShowCustomRanking(RankingType type, string rankingKey, Func<IRankingObject<IRankable>, double> orderFunc)
        {
            _currRankingType = type;
            
            EnsureCurrentRankingFreshness();

            if (_cachedRankingOutputs.TryGetValue(rankingKey, out var cachedData))
            {
                _RankingView.RefreshData(cachedData);
                Debug.Log($"[RankingTab] Custom ranking taken from cache. Type {type}, Key {rankingKey}");
                return;
            }

            Debug.Log($"[RankingTab] Updating cache with new ranking. Type {type}, Key {rankingKey}");
            
            var ranking = _rankingsController.GetCurrentRanking(_currRankingType);

            var sortedRanking = ranking.OrderByDescending(orderFunc);

            List<IRecyclableData> outputRanking = new();
            
            int rankingPlace = 1;

            foreach (var item in sortedRanking)
            {
                outputRanking.Add(new RankingObjectData($"{rankingPlace}. {item.Value.GetDisplayName()}", item.SongCount.ToString(),
                    item.AvgRating.ToString("F2"), item.CustomRating.ToString("F2")));
                rankingPlace++;
            }

            _cachedRankingOutputs.Add(rankingKey, outputRanking);
            _RankingView.RefreshData(outputRanking);
        }
    }

    public interface IRankingObject<out T> where T : IRankable
    {
        public T Value { get; }
        public int SongCount { get; }
        public float AvgRating { get; }
        public float CustomRating { get; }
    }
    
    public class RankingObject<T> : IRankingObject<T> where T : IRankable
    {
        public T Value { get; }
        public int SongCount { get; }
        public float AvgRating { get; }
        public float CustomRating { get; }

        public RankingObject(T value, int songCount, float avgRating, float customRating)
        {
            Value = value;
            SongCount = songCount;
            AvgRating = avgRating;
            CustomRating = customRating;
        }
    }

    public class RankingObjectData : IRecyclableData
    {
        public string Name { get; }
        public string SongCount { get; }
        public string AvgRating { get; }
        public string CustomRating { get; }

        public RankingObjectData(string name, string songCount, string avgRating, string customRating)
        {
            Name = name;
            SongCount = songCount;
            AvgRating = avgRating;
            CustomRating = customRating;
        }
    }
}
