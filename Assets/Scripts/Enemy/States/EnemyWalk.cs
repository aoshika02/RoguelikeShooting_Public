using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class EnemyWalk : EnemyStateBase
{
    public EnemyWalk(EnemyObj enemyObj)
    {
        _enemyObj = enemyObj;
    }
    public override async UniTask Entry(CancellationToken token)
    {
        await UniTask.CompletedTask;
    }
    public override async UniTask Do(CancellationToken token)
    {
        //ターゲットを取得
        var target = _enemyObj.GetTargetPoint(_enemyObj.CurrentIndex);
        //パスを作成
        _enemyObj.UpdateNavPath(target);
        while (!token.IsCancellationRequested && Vector3.Distance(_enemyObj.CurrentPos, target) > 0.1f)
        {
            if (token.IsCancellationRequested) break;
            //移動
            if (_enemyObj.NavMove(_enemyObj.CurrentPos, _enemyObj.MoveSpeed))
            {
                _enemyObj.AnimController.SetAnim(ParamConsts.WALK, true);
                //ターゲットに到着するまで待機
                await UniTask.WaitUntil(() => Vector3.Distance(_enemyObj.CurrentPos, _enemyObj.NextPoint) < 0.1f, cancellationToken: token);
            }
            else
            {
                //パスが存在しない場合は再度パスを作成
                _enemyObj.UpdateNavPath(target);
                await UniTask.Yield();
            }
        }
        //インデックス更新
        _enemyObj.UpdateTargetIndex();
    }
    public override async UniTask Exit(CancellationToken token)
    {
        _enemyObj.StopMove();
        _enemyObj.AnimController.SetAnim(ParamConsts.WALK, false);
        await UniTask.CompletedTask;
    }
}