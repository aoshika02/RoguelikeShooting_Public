using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBase : MonoBehaviour
{
    #region ステータス
    public int Hp => _hp;
    private int _hp;
    public int MaxHp => _maxHp;
    private int _maxHp;
    public float Atk => _atk;
    private float _atk;
    public int Def => _def;
    private int _def;
    public float MoveSpeed => _moveSpeed;
    private float _moveSpeed;
    public float RunSpeed => _runSpeed;
    private float _runSpeed;
    public float MoveThreshold => _moveThreshold;
    private float _moveThreshold;
    public float ChaseThreshold => _chaseThreshold;
    private float _chaseThreshold;
    public float WaitTime => _waitTime;
    private float _waitTime;
    #endregion

    #region ステート
    public EnemyStateType EnemyStateType => _enemyStateType;
    private EnemyStateType _enemyStateType = EnemyStateType.None;
    private bool _isLockState = false;
    #endregion

    #region 移動
    public Vector3 MoveDirection => _moveDirection;
    private Vector3 _moveDirection;
    private Rigidbody _rb;
    public Vector3 CurrentPos => transform.position;
    public Vector3 NextPoint => _nextPoint;
    private Vector3 _nextPoint;
    public Vector3 Foward => transform.forward;
    private NavMeshPath _navMeshPath;
    private Queue<Vector3> _navMeshCorners = new Queue<Vector3>();
    #endregion

    private Tween _rotationTween;
    private Material _material;
    private int _dissolveID = Shader.PropertyToID(ParamConsts.DISSOLVE_THRESHOLD);
    [SerializeField] private List<SkinnedMeshRenderer> _skinnedMeshRenderers = new List<SkinnedMeshRenderer>();

    public async void Init(EnemyParam enemyParam)
    {
        if (_rb == null) _rb = GetComponent<Rigidbody>();
        _navMeshPath = new NavMeshPath();
        _maxHp = enemyParam.Hp;
        _hp = enemyParam.Hp;
        _atk = enemyParam.Atk;
        _def = enemyParam.Def;
        _moveSpeed = enemyParam.MoveSpeed;
        _runSpeed = enemyParam.RunSpeed;
        _moveThreshold = enemyParam.MoveThreshold;
        _chaseThreshold = enemyParam.ChaseThreshold;
        _waitTime = enemyParam.WaitTime;
        _isLockState = false;

        _material = new Material(_skinnedMeshRenderers[0].material);
        _skinnedMeshRenderers.ForEach(x => x.material = _material);

        await DissolveAsync(1f, 0);
    }
    public void SetEnemyState(EnemyStateType enemyStateType, bool isLockState = false)
    {
        if (_isLockState) return;
        _isLockState = isLockState;
        _enemyStateType = enemyStateType;
    }
    public void ReleaseLockState()
    {
        _isLockState = false;
    }
    public void SetDirection(Vector3 targetPos, Vector3 currentPos)
    {
        _moveDirection = (targetPos - currentPos);
        _moveDirection = _moveDirection.normalized;
    }
    public void SetVelocity(Vector3 targetPos, Vector3 currentPos, float speed)
    {
        SetDirection(targetPos, currentPos);
        RotationTween(_moveDirection);
        _rb.velocity = _moveDirection * speed;
    }
    public bool NavMove(Vector3 currentPos, float speed)
    {
        if (NavCornerExists() == false)
        {
            UpdateNavPath(_nextPoint);
            if (NavCornerExists() == false)
            {
                return false;
            }
        }
        if (Vector3.Distance(_nextPoint, currentPos) > 0.1f) return false;
        _nextPoint = _navMeshCorners.Dequeue();
        _nextPoint = new Vector3(_nextPoint.x, 0, _nextPoint.z);
        SetVelocity(_nextPoint, currentPos, speed);
        return true;
    }
    public void RotationTween(Vector3 targetDir, float duration = 1)
    {
        _rotationTween?.Kill();
        _rotationTween = transform.DORotateQuaternion(Quaternion.LookRotation(targetDir), duration).SetEase(Ease.Linear);
        _rotationTween.Play();
    }
    public void UpdateNavPath(Vector3 targetPos)
    {
        if (!NavMesh.SamplePosition(transform.position, out var hitStart, 2f, NavMesh.AllAreas))
        {
            Debug.LogWarning($"Enemy({this})が{nameof(NavMesh)}上にない");
            return;
        }

        if (!NavMesh.SamplePosition(targetPos, out var hitTarget, 2f, NavMesh.AllAreas))
        {
            Debug.LogWarning($"targetPos({targetPos})が{nameof(NavMesh)}上にない");
            return;
        }

        bool isOk = NavMesh.CalculatePath(hitStart.position, hitTarget.position, NavMesh.AllAreas, _navMeshPath);
        if (!isOk || _navMeshPath.corners.Length < 2)
        {
            //Debug.LogWarning($"{nameof(_navMeshPath.corners)}が想定数分ありません");
            StopMove();
            _navMeshPath = new NavMeshPath();
            return;
        }
        _nextPoint = CurrentPos;
        _navMeshCorners.Clear();
        _navMeshCorners.EnqueueRange(_navMeshPath.corners.Skip(1));
    }
    public virtual void StopMove()
    {
        if (_rb == null) return;
        _rb.velocity = Vector3.zero;
        NavClear();
    }
    public void NavClear()
    {
        _navMeshCorners.Clear();
        _navMeshPath.ClearCorners();
    }
    private bool NavCornerExists()
    {
        return _navMeshCorners.Count > 0;
    }
    public virtual void TakeDamage(int damage)
    {
        _hp -= damage;
    }
    public async UniTask DissolveAsync(float targetValue, float duration = 0.5f)
    {
        await DOTween.To(
            () => _material.GetFloat(_dissolveID),
            x => _material.SetFloat(_dissolveID, x),
            targetValue,
            duration)
            .SetEase(Ease.Linear)
            .ToUniTask();
    }

    public void SetKinematic(bool kinematic)
    {
        _rb.isKinematic = kinematic;
    }
    private void OnDestroy()
    {
        _rotationTween?.Kill();
    }
}
public static class QueueExtension
{
    public static void EnqueueRange<T>(this Queue<T> self, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            self.Enqueue(item);
        }
    }
}
