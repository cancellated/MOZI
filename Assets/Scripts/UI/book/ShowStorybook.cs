using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

/// <summary>
/// 故事书展示控制器，负责管理关卡详情UI的显示和更新
/// </summary>
public class ShowStorybook : MonoBehaviour
{
    [Header("一级菜单")]
    [SerializeField] private Transform levelOverviewPanel; // 关卡总览面板
    [SerializeField] private GameObject levelOverviewPrefab; // 关卡总览项预制体
    
    [Header("二级菜单")]
    [SerializeField] private Transform levelDetailsPanel; // 关卡详情面板
    [SerializeField] private GameObject levelDetailPrefab; // 关卡详情预制体
    
    [Header("动画设置")] 
    [SerializeField] private float zoomDuration = 0.5f;
    [SerializeField] private float scrollUnrollDuration = 1f;
    
    private LevelDataManager dataManager;
    private List<GameObject> overviewInstances = new List<GameObject>();
    private List<GameObject> detailInstances = new List<GameObject>(); // 已生成的关卡详情实例
    private GameObject currentDetailInstance;
    private Camera mainCamera;

    private void Awake()
    {
        // 获取LevelDataManager组件
        dataManager = GetComponent<LevelDataManager>();
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        // 加载关卡配置并更新显示
        dataManager.LoadLevelConfigs();
        ShowOverviewMenu();
    }

    /// <summary>
    /// 显示一级菜单（关卡总览）
    /// </summary>
    public void ShowOverviewMenu()
    {
        levelDetailsPanel.gameObject.SetActive(false);
        levelOverviewPanel.gameObject.SetActive(true);
        
        ClearExistingOverviews();
        CreateLevelOverviewItems();
    }


    /// <summary>
    /// 显示二级菜单（关卡详情）
    /// </summary>
    public void ShowDetailMenu(int levelId)
    {
        StartCoroutine(TransitionToDetailMenu(levelId));
    }

    private System.Collections.IEnumerator TransitionToDetailMenu(int levelId)
    {
        // // 1. 找到点击的关卡总览项
        // var clickedOverview = overviewInstances[levelId - 1];
        
        // // 2. 摄像机聚焦动画
        // mainCamera.transform.DOMove(clickedOverview.transform.position + Vector3.back * 2, zoomDuration);
        
        // // 3. 播放卷轴展开动画
        // var scroll = clickedOverview.GetComponentInChildren<Image>();
        // scroll.transform.DOScaleX(1.5f, scrollUnrollDuration);
        // scroll.transform.DOScaleY(1.5f, scrollUnrollDuration);
        
        yield return new WaitForSeconds(scrollUnrollDuration);
        
        // 4. 切换到详情页
        levelOverviewPanel.gameObject.SetActive(false);
        levelDetailsPanel.gameObject.SetActive(true);
        
        CreateLevelDetailItem(levelId - 1);
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
    
    private void CreateLevelOverviewItems()
    {

    }

    private void ClearExistingOverviews()
    {

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

