using Cysharp.Threading.Tasks;
using UnityEngine;

public class HitEffctObj : MonoBehaviour, IPool
{
    [SerializeField] private ParticleSystem _hitEffect;
    private HitEffectPool _hitEffectPool;
    public bool IsGenericUse { get; set; }
    public void Init(Vector3 pos, Vector3 angle)
    {
        _hitEffect.transform.rotation = Quaternion.Euler(angle);
        transform.position = pos;
        PlayEffect().Forget();
        if (_hitEffectPool == null) _hitEffectPool = HitEffectPool.Instance;
    }
    private async UniTask PlayEffect()
    {
        await _hitEffect.PlayAsync();
        _hitEffectPool.ReleaseHitEffectObj(this);
    }
    public void OnRelease()
    {
        gameObject.SetActive(false);
    }

    public void OnReuse()
    {
        gameObject.SetActive(true);
    }
}
