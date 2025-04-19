using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

public class ShowStorybook : MonoBehaviour
{
    [Header("UI元素")]
    [SerializeField] private Text progressText;
    [SerializeField] private Transform levelDetailsPanel;
    [SerializeField] private GameObject levelDetailPrefab;

    [Header("配置文件")]
    [SerializeField] private TextAsset levelConfigCSV;

    private List<LevelConfigData> levelConfigs = new List<LevelConfigData>();
    private List<GameObject> detailInstances = new List<GameObject>();

    private void OnEnable()
    {
        LoadLevelConfigs();
        UpdateProgressDisplay();
    }

    private void LoadLevelConfigs()
    {
        if (levelConfigCSV == null)
        {
            Debug.LogError("关卡配置文件未设置");
            return;
        }

        string csvText = levelConfigCSV.text;
        
        // 移除BOM头
        if (csvText.StartsWith("\uFEFF")) {
            csvText = csvText[1..];
        }

        var lines = csvText.Split('\n');
        if (lines.Length < 3) {
            Debug.LogError("CSV行数不足");
            return;
        }

        // 解析表头
        var headers = lines[1].TrimEnd('\r', '\n').Split(',');
        var headerMap = new Dictionary<string, int>();
        for (int i = 0; i < headers.Length; i++) {
            headerMap[headers[i].Trim()] = i;
        }

        // 解析数据行
        for (int i = 2; i < lines.Length; i++) {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            
            var fields = lines[i].Split(',');
            levelConfigs.Add(new LevelConfigData(fields, headerMap));
        }
    }

    public void UpdateProgressDisplay()
    {
        // 计算完成进度
        int completedCount = 0;
        int totalLevels = GameManager.Instance.levelScenes.Count;
        
        // 统计已完成的CG数量作为通关进度
        for (int i = 0; i < totalLevels; i++)
        {
            int cgId = GameManager.Instance.CalculateCGId(i + 1);
            if (GameManager.Instance.IsCGCompleted(cgId))
            {
                completedCount++;
            }
        }
        
        progressText.text = $"进度: {completedCount}/{totalLevels}";

        // 清空现有详情
        foreach (var instance in detailInstances)
        {
            Destroy(instance);
        }
        detailInstances.Clear();

        // 为每个关卡创建详情项
        for (int i = 0; i < totalLevels; i++)
        {
            int levelId = i + 1;
            var detailObj = Instantiate(levelDetailPrefab, levelDetailsPanel);
            
            // 获取CG ID
            int cgId = GameManager.Instance.CalculateCGId(levelId);
            bool isCompleted = GameManager.Instance.IsCGCompleted(cgId);
            bool isUnlocked = GameManager.Instance.IsLevelUnlocked(levelId);

            // 从配置数据获取关卡信息
            var config = levelConfigs[i];
            Sprite icon = Resources.Load<Sprite>(config.iconPath);

            // 设置详情项内容
            detailObj.GetComponent<LevelDetailItem>().Setup(
                config.title,
                config.description,
                icon,
                isCompleted,
                isUnlocked
            );

            detailInstances.Add(detailObj);
        }
    }
}

[System.Serializable]
public class LevelConfigData
{
    public string title;
    public string description;
    public string iconPath;

    public LevelConfigData(string[] fields, Dictionary<string, int> headerMap)
    {
        title = fields[headerMap["Title"]];
        description = fields[headerMap["Description"]];
        iconPath = fields[headerMap["IconPath"]];
    }
}
