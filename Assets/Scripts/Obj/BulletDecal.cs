using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BulletDecal : MonoBehaviour, IPool
{
    [SerializeField] private DecalProjector _decalProjector;
    private CancellationTokenSource _tokenSource;
    private float _viewTime;

    public bool IsGenericUse { get; set; }
    public void Init(Vector3 pos, float viewTime = 3)
    {
        transform.position = pos;
        _viewTime = viewTime;
        ViewAsync().Forget();
    }
    private async UniTask ViewAsync()
    {
        await UniTask.WaitForSeconds(_viewTime);
        await DOVirtual.Float(1, 0, 0.5f, f =>
        {
            _decalProjector.fadeFactor = f;
        }).ToUniTask(cancellationToken: _tokenSource.Token);
        DecalPool.Instance.ReleaseBulletDecal(this);
    }
    public void OnRelease()
    {
        _decalProjector.fadeFactor = 0;
        _tokenSource?.Cancel();
    }

    public void OnReuse()
    {
        _decalProjector.fadeFactor = 1;
        _tokenSource?.Cancel();
        _tokenSource = new CancellationTokenSource();
    }
    private void OnDestroy()
    {
        _tokenSource?.Cancel();
    }
}
