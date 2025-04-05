using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class GameManager : SingletonBase<GameManager>
{
    [Header("玩家进度")]
    [SerializeField] private GameProgress _progress = new();

    protected override void Initialize()
    {
        LoadProgress();
        RegisterEventHandlers();
        ValidateProgress();
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
    }
    #endregion

    #region 事件处理
    private void HandleLevelEnter(int levelId)
    {
        SetCurrentLevel(levelId);
        Debug.Log($"进入关卡: {levelId}");
    }

    // 修改HandleLevelComplete方法
    private void HandleLevelComplete(int levelId)
    {
        // 添加状态检查
        if (levelId == _progress.currentLevel && !IsLevelCompleted(levelId))
        {
            CompleteLevel(levelId);
            UnlockLevel(levelId + 1);
            int nextStoryId = CalculatePreStoryId(levelId + 1);
            UnlockStory(nextStoryId);
            SaveProgress();
            int postStoryId = CalculatePostStoryId(levelId);
            GameEvents.TriggerStoryEnter(postStoryId);
        }
    }

    private void HandleStoryEnter(int storyId)
    {
        SetCurrentStory(storyId);
        Debug.Log($"进入故事: {storyId}");
    }

    // 修改HandleStoryComplete方法
    private void HandleStoryComplete(int storyId)
    {
        if (storyId == _progress.currentStory && !IsStoryCompleted(storyId))
    {
        CompeleteStory(storyId);
        SaveProgress();
        GameEvents.TriggerStoryComplete(storyId);
    }
    }
    public bool NeedPlayStory(int storyId){
        return !IsStoryCompleted(storyId); 
    }

        #region 获取/设置游戏进程参数
            // 获取/设置当前关卡
            public int GetCurrentLevel(){
                return _progress.currentLevel; 
            }
            public void SetCurrentLevel(int levelId){
                _progress.currentLevel = levelId;
            }
            public int GetCurrentStory(){
                return _progress.currentStory; 
            }
            public void SetCurrentStory(int storyId){
                _progress.currentStory = storyId;
            }
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
            public int CalculatePreStoryId(int levelId) => 1000 + levelId;
            public int CalculatePostStoryId(int levelId) => 2000 + levelId;
            public int GetLevelIdFromStoryId(int storyId) => storyId % 1000;
            public bool IsPostStory(int storyId) => storyId >= 2000;
        #endregion

    #endregion

    #region 存档管理
    public void SaveProgress()
    {
        string path = Path.Combine(Application.persistentDataPath, "savedata", "savegame.dat");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, JsonUtility.ToJson(_progress));
        Debug.Log("进度已保存");
    }

    private void LoadProgress()
    {
        string path = Path.Combine(Application.persistentDataPath, "savedata", "savegame.dat");
        if (File.Exists(path))
        {
            _progress = JsonUtility.FromJson<GameProgress>(File.ReadAllText(path));
        }
    }
    #endregion

    #region 关卡管理
    // 完成关卡和解锁关卡
    public void CompleteLevel(int levelId)
    {
        if (levelId > 0 && !IsLevelCompleted(levelId))
        {
            _progress.completedLevels[levelId] = true;
            GameEvents.TriggerLevelComplete(levelId);
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
        if (storyId > 0 &&!IsStoryCompleted(storyId))
        {
            _progress.unlockedStories[storyId] = true;
            GameEvents.TriggerStoryUnlocked(storyId);
            SaveProgress();
            Debug.Log($"解锁故事: {storyId}");
        }
    }
    #region 进度检查
        // 检查关卡和故事是否已解锁或已完成
        public bool IsLevelCompleted(int levelId) =>
            _progress.completedLevels.ContainsKey(levelId) && _progress.completedLevels[levelId];
        public bool IsLevelUnlocked(int levelId) => 
            _progress.unlockedLevels.ContainsKey(levelId) && _progress.unlockedLevels[levelId];
        public bool IsStoryUnlocked(int storyId) =>
            _progress.unlockedStories.ContainsKey(storyId) && _progress.unlockedStories[storyId];
        public bool IsStoryCompleted(int storyId) =>
            _progress.completedStories.ContainsKey(storyId) && _progress.completedStories[storyId];
    #endregion
    // 进度验证
    private void ValidateProgress()
    {
        int currentLevel = GetCurrentLevel();
        int currentStory = GetCurrentStory();
        
        // 修正关卡解锁验证
        if (!_progress.unlockedLevels.ContainsKey(currentLevel))
        {
            _progress.unlockedLevels[currentLevel] = true;
        }
        
        // 修正故事解锁验证（使用前置故事ID）
        int preStoryId = CalculatePreStoryId(currentLevel);
        if (!_progress.unlockedStories.ContainsKey(preStoryId))
        {
            _progress.unlockedStories[preStoryId] = true;
        }
        
        // 修正故事完成验证
        if (!_progress.completedStories.ContainsKey(currentStory))
        {
            _progress.completedStories[currentStory] = false; // 初始应为未完成
        }
    }
    #endregion

    [System.Serializable]
    public class GameProgress
    {
        public int currentLevel = 1;
        public int currentStory = 1;
        public Dictionary<int, bool> completedStories = new();
        public Dictionary<int, bool> completedLevels = new();
        public Dictionary<int, bool> unlockedLevels = new();
        public Dictionary<int, bool> unlockedStories = new();
    }
}