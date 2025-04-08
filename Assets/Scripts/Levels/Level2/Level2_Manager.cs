using UnityEngine;

/// <summary>
/// 第二关管理器，负责关卡逻辑和状态管理
/// </summary>
public class Level2_Manager : MonoBehaviour
{
    [Header("关前引导")]
    [Tooltip("关前解说图")]
    [SerializeField] private GameObject guideImage; // 新增关前引导图

    [Header("关卡设置")]
    [Tooltip("光线管理器")]
    [SerializeField] private LightBeamManager lightBeamManager;
    
    [Header("音频管理")] 
    [SerializeField] private AudioManager audioManager;

    private bool isLevelComplete = false;

    [Header("点燃火堆")] 
    [SerializeField] private GameObject completeImage;


    private void Start()
    {
        // 显示关前引导图
        if (guideImage != null)
        {
            guideImage.SetActive(true);
        }

        // 播放背景音乐
        if (audioManager != null)
        {
            audioManager.PlayBackgroundMusic();
        }
    }

    private void Update()
    {
        // 检测点击事件以关闭引导图
        if (guideImage != null && guideImage.activeSelf && Input.GetMouseButtonDown(0))
        {
            guideImage.SetActive(false);
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
        completeImage.SetActive(true);
        GameEvents.TriggerLevelComplete(2);
        Debug.Log("第二关完成！");
    }
}