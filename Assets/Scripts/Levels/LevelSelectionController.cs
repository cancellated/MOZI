using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class LevelSelectionController : MonoBehaviour
{
    [Header("UI配置")]
    [SerializeField] private GameObject storyReviewPanel;
    [SerializeField] private CanvasGroup mapCanvasGroup;

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
        GameEvents.OnMapLock += OnMapLockChanged;
        GameEvents.OnMapDialogEnter += OnMapDialogEnter;
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
        return GameManager.Instance.IsLevelCompleted(levelId) && 
               GameManager.Instance.IsStoryCompleted(GameManager.Instance.CalculatePostStoryId(levelId));
    }

    // 进入地图对话
    private void OnMapDialogEnter(int dialogId)
{
    // 锁定地图操作
    GameEvents.TriggerMapLock(true);
    
}
    private void OnMapLockChanged(bool isLocked)
    {
        if(mapCanvasGroup != null)
        {
            mapCanvasGroup.interactable = !isLocked;
            mapCanvasGroup.blocksRaycasts = !isLocked;
        }
    }


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
        if(lastLevel == 0) 
        {
            Debug.Log("使用默认起始点");
            return;
        }
        RectTransform departurePoint = FindDeparturePoint(lastLevel);
        if(departurePoint == null) 
        {
            Debug.LogError($"未找到关卡 {lastLevel} 的出发点");
            return;
        }
        
        Debug.Log($"设置关卡 {lastLevel} 的出发点，位置: {departurePoint.anchoredPosition}");
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if(player != null)
        {
            if(player.TryGetComponent<RectTransform>(out var playerRect))
            {
                Debug.Log($"Player当前位置: {playerRect.anchoredPosition}");
                playerRect.anchoredPosition = departurePoint.anchoredPosition;
                Debug.Log($"Player新位置: {playerRect.anchoredPosition}");
            }
            else
            {
                Debug.LogError("Player对象缺少RectTransform组件");
            }
        }
        else
        {
            Debug.LogError("未找到Player对象");
        }
    }
    
    private RectTransform FindDeparturePoint(int levelId)
    {
        Debug.Log($"正在查找关卡 {levelId} 的出发点");
        if(levelId == 0) 
        {
            GameObject startPoint = GameObject.Find("Start Point");
            if (startPoint != null)
            {
                Debug.Log("使用默认起始点");
                return startPoint.GetComponent<RectTransform>();
            }
            Debug.LogError("未找到Start Point对象");
            return null;
        }
        
        foreach(var button in _levelButtons.Values)
        {
            if(button.targetLevelId == levelId)
            {
                Transform foundTransform = button.transform.Find("Departure");
                RectTransform departure = null;
                if (foundTransform != null)
                {
                    departure = foundTransform.GetComponent<RectTransform>();
                }
                if(departure != null) 
                {
                    Debug.Log($"找到关卡 {levelId} 的出发点");
                    return departure;
                }
                Debug.LogError($"关卡 {levelId} 按钮缺少Departure子物体");
            }
        }
        Debug.LogError($"未找到关卡 {levelId} 的按钮");
        return null;
    }

    private void OnDestroy()
    {
        GameEvents.OnLevelComplete -= UpdateButtonState;
        GameEvents.OnStoryComplete -= UpdateButtonState;
        GameEvents.OnLevelUnlocked -= UpdateButtonState;
        GameEvents.OnMapLock -= OnMapLockChanged;
    }
}

