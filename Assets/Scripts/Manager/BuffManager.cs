using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffManager : SingletonMonoBehaviour<BuffManager>
{
    [SerializeField] private BuffDatas _buffDatas;
    private Queue<BuffData> _buffTypeQueue = new Queue<BuffData>();
    private List<BuffData> _targetBuffs = new List<BuffData>();
    private List<BuffData> _activeBuffs = new List<BuffData>();
    private PlayerStatusManager _playerStatusManager;
    protected override void Awake()
    {
        if (CheckInstance() == false) return;
        _playerStatusManager = PlayerStatusManager.Instance;
    }
    public void Init()
    {
        foreach (var buffData in _buffDatas.BuffDataSets)
        {
            _targetBuffs.Add(new BuffData
            {
                BuffType = buffData.BuffType,
                RankType = buffData.RankType,
                TextEventType = buffData.TextEventType,
                Value = buffData.Value,
            });
        }
    }
    public List<BuffData> GetBuffTypes(int count = 3, bool isMix = true, int repeatCount = 1)
    {
        if (_buffTypeQueue.Count < 3) _buffTypeQueue.Clear();
        return RandomTypeBase.GetRandomType<BuffData>(count, _buffTypeQueue, isMix, _buffDatas.BuffDataSets, repeatCount);
    }
    public void AddActiveBuff(BuffData buffData)
    {
        _activeBuffs.Add(buffData);
        if (_playerStatusManager.AddBuff(buffData))
        {
            _targetBuffs.RemoveAll(x => x.BuffType == buffData.BuffType && x.RankType == buffData.RankType);
        }
    }
}
