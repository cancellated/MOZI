using UnityEngine.Events;
using System;
using UnityEngine;

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
    public static void TriggerLevelEnter(int levelId)
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

    #region 场景切换
    /// <summary>
    /// 触发场景切换事件（新版带ID参数）
    /// </summary>
    public static void TriggerSceneTransition(SceneTransitionType transitionType, int id = 0)
    {
        // 需要ID的场景(故事和关卡)必须提供有效ID
        if ((transitionType == SceneTransitionType.ToLevel || 
             transitionType == SceneTransitionType.ToStory) && id <= 0)
        {
            Debug.LogError($"切换{transitionType}场景必须提供有效ID");
            return;
        }

        OnSceneTransitionRequest?.Invoke(transitionType);

        // 自动触发关联事件
        switch(transitionType)
        {
            case SceneTransitionType.ToLevel:
                TriggerLevelEnter(id);
                break;
            case SceneTransitionType.ToStory:
                TriggerStoryEnter(id);
                break;
        }
    }

    public enum SceneTransitionType
    {
        ToMainMenu,
        ToLevelSelect,
        ToLevel,   // 需要配合LevelID
        ToStory    // 需要配合StoryID
    }
    #endregion
}