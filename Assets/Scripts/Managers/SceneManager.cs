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
    [SerializeField] private string loadingScene = "LoadingScene";
    private string _targetScene;

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
        // 使用CalculatePreStoryId获取前置故事ID
        int preStoryId = GameManager.Instance.CalculatePreStoryId(levelId);
        bool needPlayStory = GameManager.Instance.NeedPlayStory(preStoryId);
        
        if(needPlayStory)
        {
            GameManager.Instance.SetCurrentLevel(levelId);
            // 移除旧的GetDialogId调用，直接使用故事ID
            Debug.Log($"准备播放关卡 {levelId} 的前置故事: {preStoryId}");
            LoadScene(dialogScene);
        }
        else
        {
            LoadScene(levelScenes[levelId - 1]);
        }
    }
    }

    private void HandleSceneTransition(GameEvents.SceneTransitionType transitionType)
    {
        switch (transitionType)
        {
            case GameEvents.SceneTransitionType.ToLevelSelect:
                LoadScene(levelSelectScene);
                break;
            case GameEvents.SceneTransitionType.ToLevel:
                int currentLevel = GameManager.Instance.GetCurrentLevel();
                LoadScene(levelScenes[currentLevel - 1]);
                break;
        }
    }

private void LoadScene(string sceneName)
{
    if (!string.IsNullOrEmpty(sceneName))
    {
        _targetScene = sceneName;
        Debug.Log($"准备加载场景: {sceneName}");
        // 先切换到加载场景
        UnityEngine.SceneManagement.SceneManager.LoadScene(loadingScene);
    }
}

    public string GetTargetScene()
    {
        return _targetScene;
    }

    public float GetLoadingProgress()
    {
        // 修改为正确的获取AsyncOperation的方式
        var asyncOp = UnityEngine.SceneManagement.SceneManager.GetSceneByName(_targetScene).isLoaded ? 
            null : 
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(_targetScene);
        return asyncOp?.progress ?? 0f;
    }
}