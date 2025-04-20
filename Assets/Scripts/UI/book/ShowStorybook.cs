using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShowStorybook : MonoBehaviour
{
    [Header("UI元素")]
    //[SerializeField] private Text progressText;
    [SerializeField] private Transform levelDetailsPanel;
    [SerializeField] private GameObject levelDetailPrefab;

    private LevelDataManager dataManager;
    private List<GameObject> detailInstances = new List<GameObject>();

    private void Awake()
    {
        dataManager = GetComponent<LevelDataManager>();
    }

    private void OnEnable()
    {
        dataManager.LoadLevelConfigs();
        UpdateProgressDisplay();
    }

    public void UpdateProgressDisplay()
    {
        //UpdateProgressText();
        ClearExistingDetails();
        CreateLevelDetailItems();
    }

    // private void UpdateProgressText()
    // {
    //     int completedCount = CalculateCompletedLevels();
    //     progressText.text = $"进度: {completedCount}/{dataManager.GetTotalLevels()}";
    // }
    
    //计算当前完成关卡数量
    private int CalculateCompletedLevels()
    {
        int completedCount = 0;
        for (int i = 0; i < dataManager.GetTotalLevels(); i++)
        {
            int cgId = GameManager.Instance.CalculateCGId(i + 1);
            if (GameManager.Instance.IsCGCompleted(cgId))
            {
                completedCount++;
            }
        }
        return completedCount;
    }

    private void ClearExistingDetails()
    {
        foreach (var instance in detailInstances)
        {
            Destroy(instance);
        }
        detailInstances.Clear();
    }

    private void CreateLevelDetailItems()
    {
        for (int i = 0; i < dataManager.GetTotalLevels(); i++)
        {
            var detailObj = CreateLevelDetailItem(i);
            detailInstances.Add(detailObj);
        }
    }

    private GameObject CreateLevelDetailItem(int levelIndex)
    {
        int levelId = levelIndex + 1;
    var detailObj = Instantiate(levelDetailPrefab, levelDetailsPanel);
    
    var config = dataManager.GetLevelConfig(levelId);
    bool isCompleted = IsLevelCompleted(levelId);
    bool isUnlocked = IsLevelUnlocked(levelId);

    // 修改为传递整个LevelConfigData
    detailObj.GetComponent<LevelDetailItem>().Setup(
        config,
        isCompleted,
        isUnlocked
    );
    
    return detailObj;
    }

    private bool IsLevelCompleted(int levelId)
    {
        int cgId = GameManager.Instance.CalculateCGId(levelId);
        return GameManager.Instance.IsCGCompleted(cgId);
    }

    private bool IsLevelUnlocked(int levelId)
    {
        return GameManager.Instance.IsLevelUnlocked(levelId);
    }
}

