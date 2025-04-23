using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 故事书展示控制器，负责管理关卡详情UI的显示和更新
/// </summary>
public class ShowStorybook : MonoBehaviour
{
    [Header("UI元素")]
    [SerializeField] private Transform levelDetailsPanel; // 关卡详情项的父容器
    [SerializeField] private GameObject levelDetailPrefab; // 单个关卡详情的预制体

    private LevelDataManager dataManager; // 关卡数据管理器
    private List<GameObject> detailInstances = new List<GameObject>(); // 已生成的关卡详情实例

    private void Awake()
    {
        // 获取LevelDataManager组件
        dataManager = GetComponent<LevelDataManager>();
    }

    private void OnEnable()
    {
        // 加载关卡配置并更新显示
        dataManager.LoadLevelConfigs();
        UpdateProgressDisplay();
    }

    /// <summary>
    /// 更新整个UI显示
    /// </summary>
    public void UpdateProgressDisplay()
    {
        ClearExistingDetails(); // 清除现有详情
        CreateLevelDetailItems(); // 创建新的关卡详情项
    }

    /// <summary>
    /// 计算已完成的关卡数量
    /// </summary>
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

    /// <summary>
    /// 清除现有的所有关卡详情项
    /// </summary>
    private void ClearExistingDetails()
    {
        foreach (var instance in detailInstances)
        {
            Destroy(instance);
        }
        detailInstances.Clear();
    }

    /// <summary>
    /// 创建所有关卡的详情项UI元素
    /// </summary>
    private void CreateLevelDetailItems()
    {
        for (int i = 0; i < dataManager.GetTotalLevels(); i++)
        {
            var detailObj = CreateLevelDetailItem(i);
            detailInstances.Add(detailObj);
        }
    }

    /// <summary>
    /// 创建单个关卡的详情项UI元素
    /// </summary>
    /// <param name="levelIndex">关卡索引(从0开始)</param>
    private GameObject CreateLevelDetailItem(int levelIndex)
    {
        int levelId = levelIndex + 1;
        var detailObj = Instantiate(levelDetailPrefab, levelDetailsPanel);
        
        var config = dataManager.GetLevelConfig(levelId);
        bool isCompleted = IsLevelCompleted(levelId);
        bool isUnlocked = IsLevelUnlocked(levelId);

        // 初始化关卡详情项
        detailObj.GetComponent<LevelDetailItem>().Setup(
            config,
            isCompleted,
            isUnlocked
        );
        
        return detailObj;
    }

    /// <summary>
    /// 检查指定关卡是否已完成
    /// </summary>
    /// <param name="levelId">关卡ID(从1开始)</param>
    private bool IsLevelCompleted(int levelId)
    {
        int cgId = GameManager.Instance.CalculateCGId(levelId);
        return GameManager.Instance.IsCGCompleted(cgId);
    }

    /// <summary>
    /// 检查指定关卡是否已解锁
    /// </summary>
    /// <param name="levelId">关卡ID(从1开始)</param>
    private bool IsLevelUnlocked(int levelId)
    {
        return GameManager.Instance.IsLevelUnlocked(levelId);
    }
}

