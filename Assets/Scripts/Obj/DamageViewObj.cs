using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using TMPro;
using UnityEngine;

public class DamageViewObj : MonoBehaviour, IPool
{
    [SerializeField] private TextMeshProUGUI _damegeText;
    [SerializeField] private RectTransform _damegeRect;
    private int _maxFontSize = 60;
    private int _minFontSize = 50;
    private Camera _camera;
    private Vector3 _targetPos;
    private DamagePool _pool;
    private CancellationTokenSource _cancellationTokenSource;
    public bool IsGenericUse { get; set; }
    public void Init(Vector3 pos, int damage, Camera camera)
    {
        _damegeText.text = damage.ToString();
        _damegeText.fontSize = _minFontSize;
        _targetPos = pos;
        _camera = camera;
        _cancellationTokenSource = new CancellationTokenSource();
        if (_pool == null) _pool = DamagePool.Instance;
        DamageFlow(_cancellationTokenSource.Token).Forget();
    }
    private void LateUpdate()
    {
        if (IsGenericUse == false) return;
        var screenPoint = _camera.WorldToScreenPoint(_targetPos);
        _damegeRect.transform.position = screenPoint;
    }
    private async UniTask DamageFlow(CancellationToken token)
    {
        await DOTween.To(() => _damegeText.fontSize,
            (x) => _damegeText.fontSize = x,
            _maxFontSize,
            0.2f).ToUniTask(cancellationToken: token);
        await UniTask.WaitForSeconds(0.3f, cancellationToken: token);
        await DOTween.To(() => _damegeText.fontSize,
            (x) => _damegeText.fontSize = x,
            _minFontSize,
            0.2f).ToUniTask(cancellationToken: token);
        _pool.ReleaseDamageViewObj(this);
    }
    public void OnRelease()
    {
        gameObject.SetActive(false);
        _cancellationTokenSource?.Cancel();
    }

    public void OnReuse()
    {
        gameObject.SetActive(true);
    }
    private void OnDestroy()
    {
        _cancellationTokenSource?.Cancel();
    }
}
