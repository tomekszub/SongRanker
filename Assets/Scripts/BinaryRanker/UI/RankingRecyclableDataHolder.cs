using Immortus.SongRanker;
using TMPro;
using UnityEngine;

public class RankingRecyclableDataHolder :  MonoBehaviour, IRecyclableDataHolder
{
    [SerializeField] TextMeshProUGUI _Name;
    [SerializeField] TextMeshProUGUI _SongCount;
    [SerializeField] TextMeshProUGUI _AvgRating;
    [SerializeField] TextMeshProUGUI _CustomRating;
    
    public GameObject GameObject => gameObject;
    public RectTransform RectTransform => transform as RectTransform;
    
    public void FeedData(IRecyclableData data)
    {
        if(data is not RankingObjectData rankingObjectData)
        {
            Debug.LogError("Type mismatch. Require RankingObjectData");
            return;
        }
        
        _Name.text = rankingObjectData.Name;
        _SongCount.text = rankingObjectData.SongCount;
        _AvgRating.text = rankingObjectData.AvgRating;
        _CustomRating.text = rankingObjectData.CustomRating;
    }
}
