using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class EnemyObj : EnemyMovementBase, IPool
{
    public bool IsGenericUse { get; set; }
    public EnemyType EnemyType => _enemyType;
    [SerializeField] private EnemyType _enemyType;
    private Dictionary<EnemyStateType, EnemyStateBase> _enemyStates = new Dictionary<EnemyStateType, EnemyStateBase>();
    public EnemyAnimController AnimController => _animController;
    private BulletPool _bulletPool;
    [SerializeField] private EnemyAnimController _animController;
    [SerializeField] private Transform _origin;
    [SerializeField] private Transform _target;
    [SerializeField] private ParticleSystem _muzzle;
    private bool _isInit = false;
    private float _speed = 30f;
    private KillCounter _killCounter;
    private DamageCounter _damageCounter;
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private CancellationTokenSource _deadTokenSource = new CancellationTokenSource();
    private void InitDic()
    {
        if (_enemyStates.Count > 0) return;
        _enemyStates.TryAdd(EnemyStateType.None, new EnemyNone(this));
        _enemyStates.TryAdd(EnemyStateType.Init, new EnemyInit(this));
        _enemyStates.TryAdd(EnemyStateType.Walk, new EnemyWalk(this));
        _enemyStates.TryAdd(EnemyStateType.Stay, new EnemyStay(this));
        _enemyStates.TryAdd(EnemyStateType.Attack, new EnemyAttack(this));
        _enemyStates.TryAdd(EnemyStateType.Dead, new EnemyDead(this));
        _enemyStates.TryAdd(EnemyStateType.Damage, new EnemyDamage(this));
        _enemyStates.TryAdd(EnemyStateType.Chase, new EnemyChase(this));
        _enemyStates.TryAdd(EnemyStateType.Shoot, new EnemyShootAttack(this));
    }
    public void Init(EnemyParam enemyParam, List<Transform> transforms)
    {
        _isInit = false;
        GameManager.Instance.AddEnemyCount();
        InitDic();
        if (_killCounter == null) _killCounter = KillCounter.Instance;
        if (_damageCounter == null) _damageCounter = DamageCounter.Instance;
        base.Init(enemyParam);
        base.Init(transforms);
        DeadAsync().Forget();
    }
    public bool GetShootParam(out Transform origin, out Transform target, out ParticleSystem muzzle)
    {
        if (_origin != null && _target != null && _muzzle != null)
        {
            origin = _origin;
            target = _target;
            muzzle = _muzzle;
            return true;
        }
        Debug.Log($"{nameof(Transform)}が設定されていません");
        origin = null;
        target = null;
        muzzle = null;
        return false;
    }
    public async UniTask FlowAsync(List<EnemyStateType> patrolStateTypes, List<EnemyStateType> chaseStateTypes)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        SetKinematic(false);
        StateFlow(patrolStateTypes, _cancellationTokenSource.Token).Forget();
        await SearchPlayer(_cancellationTokenSource.Token);
        if (_cancellationTokenSource == null) return;
        if (_cancellationTokenSource.IsCancellationRequested) return;
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        await StateFlow(chaseStateTypes, _cancellationTokenSource.Token);
    }
    private async UniTask DeadAsync()
    {
        await UniTask.WaitUntil(() => Hp <= 0);
        _cancellationTokenSource?.Cancel();
        _deadTokenSource = new CancellationTokenSource();
        await StateSequence(EnemyStateType.Dead, _deadTokenSource.Token);
    }
    private async UniTask StateFlow(List<EnemyStateType> stateTypes, CancellationToken token)
    {
        if (EnemyStateType == EnemyStateType.Dead)
        {
            await UniTask.Yield();
            return;
        }
        int index = 0;
        try
        {
            while (!token.IsCancellationRequested)
            {
                await StateSequence(stateTypes[index], token);
                await UniTask.Yield(token);
                index = (index + 1) % stateTypes.Count;
            }
        }
        catch (System.OperationCanceledException)
        {
            Debug.Log("EnemyObjのFlowAsyncがキャンセルされました");
        }
    }
    private async UniTask StateSequence(EnemyStateType enemyState, CancellationToken token)
    {
        if (EnemyStateType == EnemyStateType.Dead)
        {
            await UniTask.Yield();
            return;
        }
        if (enemyState == EnemyStateType.Init)
        {
            if (_isInit)
            {
                await UniTask.Yield();
                return;
            }
            _isInit = true;
        }
        SetEnemyState(enemyState);
        if (_enemyStates.TryGetValue(enemyState, out EnemyStateBase stateBase) == false)
        {
            Debug.LogError($"StateBaseがありません ->{enemyState}");
            await UniTask.Yield();
            return;
        }
        await stateBase.Entry(token).SuppressCancellationThrow();
        await stateBase.Do(token).SuppressCancellationThrow();
        await stateBase.Exit(token).SuppressCancellationThrow();
    }
    public void Shoot()
    {
        if (_bulletPool == null) _bulletPool = BulletPool.Instance;
        if (GetShootParam(out var origin, out var target, out var muzzle))
        {
            var dir = (target.transform.position - origin.transform.position).normalized;
            //Debug.DrawRay(origin.position, dir * 5f, Color.green, 1f);
            var obj = _bulletPool.GetEnemyBullet();
            obj.transform.position = target.transform.position;
            obj.Init(dir, _speed, this);
            muzzle.Play();
        }
    }
    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
        _damageCounter.AddDamage(damage);
    }
    public void OnRelease()
    {
        _killCounter.AddKillCount();
        GameManager.Instance.RemoveEnemyCount();
        gameObject.SetActive(false);
        StopMove();
        ReleaseLockState();
        SetEnemyState(EnemyStateType.None);
        _cancellationTokenSource?.Cancel();
        _deadTokenSource?.Cancel();
        SetKinematic(true);
    }

    public void OnReuse()
    {
        gameObject.SetActive(true);
    }
    private void OnDestroy()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = null;
        _deadTokenSource?.Cancel();
        _deadTokenSource = null;
    }
}
