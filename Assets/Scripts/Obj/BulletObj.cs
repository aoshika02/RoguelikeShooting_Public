using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BulletObj : MonoBehaviour, IPool
{
    public bool IsGenericUse { get; set; }
    private Vector3 _lastPos;
    private Vector3 _moveDir;
    private Rigidbody _rb;
    private PlayerStatusManager _playerStatusManager;
    private HitEffectPool _hitEffectPool;
    public void Init(Vector3 dir, float speed)
    {
        if (_rb == null) _rb = GetComponent<Rigidbody>();
        transform.forward = dir;
        _rb.velocity = dir * speed;
        _moveDir= _rb.velocity;
        if (_playerStatusManager == null) _playerStatusManager = PlayerStatusManager.Instance;
        if (_hitEffectPool == null) _hitEffectPool = HitEffectPool.Instance;
    }
    private void Update()
    {
        // 弾の移動量と方向を保存（TriggerEnterで使う）
        Vector3 currentPos = transform.position;
        _moveDir = (currentPos - _lastPos).normalized;
        _lastPos = currentPos;
    }
    public void OnRelease()
    {
        gameObject.SetActive(false);
        _rb.velocity = Vector3.zero;
    }

    public void OnReuse()
    {
        gameObject.SetActive(true);
    }
    private void ViewHitEffect(Collider other) 
    {
        var hitEffect = _hitEffectPool.GetHitEffctObj();
        if (hitEffect != null)
        {
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            Vector3 hitNormal = -_moveDir;
            hitEffect.Init(hitPoint, hitNormal);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(ParamConsts.ENEMY))
        {
            var damageValue = _playerStatusManager.GetAtk();
            if (other.TryGetComponent(out EnemyObj enemyObj))
            {
                var calcDaageValue = damageValue - enemyObj.Def;
                if (calcDaageValue <= 1) calcDaageValue = 1;
                SoundManager.Instance.PlaySE(SEType.Hit,transform.position);
                enemyObj.TakeDamage(calcDaageValue);
                DamageManager.Instance.OnDamage(calcDaageValue, transform.position);
                ViewHitEffect(other);
            }
            
            BulletPool.Instance.ReleaseBulletObj(this);
        }
        if (other.CompareTag(ParamConsts.WALL))
        {
            ViewHitEffect(other);
            BulletPool.Instance.ReleaseBulletObj(this);
        }
        if (other.CompareTag(ParamConsts.OBJ))
        {
            ViewHitEffect(other);
            BulletPool.Instance.ReleaseBulletObj(this);
        }
    }
}
