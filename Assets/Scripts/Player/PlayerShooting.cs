using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UniRx;
using UnityEngine;

public class PlayerShooting : SingletonMonoBehaviour<PlayerShooting>
{
    [SerializeField] private Transform _origin;
    [SerializeField] private Transform _target;
    [SerializeField] private float _speed;
    public int CurrentBullet => _currentBullet;
    private int _currentBullet;
    private int _maxBullet;
    private bool _isShootCall = false;
    private bool _isShoot = false;
    private bool _isCharge = false;
    private BulletPool _bulletPool;
    private PlayerStatusManager _statusManager;
    private ShootView _shootView;
    private CancellationTokenSource _cancellationTokenSource;
    public IReadOnlyReactiveProperty<float> BulletCharge => _bulletCharge;
    private ReactiveProperty<float> _bulletCharge = new ReactiveProperty<float>();
    public IObservable<(float, int)> BulltCount => _bulletCount;
    private Subject<(float, int)> _bulletCount = new Subject<(float, int)>();
    public IObservable<int> MaxBulltCount => _maxBulletCount;
    private Subject<int> _maxBulletCount = new Subject<int>();

    private SoundManager _soundManager;
    void Start()
    {
        _bulletPool = BulletPool.Instance;
        _shootView = ShootView.Instance;
        _soundManager = SoundManager.Instance;
        GameStateManager.Instance.SetStateChangedAction(OnStateChange);
        InputManager.Instance.MouseClicked.Subscribe(x =>
        {
            if (x != 1) return;
            if (_isShoot == false) return;
            Shoot().Forget();
        }).AddTo(this);
    }
    public void SetCharge(bool isCharge) 
    {
        _isCharge = isCharge;
    }
    public void OnStateChange(GameStateType stateType)
    {
        if (stateType == GameStateType.Action)
        {
            _isShoot = true;
            return;
        }
        _isShoot = false;
    }
    public void Init()
    {
        _statusManager = PlayerStatusManager.Instance;
        UpdateBullet();
        _bulletCharge.Value = 1;
        _cancellationTokenSource = new CancellationTokenSource();
    }
    public void ResetBullt()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        if (_maxBullet != _statusManager.GetBulletCount())
        {
            UpdateBullet();
        }
    }
    private void UpdateBullet()
    {
        _maxBullet = _statusManager.GetBulletCount();
        _currentBullet = _maxBullet;
        _maxBulletCount.OnNext(_maxBullet);
    }
    private async UniTask Shoot()
    {
        if (_isShootCall) return;
        if (_currentBullet <= 0) return;
        _isShootCall = true;
        var dir = (_target.transform.position - _origin.transform.position).normalized;
        Debug.DrawRay(_origin.position, dir * 5f, Color.blue, 1f);
        var obj = _bulletPool.GetBulletObj();
        obj.transform.position = _target.transform.position;
        obj.Init(dir, _speed);
        _soundManager.PlaySE(SEType.Shoot);
        _currentBullet--;
        _bulletCount.OnNext((0, _currentBullet));
        if (_currentBullet == 0) BulletChargeAsync().Forget();
        await _shootView.ShootAsync();
        _isShootCall = false;
    }
    private async UniTask BulletChargeAsync()
    {
        await UniTask.WaitUntil(()=> _isCharge);
        _soundManager.PlaySE(SEType.NoBullet);
        _bulletCharge.Value = 0;
        _shootView.SetGunColor(false);
        var duration = 3.5f / _statusManager.GetReloadSpeed();
        await DOVirtual.Float(0, 1, duration, f =>
        {
            _bulletCharge.Value = f;
        }).ToUniTask(cancellationToken: _cancellationTokenSource.Token);
        _soundManager.PlaySE(SEType.Reload);
        _currentBullet = _maxBullet;
        _shootView.SetGunColor(true);
    }
    private void OnDestroy()
    {
        GameStateManager.Instance.RemoveStateChangedAction(OnStateChange);
    }
}
