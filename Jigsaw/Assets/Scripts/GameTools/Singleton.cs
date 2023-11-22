using UnityEngine;

namespace Tools.GameTools
{
    public abstract  class Singleton<T> : MonoBehaviour where T:Component
    {
        public static bool isApplicationQuiting;
        private static T _instance;
        private static System.Object _lock = new System.Object();

        public static T Instance
        {
            get
            {
                if (isApplicationQuiting)
                    return null;

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindObjectOfType<T>();

                        if (_instance == null)
                        {
                            var singleton = new GameObject("[SINGLETON] " + typeof(T));
                            _instance = singleton.AddComponent<T>();
                            DontDestroyOnLoad(singleton);
                        }   
                    }
                    return _instance;
                }
            }
        }
        protected virtual bool EverySceneSingleton => true;
        protected virtual void Awake()
        {
            _instance = this as T;
            if (EverySceneSingleton)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        protected virtual void OnDestroy()
        {
            isApplicationQuiting = true;
        }
    }
}

