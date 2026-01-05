using UnityEngine;

public class DamagePool : SingletonMonoBehaviour<DamagePool>
{
    [SerializeField] private GameObject _damageViewPrefab;
    [SerializeField] private Transform _damageViewTransform;
    private GenericObjectPool<DamageViewObj> _damageViewPool;
    protected override void Awake()
    {
        if (CheckInstance() == false) return;
        _damageViewPool = new GenericObjectPool<DamageViewObj>(_damageViewPrefab, _damageViewTransform);
    }
    public DamageViewObj GetDamageViewObj()
    {
        return _damageViewPool.Get();
    }
    public void ReleaseDamageViewObj(DamageViewObj damageViewObj)
    {
        _damageViewPool.Release(damageViewObj);
    }
}
