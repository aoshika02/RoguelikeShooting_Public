using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TextConverter : SingletonMonoBehaviour<TextConverter>
{
    private static List<KeyToTextBase> _keyToTextBases;
    [SerializeField] private TextAsset _jsonText;
    protected override void Awake()
    {
        if (CheckInstance())
        {
            DontDestroyOnLoad(gameObject);

            try
            {
                _keyToTextBases = JsonConverter.FromJson<KeyToTextBase>(_jsonText.text);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
    /// <summary>
    /// keyからテキストを取得
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static (string message,string name) GetText(string key)
    {
        var text = _keyToTextBases.FirstOrDefault(x => x.FindKey == key);
        if (text == null)
        {
            Debug.LogError($"Keyが存在しません log : {key} is null");
            return (key, key);
        }
        return (text.Message, text.Name);
    }
}
