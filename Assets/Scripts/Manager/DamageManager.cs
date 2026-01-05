using UnityEngine;

public class DamageManager : SingletonMonoBehaviour<DamageManager>
{
    private DamagePool _pool;
    [SerializeField] private Camera _camera;
    protected override void Awake()
    {
        if (CheckInstance() == false) return;
        _pool = DamagePool.Instance;
    }
    public void OnDamage(int damageValue,Vector3 damagePoint) 
    {
        var damageViewObj= _pool.GetDamageViewObj();
        if (damageViewObj == null) return;
        damageViewObj.Init(damagePoint, damageValue, _camera);
    }
}
