using UnityEngine;
using TMPro;

public class SongObject : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _Rating;
    [SerializeField] TextMeshProUGUI _Name;

    Song Song;

    public void Set(string name, int rating)
    {
        if (Song == null)
            Song = new Song();

        Song.Name = name;
        _Name.text = name;
        SetRating(rating);
    }
    public void Set(Song s)
    {
        Set(s.Name, s.Rating);
    }

    public int GetRating() => Song.Rating;
    public string GetName() => Song.Name;
    public Song GetSong() => Song;
    public int SetRating(int rating)
    {
        Song.Rating = rating;
        _Rating.text = Song.Rating.ToString();
        return rating;
    }
    public int ChangeRating(int amount)
    {
        return SetRating(Song.Rating + amount);
    }
}
public class Song
{
    private int _rating;
    public int Rating
    { 
        get => _rating;
        set => _rating = Mathf.Clamp(value, 1, 100);  
    }
    public string Name { get; set; }

    public Song(int rating, string name)
    {
        Rating = rating;
        Name = name;
    }

    public Song()
    {
        Rating = 50;
        Name = "Song";
    }
}
