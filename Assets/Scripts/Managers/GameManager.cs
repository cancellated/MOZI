using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using System.Linq;

public class GameManager : SingletonBase<GameManager>
{
    public static class StoryConfig 
    {
        public const int PreStoryOffset = 1000;
        public const int PostStoryOffset = 2000;
        public const int MapSroryOffset = 3000;
    }

    public static class CGConfig
    {
        public const int CGOffset = 10000;
        public const int ChapterCGOffset = 20000;
    }

    [Header("场景配置")]
    public string startScene = "Start Scene";
    public string levelSelectScene = "Level Select Scene";
    public List<string> levelScenes = new() { "Level_1", "Level_2" };
    public string dialogScene = "Dialog";

    [Header("玩家进度")]
    [SerializeField] private GameProgress _progress = new();
    
    [Header("章节末尾CG")]
    [SerializeField] private List<int> chapterEndVideoMappings = new();

    protected override void Initialize()
    {
        LoadProgress();
        ValidateProgress();        
        RegisterEventHandlers();
        _isInitialized = true;
    }

    #region 事件监听注册
    private void RegisterEventHandlers()
    {
        GameEvents.OnLevelEnter += HandleLevelEnter;
        GameEvents.OnLevelComplete += HandleLevelComplete;
        GameEvents.OnStoryEnter += HandleStoryEnter;
        GameEvents.OnStoryComplete += HandleStoryComplete;
        GameEvents.OnSceneTransitionRequest += HandleSceneTransition;
        GameEvents.OnCGEnter += HandleCGEnter;
        GameEvents.OnCGComplete += HandleCGComplete;
        GameEvents.OnMapStoryEnter += HandleMapStoryEnter;
        GameEvents.OnMapStoryComplete += HandleMapStoryComplete;
        GameEvents.OnChapterComplete += HandleChapterComplete;
    }

    protected override void OnDestroy()
    {
        GameEvents.OnLevelEnter -= HandleLevelEnter;
        GameEvents.OnLevelComplete -= HandleLevelComplete;
        GameEvents.OnStoryEnter -= HandleStoryEnter;
        GameEvents.OnStoryComplete -= HandleStoryComplete;
        GameEvents.OnSceneTransitionRequest -= HandleSceneTransition;
        GameEvents.OnCGEnter -= HandleCGEnter;
        GameEvents.OnCGComplete -= HandleCGComplete;
        GameEvents.OnChapterComplete -= HandleChapterComplete;
    }

    #endregion


    #region 事件处理
    private void HandleLevelEnter(int levelId) {
        SetCurrentLevel(levelId);
        SetLastPlayedLevel(levelId);
        Debug.Log($"进入关卡: {levelId}");
        if(levelId > 0 && levelId <= levelScenes.Count) {
            GameEvents.TriggerSceneTransition(GameEvents.SceneTransitionType.ToLevel);
        }
    }

    private void HandleLevelComplete(int levelId)
    {
        if (levelId == _progress.currentLevel && !IsLevelCompleted(levelId))
        {
            // 完成关卡
            _progress.completedLevels[levelId] = true;
            SaveProgress();
            Debug.Log($"完成关卡: {levelId}");

            
            // 解锁下一关和它的前故事
            if(levelId + 1 > 0 && !IsLevelUnlocked(levelId + 1) && levelId + 1 <= levelScenes.Count)
            {
                _progress.unlockedStories[StoryConfig.PreStoryOffset + levelId + 1] = true;
                GameEvents.TriggerStoryUnlocked(StoryConfig.PreStoryOffset + levelId + 1);
                SaveProgress();
                Debug.Log($"解锁前故事: {StoryConfig.PreStoryOffset + levelId + 1}");

                _progress.unlockedLevels[levelId + 1] = true;
                GameEvents.TriggerLevelUnlocked(levelId + 1);
                SaveProgress();
                Debug.Log($"解锁关卡: {levelId + 1}");
            }
        
            //解锁后故事
            int PostStoryId = StoryConfig.PostStoryOffset + levelId;
            if(PostStoryId > 0 &&! IsStoryCompleted(PostStoryId))
            {
                _progress.unlockedStories[PostStoryId] = true;
                GameEvents.TriggerStoryUnlocked(PostStoryId);
                SaveProgress(); 
                Debug.Log($"解锁故事: {PostStoryId}");
            }

            // 检查是否需要播放后故事
            int postStoryId = StoryConfig.PostStoryOffset + levelId;
            if(NeedPlayStory(postStoryId)) 
            {
                GameEvents.TriggerStoryEnter(postStoryId);
                SetCurrentLevel(0);
                return;
            }
        }
        SetCurrentLevel(0);
        GameEvents.TriggerSceneTransition(GameEvents.SceneTransitionType.ToLevelSelect); 
    }
    
    private void HandleStoryEnter(int storyId) {
        SetCurrentStory(storyId);
        _progress.unlockedStories[storyId] = true;
        GameEvents.TriggerSceneTransition(GameEvents.SceneTransitionType.ToStory);
    }

    private void HandleStoryComplete(int storyId)
    {
        //进度保存
        SetCurrentStory(0);
        if(storyId > 0 &&!IsStoryCompleted(storyId))
        {
            _progress.completedStories[storyId] = true;
            SaveProgress();
            Debug.Log($"完成故事: {storyId}");
        }
        int levelId = GetLevelIdFromStoryId(storyId);
        
        // 处理后故事(2000-2999)-检查是否需要播放CG
        bool isPostStory = storyId >= StoryConfig.PostStoryOffset && storyId < 3000;
        if(isPostStory && levelId > 0 && NeedPlayCG(CGConfig.CGOffset + levelId))
        {
            SetCurrentCG(CGConfig.CGOffset + levelId);
            GameEvents.TriggerCGEnter(CGConfig.CGOffset + levelId);
            return;
        }
        
        //处理前故事(1000-1999)-检查是否需要进入关卡
        if(levelId > 0 && !IsLevelCompleted(levelId))
            GameEvents.TriggerLevelEnter(levelId);
        else
            GameEvents.TriggerSceneTransition(GameEvents.SceneTransitionType.ToLevelSelect);
    }
    
    private void HandleCGEnter(int cgId) {
        SetCurrentCG(cgId);
        Debug.Log($"进入CG: {cgId}");

    }

    private void HandleMapStoryEnter(int storyId) {
        SetCurrentStory(storyId);
        Debug.Log($"进入地图对话: {storyId}");
    }

    private void HandleMapStoryComplete(int storyId) {
        _progress.completedStories[storyId] = true;
        SaveProgress();
        SetCurrentStory(0);
        Debug.Log($"完成地图对话: {storyId}");
    }

    private void HandleCGComplete(int cgId) 
{
    SetCurrentCG(0);
    if(!_progress.completedCGs.ContainsKey(cgId)) {
        _progress.completedCGs[cgId] = true;
        SaveProgress();
    }
    
    if(chapterEndVideoMappings.Contains(cgId))
    {
        int chapterCGId = CGConfig.ChapterCGOffset + _progress.currentChapter;
        // 检查章节CG是否已完成
        if(!_progress.completedCGs.ContainsKey(chapterCGId) || !_progress.completedCGs[chapterCGId])
        {
            GameEvents.TriggerCGEnter(chapterCGId);
            return;
        }
    }
    
    GameEvents.TriggerSceneTransition(GameEvents.SceneTransitionType.ToLevelSelect);
    }  
    
    private void HandleChapterComplete(int chapterId) {
        int chapterCGId = CGConfig.ChapterCGOffset + chapterId;
        SetCurrentCG(chapterCGId);
        Debug.Log($"完成章节: {chapterCGId}，开始播放CG");
        _progress.currentChapter++;
    }
    private void HandleSceneTransition(GameEvents.SceneTransitionType transitionType) {
        switch (transitionType) {
            case GameEvents.SceneTransitionType.ToLevel:
                int currentLevel = GetCurrentLevel();
                if(currentLevel > 0 && currentLevel <= levelScenes.Count) {
                    LoadScene(levelScenes[currentLevel - 1]);
                }
                break;

            case GameEvents.SceneTransitionType.ToStory:
                int currentStory = GetCurrentStory();
                if(currentStory > 0) {
                    LoadScene(dialogScene);
                }
                break;

            case GameEvents.SceneTransitionType.ToLevelSelect:
                LoadScene(levelSelectScene);
                break;
        }
    }

    private void LoadScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            StartCoroutine(LoadingScreen.Instance.LoadSceneAsync(sceneName));
        }
    }

    #endregion
    

    #region 获取/设置游戏参数
    public int GetCurrentLevel() => _progress.currentLevel;
    public void SetCurrentLevel(int levelId) => _progress.currentLevel = levelId;

    public int GetCurrentStory() => _progress.currentStory;
    public void SetCurrentStory(int storyId) => _progress.currentStory = storyId;

    public int GetCurrentCG() => _progress.currentCG;
    public void SetCurrentCG(int cgId) => _progress.currentCG = cgId;

    public int GetLastPlayedLevel() => _progress.lastPlayedLevel;

    public int GetCurrentChapter() => _progress.currentChapter;

    public int SetLastPlayedLevel(int levelId) => _progress.lastPlayedLevel = levelId;
    public int CalculatePreStoryId(int levelId) => StoryConfig.PreStoryOffset + levelId;
    public int CalculatePostStoryId(int levelId) => StoryConfig.PostStoryOffset + levelId;
    public int CalculateCGId(int levelId) => CGConfig.CGOffset + levelId;
    public int CalculateChapterCGId(int chapterId) => CGConfig.ChapterCGOffset + chapterId;
    public int GetLevelIdFromStoryId(int storyId) => storyId % 1000;


    // 检查是否需要播放故事
    public bool NeedPlayStory(int storyId)
    {
        if(storyId <= 0) return false;
        
        // 先检查是否已完成
        if(_progress.completedStories.TryGetValue(storyId, out bool completed) && completed) 
        {
            return false;
        }
        
        // 检查是否解锁
        if(!_progress.unlockedStories.TryGetValue(storyId, out bool unlocked) || !unlocked)
        {
            return false;
        }
        
        // 如果是前故事(1000-1999)且关联关卡未完成，则需要播放
        bool isPreStory = storyId >= StoryConfig.PreStoryOffset && storyId < StoryConfig.PostStoryOffset;
        if(isPreStory && !IsLevelCompleted(GetLevelIdFromStoryId(storyId)))
        {
            return true;
        }
        
        // 如果是后故事(2000-2999)且关联关卡已完成但故事未完成，则需要播放
        bool isPostStory = storyId >= StoryConfig.PostStoryOffset && storyId < 3000;
        if(isPostStory && IsLevelCompleted(GetLevelIdFromStoryId(storyId)))
        {
            return true;
        }
        
        return false;
    }
    //检查是否需要播放CG
    public bool NeedPlayCG(int cgId)
    {
        if(cgId <= 0) return false;
        if(!_progress.completedCGs.TryGetValue(cgId, out bool completed) ||!completed)
            return true;
        return false;
    }
    #endregion

    #region 存档管理
    private void SaveProgress()
    {
        // 确保字典数据被初始化
        _progress.completedLevels ??= new Dictionary<int, bool>();
        _progress.unlockedLevels ??= new Dictionary<int, bool>();
        _progress.completedStories ??= new Dictionary<int, bool>();
        _progress.unlockedStories ??= new Dictionary<int, bool>();
        _progress.completedCGs ??= new Dictionary<int, bool>();
        
        string saveDir = Path.Combine(Application.dataPath, "..", "SaveData");
        string path = Path.Combine(saveDir, "savedata.dat");
        
        // 确保目录存在
        if (!Directory.Exists(saveDir))
        {
            Directory.CreateDirectory(saveDir);
        }
        
        // 使用自定义序列化方法
        string json = JsonUtility.ToJson(new GameProgressSerializable(_progress));
        File.WriteAllText(path, json);
        Debug.Log($"进度已保存到: {path.Replace('\\', '/')}");
    }

    // 新增可序列化的进度类
    [System.Serializable]
    private class GameProgressSerializable
    {
        public int currentLevel;
        public int lastPlayedLevel;
        public int currentStory;
        public int currentCG;
        public int currentChapter;
        public List<int> completedLevels = new();
        public List<int> unlockedLevels = new();
        public List<int> completedStories = new();
        public List<int> unlockedStories = new();
        public List<int> completedCGs = new();
    
        public GameProgressSerializable(GameProgress progress)
        {
            currentLevel = progress.currentLevel;
            lastPlayedLevel = progress.lastPlayedLevel;
            currentStory = progress.currentStory;
            currentCG = progress.currentCG;
            currentChapter = progress.currentChapter;
            
            // 转换字典为列表
            completedLevels = progress.completedLevels.Where(kv => kv.Value).Select(kv => kv.Key).ToList();
            unlockedLevels = progress.unlockedLevels.Where(kv => kv.Value).Select(kv => kv.Key).ToList();
            completedStories = progress.completedStories.Where(kv => kv.Value).Select(kv => kv.Key).ToList();
            unlockedStories = progress.unlockedStories.Where(kv => kv.Value).Select(kv => kv.Key).ToList();
            completedCGs = progress.completedCGs.Where(kv => kv.Value).Select(kv => kv.Key).ToList();
        }
    }

    private void LoadProgress()
    {
        string saveDir = Path.Combine(Application.dataPath, "..", "SaveData");
        string path = Path.Combine(saveDir, "savegame.dat");
        
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            var serializable = JsonUtility.FromJson<GameProgressSerializable>(json);
            
            // 初始化所有字典
            _progress.unlockedLevels = new Dictionary<int, bool>();
            _progress.completedLevels = new Dictionary<int, bool>();
            _progress.unlockedStories = new Dictionary<int, bool>();
            _progress.completedStories = new Dictionary<int, bool>();
            _progress.completedCGs = new Dictionary<int, bool>();
            
            // 转换列表为字典
            serializable.unlockedLevels.ForEach(id => _progress.unlockedLevels[id] = true);
            serializable.completedLevels.ForEach(id => _progress.completedLevels[id] = true);
            serializable.unlockedStories.ForEach(id => _progress.unlockedStories[id] = true);
            serializable.completedStories.ForEach(id => _progress.completedStories[id] = true);
            serializable.completedCGs.ForEach(id => _progress.completedCGs[id] = true);
            
            // 设置其他字段
            _progress.currentLevel = serializable.currentLevel;
            _progress.lastPlayedLevel = serializable.lastPlayedLevel;
            _progress.currentStory = serializable.currentStory;
            _progress.currentCG = serializable.currentCG;
            _progress.currentChapter = serializable.currentChapter;
            
            Debug.Log($"从 {path.Replace('\\', '/')} 加载存档");
        }
        else
        {
            // 初始化所有字典
            _progress.unlockedLevels = new Dictionary<int, bool>();
            _progress.completedLevels = new Dictionary<int, bool>();
            _progress.unlockedStories = new Dictionary<int, bool>();
            _progress.completedStories = new Dictionary<int, bool>();
            _progress.completedCGs = new Dictionary<int, bool>();
            
            // 设置默认关卡状态
            _progress.currentLevel = 0;
            _progress.lastPlayedLevel = 0;
            _progress.unlockedLevels[1] = true; 
            _progress.completedLevels[1] = false;
            _progress.unlockedStories[StoryConfig.PreStoryOffset + 1] = true;
            
            Debug.Log("创建新存档，初始化默认值");
            SaveProgress();
        }
    }
    #endregion

    #region 关卡状态
    public bool IsLevelCompleted(int levelId) =>
        _progress.completedLevels.ContainsKey(levelId) && _progress.completedLevels[levelId];
    public bool IsLevelUnlocked(int levelId) => 
        _progress.unlockedLevels.ContainsKey(levelId) && _progress.unlockedLevels[levelId];
    public bool IsStoryUnlocked(int storyId) =>
        _progress.unlockedStories.ContainsKey(storyId) && _progress.unlockedStories[storyId];
    public bool IsStoryCompleted(int storyId) =>
        _progress.completedStories.ContainsKey(storyId) && _progress.completedStories[storyId];
    public bool IsCGCompleted(int cgId) =>
        _progress.completedCGs.ContainsKey(cgId) && _progress.completedCGs[cgId];

    private void ValidateProgress()
    {
     
        // 确保第一关解锁状态正确
        if(!_progress.unlockedLevels.ContainsKey(1))
        {
            _progress.unlockedLevels[1] = true;
        }
        
        // 确保已完成状态与解锁状态一致
        foreach(var level in _progress.completedLevels)
        {
            if(level.Value && !_progress.unlockedLevels.ContainsKey(level.Key))
            {
                _progress.unlockedLevels[level.Key] = true;
            }
        }
    }

    #endregion

    #region 调试功能
    #if UNITY_EDITOR
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.F1))
        {
            // 检查当前场景是否是关卡场景
            string currentScene = SceneManager.GetActiveScene().name;
            if(levelScenes.Contains(currentScene))
            {
                SkipLevel();
            }
            else
            {
                Debug.Log($"[编辑器]一键通关只能在关卡场景中使用，当前场景: {currentScene}");
            }
        }
    }
    
    //一键通关
    private void SkipLevel()
    {
        int currentLevel = GetCurrentLevel();
        if(currentLevel > 0 && currentLevel <= levelScenes.Count)
        {
            Debug.Log($"[编辑器]跳过关卡: {currentLevel}");
            GameEvents.TriggerLevelComplete(currentLevel);
        }
    }
    #endif
    #endregion
}


#region 存档类
    //游戏进度类
    [System.Serializable]
    public class GameProgress
    {
        public int currentLevel = 0;
        public int lastPlayedLevel = 0;
        public int currentStory = GameManager.StoryConfig.PreStoryOffset + 1;
        public int currentCG = GameManager.CGConfig.CGOffset + 1;
        public int currentChapter = 1;
        public Dictionary<int, bool> completedStories = new();
        public Dictionary<int, bool> completedLevels = new();
        public Dictionary<int, bool> unlockedLevels = new();
        public Dictionary<int, bool> unlockedStories = new();
        public Dictionary<int, bool> completedCGs = new();
    }

#endregion






