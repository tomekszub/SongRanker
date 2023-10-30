using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public static class FileSaver
{
    static string SAVED_FILES_PATH => Application.dataPath + "/SavedFiles/";

    public static void SaveDictionary<K,V>(string fileName, Dictionary<K,V> dict)
    {
        string path = $"{SAVED_FILES_PATH}{fileName}";

        File.WriteAllText(path, JsonConvert.SerializeObject(dict, Formatting.Indented));
    }

    public static void SaveCollection<T>(string fileName, IEnumerable<T> enumerable)
    {
        string path = $"{SAVED_FILES_PATH}{fileName}";

        File.WriteAllText(path, JsonConvert.SerializeObject(enumerable, Formatting.Indented));
    }

    public static IEnumerable<T> LoadCollection<T>(string fileName)
    {
        string path = $"{SAVED_FILES_PATH}{fileName}";

        IEnumerable<T> ret = null;

        if (File.Exists(path))
        {
            var content = File.ReadAllText(path);

            if (!string.IsNullOrEmpty(content))
            {
                ret = JsonConvert.DeserializeObject<IEnumerable<T>>(content);
            }
        }

        return ret ?? new List<T>();
    }

    public static Dictionary<K, V> LoadDictionary<K,V>(string fileName)
    {
        string path = $"{SAVED_FILES_PATH}{fileName}";

        Dictionary<K, V> ret = null;

        if (File.Exists(path))
        {
            var content = File.ReadAllText(path);

            if(!string.IsNullOrEmpty(content))
            {
                ret = JsonConvert.DeserializeObject<Dictionary<K,V>>(content);
            }
        }

        return ret ?? new Dictionary<K, V>();
    }
}
