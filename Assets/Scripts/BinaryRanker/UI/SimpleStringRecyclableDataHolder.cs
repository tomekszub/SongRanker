using Immortus.SongRanker;
using TMPro;
using UnityEngine;

public class SimpleStringRecyclableDataHolder : MonoBehaviour, IRecyclableDataHolder
{
    [SerializeField] TextMeshProUGUI _Text;
    
    public GameObject GameObject => gameObject;
    public RectTransform RectTransform => transform as RectTransform;
    
    public void FeedData(IRecyclableData data)
    {
        if(data is not SimpleStringRecyclableData simpleStringRecyclableData)
        {
            Debug.LogError("Type mismatch. Require RankerTab.SimpleStringRecyclableData");
            return;
        }
        
        _Text.text = simpleStringRecyclableData.Content;
    }
}
