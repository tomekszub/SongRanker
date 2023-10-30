using System.Collections.Generic;
using UnityEngine;

public class PopupsManager : MonoBehaviour
{
    public static PopupsManager Instance;
    [SerializeField] List<Popup> _Popups = new List<Popup>(); 
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public Popup GetPopup(string name)
    {
        for (int i = 0; i < _Popups.Count; i++)
        {
            if (_Popups[i].transform.name == name)
                return _Popups[i];
        }
        return null;
    }
}
