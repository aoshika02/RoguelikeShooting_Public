using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BuffDatas))]
public class BuffDatasEditor : Editor
{
    // チェックボックスの状態を保存するための辞書
    private Dictionary<BuffType, HashSet<RankType>> _selectedRanks = new Dictionary<BuffType, HashSet<RankType>>();
    public override void OnInspectorGUI()
    {
        BuffDatas buffDatas = (BuffDatas)target;
        // BuffType と RankType の列挙値を取得
        List<BuffType> allBuffTypes = new List<BuffType>((BuffType[])System.Enum.GetValues(typeof(BuffType)));
        List<RankType> allRankTypes = new List<RankType>((RankType[])System.Enum.GetValues(typeof(RankType)));

        // `SerializedObject` と `SerializedProperty` を使用して、インスペクターの状態を管理
        SerializedObject serializedBuffDatas = new SerializedObject(buffDatas);

        // BuffDataSetsのクリアボタン
        if (GUILayout.Button("Clear BuffDataSets"))
        {
            buffDatas.BuffDataSets.Clear();
        }

        // ヘッダー部分：RankTypeの列
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("", GUILayout.Width(100)); // 左上の空セル
        foreach (RankType rankType in allRankTypes)
        {
            EditorGUILayout.LabelField(rankType.ToString(), GUILayout.Width(60)); // RankTypeラベル
        }
        EditorGUILayout.EndHorizontal();

        // BuffTypeごとに行を表示
        foreach (BuffType buffType in allBuffTypes)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(buffType.ToString(), GUILayout.Width(100)); // BuffTypeラベル
            // BuffTypeごとに対応するRankTypeのチェックボックスを表示
            if (!_selectedRanks.ContainsKey(buffType))
            {
                _selectedRanks[buffType] = new HashSet<RankType>(); // 初期化
            }

            HashSet<RankType> selectedRanksForBuffType = _selectedRanks[buffType];

            foreach (RankType rankType in allRankTypes)
            {
                bool isChecked = selectedRanksForBuffType.Contains(rankType);
                // チェックボックスを描画
                bool newCheck = EditorGUILayout.Toggle("", isChecked, GUILayout.Width(60));
                // チェック状態の変更を反映
                if (newCheck && !selectedRanksForBuffType.Contains(rankType))
                {
                    selectedRanksForBuffType.Add(rankType);
                }
                else if (!newCheck && selectedRanksForBuffType.Contains(rankType))
                {
                    selectedRanksForBuffType.Remove(rankType);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        // チェックされたBuffTypeとRankTypeに基づいてBuffDataを生成
        if (GUILayout.Button("Generate BuffData"))
        {
            buffDatas.BuffDataSets.Clear(); // 古いデータをクリア
            // 選択されたBuffTypeとRankTypeの組み合わせをもとにBuffDataを生成
            foreach (var buffType in _selectedRanks)
            {
                BuffType selectedBuffType = buffType.Key;
                foreach (RankType selectedRank in buffType.Value)
                {
                    if (Enum.TryParse(selectedBuffType.ToString(), true, out TextEventType textEventType))
                    {
                        BuffData newBuffData = new BuffData()
                        {
                            BuffType = selectedBuffType,
                            RankType = selectedRank,
                            TextEventType = textEventType
                        };
                        buffDatas.BuffDataSets.Add(newBuffData);
                    }
                }
            }
        }
        // BuffDataSetsの変更があれば保存
        if (GUI.changed)
        {
            serializedBuffDatas.ApplyModifiedProperties();
            EditorUtility.SetDirty(buffDatas);
        }
        // デフォルトのインスペクターを描画（他のフィールドがあれば表示）
        DrawDefaultInspector();
    }
}
