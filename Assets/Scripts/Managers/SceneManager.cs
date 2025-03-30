using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SceneManager : SingletonBase<SceneManager>
{
    [Header("场景配置")]
    [SerializeField] private string startScene = "Start Scene";
    [SerializeField] private string levelSelectScene = "Level Select Scene";
    [SerializeField] private List<string> levelScenes = new() { "Level_1" };
    [SerializeField] private string storyScene = "StoryScene";

    protected override void Initialize()
    {
        GameEvents.OnLevelEnter += HandleLevelSelected;
        GameEvents.OnSceneTransitionRequest += HandleSceneTransition;
    }

    // 添加 override 关键字以重写基类的 OnDestroy 方法
    protected override void OnDestroy()
    {
        GameEvents.OnLevelEnter -= HandleLevelSelected;
        GameEvents.OnSceneTransitionRequest -= HandleSceneTransition;
    }

    private void HandleLevelSelected(int levelId)
    {
        if (levelId > 0 && levelId <= levelScenes.Count)
        {
            // 通过GameManager判断是否需要播放故事
            bool needPlayStory = GameManager.Instance.NeedPlayStory(levelId);
            
            if(needPlayStory)
            {
                // 设置当前关卡ID后再加载故事场景
                GameManager.Instance.SetCurrentLevel(levelId);
                // 根据关卡ID获取对话ID
                int dialogId = GameManager.Instance.GetDialogId(levelId);
                Debug.Log($"当前关卡 {levelId} 对应的对话ID是 {dialogId}");
                LoadScene(storyScene);
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
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }
    }


