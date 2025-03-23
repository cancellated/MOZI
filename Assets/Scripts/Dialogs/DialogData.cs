using System.Collections.Generic;

/// <summary>
/// 对话数据实体类，用于存储从CSV配置表加载的单条对话数据
/// </summary>
[System.Serializable]
public class DialogData
{
    /// <summary>
    /// 对话唯一标识符，格式：章节_序号（例：1_001）
    /// </summary>
    public string DialogID;
    
    /// <summary>
    /// 对话正文内容，支持富文本标签
    /// </summary>
    public string Content;
    
    /// <summary>
    /// 说话角色名称
    /// </summary>
    public string Character;
    
    /// <summary>
    /// 关联的背景音乐资源路径
    /// </summary>
    public string BGM;
    
    /// <summary>
    /// 关联的背景图片资源路径
    /// </summary>
    public string Background;

    /// <summary>
    /// 构造函数，从CSV行数据创建对话数据对象
    /// </summary>
    /// <param name="fields">CSV分割后的字段数组</param>
    /// <param name="headerMap">CSV表头映射字典（列名 -> 列索引）</param>
    public DialogData(string[] fields, Dictionary<string, int> headerMap)
    {
        DialogID = GetField(fields, "DialogID", headerMap);
        Content = GetField(fields, "Content", headerMap);
        Character = GetField(fields, "Character", headerMap);
        BGM = GetField(fields, "BGM", headerMap);
        Background = GetField(fields, "Background", headerMap);
    }

    private string GetField(string[] fields, string header, Dictionary<string, int> headerMap)
    {
        return headerMap.ContainsKey(header) ? fields[headerMap[header]] : "";
    }
}