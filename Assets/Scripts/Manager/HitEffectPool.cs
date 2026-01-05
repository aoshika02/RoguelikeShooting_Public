using UnityEngine;

public class HitEffectPool : SingletonMonoBehaviour<HitEffectPool>
{
    [SerializeField] private GameObject _effectPrefab;
    private GenericObjectPool<HitEffctObj> _effctObjPool;
    protected override void Awake()
    {
        if (CheckInstance() == false) return;
        _effctObjPool = new GenericObjectPool<HitEffctObj>(_effectPrefab, transform);
    }
    public HitEffctObj GetHitEffctObj()
        => _effctObjPool.Get();
    public void ReleaseHitEffectObj(HitEffctObj effctObj)
        => _effctObjPool.Release(effctObj);
}
