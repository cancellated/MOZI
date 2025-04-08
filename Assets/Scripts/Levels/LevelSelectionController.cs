using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[DefaultExecutionOrder(-50)] 
public class LevelSelectionController : MonoBehaviour
{
    [Header("UI配置")]
    [SerializeField] private GameObject storyReviewPanel;

    [Header("音频管理")]
    [SerializeField] private AudioManager audioManager;

    private readonly Dictionary<int, LevelSelectButton> _levelButtons = new();

    private void Start()
    {
        // 确保GameManager已初始化
        if(!GameManager.Instance.IsInitialized)
        {
            Debug.LogError("GameManager未初始化！");
            StartCoroutine(WaitForGameManagerInitialization());
            return;
        }

        // 播放选关界面BGM
        if(audioManager != null)
        {
            audioManager.PlayBackgroundMusic();
        }
        else
        {
            Debug.LogError("AudioManager未赋值！");
        }

        InitializeLevelButtons();
        RegisterEventHandlers();
        
        if(storyReviewPanel != null)
        {
            storyReviewPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("storyReviewPanel未赋值！");
        }
        
        Debug.Log($"关卡选择控制器初始化 - 第一关状态: 解锁:{GameManager.Instance.IsLevelUnlocked(1)} 完成:{GameManager.Instance.IsLevelCompleted(1)}");
    }

    public void OnLevelButtonClicked(int levelId)
    {
        if (!GameManager.Instance.IsLevelUnlocked(levelId))
            return;

        int preStoryId = 1000 + levelId;
        int postStoryId = 2000 + levelId;

        // 检查是否需要播放前置故事
        if (!GameManager.Instance.IsStoryCompleted(preStoryId))
        {
            GameManager.Instance.SetCurrentLevel(levelId);
            GameEvents.TriggerStoryEnter(preStoryId);
        }
        // 检查是否需要播放后置故事
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
        Debug.Log($"请求加载关卡: {levelId}");
        GameManager.Instance.SetCurrentLevel(levelId);
        GameEvents.TriggerSceneTransition(GameEvents.SceneTransitionType.ToLevel, levelId);
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
            bool shouldHide = !isUnlocked || IsLevelFullyCompleted(levelId);
            
            button.gameObject.SetActive(!shouldHide);
            button.UpdateVisualState(isUnlocked);
            
            Debug.Log($"关卡 {levelId} 状态 - 解锁:{isUnlocked} 完成:{shouldHide}");
        }
    }
    
    // 判断关卡是否完全完成(通关且后置故事已完成)
    private bool IsLevelFullyCompleted(int levelId)
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

    private void OnDestroy()
    {
        GameEvents.OnLevelComplete -= UpdateButtonState;
        GameEvents.OnStoryComplete -= UpdateButtonState;
        GameEvents.OnLevelUnlocked -= UpdateButtonState;
    }

    private IEnumerator WaitForGameManagerInitialization()
    {
        while(!GameManager.Instance.IsInitialized)
        {
            yield return null;
        }
        
        InitializeLevelButtons();
        RegisterEventHandlers();
        
        if(storyReviewPanel != null)
        {
            storyReviewPanel.SetActive(false);
        }
    }
}