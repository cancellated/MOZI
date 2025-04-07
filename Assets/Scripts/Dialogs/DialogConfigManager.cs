using UnityEngine;
using System.Collections.Generic;
using System.IO;

public static class DialogConfigManager
{
    private static readonly Dictionary<int, TextAsset> _storyConfigMap = new();
    private static bool _isInitialized = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (_isInitialized) return;
        
        // 确保Resources目录存在
        string resourcesPath = Path.Combine(Application.dataPath, "Resources");
        if (!Directory.Exists(resourcesPath))
        {
            Debug.LogError($"Resources目录不存在: {resourcesPath}");
            return;
        }

        LoadAllDialogs();
        _isInitialized = true;
    }

    private static void LoadAllDialogs()
    {
        TextAsset[] allDialogs = Resources.LoadAll<TextAsset>("Config/Dialogs");
        Debug.Log($"找到{allDialogs.Length}个对话配置文件");

        foreach (var config in allDialogs)
        {
            if (int.TryParse(config.name, out int storyId))
            {
                _storyConfigMap[storyId] = config;
                Debug.Log($"成功加载故事配置: {storyId}, 文件名: {config.name}");
            }
            else
            {
                Debug.LogError($"无效的对话文件名: {config.name}, 必须为数字ID");
            }
        }

        if (_storyConfigMap.Count == 0)
        {
            Debug.LogWarning("未加载到任何对话配置，将创建默认配置");
            CreateDefaultConfigs();
        }
    }

    private static void CreateDefaultConfigs()
    {
        string defaultCsv = "DialogID,Content,Character\n1,默认对话内容,默认角色";
        var defaultConfig = new TextAsset(defaultCsv);
        _storyConfigMap[1001] = defaultConfig;
        Debug.Log("已创建默认对话配置 (ID: 1001)");
    }

    public static List<DialogData> GetDialogsByStoryId(int storyId)
    {
        if (!_isInitialized) Initialize();

        if (_storyConfigMap.TryGetValue(storyId, out var config))
        {
            Debug.Log($"开始解析故事ID: {storyId}的对话配置");
            try 
            {
                var dialogs = ParseCSV(config.text);
                if(dialogs == null || dialogs.Count == 0)
                {
                    Debug.LogError($"解析后的对话列表为空，storyId: {storyId}");
                }
                return dialogs;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"解析故事{storyId}配置时出错: {e.Message}\n{e.StackTrace}");
                return null;
            }
        }
        
        Debug.LogError($"找不到storyId={storyId}的对话配置");
        return null;
    }

    private static List<DialogData> ParseCSV(string csvText)
    {
        if (string.IsNullOrEmpty(csvText))
        {
            Debug.LogError("CSV文本为空");
            return new List<DialogData>();
        }

        // 移除BOM头
        if (csvText.StartsWith("\uFEFF")) {
            csvText = csvText[1..];
        }

        var lines = csvText.Split('\n');
        if (lines.Length < 3) {
            Debug.LogError("CSV行数不足，至少需要两行表头+一行数据");
            return new List<DialogData>();
        }

        // 跳过第一行中文表头，直接使用第二行英文表头
        var headerLine = lines[1]; 
        var headers = headerLine.Split(',');

        // 创建字段名映射字典
        var headerMap = new Dictionary<string, int>();
        for (int i = 0; i < headers.Length; i++) {
            string header = headers[i].Trim();
            headerMap[header] = i;
            Debug.Log($"表头映射: '{header}' -> {i}");
        }

        // 从第三行开始解析数据
        List<DialogData> dialogList = new();
        for (int i = 2; i < lines.Length; i++) {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            
            var fields = lines[i].Split(',');
            dialogList.Add(new DialogData(fields, headerMap));
        }

        Debug.Log($"成功解析出{dialogList.Count}条对话");
        return dialogList;
    }
}