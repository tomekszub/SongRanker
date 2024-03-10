using PrimeTween;
using System.Collections.Generic;
using UnityEngine;

public class TabController : MonoBehaviour
{
    [SerializeField] List<GameObject> _Tabs;
    [SerializeField] RectTransform _SavePanel;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyUp(KeyCode.F1))
            EnableTab(0);
        else if (Input.GetKeyUp(KeyCode.F2))
            EnableTab(1);
        else if (Input.GetKeyUp(KeyCode.F3))
            EnableTab(2);
    }

    public void ShowSavePanel()
    {
        var seq = Sequence.Create();
        seq.Chain(Tween.UIAnchoredPositionY(_SavePanel, endValue: -100, duration: 0.5f, ease: Ease.InQuad));
        seq.ChainDelay(0.3f);
        seq.Chain(Tween.UIAnchoredPositionY(_SavePanel, endValue: 20, duration: 0.5f, ease: Ease.InQuad));
    }

    void EnableTab(int index)
    {
        DeactivateAllTabs();
        _Tabs[index].SetActive(true);
    }

    void DeactivateAllTabs() => _Tabs.ForEach(tab => tab.SetActive(false));
}
