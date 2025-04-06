using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SceneManager : SingletonBase<SceneManager>
{
    [Header("场景配置")]
    [SerializeField] private string startScene = "Start Scene";
    [SerializeField] private string levelSelectScene = "Level Select Scene";
    [SerializeField] private List<string> levelScenes = new() { "Level_1" };
    [SerializeField] private string dialogScene = "Dialog";

    [Header("加载设置")]
    [SerializeField] private string loadingScene = "Loading Scene";
    private string _targetScene;
    private int _targetStoryId;

    protected override void Initialize()
    {
        GameEvents.OnLevelEnter += HandleLevelSelected;
        GameEvents.OnSceneTransitionRequest += HandleSceneTransition;
    }

    protected override void OnDestroy()
    {
        GameEvents.OnLevelEnter -= HandleLevelSelected;
        GameEvents.OnSceneTransitionRequest -= HandleSceneTransition;
    }

    private void HandleLevelSelected(int levelId)
    {
        if (levelId > 0 && levelId <= levelScenes.Count)
        {
            int preStoryId = GameManager.Instance.CalculatePreStoryId(levelId);
            bool needPlayStory = GameManager.Instance.NeedPlayStory(preStoryId);
            
            if(needPlayStory)
            {
                GameManager.Instance.SetCurrentLevel(levelId);
                _targetStoryId = preStoryId;
                
                // 注册故事完成事件处理
                GameEvents.OnStoryComplete += HandleStoryComplete;
                
                LoadScene(dialogScene);
            }
            else
            {
                LoadScene(levelScenes[levelId - 1]);
            }
        }
    }

    private void HandleStoryComplete(int storyId)
    {
        if(storyId == _targetStoryId)
        {
            // 取消注册事件
            GameEvents.OnStoryComplete -= HandleStoryComplete;
            
            // 加载目标关卡
            int levelId = GameManager.Instance.GetCurrentLevel();
            LoadScene(levelScenes[levelId - 1]);
        }
    }

    private void HandleSceneTransition(GameEvents.SceneTransitionType transitionType)
    {
        switch (transitionType)
        {
            case GameEvents.SceneTransitionType.ToMainMenu:
                LoadScene(startScene);
                break;
            case GameEvents.SceneTransitionType.ToLevelSelect:
                LoadScene(levelSelectScene);
                break;
            case GameEvents.SceneTransitionType.ToLevel:
                int currentLevel = GameManager.Instance.GetCurrentLevel();
                if(currentLevel > 0 && currentLevel <= levelScenes.Count)
                {
                    LoadScene(levelScenes[currentLevel - 1]);
                }
                break;
            case GameEvents.SceneTransitionType.ToStory:
                int currentStory = GameManager.Instance.GetCurrentStory();
                if(currentStory > 0)
                {
                    _targetStoryId = currentStory; // 存储故事ID
                    LoadScene(dialogScene);
                }
            break;
        }
    }

    private void LoadScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            _targetScene = sceneName;
            Debug.Log($"准备加载场景: {sceneName}");
            UnityEngine.SceneManagement.SceneManager.LoadScene(loadingScene);
        }
    }

    public string GetTargetScene()
    {
        return _targetScene;
    }

    public int GetTargetStoryId()
    {
        return _targetStoryId;
    }
    
    public float GetLoadingProgress()
    {
        var asyncOp = UnityEngine.SceneManagement.SceneManager.GetSceneByName(_targetScene).isLoaded ? 
            null : 
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(_targetScene);
        return asyncOp?.progress ?? 0f;
    }
}