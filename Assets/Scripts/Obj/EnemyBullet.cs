using UnityEngine;

public class EnemyBullet : MonoBehaviour, IPool
{
    public bool IsGenericUse { get; set; }
    private Rigidbody _rb;
    [SerializeField] private EnemyAttackCall _attackCall;
    public void Init(Vector3 dir, float speed,EnemyObj enemyObj)
    {
        _attackCall.SetEnemyObj(enemyObj);
        if (_rb == null) _rb = GetComponent<Rigidbody>();
        transform.forward = dir;
        _rb.velocity = dir * speed;
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
    private void OnTriggerEnter(Collider other)
    {
        bool hit =
            other.CompareTag(ParamConsts.PLAYER) ||
            other.CompareTag(ParamConsts.WALL) ||
            other.CompareTag(ParamConsts.OBJ);
        if (hit)
        {
            BulletPool.Instance.ReleaseEnemyBullet(this);
        }
        if (other.CompareTag(ParamConsts.WALL)) Debug.Log("Wall");
        if (other.CompareTag(ParamConsts.PLAYER)) Debug.Log("PLAYER");
    }
}
