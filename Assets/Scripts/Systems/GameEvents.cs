using UnityEngine.Events;
using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 游戏全局事件系统，负责管理跨系统的游戏事件通信
/// 使用说明：
/// 1. 事件订阅：各系统在初始化时订阅相关事件
/// 2. 事件触发：游戏逻辑触发点调用TriggerXXX方法
/// 3. 事件清理：场景切换时使用ClearAllListeners
/// </summary>
public static class GameEvents
{
    #region 场景上下文类型
    /// <summary>
    /// 场景上下文类型，定义场景加载的不同入口类型
    /// </summary>
    public enum SceneContextType
    {
        LevelEntry,     // 常规关卡入口（默认进入点）
        LevelSelection, // 选关界面入口（从主菜单进入）
        StoryTrigger,   // 剧情触发入口（关卡间的过场动画）
        DebugPortal     // 调试用快速通道入口
    }
    #endregion

    #region 场景过渡类型
    /// <summary>
    /// 场景过渡类型，定义场景之间的合法跳转路径
    /// </summary>
    public enum SceneTransitionType
    {
        LevelToStory,    // 关卡玩法场景→剧情过场场景
        StoryToLevel,    // 剧情过场场景→关卡玩法场景 
        StoryToMenu,     // 剧情过场场景→主菜单场景
        LevelToMenu      // 关卡玩法场景→主菜单场景
    }
    #endregion

    #region 核心事件定义
    /// <summary>
    /// 场景过渡请求事件（参数：请求的过渡类型）
    /// 触发时机：当需要执行场景跳转时
    /// </summary>
    public static readonly UnityEvent<SceneTransitionType> OnSceneTransitionRequest = new();
    
    /// <summary>
    /// 关卡选中事件（参数1：关卡ID，参数2：场景过渡类型）
    /// 触发时机：玩家在选关界面选择具体关卡时
    /// </summary>
    public static readonly UnityEvent<int, SceneTransitionType> OnLevelSelected = new();

    /// <summary>
    /// 场景上下文变更事件（参数：新的场景上下文类型）
    /// 触发时机：每次场景加载完成后
    /// </summary>
    public static readonly UnityEvent<SceneContextType> OnSceneContextChanged = new();

    /// <summary>
    /// 关卡前剧情触发事件（参数：目标关卡ID）
    /// 触发时机：进入需要播放前置剧情的关卡时
    /// </summary>
    public static readonly UnityEvent<int> OnPreLevelStory = new();

    /// <summary>
    /// 对话开始事件（参数：对话数据列表）
    /// 触发时机：当新对话序列开始时
    /// </summary>
    public static readonly UnityEvent<List<DialogData>> OnDialogStart = new();

    /// <summary>
    /// 对话结束事件（参数：触发对话的场景上下文类型）
    /// 触发时机：当对话序列正常结束时
    /// </summary>
    public static readonly UnityEvent<SceneContextType> OnDialogEnd = new();

    /// <summary>
    /// 首次启动完成事件
    /// 触发时机：玩家完成首次启动流程时
    /// </summary>
    public static event Action OnFirstLaunchCompleted;

    /// <summary>
    /// 关卡解锁事件（参数：解锁的关卡ID）
    /// 触发时机：当新关卡可用时
    /// </summary>
    public static UnityEvent<int> OnLevelUnlocked = new();

    /// <summary>
    /// 关卡完成事件（参数1：关卡ID，参数2：是否已观看剧情）
    /// 触发时机：玩家成功完成关卡时
    /// </summary>
    public static readonly UnityEvent<int, bool> OnLevelComplete = new();

    /// <summary>
    /// 故事开始事件
    /// 触发时机：过场动画开始时
    /// </summary>
    public static UnityEvent OnStoryBegin = new();

    /// <summary>
    /// 故事结束事件
    /// 触发时机：过场动画正常结束时
    /// </summary>
    public static UnityEvent OnStoryEnd = new();
    #endregion

    #region 事件触发接口
    /// <summary>
    /// 触发首次启动完成事件
    /// </summary>
    public static void TriggerFirstLaunchCompleted() => OnFirstLaunchCompleted?.Invoke();

    /// <summary>
    /// 触发关卡解锁事件
    /// </summary>
    /// <param name="levelId">解锁的关卡ID</param>
    public static void TriggerLevelUnlocked(int levelId)
    {
        OnLevelUnlocked?.Invoke(levelId);
        Debug.Log($"关卡解锁事件触发: Level {levelId}");
    }

    /// <summary>
    /// 触发关卡选中事件
    /// </summary>
    /// <param name="levelId">目标关卡ID</param>
    /// <param name="transitionType">场景过渡类型</param>
    public static void TriggerLevelSelected(int levelId, SceneTransitionType transitionType)
    {
        OnLevelSelected.Invoke(levelId, transitionType);
        Debug.Log($"关卡选中事件触发: Level {levelId}, 过渡类型: {transitionType}");
    }

    /// <summary>
    /// 触发关卡前剧情事件
    /// </summary>
    /// <param name="levelId">目标关卡ID</param>
    public static void TriggerPreLevelStory(int levelId)
    {
        OnPreLevelStory.Invoke(levelId);
        Debug.Log($"触发关卡前剧情: Level {levelId}");
    }

    /// <summary>
    /// 触发关卡完成事件
    /// </summary>
    /// <param name="levelId">完成的关卡ID</param>
    /// <param name="hasViewedStory">是否已观看关联剧情</param>
    public static void TriggerLevelComplete(int levelId, bool hasViewedStory)
    {
        OnLevelComplete.Invoke(levelId, hasViewedStory);
        Debug.Log($"关卡完成事件触发: Level {levelId}, 已观看剧情: {hasViewedStory}");
    }
    #endregion

    #region 系统维护
    /// <summary>
    /// 清除所有事件的监听器
    /// 使用时机：场景切换或游戏重置时
    /// </summary>
    public static void ClearAllListeners()
    {
        OnLevelUnlocked.RemoveAllListeners();
        OnLevelSelected.RemoveAllListeners();
        OnPreLevelStory.RemoveAllListeners();
        OnStoryBegin.RemoveAllListeners();
        OnStoryEnd.RemoveAllListeners();
        OnDialogStart.RemoveAllListeners();
        OnDialogEnd.RemoveAllListeners();
    }
    #endregion
}
