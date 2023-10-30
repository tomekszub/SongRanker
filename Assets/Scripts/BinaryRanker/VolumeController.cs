using System.Collections.Generic;
using UnityEngine;

public class VolumeController : MonoBehaviour
{
    [SerializeField] List<AudioSource> _AudioSources;

    public void OnVolumeChanged(float newVal)
    {
        _AudioSources.ForEach(source =>  source.volume = newVal);
    }
}
