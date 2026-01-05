using Cysharp.Threading.Tasks;
using System.Threading;

public class EnemyStay : EnemyStateBase
{
    public EnemyStay(EnemyObj enemyObj)
    {
        _enemyObj = enemyObj;
    }
    public override async UniTask Entry(CancellationToken token)
    {
        await UniTask.CompletedTask;
    }
    public override async UniTask Do(CancellationToken token)
    {
        //待機処理
        await UniTask.WaitForSeconds(_enemyObj.WaitTime, cancellationToken: token);
    }
    public override async UniTask Exit(CancellationToken token)
    {
        await UniTask.CompletedTask;
    }
}