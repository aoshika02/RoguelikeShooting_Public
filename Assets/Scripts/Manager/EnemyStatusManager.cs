using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStatusManager : SingletonMonoBehaviour<EnemyStatusManager>
{
    [SerializeField] private EnemyParamDatas _paramDatas;
    [SerializeField] private EnemyStatesDatas _statesDatas;

    private Dictionary<EnemyType, EnemyParam> _paramDic = new Dictionary<EnemyType, EnemyParam>();
    private Dictionary<EnemyType, EnemyStatesData> _statesDic = new Dictionary<EnemyType, EnemyStatesData>();

    public async UniTask InitDic()
    {
        _paramDic.Clear();
        _statesDic.Clear();
        foreach (var param in _paramDatas.EnemyParamSet)
        {
            if (_paramDic.TryAdd(param.EnemyType, param) == false)
            {
                Debug.LogError($"{nameof(_paramDic)}の追加に失敗しました ->{param.EnemyType}");
            }
        }
        foreach (var statesData in _statesDatas.EnemyStatesDataSet)
        {
            if (_statesDic.TryAdd(statesData.EnemyType, statesData) == false)
            {
                Debug.LogError($"{nameof(_statesDic)}の追加に失敗しました ->{statesData.EnemyType}");
            }
        }
        await UniTask.CompletedTask;
    }
    public EnemyParam GetEnemyParam(EnemyType enemyType, int index)
    {
        if (_paramDic == null || _paramDic.Count == 0)
        {
            return null;
        }
        if (_paramDic.TryGetValue(enemyType, out var enemyParam))
        {
            return new EnemyParam
            {
                Hp = Mathf.RoundToInt(enemyParam.Hp * (1f + index / 5f)),
                Atk = Mathf.RoundToInt(enemyParam.Atk * (1f + index / 5f)),
                Def = Mathf.RoundToInt(enemyParam.Def * (1f + index / 5f)),
                MoveSpeed = enemyParam.MoveSpeed,
                RunSpeed = enemyParam.RunSpeed,
                MoveThreshold = enemyParam.MoveThreshold,
                ChaseThreshold = enemyParam.ChaseThreshold,
                WaitTime = enemyParam.WaitTime
            };
        }
        return null;
    }
    public EnemyStatesData GetEnemyStatesData(EnemyType enemyType)
    {
        if (_statesDic == null || _statesDic.Count == 0)
        {
            return null;
        }
        if (_statesDic.TryGetValue(enemyType, out var enemyStatesData))
        {
            return new EnemyStatesData
            {
                PatrolStateTypes = new List<EnemyStateType>(enemyStatesData.PatrolStateTypes),
                ChaseStateTypes = new List<EnemyStateType>(enemyStatesData.ChaseStateTypes)
            };
        }
        return null;
    }
}