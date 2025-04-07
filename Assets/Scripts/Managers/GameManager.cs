using UnityEngine;
using System.Collections.Generic;
using System.IO;

// 在类定义前添加执行顺序属性
[DefaultExecutionOrder(-100)]
public class GameManager : SingletonBase<GameManager>
{
    public static class StoryConfig 
    {
        public const int PreStoryOffset = 1000;
        public const int PostStoryOffset = 2000;
    }

    [Header("玩家进度")]
    [SerializeField] private GameProgress _progress = new();

    // 添加初始化状态
    public bool IsInitialized { get; private set; } = false;

    protected override void Initialize()
    {
        LoadProgress();
        RegisterEventHandlers();
        ValidateProgress();
        
        // 标记初始化完成
        IsInitialized = true;
        Debug.Log("GameManager初始化完成");
    }

    #region 事件注册
    private void RegisterEventHandlers()
    {
        GameEvents.OnLevelEnter += HandleLevelEnter;
        GameEvents.OnLevelComplete += HandleLevelComplete;
        GameEvents.OnStoryEnter += HandleStoryEnter;
        GameEvents.OnStoryComplete += HandleStoryComplete;
    }

    protected override void OnDestroy()
    {
        GameEvents.OnLevelEnter -= HandleLevelEnter;
        GameEvents.OnLevelComplete -= HandleLevelComplete;
        GameEvents.OnStoryEnter -= HandleStoryEnter;
        GameEvents.OnStoryComplete -= HandleStoryComplete;
    }
    #endregion

    #region 事件处理
    private void HandleLevelEnter(int levelId)
    {
        SetCurrentLevel(levelId);
        Debug.Log($"进入关卡: {levelId}");
    }

    private void HandleLevelComplete(int levelId)
    {
        if (levelId == _progress.currentLevel && !IsLevelCompleted(levelId))
        {
            CompleteLevel(levelId);
            UnlockLevel(levelId + 1);
            
            int nextPreStoryId = StoryConfig.PreStoryOffset + (levelId + 1);
            UnlockStory(nextPreStoryId);
            
            SaveProgress();
            
            int postStoryId = StoryConfig.PostStoryOffset + levelId;
            if(NeedPlayStory(postStoryId)) 
            {
                GameEvents.TriggerStoryEnter(postStoryId);
            }
        }
    }

    private void HandleStoryEnter(int storyId)
    {
        SetCurrentStory(storyId);
        Debug.Log($"进入故事: {storyId}");
    }

    private void HandleStoryComplete(int storyId)
    {
        if (storyId == _progress.currentStory && !IsStoryCompleted(storyId))
        {
            CompeleteStory(storyId);
            SaveProgress();
            GameEvents.TriggerStoryComplete(storyId);
        }
    }

    public bool NeedPlayStory(int storyId)
    {
        bool isPostStory = storyId >= StoryConfig.PostStoryOffset;
        return !IsStoryCompleted(storyId) && 
               (isPostStory || IsLevelUnlocked(GetLevelIdFromStoryId(storyId)));
    }
    #endregion

    #region 获取/设置游戏进程参数
    public int GetCurrentLevel() => _progress.currentLevel;
    public void SetCurrentLevel(int levelId) => _progress.currentLevel = levelId;
    public int GetCurrentStory() => _progress.currentStory;
    public void SetCurrentStory(int storyId) => _progress.currentStory = storyId;

    public int GetCompletedLevels()
    {
        if (_progress.completedLevels.Count == 0)
            return 0;
        int maxLevel = 0;
        foreach (var level in _progress.completedLevels)
        {
            if (level.Value && level.Key > maxLevel)
                maxLevel = level.Key;
        }
        return maxLevel;
    }

    public int GetViewedStories()
    {
        if (_progress.completedStories.Count == 0)
            return 0;
        int maxStory = 0;
        foreach (var story in _progress.completedStories)
        {
            if (story.Value && story.Key > maxStory)
                maxStory = story.Key;
        }
        return maxStory;
    }

    public int CalculatePreStoryId(int levelId) => StoryConfig.PreStoryOffset + levelId;
    public int CalculatePostStoryId(int levelId) => StoryConfig.PostStoryOffset + levelId;
    public int GetLevelIdFromStoryId(int storyId) => storyId % 1000;
    public bool IsPostStory(int storyId) => storyId >= StoryConfig.PostStoryOffset;
    #endregion

    #region 存档管理
    public void SaveProgress()
    {
        string path = Path.Combine(Application.dataPath, "../SaveData/savegame.dat");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, JsonUtility.ToJson(_progress));
        Debug.Log($"进度已保存到: {path}");
    }

    private void LoadProgress()
    {
        string path = Path.Combine(Application.dataPath, "../SaveData/savegame.dat");
        if (File.Exists(path))
        {
            _progress = JsonUtility.FromJson<GameProgress>(File.ReadAllText(path));
            Debug.Log($"从 {path} 加载存档");
        }
        else
        {
            // 确保新游戏时第一关是解锁且未完成状态
            _progress.unlockedLevels[1] = true; 
            _progress.completedLevels[1] = false;
            Debug.Log("创建新存档，初始化默认值");
        }
    }
    #endregion

    #region 关卡管理
    public void CompleteLevel(int levelId)
    {
        if (levelId > 0 && !IsLevelCompleted(levelId))
        {
            _progress.completedLevels[levelId] = true;
            //GameEvents.TriggerLevelComplete(levelId);
            SaveProgress();
            Debug.Log($"完成关卡: {levelId}");
        }
    }

    public void UnlockLevel(int levelId)
    {
        if (levelId >= 0 && !IsLevelUnlocked(levelId))
        {
            _progress.unlockedLevels[levelId] = true;
            GameEvents.TriggerLevelUnlocked(levelId);
            SaveProgress();
            Debug.Log($"解锁关卡: {levelId}");
        }
    }

    public void CompeleteStory(int storyId)
    {
        if (storyId >= 0 && !IsStoryCompleted(storyId))
        {
            _progress.completedStories[storyId] = true;
            GameEvents.TriggerStoryComplete(storyId);
            SaveProgress();
            Debug.Log($"完成故事: {storyId}");
        }
    }

    public void UnlockStory(int storyId)
    {
        if (storyId > 0 && !IsStoryCompleted(storyId))
        {
            _progress.unlockedStories[storyId] = true;
            GameEvents.TriggerStoryUnlocked(storyId);
            SaveProgress();
            Debug.Log($"解锁故事: {storyId}");
        }
    }

    public bool IsLevelCompleted(int levelId) =>
        _progress.completedLevels.ContainsKey(levelId) && _progress.completedLevels[levelId];
    public bool IsLevelUnlocked(int levelId) => 
        _progress.unlockedLevels.ContainsKey(levelId) && _progress.unlockedLevels[levelId];
    public bool IsStoryUnlocked(int storyId) =>
        _progress.unlockedStories.ContainsKey(storyId) && _progress.unlockedStories[storyId];
    public bool IsStoryCompleted(int storyId) =>
        _progress.completedStories.ContainsKey(storyId) && _progress.completedStories[storyId];

    private void ValidateProgress()
    {
        int currentLevel = GetCurrentLevel();
        int currentStory = GetCurrentStory();
        
        if (!_progress.unlockedLevels.ContainsKey(currentLevel))
        {
            _progress.unlockedLevels[currentLevel] = true;
        }
        
        int preStoryId = StoryConfig.PreStoryOffset + currentLevel;
        if (!_progress.unlockedStories.ContainsKey(preStoryId))
        {
            _progress.unlockedStories[preStoryId] = true;
        }
        
        if (!_progress.completedStories.ContainsKey(currentStory))
        {
            _progress.completedStories[currentStory] = false;
        }
    }
    #endregion

    [System.Serializable]
    public class GameProgress
    {
        public int currentLevel = 1;
        public int currentStory = StoryConfig.PreStoryOffset + 1;
        public Dictionary<int, bool> completedStories = new();
        public Dictionary<int, bool> completedLevels = new();
        public Dictionary<int, bool> unlockedLevels = new();
        public Dictionary<int, bool> unlockedStories = new();
    }
}