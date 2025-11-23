using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RecyclableVerticalScrollView : MonoBehaviour
{
    [SerializeField] ScrollRect scrollRect;         // The ScrollRect component
    [SerializeField] GameObject itemPrefab;         // Prefab for a single UI item
    [SerializeField] Transform contentContainer;    // The container holding items
    [SerializeField] int visibleItemCount = 20;     // Number of visible items
    
    List<IRecyclableDataHolder> _items = new(); // Pool of UI items
    List<IRecyclableData> _dataList = new();     // Your data source (500 items)
    int _topIndex = 0;                               // Index of the topmost visible item
    int _totalItemCount;                             // Total items in the list

    bool _initialized;
    float _itemHeight;
    float _scrollContentHeight;
    VerticalLayoutGroup verticalLayoutGroup;

    public void RefreshData(List<IRecyclableData> data)
    {
        _dataList = data;
        _totalItemCount = _dataList.Count;
        
        if(!_initialized)
            Init();
        else
            UpdateTotalItemCount();
    }

    void Init()
    {
        // Get the height of a single item
        RectTransform itemRect = itemPrefab.GetComponent<RectTransform>();
        _itemHeight = itemRect.rect.height;

        // Calculate the total scrollable height of the content
        _scrollContentHeight = _itemHeight * _totalItemCount;

        // Set the Content container's height
        contentContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(0, _scrollContentHeight);

        // Initialize the pool of visible items
        for(int i = 0; i < visibleItemCount; i++)
        {
            GameObject item = Instantiate(itemPrefab, contentContainer);
            var recyclableDataHolder = item.GetComponent<IRecyclableDataHolder>();
            
            _items.Add(recyclableDataHolder);
            UpdateItem(recyclableDataHolder, i); // Set initial data
        }

        scrollRect.onValueChanged.AddListener(OnScroll);

        verticalLayoutGroup = contentContainer.GetComponent<VerticalLayoutGroup>();

        Invoke(nameof(DisableLayoutGroup), 0.5f);

        _initialized = true;
    }

    void DisableLayoutGroup()
    {
        if(verticalLayoutGroup != null)
            verticalLayoutGroup.enabled = false;
    }
    
    void OnScroll(Vector2 scrollPosition)
    {
        // Calculate the index of the topmost visible item based on the scroll position
        float contentPositionY = contentContainer.GetComponent<RectTransform>().anchoredPosition.y;
        int newTopIndex = Mathf.FloorToInt(contentPositionY / _itemHeight);

        // Clamp the index to stay within bounds
        newTopIndex = Mathf.Clamp(newTopIndex, 0, Math.Max(0, _totalItemCount - visibleItemCount));

        // Check if the new top index is significantly different from the current one
        if (newTopIndex != _topIndex)
        {
            // Update the top index and refresh visible items
            _topIndex = newTopIndex;
            RefreshVisibleItems();
        }
    }
    
    void RefreshVisibleItems()
    {
        for (int i = 0; i < _items.Count; i++)
        {
            int dataIndex = _topIndex + i;

            if (dataIndex < _totalItemCount)
            {
                // Update the item with new data
                UpdateItem(_items[i], dataIndex);

                // Position the item correctly
                _items[i].RectTransform.anchoredPosition = new Vector2(0, -dataIndex * _itemHeight);

                // Ensure the item is active
                _items[i].GameObject.SetActive(true);
            }
            else
            {
                // Hide items that are out of range
                _items[i].GameObject.SetActive(false);
            }
        }
        
        // If there are fewer total items than visible items, ensure proper display
        if (_totalItemCount < visibleItemCount)
        {
            for (int i = _totalItemCount; i < _items.Count; i++)
            {
                _items[i].GameObject.SetActive(false);
            }
        }
    }
    
    void UpdateTotalItemCount()
    {
        // Update the content container size
        UpdateContentSize();

        ClampScrollPosition();
        
        // Recalculate the top index based on the new scroll position
        RectTransform contentRect = contentContainer.GetComponent<RectTransform>();
        float contentPositionY = contentRect.anchoredPosition.y;
        _topIndex = Mathf.FloorToInt(contentPositionY / _itemHeight);
        _topIndex = Mathf.Clamp(_topIndex, 0, Mathf.Max(0, _totalItemCount - visibleItemCount));
        
        // Refresh the visible items to immediately update their positions and content
        RefreshVisibleItems();
    }
    
    void ClampScrollPosition()
    {
        float maxScrollY = Mathf.Max(0, (_totalItemCount - visibleItemCount) * _itemHeight);
        RectTransform contentRect = contentContainer.GetComponent<RectTransform>();
        Vector2 currentPos = contentRect.anchoredPosition;

        // Clamp the vertical position
        currentPos.y = Mathf.Clamp(currentPos.y, 0, maxScrollY);
        contentRect.anchoredPosition = currentPos;
    }
    
    void UpdateContentSize()
    {
        float newHeight = Mathf.Max(_itemHeight * _totalItemCount, _itemHeight * visibleItemCount);
        contentContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(0, newHeight);
    }
    
    void UpdateItem(IRecyclableDataHolder item, int dataIndex)
    {
        if (dataIndex < 0 || dataIndex >= _totalItemCount) 
            return;

        // Update the content of the UI element based on data
        item.FeedData(_dataList[dataIndex]);
    }
}

public interface IRecyclableData{}

public interface IRecyclableDataHolder
{
    public GameObject GameObject { get; }
    public RectTransform RectTransform { get; }
    public void FeedData(IRecyclableData data);
}
