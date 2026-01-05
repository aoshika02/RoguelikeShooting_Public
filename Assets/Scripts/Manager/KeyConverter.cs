using System.Linq;
using UnityEngine;

public class KeyConverter : SingletonMonoBehaviour<KeyConverter>
{
    [SerializeField] private TextEventDatas _textEventDatas;
    protected override void Awake()
    {
        if (CheckInstance())
        {
            DontDestroyOnLoad(gameObject);
        }
    }
    public string GetFindKey(TextEventType eventType, int index)
    {
        var textEvent = _textEventDatas.TextDatas.FirstOrDefault(x => x.TextEventType == eventType);
        if (textEvent != null)
        {
            if (0 <= index && index < textEvent.TextEventDataBases.Count)
                return textEvent.TextEventDataBases[index].FindKey;
        }
        Debug.LogWarning($"{eventType}:{index}がありません");
        return null;
    }
    public int TextEventCount(TextEventType eventType)
    {
        var textEvent = _textEventDatas.TextDatas.FirstOrDefault(x => x.TextEventType == eventType);
        if (textEvent != null)
        {
            return textEvent.TextEventDataBases.Count;
        }
        return 0;
    }
}
