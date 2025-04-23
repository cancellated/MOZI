using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapDialogData
{
    public string DialogID;
    public string Character;
    public string Content;
    
    public MapDialogData(string[] fields, Dictionary<string, int> headerMap)
    {
        DialogID = GetFieldSafe(fields, "DialogID", headerMap);
        Character = GetFieldSafe(fields, "Character", headerMap);
        Content = GetFieldSafe(fields, "Content", headerMap);
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