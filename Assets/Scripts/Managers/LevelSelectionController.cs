using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 关卡选择系统控制器，功能包括：
/// 1. 动态生成关卡选择按钮
/// 2. 根据游戏进度更新按钮状态
/// 3. 处理关卡选择事件
/// </summary>
public class LevelSelectionController : SingletonBase<LevelSelectionController>
{
    #region 编辑器配置
    [Header("按钮容器")]
    [Tooltip("用于存放关卡按钮的父物体")]
    [SerializeField] private Transform buttonContainer;

    [Header("按钮预制体")]
    [Tooltip("关卡按钮预制体，必须包含Button和Text组件")]
    [SerializeField] private GameObject buttonPrefab;

    [Header("视觉反馈")]
    [Tooltip("解锁状态的按钮颜色")]
    [SerializeField] private Color unlockedColor = Color.white;

    [Tooltip("锁定状态的按钮颜色")]
    [SerializeField] private Color lockedColor = Color.gray;
    #endregion

    #region 运行时数据
    private Dictionary<int, Button> _levelButtons = new();
    #endregion

    #region 初始化
    /// <summary>
    /// 初始化时生成所有关卡按钮
    /// </summary>
    protected override void Initialize()
    {
        GenerateLevelButtons();
        SubscribeToEvents();
    }

    /// <summary>
    /// 根据总关卡数动态生成按钮
    /// </summary>
    private void GenerateLevelButtons()
    {

    }

    /// <summary>
    /// 配置单个按钮的显示和行为
    /// </summary>
    private void SetupButton(GameObject buttonObj, int levelId)
    {
        // 获取组件引用
        Button btn = buttonObj.GetComponent<Button>();
        Text btnText = buttonObj.GetComponentInChildren<Text>();
        Image btnImage = buttonObj.GetComponent<Image>();

        // 设置显示内容
        btnText.text = $"关卡 {levelId}";
        btn.interactable = GameManager.Instance.IsLevelUnlocked(levelId);
        btnImage.color = btn.interactable ? unlockedColor : lockedColor;

        // 绑定点击事件
        btn.onClick.AddListener(() => OnLevelSelected(levelId));
    }
    #endregion

    #region 事件处理
    /// <summary>
    /// 订阅相关游戏事件
    /// </summary>
    private void SubscribeToEvents()
    {
        
    }

    /// <summary>
    /// 当关卡被解锁时更新对应按钮状态
    /// </summary>
    private void UpdateButtonState(int unlockedLevelId)
    {
        if (_levelButtons.TryGetValue(unlockedLevelId, out Button button))
        {
            button.interactable = true;
            button.GetComponent<Image>().color = unlockedColor;
            PlayUnlockAnimation(button.gameObject);
        }
    }

    /// <summary>
    /// 播放按钮解锁动画
    /// </summary>
    private void PlayUnlockAnimation(GameObject button)
    {
        // 实际项目中可接入DOTween或Animator

    }
    #endregion

    #region 用户输入处理
    /// <summary>
    /// 处理关卡选择事件
    /// </summary>
    private void OnLevelSelected(int selectedLevelId)
    {
    }

    /// <summary>
    /// 播放关卡锁定反馈效果
    /// </summary>
    private void PlayLockedFeedback(GameObject button)
    {
        
    }
    #endregion
}