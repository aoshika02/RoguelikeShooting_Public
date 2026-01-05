using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : SingletonMonoBehaviour<EnemyPool>
{
    [SerializeField] private List<EnemyPoolData> _enemyPoolDatas = new List<EnemyPoolData>();
    private Dictionary<EnemyType, GenericObjectPool<EnemyObj>> _enemyPoolDic = new Dictionary<EnemyType, GenericObjectPool<EnemyObj>>();
    protected override void Awake()
    {
        if(CheckInstance()== false) return;
    }
    public async UniTask Init()
    {
        foreach (var poolData in _enemyPoolDatas)
        {
            _enemyPoolDic.TryAdd(
                poolData.EnemyType,
                new GenericObjectPool<EnemyObj>(poolData.EnemyPrefab, poolData.ParentTransform));
        }
        await UniTask.CompletedTask;
    }
    public EnemyObj GetEnemy(EnemyType enemyType)
    {
        if (_enemyPoolDic.TryGetValue(enemyType, out var pool))
        {
            return pool.Get();
        }
        Debug.LogError($"{nameof(EnemyPool)}に{enemyType}が存在しません");
        return null;
    }
    public void ReleaseEnemy(EnemyObj enemy)
    {
        if (_enemyPoolDic.TryGetValue(enemy.EnemyType, out var pool))
        {
            pool.Release(enemy);
            return;
        }
        Debug.LogError($"{nameof(EnemyPool)}に{enemy.EnemyType}が存在しません");
    }
    public void ReleaseAllEnemy()
    {
        foreach (var pool in _enemyPoolDic.Values)
        {
            pool.ReleaseAll();
        }
    }
}
[Serializable]
public class EnemyPoolData
{
    public EnemyType EnemyType;
    public GameObject EnemyPrefab;
    public Transform ParentTransform;
}
