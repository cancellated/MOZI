using UnityEngine;

/// <summary>
/// 第二关管理器，负责关卡逻辑和状态管理
/// </summary>
public class Level2_Manager : MonoBehaviour
{
    [Header("关卡设置")]
    [Tooltip("光线管理器")]
    [SerializeField] private LightBeamManager lightBeamManager;
    
    [Header("音频管理")] 
    [SerializeField] private AudioManager audioManager;

    private bool isLevelComplete = false;

    private void Start()
    {
        // 进入关卡时播放背景音乐
        if (audioManager != null)
        {
            audioManager.PlayBackgroundMusic();
        }
    }

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
        // 切换为通关音乐
        if (audioManager != null)
        {
            audioManager.StopBackgroundMusic();
            audioManager.PlayCompleteMusic();
        }
        
        GameEvents.TriggerLevelComplete(2);
        Debug.Log("第二关完成！");
    }
}