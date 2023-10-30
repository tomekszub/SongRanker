using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class InteractionHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler, IEndDragHandler, IBeginDragHandler
{
    [SerializeField] float _SelectionTreshold = 0.25f;
    [SerializeField] Color32 _HighlightColor;

    ScrollRect _scrollRect;
    Image _image;
    bool _selected = false;
    float _mouseDownTime;
    int _oldSiblingIndex;

    void Awake()
    {
        _image = GetComponent<Image>();
        _scrollRect = GetComponentInParent<ScrollRect>();
    }
    public void Select()
    {
        _image.color = new Color32(120, 85, 20, 255);
        _selected = true;
        SongManager.Instance.SongSelected(GetComponent<SongObject>());
    }
    public void Deselect(bool byMouse)
    {
        if (!byMouse)
            SetNormalColor();
        else
            _image.color = _HighlightColor;
        _selected = false;
        SongManager.Instance.SongDeselected(GetComponent<SongObject>());
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_selected)
            _image.color = _HighlightColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_selected)
            SetNormalColor();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //_mouseDownTime = Time.time;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_selected)
        {
            Deselect(true);
        }
        else
        {
            Select();
        }
    }
    public void SetNormalColor()
    {
        _image.color = new Color32(0, 0, 0, 0);
    }
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = new Vector3(transform.position.x, Input.mousePosition.y, 0);
        ExecuteEvents.Execute(_scrollRect.gameObject, eventData, ExecuteEvents.scrollHandler);
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        int layerOfDraggedSong = LayerMask.NameToLayer("DraggedObject");
        gameObject.layer = layerOfDraggedSong;

        //Set up the new Pointer Event
        PointerEventData m_PointerEventData = new PointerEventData(EventSystem.current);
        //Set the Pointer Event Position to that of the game object
        m_PointerEventData.position = Input.mousePosition;

        //Create a list of Raycast Results
        List<RaycastResult> results = new List<RaycastResult>();

        //Raycast using the Graphics Raycaster and mouse click position
        SongManager.Instance.transform.parent.GetComponent<GraphicRaycaster>().Raycast(m_PointerEventData, results);

        if (results.Count > 1)
        {
            int siblingIndex;
            int newRating = -1;
            if (results[1].gameObject.TryGetComponent(out SongObject s) && results[1].gameObject.layer != layerOfDraggedSong)
            {
                siblingIndex = results[1].gameObject.transform.GetSiblingIndex();
                newRating = s.GetRating();
            }
            else
            {
                siblingIndex = SongManager.Instance.SongsParent.childCount;
                SongObject song = siblingIndex == 0 ? null : SongManager.Instance.SongsParent.GetChild(siblingIndex - 1).GetComponent<SongObject>();
                if (song != null)
                    newRating = song.GetRating();
            }
            transform.SetParent(SongManager.Instance.SongsParent);
            transform.SetSiblingIndex(siblingIndex);
            if(newRating != -1)
                GetComponent<SongObject>().SetRating(newRating);
            SongManager.Instance.MoveSong(_oldSiblingIndex, siblingIndex, true);
        }
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        _oldSiblingIndex = transform.GetSiblingIndex();
        transform.SetParent(SongManager.Instance.transform);
    }
}
