using System.Collections;
using UnityEngine;

public class CoroutineHandler : MonoBehaviour
{
    static CoroutineHandler Instance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public static new void StartCoroutine(IEnumerator coroutine)
    {
        ((MonoBehaviour)Instance).StartCoroutine(coroutine);
    }

    public static void StopAllCoroutinesNow() => Instance.StopAllCoroutines();
}
