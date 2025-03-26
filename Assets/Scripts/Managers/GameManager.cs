using UnityEngine;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// 游戏中枢管理系统，负责：
/// 1. 关卡数据管理
/// 2. 玩家进度存储
/// 3. 全局状态协调
/// </summary>
public class GameManager : SingletonBase<GameManager>
{
    [Header("关卡配置")]
    [Tooltip("按顺序存储关卡场景名称，索引对应关卡ID-1")]
    [SerializeField]
    private List<string> levelScenes = new()
    {
        "Level_1", "Level_2", "Level_3"
    };

    [Header("玩家进度")]
    [Tooltip("当前游戏进度数据，包含解锁状态等信息")]
    [SerializeField] private GameProgress _progress = new();

    #region 核心接口
    /// <summary>
    /// 是否首次启动游戏
    /// </summary>
    public bool IsFirstLaunch() => _progress.isFirstLaunch;

    /// <summary>
    /// 首次进入游戏（播放完成后调用）
    /// </summary>
    public void CompleteFirstLaunch()
    {
        _progress.isFirstLaunch = false;
        SaveProgress();
        GameEvents.TriggerFirstLaunchCompleted(); // 新增事件通知
    }

    /// <summary>
    /// 获取总关卡数
    /// </summary>
    public int TotalLevels => levelScenes.Count;

    /// <summary>
    /// 检查指定关卡是否已解锁
    /// </summary>
    /// <param name="levelId">关卡ID（从1开始）</param>
    public bool IsLevelUnlocked(int levelId)
    {
        return _progress.unlockedLevels.ContainsKey(levelId) &&
               _progress.unlockedLevels[levelId];
    }

    /// <summary>
    /// 解锁指定关卡并触发相关事件
    /// </summary>
    /// <param name="levelId">要解锁的关卡ID</param>
    public void UnlockLevel(int levelId)
    {
        if (levelId < 1 || levelId > TotalLevels) return;

        if (!_progress.unlockedLevels.ContainsKey(levelId))
        {
            _progress.unlockedLevels.Add(levelId, true);
            GameEvents.TriggerLevelUnlocked(levelId);
            SaveProgress();
        }
    }
    #endregion

    #region 初始化与持久化

    /// <summary>
    /// 初始化游戏数据，加载存档并验证完整性
    /// </summary>
    protected override void Initialize()
    {
        LoadProgress();
        ValidateProgress();
    }

    /// <summary>
    /// 验证进度数据完整性，确保至少解锁第一关
    /// </summary>
    private void ValidateProgress()
    {
        if (!_progress.unlockedLevels.ContainsKey(1))
        {
            _progress.unlockedLevels[1] = true;
        }
    }

    /// <summary>
    /// 将当前进度序列化存储到文件系统
    /// </summary>
    public void SaveProgress()
    {
        string json = JsonUtility.ToJson(_progress);
        string saveDir = Path.Combine(Application.persistentDataPath, "savedata");
        Directory.CreateDirectory(saveDir); // 确保目录存在
        string savePath = Path.Combine(saveDir, "savegame.dat");
        File.WriteAllText(savePath, json);
    }

    /// <summary>
    /// 从文件系统加载存档数据
    /// </summary>
    private void LoadProgress()
    {
        string savePath = Path.Combine(
            Application.persistentDataPath, 
            "savedata", 
            "savegame.dat"
        );

        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            _progress = JsonUtility.FromJson<GameProgress>(json);
        }
    }
    #endregion


    #region 关卡进入逻辑
/// <summary>
/// 处理关卡进入逻辑（自动判断是否需要播放剧情）
/// </summary>
public void HandleLevelSelection(int levelId, bool forceSkipStory = false)
{
    if (!_progress.viewedStory.ContainsKey(levelId))
    {
        _progress.viewedStory[levelId] = false;
    }

    bool hasViewed = _progress.viewedStory[levelId] || forceSkipStory;
    
    if (!hasViewed)
    {
        GameEvents.TriggerPreLevelStory(levelId); // ✅ 使用新定义的事件
        LoadStoryScene(levelId);
    }
    else
    {
        LoadGameplayScene(levelId);
    }
}



private void LoadStoryScene(int levelId)
{
    string storyScene = GetLevelScene(levelId, GameEvents.SceneContextType.StoryTrigger);
    SceneLoader.Instance.LoadSceneDirect(storyScene);
}

private void LoadGameplayScene(int levelId)
{
    string levelScene = GetLevelScene(levelId, GameEvents.SceneContextType.LevelEntry);
    SceneLoader.Instance.LoadSceneDirect(levelScene);
}
#endregion
    #region 关卡完成逻辑
/// <summary>
/// 处理关卡完成逻辑
/// </summary>
public void CompleteLevel(int levelId)
{
    bool hasViewed = _progress.viewedStory.ContainsKey(levelId) && _progress.viewedStory[levelId];
    
    // 统一使用事件驱动方式
    GameEvents.TriggerLevelComplete(levelId, hasViewed);
    
    if (!hasViewed)
    {
        _progress.viewedStory[levelId] = true;
        SaveProgress();
        GameEvents.OnDialogEnd.Invoke(GameEvents.SceneContextType.StoryTrigger);
    }
    else
    {
        SceneLoader.Instance.LoadSceneDirect(LevelSelectScene);
    }
}

/// <summary>
/// 标记某个关卡的剧情为已观看状态
/// </summary>
public void MarkStoryViewed(int levelId)
{
    _progress.viewedStory[levelId] = true;
    SaveProgress();
}
#endregion


    #region 场景接口
    [Tooltip("主菜单场景名称")]
    [SerializeField] private string _startscene = "Start Scene";
    
    [Tooltip("选关场景名称")]
    [SerializeField] private string _levelSelectScene = "Level Select Scene";

    /// <summary>
    /// 获取主菜单场景名称
    /// </summary>
    public string StartScene => _startscene;

    /// <summary>
    /// 获取选关场景名称
    /// </summary>
    public string LevelSelectScene => _levelSelectScene;
 
    /// <summary>
    /// 根据关卡ID获取对应的场景名称
    /// </summary>
    /// <exception cref="System.ArgumentOutOfRangeException">当ID超出范围时抛出</exception>
    [System.Serializable]
    public class SceneMapping
    {
        [Tooltip("场景ID（与关卡ID一致）")]
        public int levelId;
        
        [Tooltip("场景类型上下文")]
        public GameEvents.SceneContextType contextType;
        
        [Tooltip("Unity场景名称")]
        public string sceneName;
    }
    
    
    [Header("场景配置")]
    [Tooltip("场景映射关系配置")]
    [SerializeField] 
    private List<SceneMapping> sceneMappings = new()
    {
        new SceneMapping{ levelId=1, contextType=GameEvents.SceneContextType.LevelEntry, sceneName="Level_1" }
    };
    public string GetLevelScene(int levelId, GameEvents.SceneContextType context)
    {
        var mapping = sceneMappings.Find(m => m.levelId == levelId && m.contextType == context);
        return mapping?.sceneName ?? levelScenes[levelId - 1]; // 保持向后兼容
    }
    #endregion

    /// <summary>
    /// 游戏进度数据类，存储玩家游戏状态
    /// </summary>
    [System.Serializable]
    public class GameProgress
    {
        [Tooltip("是否首次启动游戏")]
        public bool isFirstLaunch = true;
        [Tooltip("当前所在关卡ID")]
        public int currentLevel = 1;

        [Tooltip("关卡解锁状态字典，Key为关卡ID")]
        public Dictionary<int, bool> unlockedLevels = new();

        [Tooltip("剧情观看状态字典，Key为关卡ID")]
        public Dictionary<int, bool> viewedStory = new();
    }
}

