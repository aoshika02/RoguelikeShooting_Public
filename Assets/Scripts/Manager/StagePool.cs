using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public class StagePool : SingletonMonoBehaviour<StagePool>
{
    [SerializeField] private List<StagePoolData> _stagePoolDatas = new List<StagePoolData>();
    private Dictionary<StageType, GenericObjectPool<StageObj>> _stagePoolDic = new Dictionary<StageType, GenericObjectPool<StageObj>>();
    protected override void Awake()
    {
        if (CheckInstance() == false) return;
        
    }
    public async UniTask Init() 
    {
        foreach (var poolData in _stagePoolDatas)
        {
            _stagePoolDic.TryAdd(
                poolData.StageType,
                new GenericObjectPool<StageObj>(poolData.StagePrefab, poolData.ParentTransform));
        }
        await UniTask.CompletedTask;
    }
    public StageObj GetStage(StageType stageType)
    {
        if (_stagePoolDic.TryGetValue(stageType, out var pool))
        {
            return pool.Get();
        }
        Debug.LogError($"{nameof(StagePool)}に{stageType}が存在しません");
        return null;
    }
    public void ReleaseStage(StageObj stage)
    {
        if (_stagePoolDic.TryGetValue(stage.StageType, out var pool))
        {
            pool.Release(stage);
            return;
        }
        Debug.LogError($"{nameof(StagePool)}に{stage.StageType}が存在しません");
    }
    public void ReleaseAllStage()
    {
        foreach (var pool in _stagePoolDic.Values)
        {
            pool.ReleaseAll();
        }
    }
}
[Serializable]
public class StagePoolData
{
    public StageType StageType;
    public GameObject StagePrefab;
    public Transform ParentTransform;
}
