using UnityEngine;

public class BasePopup : MonoBehaviour
{
    public void Show() => gameObject.SetActive(true);
    
    public void Hide() => gameObject.SetActive(false);
}
