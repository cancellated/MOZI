using UnityEngine;
using UnityEngine.UI;

public class UIManager : SingletonBase<UIManager>
{
    // ====== 组件绑定 ======
    [Header("核心界面")]
    [SerializeField] private CanvasGroup startScreen;
    [SerializeField] private CanvasGroup mapScreen;
    [SerializeField] private CanvasGroup levelPopup;

    [Header("基础配置")]
    [SerializeField] private Font defaultFont;
    [SerializeField] private int baseFontSize = 24;

    protected override void Initialize()
    {
        ApplyGlobalFontSettings();
        GameEvents.OnLevelUnlocked.AddListener(UpdateLevelButtons);
    }

    private void ApplyGlobalFontSettings()
    {
        foreach (Text text in FindObjectsOfType<Text>())
        {
            text.font = defaultFont;
            text.fontSize = Mathf.RoundToInt(baseFontSize * Screen.height / 1080f);
        }
    }

    private void UpdateLevelButtons(int unlockedLevel)
    {
        // 实际项目中需实现按钮状态更新逻辑
        Debug.Log($"更新关卡按钮: {unlockedLevel}");
    }
}