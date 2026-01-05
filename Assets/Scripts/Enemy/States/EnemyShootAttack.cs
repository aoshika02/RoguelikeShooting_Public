using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class EnemyShootAttack : EnemyStateBase
{
    private PlayerMove _playerMove;
    public EnemyShootAttack(EnemyObj enemyObj)
    {
        _enemyObj = enemyObj;
        _playerMove = PlayerMove.Instance;
    }
    public override async UniTask Entry(CancellationToken token)
    {
        await UniTask.CompletedTask;
    }
    public override async UniTask Do(CancellationToken token)
    {
        var playerPos = _playerMove.transform.position;
        Vector3 forward = _enemyObj.transform.forward;
        Vector3 toPlayer = (playerPos - _enemyObj.CurrentPos).normalized;

        forward.y = 0;
        toPlayer.y = 0;

        float dot = Vector3.Dot(forward, toPlayer);

        if (dot < 0.95f)
        {
            await UniTask.WaitForSeconds((dot < 0f) ? 1.0f : 0.5f, cancellationToken: token);
            _enemyObj.AnimController.SetAnim(ParamConsts.WALK, true);
            _enemyObj.RotationTween(toPlayer);
            await UniTask.WaitForSeconds(1f, cancellationToken: token);
            _enemyObj.AnimController.SetAnim(ParamConsts.WALK, false);
            await UniTask.Yield(token);
        }
        await _enemyObj.AnimController.SetAnimAsync(ParamConsts.ATTACK, true);       
        await UniTask.WaitForSeconds(_enemyObj.WaitTime, cancellationToken: token);
        _enemyObj.AnimController.SetAnim(ParamConsts.ATTACK, false);
    }
    public override async UniTask Exit(CancellationToken token)
    {
        await UniTask.CompletedTask;
    }
}
