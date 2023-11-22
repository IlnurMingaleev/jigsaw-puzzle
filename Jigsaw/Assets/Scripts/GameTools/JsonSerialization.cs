using System.IO;
using UnityEngine;

namespace Tools.GameTools
{
    public class JsonSerialization<T>
    {
        private string _path;
        public JsonSerialization(string fileName)
        {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            _path = Path.Combine(Application.persistentDataPath, fileName);
#else
            _path  = Path.Combine(Application.dataPath, fileName);
#endif
        }

        public (bool, T) DeSerialization()
        {
            T data = default;
            
            if (File.Exists(_path))
            {
                data = JsonUtility.FromJson<T>(File.ReadAllText(_path));
                return (true, data);
            }

            return (false, data);
        }
        
        public void Serialization(T data)
        {
            File.WriteAllText(_path, JsonUtility.ToJson(data));
        }
    }
}