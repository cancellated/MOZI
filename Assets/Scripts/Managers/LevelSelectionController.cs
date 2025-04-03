using UnityEngine;
using System.Collections.Generic;

public class LevelSelectionController : SingletonBase<LevelSelectionController>
{
    [Header("UI配置")]
    [SerializeField] private GameObject storyReviewPanel;

    private Dictionary<int, LevelSelectButton> _levelButtons = new();

    protected override void Initialize()
    {
        // 初始化场景中的关卡按钮
        InitializeLevelButtons();
        RegisterEventHandlers();
    
        // 初始化时隐藏面板
        storyReviewPanel.SetActive(false);
    }

    private void InitializeLevelButtons()
    {
        var buttons = FindObjectsByType<LevelSelectButton>(FindObjectsSortMode.None);
        foreach (var button in buttons)
        {
            RegisterLevelButton(button.targetLevelId, button);
            UpdateButtonState(button.targetLevelId);
        }
    }

    private void RegisterLevelButton(int levelId, LevelSelectButton button)
    {
        if (!_levelButtons.ContainsKey(levelId))
        {
            _levelButtons.Add(levelId, button);
        }
    }

    private void RegisterEventHandlers()
    {
        GameEvents.OnLevelComplete += UpdateButtonState;
        GameEvents.OnStoryComplete += UpdateButtonState;
        GameEvents.OnLevelUnlocked += UpdateButtonState;
    }

    private void UpdateButtonState(int levelId)
    {
        if (_levelButtons.TryGetValue(levelId, out var button))
        {
            button.UpdateButtonState();
            
            // 自动隐藏已完成的关卡按钮
            if (ShouldHideButton(levelId))
            {
                button.gameObject.SetActive(false);
            }
        }
    }

    private bool ShouldHideButton(int levelId)
    {
        return GameManager.Instance.IsLevelCompleted(levelId) && 
               GameManager.Instance.IsStoryViewed(levelId);
    }

    public void OnStoryReviewButtonClicked()
    {
        // 确保每次点击都能正确切换面板状态
        if(storyReviewPanel != null)
        {
            bool newState = !storyReviewPanel.activeSelf;
            storyReviewPanel.SetActive(newState);
            Debug.Log($"面板状态切换至: {newState}");
        }
    }

    public void ReturnToLevelSelect()
    {
        GameEvents.TriggerSceneTransition(GameEvents.SceneTransitionType.ToLevelSelect);
    }

    protected override void OnDestroy()
    {
        GameEvents.OnLevelComplete -= UpdateButtonState;
        GameEvents.OnStoryComplete -= UpdateButtonState;
        GameEvents.OnLevelUnlocked -= UpdateButtonState;
    }
}