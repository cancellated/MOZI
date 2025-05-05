using UnityEngine.Events;
using System;
using UnityEngine;
using Unity.Collections;

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
    public static event Action<int> OnCGEnter;
    public static event Action<int> OnCGComplete;
    public static event Action<bool> OnMapLock;
    public static event Action<int> OnMapDialogEnter;

    public static event Action<SceneTransitionType> OnSceneTransitionRequest;

    public static event Action<int> OnChapterComplete;


    // 触发关卡进入事件

    public static void TriggerLevelEnter(int levelId)
    {
        OnLevelEnter?.Invoke(levelId);
    }

    // 触发关卡完成事件

    public static void TriggerLevelComplete(int levelId)
    {
        OnLevelComplete?.Invoke(levelId);
    }

    // 触发故事进入事件

    public static void TriggerStoryEnter(int storyId){
        OnStoryEnter?.Invoke(storyId);
    }
    
    // 触发故事完成事件
    public static void TriggerStoryComplete(int storyId){
        OnStoryComplete?.Invoke(storyId);
    }


    //触发关卡解锁事件
    public static void TriggerLevelUnlocked(int levelId)
    {
        OnLevelUnlocked?.Invoke(levelId);
    }

    //触发故事解锁事件
    public static void TriggerStoryUnlocked(int storyId)
    {
        OnStoryUnlocked?.Invoke(storyId);
    }

    //触发CG进入事件
        public static void TriggerCGEnter(int cgId)
    {
        OnCGEnter?.Invoke(cgId);
    }
    
    //触发CG完成事件
    public static void TriggerCGComplete(int cgId)
    {
        OnCGComplete?.Invoke(cgId);
    }

    //触发章节完成事件
    public static void TriggerChapterComplete(int chapterId)
    {
        OnChapterComplete?.Invoke(chapterId);
    }
    //触发地图锁定事件
    public static void TriggerMapLock(bool isLocked)
    {
        OnMapLock?.Invoke(isLocked);
    }

    //触发地图对话进入事件
    public static void TriggerMapDialogEnter(int DialogId)
    {
        OnMapDialogEnter?.Invoke(DialogId);
    }
    #endregion

    #region 场景切换
    public static void TriggerSceneTransition(SceneTransitionType transitionType)
    {
        OnSceneTransitionRequest?.Invoke(transitionType);
    }

    public enum SceneTransitionType
    {
        ToLevelSelect,
        ToLevel,   // 需要配合LevelID
        ToStory,    // 需要配合StoryID
    }
    #endregion

}