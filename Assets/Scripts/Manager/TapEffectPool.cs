using UnityEngine;

public class TapEffectPool : SingletonMonoBehaviour<TapEffectPool>
{
    [SerializeField] private GameObject _effectObj;
    private GenericObjectPool<TapEffectObj> _tapEffectPool;
    void Start()
    {
        _tapEffectPool = new GenericObjectPool<TapEffectObj>(_effectObj,transform);
    }
    public TapEffectObj GetTapEffectObj() => _tapEffectPool.Get();
    public void ReleaseTapEffectObj(TapEffectObj tapEffectObj) => _tapEffectPool.Release(tapEffectObj);
}
