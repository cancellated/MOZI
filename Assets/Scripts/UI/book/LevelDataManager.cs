using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class LevelDataManager : MonoBehaviour
{
    private const string CONFIG_PATH = "Config/Book/物品";
    private List<LevelConfigData> levelConfigs = new List<LevelConfigData>();//关卡列表

    /// <summary>
    /// 从ResourcesConfig/Book/物品.csv加载并解析关卡配置数据
    /// CSV文件格式要求：
    /// 1. 第一行为注释行(可忽略)
    /// 2. 第二行为表头行，定义各字段名称
    /// 3. 从第三行开始为数据行
    /// </summary>
    public void LoadLevelConfigs()
    {
        // 1. 从Resources文件夹加载CSV文本资源
        TextAsset csvFile = Resources.Load<TextAsset>(CONFIG_PATH);
        if (csvFile == null)
        {
            Debug.LogError($"找不到关卡配置文件: {CONFIG_PATH}");
            return;
        }
    
        // 2. 处理文本内容
        string csvText = csvFile.text;
        
        // 移除UTF-8 BOM头(Windows系统生成的CSV常见问题)
        if (csvText.StartsWith("\uFEFF")) {
            csvText = csvText[1..];
        }
    
        // 3. 按行分割文本
        var lines = csvText.Split('\n');
        if (lines.Length < 3) {
            Debug.LogError("CSV行数不足，至少需要表头行+数据行");
            return;
        }
    
        // 4. 解析表头(第二行)
        var headerLine = lines[1].TrimEnd('\r', '\n'); // 处理不同系统的换行符
        var headers = headerLine.Split(',');
        
        // 建立字段名到列索引的映射字典
        var headerMap = new Dictionary<string, int>();
        for (int i = 0; i < headers.Length; i++) {
            headerMap[headers[i].Trim()] = i; // 去除前后空格后建立映射
        }
    
        // 5. 解析数据行(从第三行开始)
        for (int i = 2; i < lines.Length; i++) {
            // 跳过空行
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            
            // 分割字段
            var fields = lines[i].Split(',');
            int unlockLevel = int.Parse(fields[0]);
            Debug.Log("读取影神图unlockLevel: " + unlockLevel);//仅测试
            // 确保有对应关卡的配置
            while(levelConfigs.Count <= unlockLevel)
            {
                levelConfigs.Add(new LevelConfigData(levelConfigs.Count));
            }
            Debug.Log("当前关卡信息数量为: " + levelConfigs.Count);
            // 添加物品信息
            levelConfigs[unlockLevel].items.Add(new LevelConfigData.ItemInfo
            {
                name = fields[1],
                description = fields[2],
                spritePath = fields[3]
            });
        }
        
        Debug.Log($"成功加载{levelConfigs.Count}个关卡的物品配置");
    }

    /// <summary>
    /// 获取关卡配置数据
    /// </summary>
    public LevelConfigData GetLevelConfig(int index)
    {
        return levelConfigs[index];
    }

    /// <summary>
    /// 获取关卡总数
    /// </summary>
    public int GetTotalLevels()
    {
        return GameManager.Instance.levelScenes.Count;
    }
}



    public class LevelConfigData
    {
        public int levelId; // 关卡ID
        public List<ItemInfo> items = new List<ItemInfo>(); // 该关卡的所有物品
        
        [System.Serializable]
        public class ItemInfo
        {
            public string name;
            public string description;
            public string spritePath; // 物品图片路径;
        }

        public LevelConfigData(int levelId)
        {
            this.levelId = levelId;
        }
    }


