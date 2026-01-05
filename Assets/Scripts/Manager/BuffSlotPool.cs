using UnityEngine;

public class BuffSlotPool : SingletonMonoBehaviour<BuffSlotPool>
{
    [SerializeField] private GameObject _buffSlotPrefab;
    [SerializeField] private Transform _buffSlotParent;
    private GenericObjectPool<BuffSlotObj> _buffSlotPool;
    protected override void Awake()
    {
        if (CheckInstance() == false) return;

    }
    public void Init() 
    {
        _buffSlotPool = new GenericObjectPool<BuffSlotObj>(_buffSlotPrefab, _buffSlotParent);
    }
    public BuffSlotObj GetBuffSlotObj()
    {
        return _buffSlotPool.Get();
    }
    public void ReleaseBuffSlotObj(BuffSlotObj buffSlotObj)
    {
        _buffSlotPool.Release(buffSlotObj);
    }
}
