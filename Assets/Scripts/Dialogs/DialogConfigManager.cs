using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DialogConfigManager : SingletonBase<DialogConfigManager>
{
    private readonly Dictionary<int, TextAsset> _storyConfigMap = new();

    protected override void Initialize()
    {
        // 初始化故事ID与配置文件的映射
        _storyConfigMap.Add(1001, Resources.Load<TextAsset>("Config/Dialogs/第一关 关卡前"));
        _storyConfigMap.Add(2001, Resources.Load<TextAsset>("Config/Dialogs/第一关 关卡后"));
        // 添加更多映射...
    }

    public List<DialogData> GetDialogsByStoryId(int storyId)
    {
        if (_storyConfigMap.TryGetValue(storyId, out var config))
        {
            return ParseCSV(config.text);
        }
        return null;
    }

    private List<DialogData> ParseCSV(string csvText)
    {
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

        return dialogList;
    }
}