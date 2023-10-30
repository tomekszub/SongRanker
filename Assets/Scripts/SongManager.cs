using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Newtonsoft.Json;
using TMPro;

public class SongManager : MonoBehaviour
{
    public static SongManager Instance;
    
    public Transform SongsParent => _SongsParent;
    public List<SongObject> Ranking => new List<SongObject>(_ranking);

    [SerializeField] TMP_InputField _SongNameInput;
    [SerializeField] TMP_InputField _RatingInput;
    [SerializeField] Slider _RatingSlider;
    [SerializeField] TextMeshProUGUI _SongsNameError;
    [SerializeField] GameObject _SongUIPrefab;
    [SerializeField] Transform _SongsParent;
    [SerializeField] int _HistoryCapacity = 10;

    List<SongObject> _ranking = new List<SongObject>();
    List<SongObject> _selectedSongs = new List<SongObject>();
    List<string> _history = new List<string>();

    bool _sortedSelectedSongs = false;
    int _currHistoryIndex = 0;
    int _lastSelectedSongIndex = -1;

    void Awake()
    {
        if (Instance == null)
            Instance = this;

        SaveCurrentRankingState();
    }

    void LoadRankingFromSongsJSON(string readData, bool appendSongs)
    {
        Song[] songs = JsonConvert.DeserializeObject<Song[]>(readData);
        for (int i = 0; i < songs.Length; i++)
        {
            if(_ranking.Count <= i)
                InstantiateNewSongObject(songs[i]);
            else
            {
                // either add to existing songs or edit currently existing songs
                if (appendSongs)
                    InstantiateNewSongObject(songs[i]);
                else
                    _ranking[i].Set(songs[i]);
            }
        }
        if(!appendSongs && songs.Length < _ranking.Count)
        {
            // we do not append songs and loaded songs have less items than
            // we have currently so we need to remove this excessive songs
            for (int i = 0; i < _ranking.Count - songs.Length; i++)
            {
                Destroy(_SongsParent.GetChild(songs.Length + i).gameObject);
            }
            _ranking.RemoveRange(songs.Length, _ranking.Count - songs.Length);
        }
    }

    string GetRankingJSON()
    {
        Song[] songs = new Song[_ranking.Count];
        for (int i = 0; i < _ranking.Count; i++)
        {
            songs[i] = _ranking[i].GetSong();
        }
        return JsonConvert.SerializeObject(songs);
    }

    int AddSongToList(SongObject songPrefab)
    {
        for (int i = 0; i < _ranking.Count; i++)
        {
            if (_ranking[i].GetRating() < songPrefab.GetRating())
            {
                _ranking.Insert(i, songPrefab);
                return i;
            }
        }
        _ranking.Add(songPrefab);
        return _ranking.Count - 1;
    }

    bool SongExist(string name)
    {
        for (int i = 0; i < _ranking.Count; i++)
        {
            if (_ranking[i].GetName().Equals(name))
                return true;
        }
        return false;
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Return))
        {
            AddSong();
            EventSystem.current.SetSelectedGameObject(null); 
            EventSystem.current.SetSelectedGameObject(_SongNameInput.gameObject); 
        }
        if(Input.GetKeyUp(KeyCode.Plus) || Input.GetKeyUp(KeyCode.KeypadPlus))
            ChangeRatingOfSelection(1);
        if (Input.GetKeyUp(KeyCode.Minus) || Input.GetKeyUp(KeyCode.KeypadMinus))
            ChangeRatingOfSelection(-1);
        if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyUp(KeyCode.A))
            SelectAll();
        if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyUp(KeyCode.Delete))
            DeselectAll();
        if(Input.GetKeyUp(KeyCode.Home))
            SelectUpwards();
        if (Input.GetKeyUp(KeyCode.End))
            SelectDownwards();
#if UNITY_EDITOR
        if (Input.GetKeyUp(KeyCode.KeypadDivide))
            Undo();
#else
        if (Input.GetKeyUp(KeyCode.LeftControl) && Input.GetKeyUp(KeyCode.Z))
            Undo();
#endif
#if UNITY_EDITOR
        if (Input.GetKeyUp(KeyCode.KeypadMultiply))
            Redo();
#else
        if (Input.GetKeyUp(KeyCode.LeftControl) && Input.GetKeyUp(KeyCode.Y))
            Redo();
#endif
    }

    void SelectUpwards()
    {
        if (_lastSelectedSongIndex != -1)
        {
            int iMax = _lastSelectedSongIndex;
            for (int i = 0; i < iMax; i++)
                _SongsParent.GetChild(i).GetComponent<InteractionHandler>().Select();
        }
    }

    void SelectDownwards()
    {
        if (_lastSelectedSongIndex != -1)
        {
            for (int i = _lastSelectedSongIndex; i < _SongsParent.childCount - 1; i++)
                _SongsParent.GetChild(i).GetComponent<InteractionHandler>().Select();
        }
    }

    void InstantiateNewSongObject(string songName, int rating)
    {
        SongObject songPrefab = Instantiate(_SongUIPrefab, _SongsParent).GetComponent<SongObject>();
        if (songPrefab != null)
        {
            if(rating == -1)
                rating = Random.Range(0, 101);

            songPrefab.Set(songName, rating);
            int index = AddSongToList(songPrefab);
            songPrefab.transform.SetSiblingIndex(index);
        }
    }

    void InstantiateNewSongObject(Song s) => InstantiateNewSongObject(s.Name, s.Rating);

    void ChangeRatingOfSelection(int change)
    {
        if (change == 0)
            return;

        if (!_sortedSelectedSongs)
            SortSelectedSongs();

        int newIndex;

        if (change > 0)
        {
            for (int i = 0; i < _selectedSongs.Count; i++)
            {
                _selectedSongs[i].ChangeRating(change);
                newIndex = _selectedSongs[i].transform.GetSiblingIndex();
                for (int j = newIndex - 1; j >= 0; j--)
                {
                    if (_ranking[j].GetRating() <= _selectedSongs[i].GetRating())
                    {
                        //Debug.Log("Overtaking " + _ranking[j].GetName() + " with rating " + _ranking[j].GetRating());
                        newIndex = j;
                    }
                    else
                        break;
                }
                MoveSong(_selectedSongs[i].transform.GetSiblingIndex(), newIndex, false);
                _selectedSongs[i].transform.SetSiblingIndex(newIndex);
            }
        }
        else
        {
            int oldIndex;
            for (int i = _selectedSongs.Count - 1; i >= 0; i--)
            {
                _selectedSongs[i].ChangeRating(change);
                oldIndex = _selectedSongs[i].transform.GetSiblingIndex();
                newIndex = oldIndex;
                for (int j = newIndex + 1; j < _ranking.Count; j++)
                {
                    if (_ranking[j].GetRating() > _selectedSongs[i].GetRating())
                    {
                        //Debug.Log("Falling behind " + _ranking[j].GetName() + " with rating " + _ranking[j].GetRating());
                        newIndex = j;
                    }
                    else
                        break;
                }
                MoveSong(oldIndex, newIndex, false);
                _selectedSongs[i].transform.SetSiblingIndex(newIndex);
            }
        }
        SaveCurrentRankingState();
    }

    void SortSelectedSongs()
    {
        _selectedSongs.Sort(delegate (SongObject s1, SongObject s2) { return s1.transform.GetSiblingIndex() - s2.transform.GetSiblingIndex(); });
        _sortedSelectedSongs = true;
    }

    void Clear()
    {
        for (int i = _SongsParent.childCount - 1; i >= 0; i--)
        {
            Destroy(_SongsParent.GetChild(i).gameObject);
        }
        if (_ranking.Count > 0)
        {
            _ranking.Clear();
            SaveCurrentRankingState();
        }
    }

    void Undo()
    {
        if (_currHistoryIndex != _history.Count - 1)
        {
            ClearSelection();
            _currHistoryIndex++;
            LoadRankingFromSongsJSON(_history[_currHistoryIndex], false);
        }
    }

    void Redo()
    {
        if (_currHistoryIndex != 0)
        {
            ClearSelection();
            _currHistoryIndex--;
            LoadRankingFromSongsJSON(_history[_currHistoryIndex], false);
        }
    }

    void SaveCurrentRankingState()
    {
        if(_currHistoryIndex == 0)
        {
            if (_history.Count == _HistoryCapacity)
                _history.RemoveAt(_history.Count-1);
        }
        else
        {
            // delete all changes to current history point
            _history.RemoveRange(0, _currHistoryIndex);
            _currHistoryIndex = 0;
        }
        _history.Insert(0,GetRankingJSON());
        Debug.LogWarning("Saving state:\n" + GetRankingJSON());
    }

    void ClearSelection()
    {
        for (int i = 0; i < _selectedSongs.Count; i++)
        {
            _selectedSongs[i].GetComponent<InteractionHandler>().SetNormalColor();
        }
        _selectedSongs.Clear();
    }

    void SelectAll()
    {
        _selectedSongs.Clear();
        for (int i = 0; i < _ranking.Count; i++)
        {
            _SongsParent.GetChild(i).GetComponent<InteractionHandler>()?.Select();
        }
        _lastSelectedSongIndex = _selectedSongs.Count - 1;
    }

    void DeselectAll()
    {
        for (int i = 0; i < _ranking.Count; i++)
        {
            _SongsParent.GetChild(i).GetComponent<InteractionHandler>()?.Deselect(false);
        }
        _lastSelectedSongIndex = -1;
    }

    public void AddSong()
    {
        string songName = _SongNameInput.text;
        if (songName.Equals(""))
        {
            _SongsNameError.text = "Empty name field!";
            return;
        }
        if (SongExist(songName))
        {
            _SongsNameError.text = "Song with provided name already exist!";
            return;
        }
        InstantiateNewSongObject(songName, (int)_RatingSlider.value);
        SaveCurrentRankingState();
    }

    public void MoveSong(int oldIndex, int newIndex, bool saveChanges)
    {
        if (oldIndex == newIndex)
            return;

        SongObject s = _ranking[oldIndex];

        if (oldIndex > newIndex)
        {
            _ranking.RemoveAt(oldIndex);
            _ranking.Insert(newIndex, s);
        }
        else
        {
            _ranking.RemoveAt(oldIndex);
            _ranking.Insert(newIndex, s);
        }

        if (saveChanges)
            SaveCurrentRankingState();
    }

    public void SaveRankingToAFile()
    {
        PopupsManager.Instance.GetPopup("ConfirmationPopup")?.Show(new GenericPopup_PopupData(
            "Are you sure?",
            "Do you want to save this data to a file?",
            "No",
            "Yes",
            null,
            Save));

        void Save()
        {
            string path = "Assets/Exports/test.txt";
            StreamWriter writer = new StreamWriter(path, false);
            writer.Write(GetRankingJSON());
            writer.Close();
#if UNITY_EDITOR
            AssetDatabase.ImportAsset(path);
#endif
        }
    }

    public void LoadRankingFromFile()
    {

        PopupsManager.Instance.GetPopup("ConfirmationPopup")?.Show(new GenericPopup_PopupData(
            "Are you sure?",
            "Do you want to load data from a file and append it to existing list?",
            "No",
            "Yes",
            null,
            Load));

        void Load()
        {
            string path = "Assets/Exports/test.txt";
            StreamReader reader = new StreamReader(path);
            string readData = reader.ReadToEnd();
            LoadRankingFromSongsJSON(readData, true);
            reader.Close();
            SaveCurrentRankingState();
        }
    }

    public void ClearRanking()
    {
        PopupsManager.Instance.GetPopup("ConfirmationPopup")?.Show(new GenericPopup_PopupData(
            "Are you sure?",
            "Do you want clear currently displayed list?",
            "No",
            "Yes",
            null,
            Clear));
    }

    public void SongSelected(SongObject song)
    {
        SliderRatingChanged(song.GetRating());
        _lastSelectedSongIndex = song.transform.GetSiblingIndex();
        if (!_selectedSongs.Contains(song))
        {
            _selectedSongs.Add(song);
            _sortedSelectedSongs = false;
        }
    }

    public void SongDeselected(SongObject song)
    {
        _selectedSongs.Remove(song);
        _lastSelectedSongIndex = -1;
    }

    public void SliderRatingChanged(float val)
    {
        val = Mathf.Clamp(val,1,100);
        _RatingInput.text = val.ToString();
    }

    public void InputRatingChanged(string val)
    {
        _RatingSlider.value = int.Parse(val);
    }

    public void IncreaseRating() => _RatingSlider.value++;

    public void DecreaseRating() => _RatingSlider.value--;
}
