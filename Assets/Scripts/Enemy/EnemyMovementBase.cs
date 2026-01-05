using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class EnemyMovementBase : EnemyBase
{
    private List<Transform> _targetPoints = new List<Transform>();
    //true:パスを折り返して動く A->B->C->D->C
    //false:パスの始点と終点が繋がっているものとして動く A->B->C->D->A
    private bool _isTurnAround = false;
    //インデックスを加算して動くかどうか
    private bool _isUpper = true;
    // 目標地点のインデックス
    public int CurrentIndex => _currentIndex;
    private int _currentIndex = 0;
    // プレイヤー探索中の多重呼び出し防止
    private bool _isSearchCall = false;
    public void Init(List<Transform> transforms)
    {
        _targetPoints = transforms;
        _currentIndex = 0;
        _isSearchCall = false;
    }
    public Vector3 GetTargetPoint(int index) => _targetPoints[index].position;
    public int GetTargetPointCount() => _targetPoints.Count;
    public void UpdateTargetIndex()
    {
        if (_targetPoints == null || _targetPoints.Count == 0) return;

        if (_isTurnAround && _targetPoints.Count > 2)
        {
            // 次のインデックスを先に計算する
            int next = _currentIndex + (_isUpper ? 1 : -1);

            // 折り返し処理
            if (next >= _targetPoints.Count)
            {
                _isUpper = false;
                next = _currentIndex - 1;
            }
            else if (next < 0)
            {
                _isUpper = true;
                next = _currentIndex + 1;
            }

            _currentIndex = next;
        }
        else
        {
            // ループ型
            _currentIndex = (_currentIndex + 1) % _targetPoints.Count;
        }
    }
    public async UniTask SearchPlayer(CancellationToken token)
    {
        if (_isSearchCall) return;
        _isSearchCall = true;

        try
        {
            var selfTransform = transform;
            while (!token.IsCancellationRequested)
            {
                if (selfTransform == null) break;

                Vector3 origin = selfTransform.position;
                Vector3 direction = MoveDirection;
                float distance = 5f;

                Vector3 halfExtents = new Vector3(5f, 1f, 0.5f);
                if (Physics.BoxCast(origin, halfExtents, direction, out RaycastHit boxHitInfo, selfTransform.rotation, distance) &&
                    boxHitInfo.collider != null && boxHitInfo.collider.CompareTag(ParamConsts.PLAYER))
                {
                    Debug.Log("Player->BoxCast");
                    break;
                }

                Vector3 rayOrigin = origin + Vector3.down * 0.5f;
                Ray ray = new Ray(rayOrigin, direction);
                if (Physics.Raycast(ray, out RaycastHit rayHitInfo, 30f) &&
                    rayHitInfo.collider != null && rayHitInfo.collider.CompareTag(ParamConsts.PLAYER))
                {
                    Debug.Log("Player->RayCast");
                    break;
                }

                Debug.DrawRay(rayOrigin, direction * 30f, UnityEngine.Color.red, Time.deltaTime);
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }
        catch (System.OperationCanceledException)
        {
            Debug.Log("SearchPlayer:キャンセルされました");
        }
        finally
        {
            _isSearchCall = false;
        }
    }
}
