using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class DialogData
{
    // 对话ID
    public string DialogID;
    // 对话正文内容，支持富文本标签
    public string Content;
    // 说话角色名称
    public string Character;
    //关联的背景音乐资源路径
    public string BGM;
    //关联的背景图片资源路径
    public string Background;
    public DialogData(string[] fields, Dictionary<string, int> headerMap)
    {
        Debug.Log($"正在解析行数据，字段数: {fields.Length}");
        for(int i=0; i<fields.Length; i++)
        {
            Debug.Log($"字段[{i}]: '{fields[i]}'");
        }

        DialogID = GetFieldSafe(fields, "DialogID", headerMap);
        Content = GetFieldSafe(fields, "Content", headerMap);
        Character = GetFieldSafe(fields, "Character", headerMap);
        BGM = GetFieldSafe(fields, "BGM", headerMap);
        Background = GetFieldSafe(fields, "Background", headerMap);

        Debug.Log($"解析结果 - ID:{DialogID} 内容:{Content} 角色:{Character}");
    }

    private string GetFieldSafe(string[] fields, string header, Dictionary<string, int> headerMap)
    {
        if (headerMap.TryGetValue(header, out int index) && index < fields.Length)
        {
            return fields[index];
        }
        return string.Empty;
    }
}