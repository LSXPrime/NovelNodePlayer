using UnityEngine;

namespace NovelNodePlayer
{
    public class SingletonScriptableObject<T> : ScriptableObject where T : ScriptableObject
    {
        private static readonly object instanceLock = new();
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instanceLock)
                    {
                        if (instance == null)
                        {
                            // Ensure that the instance is not destroyed when loading a new scene
                            instance = Resources.Load<T>(typeof(T).Name);
                            if (instance == null)
                                Debug.LogError($"Failed to load SingletonScriptableObject of type {typeof(T)}");
                            else
                                (instance as SingletonScriptableObject<T>)?.OnInitialize();
                        }
                    }
                }
                return instance;
            }
        }

        protected virtual void OnInitialize() { }
    }
}
