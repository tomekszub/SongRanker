using System;
using PrimeTween;
using System.Collections.Generic;
using UnityEngine;

public class TabController : MonoBehaviour
{
    public static TabController Instance;

    [SerializeField] List<TabData> _Tabs;
    [SerializeField] RectTransform _SavePanel;

    int _maxTabsSetToKeyboardShortcuts;
    
    void Awake()
    {
        if(Instance != null)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        
        Application.targetFrameRate = 60;
        
        // tabs count or 12 (F12 is max)
        _maxTabsSetToKeyboardShortcuts = Math.Min(_Tabs.Count, 12);
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < _maxTabsSetToKeyboardShortcuts; i++)
        {
            // F1 => Keycode enum value - 282
            if(Input.GetKeyUp((KeyCode)(i + 282)) == false)
                continue;

            EnableTab(i);
            break;
        }
    }

    public static void ShowSavePanel()
    {
        var seq = Sequence.Create();
        seq.Chain(Tween.UIAnchoredPositionY(Instance._SavePanel, endValue: -100, duration: 0.5f, ease: Ease.InQuad));
        seq.ChainDelay(0.3f);
        seq.Chain(Tween.UIAnchoredPositionY(Instance._SavePanel, endValue: 20, duration: 0.5f, ease: Ease.InQuad));
    }

    void EnableTab(int index)
    {
        DeactivateAllTabs();
        _Tabs[index].Activate();
    }

    void DeactivateAllTabs() => _Tabs.ForEach(tab => tab.Deactivate());

    [Serializable]
    public class TabData
    {
        [SerializeField] BaseTab _Tab;
        [SerializeField] int _OptionalArgument;

        public void Activate() => _Tab.Activate(_OptionalArgument);
        
        public void Deactivate() => _Tab.Deactivate();
    }
}

public abstract class BaseTab : MonoBehaviour
{
    public virtual void Activate(int arg) => gameObject.SetActive(true);
    
    public virtual void Deactivate() => gameObject.SetActive(false);
}
