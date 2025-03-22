using UnityEngine;
using System.Collections.Generic;

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
    private List<string> levelScenes = new List<string>
    {
        "Level_1", "Level_2", "Level_Boss"
    };

    [Header("玩家进度")]
    [Tooltip("当前游戏进度数据，包含解锁状态等信息")]
    [SerializeField] private GameProgress _progress = new();

    #region 核心接口

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
    /// 将当前进度序列化存储到PlayerPrefs
    /// </summary>
    public void SaveProgress()
    {
        string json = JsonUtility.ToJson(_progress);
        PlayerPrefs.SetString("GameProgress", json);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 从PlayerPrefs加载存档数据
    /// </summary>
    private void LoadProgress()
    {
        if (PlayerPrefs.HasKey("GameProgress"))
        {
            _progress = JsonUtility.FromJson<GameProgress>(
                PlayerPrefs.GetString("GameProgress")
            );
        }
    }
    #endregion

    #region 场景接口

    /// <summary>
    /// 根据关卡ID获取对应的场景名称
    /// </summary>
    /// <exception cref="System.ArgumentOutOfRangeException">当ID超出范围时抛出</exception>
    public string GetLevelScene(int levelId)
    {
        if (levelId < 1 || levelId > levelScenes.Count)
            throw new System.ArgumentOutOfRangeException(nameof(levelId));

        return levelScenes[levelId - 1];
    }
    #endregion

    /// <summary>
    /// 游戏进度数据类，存储玩家游戏状态
    /// </summary>
    [System.Serializable]
    public class GameProgress
    {
        [Tooltip("当前所在关卡ID")]
        public int currentLevel = 1;

        [Tooltip("关卡解锁状态字典，Key为关卡ID")]
        public Dictionary<int, bool> unlockedLevels = new Dictionary<int, bool>();

        [Tooltip("剧情观看状态字典，Key为关卡ID")]
        public Dictionary<int, bool> viewedStory = new Dictionary<int, bool>();
    }
}