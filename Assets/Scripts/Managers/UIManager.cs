using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 用户界面管理系统，负责：
/// 1. 全局UI元素的生命周期管理
/// 2. 字体和基础UI样式的统一配置
/// 3. 界面切换的动画控制
/// </summary>
public class UIManager : SingletonBase<UIManager>
{
    #region 编辑器配置
    [Header("核心界面")]
    [Tooltip("游戏启动时的初始界面")]
    [SerializeField] private CanvasGroup startScreen;

    [Tooltip("关卡选择地图界面")]
    [SerializeField] private CanvasGroup mapScreen;

    [Tooltip("关卡操作弹窗界面")]
    [SerializeField] private CanvasGroup levelPopup;

    [Header("字体配置")]
    [Tooltip("全局默认中文字体")]
    [SerializeField] private Font defaultFont;

    [Tooltip("基础字号（根据屏幕高度动态调整）")]
    [SerializeField] private int baseFontSize = 24;
    #endregion

    #region 初始化
    /// <summary>
    /// 初始化UI系统并配置全局字体
    /// </summary>
    protected override void Initialize()
    {
        ApplyGlobalFontSettings();
        SubscribeToEvents();
    }

    /// <summary>
    /// 为所有文本组件应用全局字体设置
    /// </summary>
    private void ApplyGlobalFontSettings()
    {
        foreach (Text text in FindObjectsOfType<Text>())
        {
            text.font = defaultFont;
            text.fontSize = CalculateDynamicFontSize();
        }
    }

    /// <summary>
    /// 动态计算适应屏幕的字号
    /// </summary>
    private int CalculateDynamicFontSize()
    {
        // 基于1080p分辨率进行比例缩放
        return Mathf.RoundToInt(baseFontSize * Screen.height / 1080f);
    }
    #endregion

    #region 事件处理
    /// <summary>
    /// 订阅必要的全局事件
    /// </summary>
    private void SubscribeToEvents()
    {
        GameEvents.OnLevelUnlocked.AddListener(UpdateLevelButtonState);
    }

    /// <summary>
    /// 当关卡解锁时更新对应按钮状态
    /// </summary>
    /// <param name="unlockedLevelId">已解锁的关卡ID</param>
    private void UpdateLevelButtonState(int unlockedLevelId)
    {
        // 实际项目中需实现具体按钮查找逻辑
        Debug.Log($"更新关卡{unlockedLevelId}按钮的可交互状态");
    }
    #endregion

    #region 界面控制
    /// <summary>
    /// 显示指定界面并隐藏当前界面
    /// </summary>
    /// <param name="screen">目标界面CanvasGroup</param>
    public void ShowScreen(CanvasGroup screen)
    {
        StartCoroutine(TransitionScreens(mapScreen, screen));
    }

    /// <summary>
    /// 执行界面过渡动画的协程
    /// </summary>
    private IEnumerator TransitionScreens(CanvasGroup current, CanvasGroup next)
    {
        yield return StartCoroutine(FadeCanvas(current, 1f, 0f, 0.3f));
        yield return StartCoroutine(FadeCanvas(next, 0f, 1f, 0.3f));
    }

    /// <summary>
    /// 控制CanvasGroup的淡入淡出效果
    /// </summary>
    private IEnumerator FadeCanvas(CanvasGroup group, float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            group.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        group.alpha = endAlpha;
    }
    #endregion
}