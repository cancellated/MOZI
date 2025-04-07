using System.Collections;
using UnityEngine;

/// <summary>
/// 单例模式基类，提供全局唯一实例访问和自动初始化功能
/// 所有管理器应继承此类并实现抽象初始化方法
/// </summary>
/// <typeparam name="T">子类类型</typeparam>
public abstract class SingletonBase<T> : MonoBehaviour where T : SingletonBase<T>
{
    private static T _instance;
    private static readonly object _lock = new();
    private bool _isInitialized;

    /// <summary>
    /// 全局访问点，线程安全且支持延迟初始化
    /// </summary>
    public static T Instance
    {
        get
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<T>();
                    if (_instance == null)
                    {
                        GameObject singleton = new();
                        _instance = singleton.AddComponent<T>();
                    }
                }
                return _instance;
            }
        }
    }

    /// <summary>
    /// Unity生命周期方法，执行单例校验和初始化
    /// </summary>
    protected virtual void Awake()
    {
        // 单例冲突检测
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            // 设置单例实例并持久化
            _instance = this as T;
            DontDestroyOnLoad(gameObject);

            // 确保只初始化一次
            if (!_isInitialized)
            {
                // 延迟初始化到下一帧
                StartCoroutine(DelayedInitialize());
            }
        }
    }

    private IEnumerator DelayedInitialize()
    {
        // 等待一帧确保所有单例都已创建
        yield return null;
        
        Initialize();
        _isInitialized = true;
    }

    /// <summary>
    /// 抽象初始化方法，子类必须实现具体初始化逻辑
    /// </summary>
    protected abstract void Initialize();

    /// <summary>
    /// 对象销毁时清理单例引用
    /// </summary>
    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
            _isInitialized = false;
        }
    }
}