using Cysharp.Threading.Tasks;
using System.Threading;

public class EnemyStateBase
{
    protected EnemyObj _enemyObj;
    public virtual async UniTask Entry(CancellationToken token)
    {
        await UniTask.CompletedTask;
    }
    public virtual async UniTask Do(CancellationToken token)
    {
        await UniTask.CompletedTask;
    }
    public virtual async UniTask Exit(CancellationToken token)
    {
        await UniTask.CompletedTask;
    }
}
