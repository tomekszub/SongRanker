using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabController : MonoBehaviour
{
    [SerializeField] List<GameObject> _Tabs;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyUp(KeyCode.F1))
        {
            _Tabs.ForEach(tab => tab.SetActive(false));
            _Tabs[0].SetActive(true);
        }
        else if (Input.GetKeyUp(KeyCode.F2))
        {
            _Tabs.ForEach(tab => tab.SetActive(false));
            _Tabs[1].SetActive(true);
        }
    }
}
