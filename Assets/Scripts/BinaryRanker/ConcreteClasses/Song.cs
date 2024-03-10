using Newtonsoft.Json;
using System;
using UnityEngine;

namespace Immortus.SongRanker
{
    [JsonObject(MemberSerialization.Fields)]
    public class Song
    {
        int _id;
        string _name;
        int _albumId;
        int _albumSongNumber;
        int[] _artistIds;
        int _year;
        int _genreId;
        TimeSpan _duration;
        string _filePath;

        [JsonIgnore] public int ID => _id;
        [JsonIgnore] public string Name => _name;
        [JsonIgnore] public int[] ArtistIds => _artistIds;
        [JsonIgnore] public string Path => _filePath;
        [JsonIgnore] public float Rating { get; set; }

        [JsonIgnore]
        public int GenreID
        {
            get => _genreId;

            set => _genreId = Mathf.Max(-1, value);
        }

        [JsonIgnore] public int Year => _year;
        [JsonIgnore] public int AlbumID => _albumId;
        [JsonIgnore] public int AlbumSongNumber => _albumSongNumber;
        [JsonIgnore] public TimeSpan Duration => _duration;

        public Song(int id, string name, int albumId, int albumSongNumber, int[] artistIds, int year, int genreId, TimeSpan duration, string filePath)
        {
            _id = id;
            _name = name;
            _albumId = albumId;
            _albumSongNumber = albumSongNumber;
            _artistIds = artistIds;
            _year = year;
            GenreID = genreId;
            _duration = duration;
            _filePath = filePath;
        }
    }
}