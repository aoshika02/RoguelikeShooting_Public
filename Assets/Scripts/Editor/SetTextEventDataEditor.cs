#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SetTextEventDataEditor : EditorWindow
{
    private TextAsset _jsonTextAsset;
    private TextEventDatas _textEventDatas;
    private List<KeyToTextBase> _keyToTextBases;
    private List<AddData> _addDatas = new List<AddData>();
    private float _defaultDuration = 0.025f;
    [MenuItem("Tools/SetTextEventDataEditor", false, 1)]
    private static void ShowEditerWindow()
    {
        SetTextEventDataEditor window = GetWindow<SetTextEventDataEditor>();
        window.titleContent = new GUIContent("AutoMakeTextEventData");
    }
    private void OnGUI()
    {
        //GUI(フィールド)でオブジェクトの参照埋め込みを行う
        EditorGUILayout.Space(10);
        GUILayout.Label($"▼{nameof(TextAsset)}をセット▼", EditorStyles.boldLabel);
        _jsonTextAsset = (TextAsset)EditorGUILayout.ObjectField("Json", _jsonTextAsset, typeof(TextAsset), true);
        EditorGUILayout.Space(10);
        GUILayout.Label($"▼{nameof(TextEventDatas)}をセット▼", EditorStyles.boldLabel);
        _textEventDatas = (TextEventDatas)EditorGUILayout.ObjectField("TextEventDatas", _textEventDatas, typeof(TextEventDatas), true);
        GUILayout.Label($"▼{nameof(_defaultDuration)}を入力▼", EditorStyles.boldLabel);
        _defaultDuration = EditorGUILayout.FloatField("Duration", _defaultDuration);
        //jsonのみ読み込み
        string path = AssetDatabase.GetAssetPath(_jsonTextAsset);
        if (_jsonTextAsset != null && !path.EndsWith(".json"))
        {
            Debug.LogWarning("このフィールドには .json ファイルのみを割り当てられます");
            _jsonTextAsset = null;
        }

        //ボタンを押した際の処理
        bool isNull = (_jsonTextAsset == null || _textEventDatas == null);
        if (isNull == false && GUILayout.Button("データを初期化してセット"))
        {
            SetData();
        }
    }
    private void SetData()
    {
        try
        {
            _textEventDatas.TextDatas.Clear();
            _keyToTextBases = JsonConverter.FromJson<KeyToTextBase>(_jsonTextAsset.text);
            //Keyのデータのリストの作成(重複回避)
            _addDatas = _keyToTextBases
                .GroupBy(x => new
                {
                    x.Type,
                    x.SubType,
                    x.TextEventType,
                })
                .Select(y => new AddData
                {
                    FrontKey = $"{y.Key.Type}_{y.Key.SubType}_",
                    KeyName = y.Key.TextEventType,
                }).ToList();
            //Keyがなければ終了
            if (_addDatas.Count == 0) return;
            foreach (var addData in _addDatas)
            {
                //Keyの生成
                string targetKey = $"{addData.FrontKey}{addData.KeyName}";
                //Keyに合致するデータの個数
                int count = _keyToTextBases.Count(ktb => ktb.FindKey.Contains(targetKey) && ktb.TextEventType == addData.KeyName);
                //データの個数が想定外ならcontinue
                if (count <= 0) continue;
                //TextEventTypeを設定
                TextEventType textEventType;
                string tetText = _keyToTextBases
                                .Where(x => x.TextEventType == addData.KeyName && !string.IsNullOrEmpty(x.TextEventType))
                                .Select(x => x.TextEventType)
                                .FirstOrDefault();
                bool isSuccess = Enum.TryParse(tetText, out textEventType);
                //TryParseに失敗した場合continue
                if (isSuccess == false) continue;
                //キャラ名がない場合Keyをクリア
                List<TextEventDataBase> textEventDatas = new List<TextEventDataBase>();

                for (int i = 0; i < count; i++)
                {
                    //データが複数ある場合はインデックスによってKeyを生成
                    string currentTargetKey = count == 1 ? targetKey : $"{targetKey}_{(i + 1).ToString("D3")}";
                    //Keyに合致するデータの取り出し
                    KeyToTextBase matchKeyToTextBase = _keyToTextBases.FirstOrDefault(k => k.FindKey == currentTargetKey);
                    //合致するデータがなければcontinue
                    if (matchKeyToTextBase == null) continue;
                    //データの追加
                    textEventDatas.Add(
                           new TextEventDataBase
                           {
                               FindKey = matchKeyToTextBase.FindKey,

                               Duration = _defaultDuration
                           });
                }
                //データの追加
                _textEventDatas.TextDatas.Add(new TextEventData
                {
                    TextEventType = textEventType,
                    TextEventDataBases = textEventDatas
                });
            }
            Debug.Log("データの追加に成功しました");
            EditorUtility.SetDirty(_textEventDatas);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"データの追加に失敗しました: {ex.Message}");
        }
    }
    //データ追加用クラス
    public class AddData
    {
        public string FrontKey;
        public string KeyName;
    }
}
#endif