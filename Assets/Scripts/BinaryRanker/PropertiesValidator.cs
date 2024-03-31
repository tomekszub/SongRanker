namespace Immortus.SongRanker
{
    public static class PropertiesValidator
    {
        public static bool ValidateName(string name)
        {
            return !string.IsNullOrEmpty(name);
        }

        public static bool ValidateArtists(string[] artists)
        {
            if(artists.Length == 0)
                return false;

            for (int i = 0; i < artists.Length; i++)
            {
                if (string.IsNullOrEmpty(artists[i]))
                    return false;
            }

            return true; ;
        }

        public static bool ValidateGenre(Genre genre)
        {
            if(genre == null)
                return false;

            return !string.IsNullOrEmpty(genre.Name);
        }

        public static bool ValidateYear(int year)
        {
            return year > 0 && year <= System.DateTime.Now.Year;
        }

        public static bool ValidateLanguage(Language language)
        {
            if (language == null)
                return false;

            return !string.IsNullOrEmpty(language.Name);
        }

        public static bool ValidateAlbum(Album album)
        {
            if (album == null)
                return true;

            return !string.IsNullOrEmpty(album.Name);
        }

        public static bool ValidateAlbumTrackNumber(int number)
        {
            return number > 0;
        }
    }
}