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
    [Header("通关动画")]
    [Tooltip("通关UI动画控制器")]
    [SerializeField] private Animator completeAnimator;
    [Tooltip("通关动画bool参数名称")]
    [SerializeField] private string completeAnimationBool = "isComplete";

    private void HandleLevelComplete()
    {
        // 切换为通关音乐
        if (audioManager != null)
        {
            audioManager.StopBackgroundMusic();
            audioManager.PlayCompleteMusic();
        }
        
        // 播放通关动画
        if (completeAnimator != null)
        {
            completeAnimator.SetBool(completeAnimationBool, true);
        }
        
        // 延迟1秒触发关卡完成事件
        StartCoroutine(DelayLevelComplete());
        
        Debug.Log("第二关完成！");
    }

    private System.Collections.IEnumerator DelayLevelComplete()
    {
        yield return new WaitForSeconds(1f);
        GameEvents.TriggerLevelComplete(2);
    }
}