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
    public static readonly UnityEvent<List<DialogData>> OnDialogStart = new();
    public static readonly UnityEvent OnDialogEnd = new();

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
    /// 当关卡被选中时触发的事件，传递选中的关卡ID。
    /// </summary>
    public static UnityEvent<int> OnLevelSelected = new();

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
    /// 触发关卡解锁事件。
    /// </summary>
    /// <param name="levelId">解锁的关卡ID。</param>
    public static void TriggerLevelUnlocked(int levelId)
    {
        OnLevelUnlocked?.Invoke(levelId);
        Debug.Log($"关卡解锁事件触发: Level {levelId}");
    }

    /// <summary>
    /// 清除所有事件的监听器。
    /// </summary>
    public static void ClearAllListeners()
    {
        OnLevelUnlocked.RemoveAllListeners();
        OnLevelSelected.RemoveAllListeners();
        OnStoryBegin.RemoveAllListeners();
        OnStoryEnd.RemoveAllListeners();
    }
}