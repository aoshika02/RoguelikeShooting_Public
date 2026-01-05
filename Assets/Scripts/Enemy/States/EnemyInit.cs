using Cysharp.Threading.Tasks;
using System.Threading;

public class EnemyInit : EnemyStateBase
{
    public EnemyInit(EnemyObj enemyObj)
    {
        _enemyObj = enemyObj;
    }
    public override async UniTask Entry(CancellationToken token)
    {
        await UniTask.CompletedTask;
    }
    public override async UniTask Do(CancellationToken token)
    {
        //スポーン位置の初期化
        _enemyObj.transform.position = _enemyObj.GetTargetPoint(0);
        await UniTask.CompletedTask;
    }
    public override async UniTask Exit(CancellationToken token)
    {
        await UniTask.CompletedTask;
    }
}
