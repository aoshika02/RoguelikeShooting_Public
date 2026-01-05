using UnityEngine;
using UnityEngine.InputSystem;
using UniRx;
using System;
using DG.Tweening;
using Cysharp.Threading.Tasks;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMove : SingletonMonoBehaviour<PlayerMove>
{
    private Vector2 _moveDirection;
    private Rigidbody _rb;
    private bool _isMove;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _dashSpeed;
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private Camera _camera;
    private int _defaultFov = 60;
    private int _dashFov = 75;
    private bool _isRotate;
    private bool _isDash = false;
    private bool _lastIsDash = false;
    private bool _isDashFov = false;
    private Tween _fovTween;
    //回転系統
    [SerializeField]
    float _maxCameraRotateY;

    [SerializeField]
    float _minCameraRotateY;

    private float _cameraAngleY = 0;
    [SerializeField] private float _rotationSpeed = 1f;

    private PlayerStatusManager _statusManager;
    private ShootView _shootView;
    void Start()
    {
        _isRotate = true;
        _isDash = false;
        _statusManager = PlayerStatusManager.Instance;
        _shootView = ShootView.Instance;
        _rb = GetComponent<Rigidbody>();
        //playerの移動イベントの登録
        InputManager.Instance.SetMoveStartedAction(PressedInputMovement);
        InputManager.Instance.SetMovePerformedAction(PressedInputMovement);
        InputManager.Instance.SetMoveCanceledAction(CanceledMoveDirection);
        GameStateManager.Instance.SetStateChangedAction(OnStateChange);

        InputManager.Instance.MousePos.Subscribe(x =>
        {
            if (_isMove == false) return;
            CameraMove(x);
        }).AddTo(this);

        InputManager.Instance.Dash.Subscribe(x =>
        {
            _isDash = x == 1;
        }).AddTo(this);
    }
    private void FovFlow()
    {
        float targetFov = _isDash ? _dashFov : _defaultFov;
        _fovTween?.Kill();

        _fovTween = DOTween.To(
            () => _camera.fieldOfView,
            f => _camera.fieldOfView = f,
            targetFov,
            0.5f
        ).SetEase(Ease.OutQuad);
    }
    void Update()
    {
        if (_isMove == false)
        {
            _rb.velocity = Vector3.zero;
            return;
        }
        var cameraAngle = Quaternion.Euler(0, _cameraTransform.transform.rotation.eulerAngles.y, 0);

        _rb.velocity = cameraAngle * new Vector3(_moveDirection.x, 0, _moveDirection.y) * (_isDash ? _statusManager.GetDashSpeed() : _moveSpeed);

        //ダッシュ時の見た目
        if (_rb.velocity == Vector3.zero)
        {
            if (_isDashFov)
            {
                FovFlow();
                _shootView.SetRunAnim(_isDash);
                _isDashFov = false;
            }
            return;
        }

        if (_isDash == _lastIsDash) return;

        FovFlow();
        _shootView.SetRunAnim(_isDash);
        _isDashFov = true;
        _lastIsDash = _isDash;

    }
    private void CameraMove(Vector2 mousePos)
    {
        if (_isRotate == false) return;
        transform.Rotate(0, mousePos.x * _rotationSpeed, 0);
        _cameraAngleY -= mousePos.y * _rotationSpeed;

        _cameraAngleY = Mathf.Clamp(_cameraAngleY, _minCameraRotateY, _maxCameraRotateY);

        var sampleAngle = _cameraTransform.eulerAngles;
        sampleAngle.x = _cameraAngleY;
        _cameraTransform.eulerAngles = sampleAngle;
    }
    public void OnStateChange(GameStateType stateType)
    {
        if (stateType == GameStateType.Action)
        {
            SetMove(true);
            return;
        }
        SetMove(false);
    }
    public void SetMove(bool isMove)
    {
        _isMove = isMove;
    }
    public void InitPos(PlayerInitData playerInitData)
    {
        transform.position = playerInitData.PlayerStartPos.position;
        transform.rotation = Quaternion.Euler(0, playerInitData.PlayerRotationY, 0);
        _cameraTransform.rotation = Quaternion.Euler(playerInitData.CameraRotationX, 0, 0);
        _cameraAngleY = playerInitData.CameraRotationX;
    }
    #region PlayerMove

    private void PressedInputMovement(InputAction.CallbackContext context)
    {
        _moveDirection = context.ReadValue<Vector2>();
    }

    private void CanceledMoveDirection(InputAction.CallbackContext context)
    {
        _moveDirection = Vector2.zero;
    }

    #endregion

    private void OnDisable()
    {
        //playerの移動イベントの登録解除
        InputManager.Instance.RemoveMoveStartedAction(PressedInputMovement);
        InputManager.Instance.RemoveMovePerformedAction(PressedInputMovement);
        InputManager.Instance.RemoveMoveCanceledAction(CanceledMoveDirection);
        GameStateManager.Instance.RemoveStateChangedAction(OnStateChange);

        _fovTween?.Kill();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(ParamConsts.ENEMY_ATTACK))
        {
            if (other.TryGetComponent(out EnemyAttackCall enemyAttackCall))
            {
                var enemyObj = enemyAttackCall.GetEnemyObj();
                var attack = enemyObj.Atk;
                _statusManager.TakeDamageAsync(attack).Forget();
            }
        }
    }
}