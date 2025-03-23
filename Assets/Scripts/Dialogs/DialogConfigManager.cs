using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class DialogConfigManager : SingletonBase<DialogConfigManager>
{
    private Dictionary<string, DialogData> _dialogDict = new();

    /// <summary>
    /// 实现单例基类的初始化方法
    /// </summary>
    protected override void Initialize()
    {
        LoadDefaultDialogues();
    }

    /// <summary>
    /// 加载默认对话配置
    /// </summary>
    private void LoadDefaultDialogues()
    {
        var defaultConfig = Resources.Load<TextAsset>("Dialogue/default");
        if(defaultConfig != null)
        {
            LoadDialogues(defaultConfig);
        }
        else
        {
            Debug.LogError("默认对话配置文件缺失！");
        }
    }

    /// <summary>
    /// 加载CSV对话配置表到内存字典
    /// </summary>
    public void LoadDialogues(TextAsset csvFile)
    {
        using (var reader = new StringReader(csvFile.text))
        {
            // 第一行（中文表头）仅用于文档说明，不参与逻辑
            var chineseHeader = reader.ReadLine();
            
            // 第二行（英文变量名）作为实际表头
            var englishHeader = reader.ReadLine().Trim();
            var headers = englishHeader.Split(',');
            
            var headerMap = new Dictionary<string, int>();
            for (int i = 0; i < headers.Length; i++)
            {
                headerMap[headers[i].Trim()] = i; // 双重Trim确保去除空格
            }

            // 调试输出表头映射
            Debug.Log($"成功解析表头：{string.Join("; ", headers)}");

            while (reader.Peek() != -1)
            {
                var line = reader.ReadLine().Trim();
                if (string.IsNullOrEmpty(line)) continue;
                
                var fields = line.Split(',');
                var data = new DialogData(fields, headerMap);
                _dialogDict[data.DialogID] = data;
            }
        }
    }

    /// <summary>
    /// 获取指定对话ID的数据
    /// </summary>
    public DialogData GetDialog(string dialogID)
    {
        return _dialogDict.TryGetValue(dialogID, out var data) ? data : null;
    }
}