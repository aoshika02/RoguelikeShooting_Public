using Cysharp.Threading.Tasks;
using System.Threading;

public class EnemyDead : EnemyStateBase
{
    public EnemyDead(EnemyObj enemyObj)
    {
        _enemyObj = enemyObj;
    }
    public override async UniTask Entry(CancellationToken token)
    {
        await UniTask.Yield();
    }
    public override async UniTask Do(CancellationToken token)
    {
        await _enemyObj.AnimController.SetAnimAsync(ParamConsts.DIE, true);
        //ディゾルブ開始
        await _enemyObj.DissolveAsync(0);
        _enemyObj.SetEnemyState(EnemyStateType.Dead, true);
        _enemyObj.OnRelease();
    }
    public override async UniTask Exit(CancellationToken token)
    {
        await UniTask.Yield();
    }
}
