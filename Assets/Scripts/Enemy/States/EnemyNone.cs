using Cysharp.Threading.Tasks;
using System.Threading;

//空の処理
public class EnemyNone : EnemyStateBase
{
    public EnemyNone(EnemyObj enemyObj)
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