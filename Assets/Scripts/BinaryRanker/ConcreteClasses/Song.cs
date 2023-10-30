using Newtonsoft.Json;
using System;

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
        [JsonIgnore] public float Rating { get; private set; }
        [JsonIgnore] public int GenreID => _genreId;

        public Song(int id, string name, int albumId, int albumSongNumber, int[] artistIds, int year, int genreId, TimeSpan duration, string filePath)
        {
            _id = id;
            _name = name;
            _albumId = albumId;
            _albumSongNumber = albumSongNumber;
            _artistIds = artistIds;
            _year = year;
            _genreId = genreId;
            _duration = duration;
            _filePath = filePath;
        }

        public void SetRating(float val) => Rating = val;
    }
}