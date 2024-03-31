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
        int _languageId;
        int _genreId;
        TimeSpan _duration;
        string _filePath;

        [JsonIgnore]
        float _rating;

        [JsonIgnore] public int ID => _id;
        [JsonIgnore] public string Name { get => _name; set => _name = value; }
        [JsonIgnore] public int[] ArtistIds { get => _artistIds; set => _artistIds = value; }
        [JsonIgnore] public string Path => _filePath;
        [JsonIgnore] public float Rating { get => _rating; set => _rating = value; }

        [JsonIgnore]
        public int GenreID
        {
            get => _genreId;

            set => _genreId = Mathf.Max(-1, value);
        }

        [JsonIgnore] public int Year { get => _year; set => _year = value; }
        [JsonIgnore] public int LanguageID { get => _languageId; set => _languageId = value; }
        [JsonIgnore] public int AlbumID { get => _albumId; set => _albumId = value; }
        [JsonIgnore] public int AlbumSongNumber { get => _albumSongNumber; set => _albumSongNumber = value; }
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