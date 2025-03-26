using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// 场景加载管理系统，功能包括：
/// 1. 异步加载场景并显示加载进度
/// 2. 管理场景切换的过渡动画
/// 3. 处理场景加载的错误情况
/// </summary>
public class SceneLoader : SingletonBase<SceneLoader>
{
    #region 编辑器配置
    [Header("加载界面")]
    [Tooltip("加载过程中显示的界面")]
    [SerializeField] private CanvasGroup loadingScreen;

    [Tooltip("显示加载进度的进度条组件")]
    [SerializeField] private Image progressBar;

    [Header("加载参数")]
    [Tooltip("最小加载时间（防止进度条瞬间完成）")]
    [SerializeField] private float minLoadTime = 1.5f;

    [Tooltip("场景切换淡出动画时间")]
    [SerializeField] private float fadeOutDuration = 0.5f;
    #endregion

    #region 初始化与销毁
    protected override void Initialize()
    {
        InitializeComponents();
        SceneManager.sceneLoaded += OnSceneLoaded;
        ToggleLoadingScreen(false);
        ValidateComponents();
    }

    private void InitializeComponents()
    {
        progressBar.type = Image.Type.Filled;
        progressBar.fillMethod = Image.FillMethod.Horizontal;
        progressBar.fillAmount = 0;
    }

    private void ValidateComponents()
    {
        if (loadingScreen == null)
            Debug.LogError("加载界面引用缺失！", this);

        if (progressBar == null)
            Debug.LogError("进度条组件引用缺失！", this);
    }
    #endregion

    #region 场景工具方法
    public static string GetCurrentSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }
    #endregion

    #region 场景加载回调
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
        var context = GetCurrentSceneContext(scene.name);
        GameEvents.OnSceneContextChanged.Invoke(context);
    }

    private GameEvents.SceneContextType GetCurrentSceneContext(string sceneName)
    {
        return sceneName switch
        {
            "MainMenu" => GameEvents.SceneContextType.LevelSelection,
            _ => GameEvents.SceneContextType.LevelEntry
        };
    }
    #endregion

    #region 核心加载功能
    public void LoadLevel(int levelId)
    {
        string sceneName = GameManager.Instance.GetLevelScene(
            levelId, 
            GameEvents.SceneContextType.LevelEntry
        );
        
        if (ValidateSceneName(sceneName))
        {
            StartCoroutine(LoadLevelRoutine(sceneName));
        }
    }

    public void LoadSceneDirect(string sceneName)
    {
        if (ValidateSceneName(sceneName))
        {
            StartCoroutine(LoadLevelRoutine(sceneName));
        }
    }
    
    private IEnumerator LoadLevelRoutine(string sceneName)
    {
        yield return StartCoroutine(FadeOut());
        ToggleLoadingScreen(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        float timer = 0f;
        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            UpdateProgressUI(progress);

            if (operation.progress >= 0.9f && timer >= minLoadTime) break;

            timer += Time.deltaTime;
            yield return null;
        }

        operation.allowSceneActivation = true;
        var context = GetCurrentSceneContext(sceneName);
        // 参数顺序修正为（上下文类型，章节ID）
        DialogConfigManager.Instance.LoadSceneDialogs(context, GetChapterId(sceneName));
        
        ToggleLoadingScreen(false);
        yield return StartCoroutine(FadeIn());
    }
    #endregion

    #region 辅助方法
    private bool ValidateSceneName(string sceneName)
    {
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"无效的场景名称: {sceneName}");
            return false;
        }
        return true;
    }

    private void ToggleLoadingScreen(bool show)
    {
        loadingScreen.alpha = show ? 1 : 0;
        loadingScreen.blocksRaycasts = show;
    }

    private void UpdateProgressUI(float progress)
    {
        progressBar.fillAmount = progress;
    }
    #endregion

    #region 动画控制
    private IEnumerator FadeOut()
    {
        GameEvents.OnSceneTransitionRequest.Invoke(
            GetCurrentTransitionType(GameEvents.SceneTransitionType.LevelToMenu));
        yield return new WaitForSeconds(fadeOutDuration);
    }

    private IEnumerator FadeIn()
    {
        yield return new WaitForSeconds(fadeOutDuration);
    }

    private GameEvents.SceneTransitionType GetCurrentTransitionType(
        GameEvents.SceneTransitionType defaultType)
    {
        // 根据当前场景上下文返回实际过渡类型
        var currentContext = GetCurrentSceneContext(GetCurrentSceneName());
        return currentContext switch
        {
            GameEvents.SceneContextType.StoryTrigger => GameEvents.SceneTransitionType.StoryToLevel,
            _ => defaultType
        };
    }
    #endregion

    #region 场景卸载功能
    public void ExitToMainMenu()
    {
        StartCoroutine(ExitSceneRoutine(GameManager.Instance.StartScene));
    }

    private IEnumerator ExitSceneRoutine(string targetScene)
    {
        GameEvents.OnSceneTransitionRequest.Invoke(
            GameEvents.SceneTransitionType.LevelToMenu);
        
        yield return StartCoroutine(FadeOut());
        
        AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(
            SceneManager.GetActiveScene());
        while (!unloadOperation.isDone) yield return null;

        yield return StartCoroutine(LoadLevelRoutine(targetScene));
    }
    #endregion

    /// <summary>
    /// 安全解析场景名称中的章节ID（使用TryParse避免格式异常）
    /// </summary>
    private int GetChapterId(string sceneName)
    {
        const string prefix = "Level";
        if (sceneName.StartsWith(prefix) && 
            int.TryParse(sceneName.Substring(prefix.Length), out int chapterId))
        {
            return chapterId;
        }
        return 0; // 返回0表示无效章节
    }
}