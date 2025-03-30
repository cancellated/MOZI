using UnityEngine.Events;
using System;

/// <summary>
/// 游戏事件系统，用于模块间通信
/// </summary>
public static class GameEvents
{
    #region 场景事件
    public static event Action<int> OnLevelEnter;
    public static event Action<int> OnLevelComplete; 
    public static event Action<int> OnStoryEnter;
    public static event Action<int> OnStoryComplete;
    public static event Action<int> OnLevelUnlocked;
    public static event Action<int> OnStoryUnlocked;

    public static event Action<SceneTransitionType> OnSceneTransitionRequest;


    /// <summary>
    /// 触发场景进入事件
    /// </summary>
    public static void TriggerSceneEnter(int levelId)
    {
        OnLevelEnter?.Invoke(levelId);
    }

    /// <summary>
    /// 触发场景完成事件
    /// </summary>
    public static void TriggerLevelComplete(int levelId)
    {
        OnLevelComplete?.Invoke(levelId);
    }

    /// <summary>
    /// 触发故事进入事件
    /// </summary>
    public static void TriggerStoryEnter(int storyId){
        OnStoryEnter?.Invoke(storyId);
    }
    
    /// <summary>
    /// 触发故事完成事件
    /// </summary>
    public static void TriggerStoryComplete(int storyId){
        OnStoryComplete?.Invoke(storyId);
    }
    /// <summary>
    /// 触发关卡解锁事件
    /// </summary>
    public static void TriggerLevelUnlocked(int levelId)
    {
        OnLevelUnlocked?.Invoke(levelId);
    }

    /// <summary>
    /// 触发故事解锁事件
    /// </summary>
    public static void TriggerStoryUnlocked(int storyId)
    {
        OnStoryUnlocked?.Invoke(storyId);
    }

    #endregion
    /// <summary>
    /// 触发场景切换事件
    /// </summary>
    public static void TriggerSceneTransition(SceneTransitionType transitionType)
    {
        OnSceneTransitionRequest?.Invoke(transitionType);
    }

    public enum SceneTransitionType
{
    ToMainMenu,
    ToLevelSelect,
    ToLevel,
    ToStory
}
}