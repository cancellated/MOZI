using UnityEngine.Events;
using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 游戏事件类，用于定义和管理游戏中的各种事件。
/// 此类为静态类，提供了一系列静态的UnityEvent和事件触发接口。
/// </summary>
/// <summary>
/// 对话系统事件定义
/// </summary>
public static class GameEvents
{
    // 新增对话事件
    // 在GameEvents类中添加枚举定义
    public enum SceneContextType
    {
        LevelEntry,     // 默认关卡入口
        LevelSelection, // 选关界面入口
        StoryTrigger,   // 剧情触发入口
        DebugPortal     // 调试传送入口
    }
    
    public static readonly UnityEvent<List<DialogData>> OnDialogStart = new();
    public static readonly UnityEvent<SceneContextType> OnDialogEnd = new();

    // ====== 事件定义 ======
    /// <summary>
    /// 当首次启动完毕时触发的事件。
    /// </summary>
    public static event Action OnFirstLaunchCompleted;
    public static void TriggerFirstLaunchCompleted() => OnFirstLaunchCompleted?.Invoke();
    /// <summary>
    /// 当关卡解锁时触发的事件，传递解锁的关卡ID。
    /// </summary>
    public static UnityEvent<int> OnLevelUnlocked = new();

    /// <summary>
    /// 当关卡被选中时触发的事件（整合进入逻辑）
    /// 参数1: 关卡ID
    /// 参数2: 是否强制跳过剧情（默认false）
    /// </summary>
    public static readonly UnityEvent<int, bool> OnLevelSelected = new();
    /// <summary>
    /// 当关卡前剧情触发的事件，传递解锁的关卡ID。
    /// </summary>
    public static readonly UnityEvent<int> OnPreLevelStory = new();

    /// <summary>
    /// 当关卡完成时触发的事件，传递选中的关卡ID。
    /// </summary>
    public static readonly UnityEvent<int, bool> OnLevelComplete = new();

    /// <summary>
    /// 当故事开始时触发的事件。
    /// </summary>
    public static UnityEvent OnStoryBegin = new();

    /// <summary>
    /// 当故事结束时触发的事件。
    /// </summary>
    public static UnityEvent OnStoryEnd = new();

    // ====== 事件触发接口 ======
    /// <summary>
    /// 触发关卡前剧情事件。
    /// </summary>

    public static void TriggerPreLevelStory(int levelId)
    {
        OnPreLevelStory.Invoke(levelId);
        Debug.Log($"触发关卡前剧情: Level {levelId}");
    }
    /// <summary>
    /// 触发关卡解锁事件。
    /// </summary>
    /// <param name="levelId">解锁的关卡ID。</param>
    public static void TriggerLevelUnlocked(int levelId)
    {
        OnLevelUnlocked?.Invoke(levelId);
        Debug.Log($"关卡解锁事件触发: Level {levelId}");
    }
    /// <summary>
    /// 触发关卡进入事件。
    /// </summary>

    public static void TriggerLevelSelected(int levelId, bool forceSkipStory = false)
    {
        OnLevelSelected.Invoke(levelId, forceSkipStory);
        Debug.Log($"关卡选中事件触发: Level {levelId}, 跳过剧情: {forceSkipStory}");
    }

    /// <summary>
    /// 触发关卡完成事件。
    /// </summary>
    public static void TriggerLevelComplete(int levelId, bool hasViewedStory)
    {
        OnLevelComplete.Invoke(levelId, hasViewedStory);
        Debug.Log($"关卡完成事件触发: Level {levelId}, 已观看剧情: {hasViewedStory}");
    }

    /// <summary>
    /// 清除所有事件的监听器。
    /// </summary>


    // 更新清理方法
    public static void ClearAllListeners()
    {
        OnLevelUnlocked.RemoveAllListeners();
        OnLevelSelected.RemoveAllListeners();
        OnPreLevelStory.RemoveAllListeners();
        OnStoryBegin.RemoveAllListeners();
        OnStoryEnd.RemoveAllListeners();
    }
}