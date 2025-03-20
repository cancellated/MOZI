using UnityEngine;

public abstract class SingletonBase<T> : MonoBehaviour where T : SingletonBase<T>
{
    private static T _instance;
    private static readonly object _lock = new object();
    private bool _isInitialized;

    public static T Instance
    {
        get
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();
                    if (_instance == null)
                    {
                        GameObject singleton = new GameObject(typeof(T).Name);
                        _instance = singleton.AddComponent<T>();
                    }
                }
                return _instance;
            }
        }
    }

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
            if (!_isInitialized)
            {
                Initialize();
                _isInitialized = true;
            }
        }
    }

    protected abstract void Initialize();

    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
            _isInitialized = false;
        }
    }
}