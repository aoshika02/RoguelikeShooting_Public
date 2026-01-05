using UnityEngine;

public class BulletPool : SingletonMonoBehaviour<BulletPool>
{
    [SerializeField] private GameObject _bulletObj;
    [SerializeField] private GameObject _enemyBulletObj;
    private GenericObjectPool<BulletObj> _bulletPool;
    private GenericObjectPool<EnemyBullet> _enemyBulletPool;
    protected override void Awake()
    {
        _bulletPool = new GenericObjectPool<BulletObj>(_bulletObj, transform);
        _enemyBulletPool = new GenericObjectPool<EnemyBullet>(_enemyBulletObj, transform);
    }
    public BulletObj GetBulletObj() => _bulletPool.Get();
    public void ReleaseBulletObj(BulletObj obj) => _bulletPool.Release(obj);
    public EnemyBullet GetEnemyBullet() => _enemyBulletPool.Get();
    public void ReleaseEnemyBullet(EnemyBullet enemyBullet) => _enemyBulletPool.Release(enemyBullet);
}
