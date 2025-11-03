using System;
using System.Collections.Generic;
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

        int _currContainerIndex;

        public override void Activate(int arg)
        {
            base.Activate(arg);
            
            var prevIndex = _currContainerIndex;
            _currContainerIndex = Mathf.Max(0, Math.Min(arg, _TabContainers.Count - 1));
            
            if(_currContainerIndex == prevIndex)
                return;

            _TabContainers.ForEach(c => c.Hide());
            _TabContainers[_currContainerIndex].Show();
        }

        void Start()
        {
            _currContainerIndex = 0;
            _TabContainers.ForEach(c => c.Init(TriggerOnChangeDone));
            _NextElementButton.onClick.RemoveAllListeners();
            _NextElementButton.onClick.AddListener(NextElementButtonClicked);
            _PrevElementButton.onClick.RemoveAllListeners();
            _PrevElementButton.onClick.AddListener(PreviousElementButtonClicked);
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
    Action _onChangeDone;
    
    public void Init(Action onChangeDone)
    {
        _onChangeDone = onChangeDone;
    }

    public void Show() => gameObject.SetActive(true);
    
    public void Hide() => gameObject.SetActive(false);
    
    public abstract void NextElement();
    
    public abstract void PreviousElement();
    
    protected void TriggerOnChangeEvents() => _onChangeDone?.Invoke();
}
