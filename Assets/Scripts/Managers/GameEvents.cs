using UnityEngine.Events;
using UnityEngine;

public static class GameEvents
{
    // ====== 事件定义 ======
    public static UnityEvent<int> OnLevelUnlocked = new UnityEvent<int>();
    public static UnityEvent<int> OnLevelSelected = new UnityEvent<int>();
    public static UnityEvent OnStoryBegin = new UnityEvent();
    public static UnityEvent OnStoryEnd = new UnityEvent();

    // ====== 事件触发接口 ======
    public static void TriggerLevelUnlocked(int levelId)
    {
        OnLevelUnlocked?.Invoke(levelId);
        Debug.Log($"关卡解锁事件触发: Level {levelId}");
    }

    public static void ClearAllListeners()
    {
        OnLevelUnlocked.RemoveAllListeners();
        OnLevelSelected.RemoveAllListeners();
        OnStoryBegin.RemoveAllListeners();
        OnStoryEnd.RemoveAllListeners();
    }
}