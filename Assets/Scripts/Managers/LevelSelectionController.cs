using UnityEngine;
using System.Collections.Generic;

public class LevelSelectionController : SingletonBase<LevelSelectionController>
{
    [Header("UI配置")]
    [SerializeField] private GameObject storyReviewPanel;

    private readonly Dictionary<int, LevelSelectButton> _levelButtons = new();

    protected override void Initialize()
    {
        InitializeLevelButtons();
        RegisterEventHandlers();
        storyReviewPanel.SetActive(false);
    }

    public void OnLevelButtonClicked(int levelId)
    {
        if (!GameManager.Instance.IsLevelUnlocked(levelId))
            return;

        if (!GameManager.Instance.IsStoryViewed(levelId))
        {
            // 触发剧情播放
            GameEvents.TriggerStoryEnter(levelId);
            // 订阅剧情完成事件
            GameEvents.OnStoryComplete += (id) => {
                if(id == levelId) LoadLevel(levelId);
            };
        }
        else
        {
            LoadLevel(levelId);
        }
    }

    private void LoadLevel(int levelId)
    {
        GameManager.Instance.SetCurrentLevel(levelId);
        GameEvents.TriggerSceneTransition(GameEvents.SceneTransitionType.ToLevel);
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