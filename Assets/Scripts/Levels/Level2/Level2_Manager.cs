using UnityEngine;

/// <summary>
/// 第二关管理器，负责关卡逻辑和状态管理
/// </summary>
public class Level2_Manager : MonoBehaviour
{
    [Header("关卡设置")]
    [Tooltip("光线管理器")]
    [SerializeField] private LightBeamManager lightBeamManager;
    
    [Tooltip("是否关卡已完成")]
    private bool isLevelComplete = false;

    /// <summary>
    /// 触发关卡完成
    /// </summary>
    public void TriggerLevelComplete()
    {
        if (!isLevelComplete)
        {
            isLevelComplete = true;
            HandleLevelComplete();
        }
    }

    /// <summary>
    /// 处理关卡完成逻辑
    /// </summary>
    private void HandleLevelComplete()
    {
        // 触发关卡完成事件
        GameEvents.TriggerLevelComplete(2);
        Debug.Log("第二关完成！");
    }
}