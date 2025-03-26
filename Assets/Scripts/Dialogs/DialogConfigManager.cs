using UnityEngine;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// 对话配置映射数据结构
/// 用于关联章节ID与对应的对话配置文件
/// </summary>
[System.Serializable]
public class ChapterDialogMapping
{
    [Tooltip("章节唯一标识符（与关卡ID对应）")]
    public int chapterID;
    
    [Tooltip("关联的CSV配置文件")]
    public TextAsset configFile;
    
    [Tooltip("适用的场景上下文类型")]
    public GameEvents.SceneContextType contextType;
}

/// <summary>
/// 对话配置管理系统，功能包括：
/// 1. 加载和管理多套对话配置
/// 2. 根据场景上下文动态切换配置
/// 3. 提供对话数据快速查询接口
/// </summary>
public class DialogConfigManager : SingletonBase<DialogConfigManager>
{
    [Header("主配置表")]
    [Tooltip("章节对话配置映射表")]
    [SerializeField] private List<ChapterDialogMapping> _chapterMappings = new();

    // 新增字典存储配置映射
    private readonly Dictionary<(int, GameEvents.SceneContextType), ChapterDialogMapping> _chapterDict = new();

    [Header("运行时数据")]
    [Tooltip("当前加载的对话配置字典（键：对话ID，值：对话数据）")]
    private Dictionary<string, DialogData> _dialogDict = new();

    /// <summary>
    /// 单例初始化方法
    /// 加载默认对话配置并准备运行时数据
    /// </summary>
    protected override void Initialize()
    {
        InitializeChapterDictionary(); // 新增字典初始化
        LoadDefaultDialogues();
    }

    /// <summary>
    /// 初始化章节配置字典
    /// </summary>
    private void InitializeChapterDictionary()
    {
        foreach (var mapping in _chapterMappings)
        {
            var key = (mapping.chapterID, mapping.contextType);
            if (!_chapterDict.ContainsKey(key))
            {
                _chapterDict.Add(key, mapping);
            }
            else
            {
                Debug.LogWarning($"发现重复章节配置：ChapterID={mapping.chapterID}, Context={mapping.contextType}");
            }
        }
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
    /// 加载CSV对话配置到内存字典
    /// </summary>
    /// <param name="csvFile">CSV文本资源</param>
    /// <remarks>
    /// 文件格式要求：
    /// 第1行：中文表头（仅文档说明）
    /// 第2行：英文变量名（实际解析用）
    /// 第3行起：对话数据行
    /// </remarks>
    public void LoadDialogues(TextAsset csvFile)
    {
        using var reader = new StringReader(csvFile.text);
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

    /// <summary>
    /// 根据上下文加载对应章节的对话配置
    /// </summary>
    /// <param name="context">场景上下文类型</param>
    /// <param name="chapterID">当前章节ID</param>
    /// <remarks>
    /// 仅在StoryTrigger类型的上下文中生效
    /// 会自动回退到默认配置当找不到匹配项时
    /// </remarks>
    /// <summary>
    /// 根据场景上下文加载对话配置
    /// </summary>
    public void LoadSceneDialogs(GameEvents.SceneContextType context, int chapterID)
    {
        if (context != GameEvents.SceneContextType.StoryTrigger) return;
        if (_chapterDict.TryGetValue((chapterID, context), out var mapping))
        {
            LoadDialogues(mapping.configFile);
            Debug.Log($"已加载章节{chapterID}对话配置");
        }
        else
        {
            Debug.LogWarning($"未找到章节{chapterID}在{context}上下文中的配置");
        }
    }
}