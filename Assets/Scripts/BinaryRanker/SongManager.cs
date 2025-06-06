using System;
using System.Collections.Generic;
using System.Linq;
using TagLib;

namespace Immortus.SongRanker
{
    public static class SongManager
    {
        const string MUSIC_PATH = @"D:\Muzyka\Sample Music";

        public static event Action OnLanguageToSongChanged;
        public static event Action OnArtistToSongChanged;
        public static event Action OnAlbumToSongChanged;
        public static event Action OnGenreToSongChanged;
        
        static Dictionary<int, Song> _songs = new();
        static HashSet<string> _loadedPaths = new();
        static UniqueIDContainer<Genre> _genres = new();
        static UniqueIDContainer<Language> _languages = new();
        static UniqueIDContainer<Album> _albums = new();
        static UniqueIDContainer<Artist> _artists = new();

        //LUTs
        static Dictionary<int, List<Song>> _artistToSongsLUT = new();
        static Dictionary<int, List<Song>> _genreToSongsLUT = new();
        static Dictionary<int, List<Song>> _languageToSongsLUT = new();
        static Dictionary<int, List<Song>> _albumToSongsLUT = new();

        public static Dictionary<int, List<Song>> ArtistToSongs => _artistToSongsLUT;
        public static Dictionary<int, List<Song>> GenreToSongs => _genreToSongsLUT;
        public static Dictionary<int, List<Song>> LanguageToSongs => _languageToSongsLUT;
        public static Dictionary<int, List<Song>> AlbumToSongs => _albumToSongsLUT;

        public static IEnumerable<string> AllGenreNames => _genres.GetAllValues().Select(g => g.Name);

        public static IEnumerable<string> AllLanguageNames => _languages.GetAllValues().Select(l => l.Name);

        public static IEnumerable<string> AllAlbumNames => _albums.GetAllValues().Select(a => a.Name);

        public static IEnumerable<string> AllArtistNames => _artists.GetAllValues().Select(a => a.Name);

        public static IEnumerable<string> AllSongNames => _songs.Values.Select(s => s.Name);

        public static void Init()
        {
            _songs = new();
            _loadedPaths = new();
            _genres = new();
            _albums = new();
            _artists = new();

            OnLanguageToSongChanged = null;
        }

        public static void SaveTag(string path, Song data)
        {
            File tagLibFile;

            try
            {
                tagLibFile = File.Create(path);
            }
            catch
            {
                return;
            }

            if (tagLibFile == null)
                return;

            tagLibFile.Tag.Title = data.Name;

            var album = SongManager.GetAlbumByID(data.AlbumID);
            if (album != null)
            {
                tagLibFile.Tag.Album = album.Name;

                var artistAlbumID = SongManager.GetArtistByID(album.ArtistID);
                if(artistAlbumID != null)
                    tagLibFile.Tag.AlbumArtists = new string[] { artistAlbumID.Name };
            }

            List<string> performers = new();
            foreach (var artistID in data.ArtistIds)
            {
                Artist artist = SongManager.GetArtistByID(artistID);
                if (artist != null)
                    performers.Add(artist.Name);
            }
            tagLibFile.Tag.Performers = performers.ToArray();

            var genre = SongManager.GetGenreByID(data.GenreID);
            if (genre != null)
            {
                tagLibFile.Tag.Genres = new string[] { genre.Name };
            }

            tagLibFile.Tag.Track = (uint)data.AlbumSongNumber;
            tagLibFile.Tag.Year = (uint)data.Year;

            tagLibFile.Save();
            tagLibFile.Dispose();
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
        
        public static Language GetLanguageByID(int id)
        {
            _languages.TryGetValue(id, out Language language);

            return language;
        }

        public static int GetGenreIDByName(string name, bool createIfNeeded)
        {
            foreach(var genreKVP in _genres.All)
            {
                if (genreKVP.Value.Name == name)
                    return genreKVP.Key;
            }

            return createIfNeeded ? _genres.AddNewItem(new Genre(name)) : -1;
        }

        public static int GetLanguageIDByName(string name, bool createIfNeeded)
        {
            foreach (var languageKVP in _languages.All)
            {
                if (languageKVP.Value.Name == name)
                    return languageKVP.Key;
            }

            return createIfNeeded ? _languages.AddNewItem(new Language(name)) : -1;
        }

        public static Album GetAlbumByID(int id)
        {
            _albums.TryGetValue(id, out Album album);
            return album;
        }

        public static int GetAlbumIDByNameAndAuthor(string name, int artistID, bool createIfNeeded)
        {
            foreach (var albumKVP in _albums.All)
            {
                if (albumKVP.Value.ArtistID == artistID && albumKVP.Value.Name == name)
                    return albumKVP.Key;
            }

            return createIfNeeded ? _albums.AddNewItem(new Album(name, artistID)) : -1;
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

        public static List<int> GetArtistIDsByNames(List<string> names)
        {
            List<int> ret = new();

            List<string> namesCopy = new(names);

            foreach(var artistKVP in _artists.All)
            {
                if (namesCopy.Remove(artistKVP.Value.Name))
                    ret.Add(artistKVP.Key);

                if (namesCopy.Count == 0)
                    return ret;
            }

            foreach (var item in namesCopy)
            {
                var id = _artists.AddNewItem(new Artist(item, -1));
                ret.Add(id);
            }

            return ret;
        }

        public static void SaveDataToFiles()
        {
            FileSaver.SaveDictionary("Songs", _songs);
            FileSaver.SaveDictionary("Genres", _genres.All);
            FileSaver.SaveDictionary("Albums", _albums.All);
            FileSaver.SaveDictionary("Artists", _artists.All);
            FileSaver.SaveDictionary("Language", _languages.All);
        }

        public static bool LoadData()
        {
            var songs = FileSaver.LoadDictionary<int, Song>("Songs");
            if(songs.Count == 0)
                return false;

            var genres = FileSaver.LoadDictionary<int, Genre>("Genres");

            var albums = FileSaver.LoadDictionary<int, Album>("Albums");

            var artists = FileSaver.LoadDictionary<int, Artist>("Artists");

            var languages = FileSaver.LoadDictionary<int, Language>("Language");

            _songs = songs;

            _loadedPaths = new();
            foreach (var song in _songs)
                _loadedPaths.Add(song.Value.Path);

            _genres = new(genres);
            _albums = new(albums);
            _artists = new(artists);
            _languages = new(languages);

            RefreshArtistsLUT();
            RefreshGenresLUT();
            RefreshLanguageLUT();
            RefreshAlbumLUT();

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

        public static void RefreshLanguageLUT()
        {
            _languageToSongsLUT = new();

            foreach (var song in _songs)
            {
                var languageID = song.Value.LanguageID;

                if (_languageToSongsLUT.ContainsKey(languageID))
                    _languageToSongsLUT[languageID].Add(song.Value);
                else
                    _languageToSongsLUT.Add(languageID, new List<Song> { song.Value });
            }
        }

        public static void RefreshAlbumLUT()
        {
            _albumToSongsLUT = new();

            foreach (var song in _songs)
            {
                var albumID = song.Value.AlbumID;

                if (_albumToSongsLUT.ContainsKey(albumID))
                    _albumToSongsLUT[albumID].Add(song.Value);
                else
                    _albumToSongsLUT.Add(albumID, new List<Song> { song.Value });
            }
        }

        internal static void SearchForNewMusic(Action<int, string[]> resultCallback)
        {
            string[] files = System.IO.Directory.GetFiles(MUSIC_PATH, "*.mp3");
            List<string> errors = new();

            int newSongs = 0;

            foreach (string filePath in files)
            {
                if (_loadedPaths.Contains(filePath))
                    continue;

                var ret = LoadSong(filePath);
                if (ret.Item1 == null)
                    errors.Add($"Song at path {filePath} was not laoded due to error: \"{ret.Item2}\"");
                else
                    newSongs++;
            }

            resultCallback?.Invoke(newSongs, errors.ToArray());

            if (newSongs > 0)
                SaveDataToFiles();
        }

        public static void RemoveSong(Song song)
        {
            int albumArtistId = -1;
            
            _songs.Remove(song.ID);
            
            // language
            if(TryRemoveSongWithLanguage(song))
                OnLanguageToSongChanged?.Invoke();
            
            // album
            var albumRemovalResult = TryRemoveSongWithAlbum(song);

            albumArtistId = albumRemovalResult.albumArtistID;
            
            if(albumRemovalResult.albumsChanged)
                OnAlbumToSongChanged?.Invoke();
            
            if(TryRemoveSongWithArtists(song, albumArtistId))
                OnArtistToSongChanged?.Invoke();
            
            if(TryRemoveSongWithGenre(song))
                OnGenreToSongChanged?.Invoke();
            
            SaveDataToFiles();
        }
        
        static (Song, string) LoadSong(string path)
        {
            File tagLibFile;

            try
            {
                tagLibFile = File.Create(path);
            }
            catch (Exception ex)
            {
                return (null, $"TagLibFileError: {ex.Message}");
            }

            if (tagLibFile == null)
                return (null, "Could not load metadata");

            if (_loadedPaths.Contains(path))
                return (null, "This song was already added.");

            int id = GetNextSongID();

            var album = tagLibFile.Tag.Album;
            string albumArtistName = "";
            int albumArtistId = -1;
            int albumId = -1;
            
            if(!string.IsNullOrEmpty(album))
            {
                var albumArtists = tagLibFile.Tag.AlbumArtists;
                albumArtistName = albumArtists.Length > 0 ? albumArtists[0] : null;
                albumArtistId = _artists.GetID(new Artist(albumArtistName, -1));
                albumId = _albums.GetID(new(album, albumArtistId));
            }

            List<int> artistIds = new();
            foreach (var performer in tagLibFile.Tag.Performers)
            {
                if(string.IsNullOrEmpty(performer))
                    continue;
                
                int artistId = albumArtistName == performer ? albumArtistId : _artists.GetID(new Artist(performer, -1));
                if (artistId != -1)
                    artistIds.Add(artistId);
            }

            var genres = tagLibFile.Tag.Genres;
            string genreName = genres.Length > 0 ? genres[0].Split(';')[0] : null;
            int genreId = _genres.GetID(new(genreName));

            var songName = tagLibFile.Tag.Title;
            if(string.IsNullOrEmpty(songName))
            {
                var nameIndexStart = path.LastIndexOf('\\') + 1;
                var nameLength = path.LastIndexOf('.') - nameIndexStart;
                songName = path.Substring(nameIndexStart, nameLength);
            }

            Song song = new(id, songName, albumId, (int)tagLibFile.Tag.Track, artistIds.ToArray(), (int)tagLibFile.Tag.Year, genreId, tagLibFile.Properties.Duration, path);

            tagLibFile.Dispose();

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

        static bool TryRemoveSongWithLanguage(Song song)
        {
            if(!_languageToSongsLUT.TryGetValue(song.LanguageID, out var languageSongs))
                return false;
            
            if(!languageSongs.Remove(song))
                return false;
            
            // if we removed song and there is no other song in that language - remove it
            if(languageSongs.Count == 0)
            {
                _languageToSongsLUT.Remove(song.LanguageID);
                _languages.RemoveItem(song.LanguageID);
            }

            return true;
        }

        static (bool albumsChanged, int albumArtistID) TryRemoveSongWithAlbum(Song song)
        {
            if(song.AlbumID == -1)
                return (false, -1);
            
            if(!_albumToSongsLUT.TryGetValue(song.AlbumID, out var albumSongs))
                return (false, -1);
                
            if(!albumSongs.Remove(song))
                return (false, -1);

            _albums.TryGetValue(song.AlbumID, out var album);
            
            // if we removed song and there is no other song in that album - remove it
            if(albumSongs.Count == 0)
            {
                _albumToSongsLUT.Remove(song.AlbumID);
                _albums.RemoveItem(song.AlbumID);
            }
            
            return (true, album?.ArtistID ?? -1);
        }

        static bool TryRemoveSongWithArtists(Song song, int albumArtistID)
        {
            bool artistChange = false;

            var artistIds = new HashSet<int>(song.ArtistIds);
            if(albumArtistID != -1)
                artistIds.Add(albumArtistID);
            
            foreach(var artistID in artistIds)
            {
                if(artistID == -1)
                    continue;

                if(!_artists.TryGetValue(artistID, out var artist))
                    continue;

                _artistToSongsLUT.TryGetValue(artistID, out var artistSongs);
                var albumWithArtistExist = _albums.GetAllValues().Any(a => a.ArtistID == artistID);

                if(artistSongs != null)
                {
                    if(artistSongs.Remove(song))
                        artistChange = true;

                    if(artistSongs.Count == 0)
                    {
                        _artistToSongsLUT.Remove(artistID);
                        
                        if(!albumWithArtistExist)
                            _artists.RemoveItem(artistID);
                    }
                }
                else if(!albumWithArtistExist)
                    _artists.RemoveItem(artistID);
            }
            
            return artistChange;
        }
        
        static bool TryRemoveSongWithGenre(Song song)
        {
            if(!_genreToSongsLUT.TryGetValue(song.GenreID, out var genreSongs))
                return false;
            
            if(!genreSongs.Remove(song))
                return false;
            
            // if we removed song and there is no other song in that genre - remove it
            if(genreSongs.Count == 0)
            {
                _genreToSongsLUT.Remove(song.GenreID);
                _genres.RemoveItem(song.GenreID);
            }

            return true;
        }
    }
}
