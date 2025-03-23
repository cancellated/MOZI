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
    /// <summary>
    /// 单例初始化方法，执行以下操作：
    /// 1. 组件初始化
    /// 2. 注册场景加载回调
    /// 3. 验证必要组件引用
    /// </summary>
    protected override void Initialize()
    {
        InitializeComponents();
        SceneManager.sceneLoaded += OnSceneLoaded;
        ToggleLoadingScreen(false);
        ValidateComponents();
    }

    /// <summary>
    /// 组件初始化方法，配置进度条基础参数
    /// </summary>
    private void InitializeComponents()
    {
        progressBar.type = Image.Type.Filled;
        progressBar.fillMethod = Image.FillMethod.Horizontal;
        progressBar.fillAmount = 0;
    }

    /// <summary>
    /// 组件引用验证方法，确保关键组件已正确配置
    /// </summary>
    private void ValidateComponents()
    {
        if (loadingScreen == null)
            Debug.LogError("加载界面引用缺失！", this);

        if (progressBar == null)
            Debug.LogError("进度条组件引用缺失！", this);
    }

    /// <summary>
    /// 场景加载完成回调，执行资源清理操作
    /// </summary>
    /// <param name="scene">加载完成的场景</param>
    /// <param name="mode">场景加载模式</param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
    }
    #endregion

    #region 核心加载功能
    /// <summary>
    /// 加载指定关卡ID对应的场景
    /// </summary>
    /// <param name="levelId">从1开始的关卡ID</param>
    public void LoadLevel(int levelId)
    {
        string sceneName = GameManager.Instance.GetLevelScene(levelId);
        if (ValidateSceneName(sceneName))
        {
            StartCoroutine(LoadLevelRoutine(sceneName));
        }
    }

    /// <summary>
    /// 异步加载场景的完整流程协程
    /// </summary>
    /// <param name="sceneName">目标场景名称</param>
    /// <returns>加载过程迭代器</returns>
    private IEnumerator LoadLevelRoutine(string sceneName)
    {
        // 阶段1：播放淡出动画
        yield return StartCoroutine(FadeOut());

        // 阶段2：显示加载界面
        ToggleLoadingScreen(true);

        // 阶段3：开始异步加载
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        float timer = 0f;
        while (!operation.isDone)
        {
            // 计算加载进度（0-90%范围）
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            UpdateProgressUI(progress);

            // 满足最小加载时间要求后激活场景
            if (operation.progress >= 0.9f && timer >= minLoadTime)
            {
                break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // 阶段4：完成加载
        operation.allowSceneActivation = true;
        ToggleLoadingScreen(false);

        // 阶段5：播放淡入动画
        yield return StartCoroutine(FadeIn());
    }
    #endregion

    #region 辅助方法
    /// <summary>
    /// 验证场景名称是否有效
    /// </summary>
    /// <param name="sceneName">待验证场景名称</param>
    /// <returns>是否有效</returns>
    private bool ValidateSceneName(string sceneName)
    {
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"无效的场景名称: {sceneName}");
            return false;
        }
        return true;
    }

    /// <summary>
    /// 控制加载界面的显示状态
    /// </summary>
    /// <param name="show">是否显示加载界面</param>
    private void ToggleLoadingScreen(bool show)
    {
        loadingScreen.alpha = show ? 1 : 0;
        loadingScreen.blocksRaycasts = show;
    }

    /// <summary>
    /// 更新加载进度显示
    /// </summary>
    /// <param name="progress">当前加载进度（0-1）</param>
    private void UpdateProgressUI(float progress)
    {
        progressBar.fillAmount = progress;
    }
    #endregion

    #region 动画控制
    /// <summary>
    /// 场景切换前的淡出动画协程
    /// （需接入具体动画系统实现）
    /// </summary>
    /// <returns>动画过程迭代器</returns>
    private IEnumerator FadeOut()
    {
        // 实际项目中应接入具体动画系统
        yield return new WaitForSeconds(fadeOutDuration);
    }

    /// <summary>
    /// 进入新场景后的淡入动画协程
    /// （需接入具体动画系统实现）
    /// </summary>
    /// <returns>动画过程迭代器</returns>
    private IEnumerator FadeIn()
    {
        // 实际项目中应接入具体动画系统
        yield return new WaitForSeconds(fadeOutDuration);
    }
    #endregion
}