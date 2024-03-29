using System;
using System.Collections.Generic;
using System.Linq;
using TagLib;

namespace Immortus.SongRanker
{
    public static class SongManager
    {
        static Dictionary<int, Song> _songs = new();
        static HashSet<string> _loadedPaths = new();
        static UniqueIDContainer<Genre> _genres = new();
        static UniqueIDContainer<Album> _albums = new();
        static UniqueIDContainer<Artist> _artists = new();

        //LUTs
        static Dictionary<int, List<Song>> _artistToSongsLUT = new();
        static Dictionary<int, List<Song>> _genreToSongsLUT = new();

        public static Dictionary<int, List<Song>> ArtistToSongs => _artistToSongsLUT;
        public static Dictionary<int, List<Song>> GenreToSongs => _genreToSongsLUT;

        public static IEnumerable<string> AllGenreNames => _genres.GetAllValues().Select(g => g.Name);

        public static void Init()
        {
            _songs = new();
            _loadedPaths = new();
            _genres = new();
            _albums = new();
            _artists = new();
        }

        public static (Song, string) LoadSong(string path)
        {
            File tagLibFile;

            try
            {
                tagLibFile = File.Create(path);
            }
            catch(Exception ex)
            {
                return (null, ex.Message);
            }

            if(tagLibFile == null)
                return (null, "Could not load metadata");

            if(_loadedPaths.Contains(path))
                return (null, "This song was already added.");

            int id = GetNextSongID();

            var album = tagLibFile.Tag.Album;
            var albumArtists = tagLibFile.Tag.AlbumArtists;
            string albumArtistName = albumArtists.Length > 0 ? albumArtists[0] : null;
            int albumArtistId = _artists.GetID(new Artist(albumArtistName, -1));
            int albumId = _albums.GetID(new(album, albumArtistId));

            List<int> artistIds = new();
            foreach( var performer in tagLibFile.Tag.Performers)
            {
                int artistId = albumArtistName == performer ? albumArtistId : _artists.GetID(new Artist(performer, -1));
                if(artistId != -1)
                    artistIds.Add(artistId);
            }

            var genres = tagLibFile.Tag.Genres;
            string genreName = genres.Length > 0 ? genres[0].Split(';')[0] : null;
            int genreId = _genres.GetID(new(genreName));

            Song song = new(id, tagLibFile.Tag.Title, albumId, (int)tagLibFile.Tag.Track, artistIds.ToArray(), (int)tagLibFile.Tag.Year, genreId, tagLibFile.Properties.Duration, path);

            _songs.Add(id, song);

            _loadedPaths.Add(song.Path);

            return (song, string.Empty);

            static int GetNextSongID()
            {
                for (int i = 0; i < int.MaxValue; i++)
                {
                    if (_songs.ContainsKey(i))
                        continue;

                    return i;
                }

                return -1;
            }
        }

        public static IEnumerable<Song> GetAllSongs() => _songs.Values;

        public static Song GetSongWithID(int id)
        {
            _songs.TryGetValue(id, out Song song);
            return song;
        }

        public static Artist GetArtistByID(int id)
        {
            _artists.TryGetValue(id, out Artist artist);
            return artist;
        }

        public static Genre GetGenreByID(int id)
        {
            _genres.TryGetValue(id, out Genre genre);
            return genre;
        }

        public static int GetGenreIDByName(string name)
        {
            foreach(var genreKVP in _genres.All)
            {
                if (genreKVP.Value.Name == name)
                    return genreKVP.Key;
            }

            return -1;
        }

        public static Album GetAlbumByID(int id)
        {
            _albums.TryGetValue(id, out Album album);
            return album;
        }

        public static string[] GetArtistNamesByIDs(int[] ids)
        {
            string[] names = new string[ids.Length];

            for (int i = 0; i < ids.Length; i++)
            {
                if (_artists.TryGetValue(ids[i], out Artist artist))
                    names[i] = artist.Name;
            }

            return names;
        }

        public static void SaveDataToFiles()
        {
            FileSaver.SaveDictionary("Songs", _songs);
            FileSaver.SaveDictionary("Genres", _genres.All);
            FileSaver.SaveDictionary("Albums", _albums.All);
            FileSaver.SaveDictionary("Artists", _artists.All);
        }

        public static bool LoadData()
        {
            var songs = FileSaver.LoadDictionary<int, Song>("Songs");
            if(songs.Count == 0)
                return false;

            var genres = FileSaver.LoadDictionary<int, Genre>("Genres");
            if (genres.Count == 0)
                return false;

            var albums = FileSaver.LoadDictionary<int, Album>("Albums");
            if (albums.Count == 0)
                return false;

            var artists = FileSaver.LoadDictionary<int, Artist>("Artists");
            if (artists.Count == 0)
                return false;

            _songs = songs;

            _loadedPaths = new();
            foreach (var song in _songs)
                _loadedPaths.Add(song.Value.Path);

            _genres = new(genres);
            _albums = new(albums);
            _artists = new(artists);

            RefreshArtistsLUT();
            RefreshGenresLUT();

            return true;
        }

        public static void RefreshArtistsLUT()
        {
            _artistToSongsLUT = new();

            foreach (var song in _songs)
            {
                foreach (var artistID in song.Value.ArtistIds)
                {
                    if (_artistToSongsLUT.ContainsKey(artistID))
                        _artistToSongsLUT[artistID].Add(song.Value);
                    else
                        _artistToSongsLUT.Add(artistID, new List<Song> { song.Value });
                }
            }
        }

        public static void RefreshGenresLUT()
        {
            _genreToSongsLUT = new();

            foreach (var song in _songs)
            {
                var genreID = song.Value.GenreID;

                if (_genreToSongsLUT.ContainsKey(genreID))
                    _genreToSongsLUT[genreID].Add(song.Value);
                else
                    _genreToSongsLUT.Add(genreID, new List<Song> { song.Value });
            }
        }

    }
}
