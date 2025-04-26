using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RecyclableVerticalScrollView : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;         // The ScrollRect component
    [SerializeField] private GameObject itemPrefab;         // Prefab for a single UI item
    [SerializeField] private Transform contentContainer;    // The container holding items
    [SerializeField] private int visibleItemCount = 20;     // Number of visible items
    
    private List<GameObject> _items = new List<GameObject>(); // Pool of UI items
    private List<string> _dataList = new List<string>();     // Your data source (500 items)
    private int _topIndex = 0;                               // Index of the topmost visible item
    private int _totalItemCount;                             // Total items in the list

    private bool _initialized;
    private float _itemHeight;
    private float _scrollContentHeight;
    private VerticalLayoutGroup verticalLayoutGroup;

    public void RefreshData(List<string> data)
    {
        _dataList = data;
        _totalItemCount = _dataList.Count;
        
        if(!_initialized)
            Init();
        else
            UpdateTotalItemCount();
    }

    public void Init()
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
            _items.Add(item);
            UpdateItem(item, i); // Set initial data
        }

        scrollRect.onValueChanged.AddListener(OnScroll);

        verticalLayoutGroup = contentContainer.GetComponent<VerticalLayoutGroup>();

        Invoke(nameof(DisableLayoutGroup), 0.5f);

        _initialized = true;
    }

    private void DisableLayoutGroup()
    {
        if(verticalLayoutGroup != null)
            verticalLayoutGroup.enabled = false;
    }
    
    private void OnScroll(Vector2 scrollPosition)
    {
        // Calculate the index of the topmost visible item based on the scroll position
        float contentPositionY = contentContainer.GetComponent<RectTransform>().anchoredPosition.y;
        int newTopIndex = Mathf.FloorToInt(contentPositionY / _itemHeight);

        // Clamp the index to stay within bounds
        newTopIndex = Mathf.Clamp(newTopIndex, 0, _totalItemCount - visibleItemCount);

        // Check if the new top index is significantly different from the current one
        if (newTopIndex != _topIndex)
        {
            // Update the top index and refresh visible items
            _topIndex = newTopIndex;
            RefreshVisibleItems();
        }
    }
    
    private void RefreshVisibleItems()
    {
        for (int i = 0; i < _items.Count; i++)
        {
            int dataIndex = _topIndex + i;

            if (dataIndex < _totalItemCount)
            {
                // Update the item with new data
                UpdateItem(_items[i], dataIndex);

                // Position the item correctly
                RectTransform rect = _items[i].GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(0, -dataIndex * _itemHeight);

                // Ensure the item is active
                _items[i].SetActive(true);
            }
            else
            {
                // Hide items that are out of range
                _items[i].SetActive(false);
            }
        }
        
        // If there are fewer total items than visible items, ensure proper display
        if (_totalItemCount < visibleItemCount)
        {
            for (int i = _totalItemCount; i < _items.Count; i++)
            {
                _items[i].SetActive(false);
            }
        }
    }
    
    public void UpdateTotalItemCount()
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
    
    private void ClampScrollPosition()
    {
        float maxScrollY = Mathf.Max(0, (_totalItemCount - visibleItemCount) * _itemHeight);
        RectTransform contentRect = contentContainer.GetComponent<RectTransform>();
        Vector2 currentPos = contentRect.anchoredPosition;

        // Clamp the vertical position
        currentPos.y = Mathf.Clamp(currentPos.y, 0, maxScrollY);
        contentRect.anchoredPosition = currentPos;
    }
    
    private void UpdateContentSize()
    {
        float newHeight = Mathf.Max(_itemHeight * _totalItemCount, _itemHeight * visibleItemCount);
        contentContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(0, newHeight);
    }
    
    private void UpdateItem(GameObject item, int dataIndex)
    {
        if (dataIndex < 0 || dataIndex >= _totalItemCount) return;

        // Update the content of the UI element based on data
        var text = item.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        text.text = _dataList[dataIndex];
    }
}

