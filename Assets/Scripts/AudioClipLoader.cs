using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AudioClipLoader : MonoBehaviour
{
    const int CLIP_CACHE_SIZE = 10;

    Dictionary<string, AudioClip> _cachedClips = new();

    public void LoadSong(string path, Action<AudioClip> onResult = null)
    {
        if(_cachedClips.TryGetValue(path, out AudioClip clip))
            onResult?.Invoke(clip);

        StartCoroutine(LoadMusic(path, onResult));
    }

    IEnumerator LoadMusic(string songPath, Action<AudioClip> onResult)
    {
        if (System.IO.File.Exists(songPath))
        {
            using (var uwr = UnityWebRequestMultimedia.GetAudioClip("file://" + songPath, AudioType.MPEG))
            {
                ((DownloadHandlerAudioClip)uwr.downloadHandler).streamAudio = true;

                yield return uwr.SendWebRequest();

                if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(uwr.error);
                    yield break;
                }

                DownloadHandlerAudioClip dlHandler = (DownloadHandlerAudioClip)uwr.downloadHandler;

                if (dlHandler.isDone)
                {
                    AudioClip audioClip = dlHandler.audioClip;

                    if (audioClip != null)
                    {
                        if (_cachedClips.Count < CLIP_CACHE_SIZE && !_cachedClips.ContainsKey(songPath))
                            _cachedClips.Add(songPath, audioClip);

                        onResult?.Invoke(audioClip);
                        yield break;
                    }
                    else
                    {
                        Debug.Log("Couldn't find a valid AudioClip :(");
                    }
                }
                else
                {
                    Debug.Log("The download process is not completely finished.");
                }
            }
        }
        else
        {
            Debug.Log("Unable to locate converted song file.");
        }

        onResult?.Invoke(null);
    }


}
