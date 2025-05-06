using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class LevelSelectionController : MonoBehaviour
{
    [Header("UI配置")]
    [SerializeField] private GameObject storyReviewPanel;
    [SerializeField] private CanvasGroup mapDialogCanvasGroup;

    [Header("音频管理")]
    [SerializeField] private AudioManager audioManager;

    [Header("角色移动")]
    [SerializeField] private MoveToLevel moveToLevel;


    private readonly Dictionary<int, LevelSelectButton> _levelButtons = new();

    private void Start()
    {
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
        SetInitialCharacterPosition();
        RegisterEventHandlers();
        
        // 添加这行确保动画状态正确初始化
        if(moveToLevel != null)
        {
            StartCoroutine(moveToLevel.MoveToTargetLevel(GameManager.Instance.GetLastPlayedLevel()));
        }
        
        if(storyReviewPanel != null)
        {
            storyReviewPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("storyReviewPanel未赋值！");
        }
}

    public void OnLevelButtonClicked(int levelId)
    {
        Debug.Log($"点击关卡按钮: {levelId}");
        
        if(moveToLevel != null)
        {
            StartCoroutine(HandleLevelTransition(levelId));
        }
    }

    private IEnumerator HandleLevelTransition(int levelId)
    {
        // 等待移动动画完成
        yield return moveToLevel.MoveToTargetLevel(levelId);
        
        // 检查是否需要播放地图对话 (地图对话ID = 3000 + levelId)
        int mapStoryId = levelId + GameManager.StoryConfig.MapSroryOffset;
        if(!GameManager.Instance.IsStoryCompleted(mapStoryId))
        {
            Debug.Log($"需要播放地图对话: {mapStoryId}");
            GameEvents.TriggerMapStoryEnter(mapStoryId);
            
            // 等待地图对话完成
            yield return new WaitUntil(() => GameManager.Instance.IsStoryCompleted(mapStoryId));
        }
        
        // 检查是否需要播放前故事
        int preStoryId = GameManager.Instance.CalculatePreStoryId(levelId);
        if(GameManager.Instance.NeedPlayStory(preStoryId))
        {
            Debug.Log($"需要播放前故事: {preStoryId}");
            GameEvents.TriggerStoryEnter(preStoryId);
        }
        else
        {
            Debug.Log($"直接进入关卡: {levelId}");
            GameEvents.TriggerLevelEnter(levelId);
        }
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
            //bool shouldHide = !isUnlocked || IsLevelFullyCompleted(levelId);
            
            //button.gameObject.SetActive(!shouldHide);
            button.UpdateVisualState(isUnlocked);
            
            //Debug.Log($"关卡 {levelId} 状态 - 解锁:{isUnlocked} 完成:{shouldHide}");
        }
    }
    
    // 判断关卡是否完全完成(通关且后置故事已完成)
    // private bool IsLevelFullyCompleted(int levelId)
    // {
    //     return GameManager.Instance.IsLevelCompleted(levelId) && 
    //            GameManager.Instance.IsStoryCompleted(GameManager.Instance.CalculatePostStoryId(levelId));
    // }




    // 设置初始角色位置
    private void SetInitialCharacterPosition()
    {
        if(moveToLevel == null || GameManager.Instance == null) 
        {
            Debug.LogError("MoveToLevel或GameManager未初始化");
            return;
        }
        int lastLevel = GameManager.Instance.GetLastPlayedLevel();
        moveToLevel.SetIdleState(lastLevel);
        Debug.Log($"获取的最后游玩关卡ID: {lastLevel}");
    }
    


    private void OnDestroy()
    {
        GameEvents.OnLevelComplete -= UpdateButtonState;
        GameEvents.OnStoryComplete -= UpdateButtonState;
        GameEvents.OnLevelUnlocked -= UpdateButtonState;
    }
}

