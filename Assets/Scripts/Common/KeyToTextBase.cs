using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class KeyToTextBase
{
    public string Type;
    public string SubType;
    public string Index;
    public string TextEventType;
    public string FindKey;
    public string Name;
    public string Message;
}
public static class JsonConverter
{
    public static List<T>FromJson<T>(string json)
    {
        KeyWrapper<T> wrapper = JsonUtility.FromJson<KeyWrapper<T>>(json);
        return wrapper.KeyTexts;
    }

    [Serializable]
    private class KeyWrapper<T>
    {
        public List<T> KeyTexts;
    }
}
