using Cysharp.Threading.Tasks;
using System.Threading;

public class EnemyDamage : EnemyStateBase
{
    public EnemyDamage(EnemyObj enemyObj)
    {
        _enemyObj = enemyObj;
    }
    public override async UniTask Entry(CancellationToken token)
    {
        await UniTask.CompletedTask;
    }
    public override async UniTask Do(CancellationToken token)
    {
        await UniTask.CompletedTask;
    }
    public override async UniTask Exit(CancellationToken token)
    {
        await UniTask.CompletedTask;
    }
}