using System.Collections;
using UnityEngine;

public class CoroutineHandler : MonoBehaviour
{
    static CoroutineHandler _instance;

    void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(gameObject);
    }

    public new static Coroutine StartCoroutine(IEnumerator coroutine)
    {
        return ((MonoBehaviour)_instance).StartCoroutine(coroutine);
    }

    public new static void StopCoroutine(Coroutine coroutine)
    {
        ((MonoBehaviour)_instance).StopCoroutine(coroutine);
    } 
    
    public static void StopAllCoroutinesNow() => _instance.StopAllCoroutines();
}
