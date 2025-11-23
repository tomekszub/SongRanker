using System;
using System.Collections.Generic;
using Immortus.SongRanker;
using UnityEngine;
using UnityEngine.UI;

namespace Immortus.SongRanker
{
    public class EditingTab : BaseTab
    {
        public static event Action OnChangeDone;

        [SerializeField] List<BaseEditingTabContainer> _TabContainers;
        [SerializeField] Button _NextElementButton;
        [SerializeField] Button _PrevElementButton;
        [SerializeField] SearchField _SearchField;
        [SerializeField] RankerController _RankerController;

        int _currContainerIndex = -1;
        bool _isInitialized;

        public override void Activate(int newIndex)
        {
            base.Activate(newIndex);
            
            Init();
            
            var prevIndex = _currContainerIndex;
            _currContainerIndex = Mathf.Max(0, Math.Min(newIndex, _TabContainers.Count - 1));
            
            if(_currContainerIndex == prevIndex)
                return;

            _TabContainers.ForEach(c => c.Hide());
            _TabContainers[_currContainerIndex].Show();
            _SearchField.SetData(_TabContainers[_currContainerIndex].GetSearchData(), _TabContainers[_currContainerIndex].ShowDataWithIndex);
        }

        void Init()
        {
            if(_isInitialized)
                return;
            
            _TabContainers.ForEach(c => c.Init(_RankerController, TriggerOnChangeDone));
            _NextElementButton.onClick.RemoveAllListeners();
            _NextElementButton.onClick.AddListener(NextElementButtonClicked);
            _PrevElementButton.onClick.RemoveAllListeners();
            _PrevElementButton.onClick.AddListener(PreviousElementButtonClicked);
            _isInitialized = true;
        }

        void Update()
        {
            if(!Input.GetKey(KeyCode.LeftControl) || !Input.GetKeyDown(KeyCode.S)) 
                return;

            SongManager.SaveDataToFiles();
            TabController.ShowSavePanel();
        }

        void NextElementButtonClicked() => _TabContainers[_currContainerIndex].NextElement();
        
        void PreviousElementButtonClicked() => _TabContainers[_currContainerIndex].PreviousElement();
        
        void TriggerOnChangeDone() => OnChangeDone?.Invoke();
    }
}

public abstract class BaseEditingTabContainer : MonoBehaviour
{
    protected RankerController _rankerController;
    
    Action _onChangeDone;
    
    public virtual void Init(RankerController rankerController, Action onChangeDone)
    {
        _rankerController = rankerController;
        _onChangeDone = onChangeDone;
    }

    public virtual void Show() => gameObject.SetActive(true);
    
    public void Hide() => gameObject.SetActive(false);

    public abstract List<string> GetSearchData();
    
    public abstract void NextElement();
    
    public abstract void PreviousElement();

    public abstract void ShowDataWithIndex(int index);
    
    protected void TriggerOnChangeEvents() => _onChangeDone?.Invoke();
}

public abstract class ContextBaseEditingTabContainer<T> : BaseEditingTabContainer
{
    protected List<T> _context;
    protected int _currIndex;
    protected T _currentElement;

    public override void Init(RankerController rankerController, Action onChangeDone)
    {
        base.Init(rankerController, onChangeDone);
        _context = new List<T>();
        _currIndex = 0;
    }

    public override void Show()
    {
        base.Show();
        OnItemChanged();
    }

    public override void NextElement()
    {
        if (_currIndex >= _context.Count - 1)
            return;

        if (Input.GetKey(KeyCode.LeftControl))
            _currIndex = Mathf.Min(_currIndex + 5, _context.Count - 1);
        else if (Input.GetKey(KeyCode.LeftShift))
            _currIndex = Mathf.Min(_currIndex + 25, _context.Count - 1);
        else
            _currIndex++;

        OnItemChanged();
    }

    public override void PreviousElement()
    {
        if (_currIndex <= 0)
            return;

        if (Input.GetKey(KeyCode.LeftControl))
            _currIndex = Mathf.Max(_currIndex - 5, 0);
        else if (Input.GetKey(KeyCode.LeftShift))
            _currIndex = Mathf.Min(_currIndex - 25, _context.Count - 1);
        else
            _currIndex--;

        OnItemChanged();
    }

    protected virtual void OnItemChanged()
    {
        _currentElement = _context[_currIndex];
        Refresh();
    }

    protected abstract void Refresh();
}
