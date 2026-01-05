using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class EnemyChase : EnemyStateBase
{
    private PlayerMove _playerMove;
    public EnemyChase(EnemyObj enemyObj)
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
        var playerMove = _playerMove;
        if (playerMove == null)
        {
            await UniTask.CompletedTask;
            return;
        }

        Vector3 lastPlayerPos = playerMove.transform.position;
        while (!token.IsCancellationRequested)
        {
            // プレイヤーが破棄されていたら抜ける
            if (playerMove == null || playerMove.gameObject == null) break;

            var currentPlayerPos = playerMove.transform.position;
            var distance = Vector3.Distance(currentPlayerPos, _enemyObj.CurrentPos);

            if (distance <= _enemyObj.ChaseThreshold) break;
            if (currentPlayerPos != lastPlayerPos)
            {
                if (!_enemyObj) break;
                _enemyObj.UpdateNavPath(currentPlayerPos);
                _enemyObj.NavMove(_enemyObj.CurrentPos, _enemyObj.RunSpeed);
                _enemyObj.AnimController?.SetAnim(ParamConsts.RUN, true);
                lastPlayerPos = currentPlayerPos;
            }
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
    }
    public override async UniTask Exit(CancellationToken token)
    {
        _enemyObj.StopMove();
        _enemyObj.AnimController.SetAnim(ParamConsts.RUN, false);
        await UniTask.CompletedTask;
    }
}