using System;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "TextEventDatas", menuName = "ScriptableObject/TextEventDatas", order = 0)]
public class TextEventDatas : ScriptableObject
{
    public List<TextEventData> TextDatas;
}
[Serializable]
public class TextEventData
{
    public TextEventType TextEventType;
    public List<TextEventDataBase> TextEventDataBases;
}
[Serializable]
public class TextEventDataBase
{
    public string FindKey;
    public float Duration;
}
