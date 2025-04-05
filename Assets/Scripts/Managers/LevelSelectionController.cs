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

        // 获取前置故事ID（使用LevelSelectButton的枚举和生成规则）
        int preStoryId = 1000 + levelId;
        int postStoryId = 2000 + levelId;

        // 检查是否需要播放前置故事
        if (!GameManager.Instance.IsStoryCompleted(preStoryId))
        {
            GameEvents.TriggerStoryEnter(preStoryId);
            GameEvents.OnStoryComplete += (id) => {
                if(id == preStoryId) LoadLevel(levelId);
            };
        }
        // 检查是否需要播放后置故事（当关卡已完成但后置故事未播放时）
        else if (GameManager.Instance.IsLevelCompleted(levelId) && 
                !GameManager.Instance.IsStoryCompleted(postStoryId))
        {
            GameEvents.TriggerStoryEnter(postStoryId);
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

    // 更新按钮状态方法
    private void UpdateButtonState(int levelId)
    {
        if (_levelButtons.TryGetValue(levelId, out var button))
        {
            bool isUnlocked = GameManager.Instance.IsLevelUnlocked(levelId);
            button.UpdateVisualState(isUnlocked);
            
            // 根据需求决定是否隐藏已完成的关卡按钮
            if (ShouldHideButton(levelId))
            {
                button.gameObject.SetActive(false);
            }
        }
    }

    // 修改隐藏条件判断
    private bool ShouldHideButton(int levelId)
    {
        int postStoryId = 2000 + levelId;
        return GameManager.Instance.IsLevelCompleted(levelId) && 
               GameManager.Instance.IsStoryCompleted(postStoryId);
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