using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.XR;

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
        _progress.currentLevel = levelId;
        Debug.Log($"当前关卡: {levelId}");
    }

    private void HandleLevelComplete(int levelId)
    {
        if (levelId == _progress.currentLevel)
        {
            UnlockLevel(levelId + 1);
            UnlockStory(levelId*2+1);
            SaveProgress();
        }
    }

    private void HandleStoryEnter(int storyId)
    {   
        if(!IsStoryViewed(storyId)){
            _progress.viewedStories[storyId] = true;
            SaveProgress();
        }
    }

    private void HandleStoryComplete(int storyId)
    {
        if (storyId == _progress.currentStory && storyId == 1)
        {
            UnlockStory(storyId++);
            SaveProgress();
        }
    }
    public bool NeedPlayStory(int storyId){
        return !IsStoryViewed(storyId); 
    }

    #region 获取/设置游戏进程参数
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

        public int GetDialogId(int levelId){
            if(!_progress.unlockedLevels.ContainsKey(levelId)){
                return levelId*2; 
            } 
            else{
                return levelId*2+1;
            }
        }
    #endregion

    #endregion

    #region 存档管理
    public void SaveProgress()
    {
        string path = Path.Combine(Application.persistentDataPath, "savedata", "savegame.dat");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, JsonUtility.ToJson(_progress));
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
    public void UnlockLevel(int levelId)
    {
        if (levelId > 0 && !IsLevelUnlocked(levelId))
        {
            _progress.unlockedLevels[levelId] = true;
            GameEvents.TriggerLevelUnlocked(levelId);
            SaveProgress();
        }
    }

    public void UnlockStory(int storyId)
    {
        if (storyId > 0 &&!IsStoryViewed(storyId))
        {
            _progress.unlockedStories[storyId] = true;
            GameEvents.TriggerStoryUnlocked(storyId);
            SaveProgress();
        }
    }


    public bool IsLevelUnlocked(int levelId) => 
        _progress.unlockedLevels.ContainsKey(levelId) && _progress.unlockedLevels[levelId];
    
    public bool IsStoryViewed(int storyId) =>
        _progress.viewedStories.ContainsKey(storyId) && _progress.viewedStories[storyId];
    private void ValidateProgress()
    {
        int CurrentLevel = GetCurrentLevel();
        int CurrentStory = GetCurrentStory();
        if (!_progress.unlockedLevels.ContainsKey(CurrentLevel))
        {
            _progress.unlockedLevels[CurrentLevel] = true;
        }
        if (!_progress.unlockedStories.ContainsKey(CurrentLevel))
        {
            _progress.unlockedStories[CurrentLevel] = true;
        }
        if (!_progress.viewedStories.ContainsKey(CurrentStory))
        {
            _progress.viewedStories[CurrentStory-1] = true; 
        }
    }
    #endregion

    [System.Serializable]
    public class GameProgress
    {
        public int currentLevel = 1;
        public int currentStory = 1;
        public Dictionary<int, bool> unlockedLevels = new();
        public Dictionary<int, bool> unlockedStories = new();
        public Dictionary<int ,bool> viewedStories = new();
    }
}