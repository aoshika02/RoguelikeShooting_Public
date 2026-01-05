using UnityEngine;
using UnityEngine.InputSystem;
using UniRx;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem.LowLevel;
public class InputManager : SingletonMonoBehaviour<InputManager>
{
    [SerializeField] private PlayerAction _playerAction;
    [SerializeField] private Camera _mainCamera;
    public IObservable<GameObject> OnTapped => _onTapped;
    private readonly Subject<GameObject> _onTapped = new Subject<GameObject>();
    public IObservable<GameObject> OnCanceled => _onCanceled;
    private readonly Subject<GameObject> _onCanceled = new Subject<GameObject>();
    public IReadOnlyReactiveProperty<Vector3> MousePos => _mousePos;
    private readonly ReactiveProperty<Vector3> _mousePos = new ReactiveProperty<Vector3>();
    public IReadOnlyReactiveProperty<Vector2> Move => _move;
    private readonly ReactiveProperty<Vector2> _move = new ReactiveProperty<Vector2>();
    public IReadOnlyReactiveProperty<float> Dash => _dash;
    private readonly ReactiveProperty<float> _dash = new ReactiveProperty<float>();
    public IReadOnlyReactiveProperty<float> Decision => _decision;
    private readonly ReactiveProperty<float> _decision = new ReactiveProperty<float>();
    public IReadOnlyReactiveProperty<float> MouseClicked => _mouseClicked;
    private readonly ReactiveProperty<float> _mouseClicked = new ReactiveProperty<float>();

    private bool _isTapping = false;
    private bool _isEffectView = false;
    [SerializeField] private List<Canvas> _hitCanvases;
    [SerializeField] private Canvas _effectCanvas;
    private List<GraphicRaycaster> _raycasters = new List<GraphicRaycaster>();
    private PointerEventData _pointerEventData;
    private EventSystem _eventSystem;
    private GameStateManager _stateManager;

    #region Move
    public void SetMoveStartedAction(Action<InputAction.CallbackContext> action)
    {
        _playerAction.Player.Move.started += action;
    }

    public void SetMovePerformedAction(Action<InputAction.CallbackContext> action)
    {
        _playerAction.Player.Move.performed += action;
    }

    public void SetMoveCanceledAction(Action<InputAction.CallbackContext> action)
    {
        _playerAction.Player.Move.canceled += action;
    }

    public void RemoveMoveStartedAction(Action<InputAction.CallbackContext> action)
    {
        _playerAction.Player.Move.started -= action;
    }

    public void RemoveMovePerformedAction(Action<InputAction.CallbackContext> action)
    {
        _playerAction.Player.Move.performed -= action;
    }

    public void RemoveMoveCanceledAction(Action<InputAction.CallbackContext> action)
    {
        _playerAction.Player.Move.canceled -= action;
    }

    #endregion

    #region Run

    public void SetRunStartedAction(Action<InputAction.CallbackContext> action)
    {
        _playerAction.Player.Dash.started += action;
    }

    public void SetRunPerformedAction(Action<InputAction.CallbackContext> action)
    {
        _playerAction.Player.Dash.performed += action;
    }

    public void SetRunCanceledAction(Action<InputAction.CallbackContext> action)
    {
        _playerAction.Player.Dash.canceled += action;
    }

    public void RemoveRunStartedAction(Action<InputAction.CallbackContext> action)
    {
        _playerAction.Player.Dash.started -= action;
    }

    public void RemoveRunPerformedAction(Action<InputAction.CallbackContext> action)
    {
        _playerAction.Player.Dash.performed -= action;
    }

    public void RemoveRunCanceledAction(Action<InputAction.CallbackContext> action)
    {
        _playerAction.Player.Dash.canceled -= action;
    }

    #endregion

    #region CallbackContext
    private void OnTapStartedCallback(InputAction.CallbackContext context)
    {
        OnTapStarted(context);
        _mouseClicked.Value = context.ReadValue<float>();
    }
    private void OnTapCanceledCallback(InputAction.CallbackContext context)
    {
        OnTapCanceled(context);
        _mouseClicked.Value = context.ReadValue<float>();
    }
    private void OnDecision(InputAction.CallbackContext context)
    {
        _decision.Value = context.ReadValue<float>();
    }
    private void OnDash(InputAction.CallbackContext context)
    {
        _dash.Value = context.ReadValue<float>();
    }
    private void OnMove(InputAction.CallbackContext context)
    {
        _move.Value = context.ReadValue<Vector2>();
    }
    #endregion

    private void OnEnable()
    {
        _playerAction.Enable();

        _playerAction.PlayerTouch.Tap.started += OnTapStartedCallback;
        _playerAction.PlayerTouch.Tap.canceled += OnTapCanceledCallback;

        _playerAction.Player.Decision.started += OnDecision;
        _playerAction.Player.Decision.performed += OnDecision;
        _playerAction.Player.Decision.canceled += OnDecision;

        _playerAction.Player.Move.started += OnMove;
        _playerAction.Player.Move.performed += OnMove;
        _playerAction.Player.Move.canceled += OnMove;

        _playerAction.Player.Dash.started += OnDash;
        _playerAction.Player.Dash.performed += OnDash;
        _playerAction.Player.Dash.canceled += OnDash;
    }

    private void OnDisable()
    {
        if (_playerAction != null)
        {
            _playerAction.PlayerTouch.Tap.started -= OnTapStartedCallback;
            _playerAction.PlayerTouch.Tap.canceled -= OnTapCanceledCallback;

            _playerAction.Player.Decision.started -= OnDecision;
            _playerAction.Player.Decision.performed -= OnDecision;
            _playerAction.Player.Decision.canceled -= OnDecision;

            _playerAction.Player.Move.started -= OnMove;
            _playerAction.Player.Move.performed -= OnMove;
            _playerAction.Player.Move.canceled -= OnMove;

            _playerAction.Player.Dash.started -= OnDash;
            _playerAction.Player.Dash.performed -= OnDash;
            _playerAction.Player.Dash.canceled -= OnDash;
            _playerAction.Disable();
        }
    }
    protected override void Awake()
    {
        if (!CheckInstance())
        {
            return;
        }

        if (_playerAction == null) _playerAction = new PlayerAction();

        foreach (var canvas in _hitCanvases)
        {
            _raycasters.Add(canvas.GetComponent<GraphicRaycaster>());
        }
        _eventSystem = EventSystem.current;
    }
    private void Start()
    {
        _stateManager = GameStateManager.Instance;
        _stateManager.SetStateChangedAction(MouseReset);
        MouseReset(_stateManager.StateType);
    }
    public void MouseReset(GameStateType stateType)
    {
        // 画面中央座標を計算
        Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);

        // 実際のカーソル位置を中央に移動
        Mouse.current.WarpCursorPosition(center);

        // InputSystemにも反映（同期用）
        InputState.Change(Mouse.current.position, center);
        if (stateType == GameStateType.BuffSelect || stateType == GameStateType.TextEvent)
        {
            // 中央に固定
            Cursor.lockState = CursorLockMode.None;
            _isEffectView = true;
            return;
        }
        Cursor.lockState = CursorLockMode.Locked;
        _isEffectView = false;
    }
    /// <summary>
    /// タップ関連
    /// </summary>
    /// <param name="context"></param>
    private void OnTapStarted(InputAction.CallbackContext context)
    {
        TapEvent(_onTapped, true, context);
        _isTapping = true;
    }
    private void Update()
    {
        _mousePos.Value = Mouse.current.delta.ReadValue();
    }

    private void OnTapCanceled(InputAction.CallbackContext context)
    {
        TapEvent(_onCanceled, false, context);
        _isTapping = false;
    }
    private void TapEvent(Subject<GameObject> subject, bool isStarted, InputAction.CallbackContext context)
    {
        var tmpScreenPos = GetDeviceValue();

        if (tmpScreenPos == null) return;
        Vector3 screenPos = tmpScreenPos.Value;
        screenPos.z = _mainCamera.nearClipPlane;
        Vector3 worldPos = _mainCamera.ScreenToWorldPoint(screenPos);
        Vector3 direction = (worldPos - _mainCamera.transform.position).normalized;

        List<GameObject> uiHits = RaycastUI(screenPos);
        if (uiHits != null && uiHits.Count > 0)
        {
            subject.OnNext(uiHits[0]);
        }

        List<GameObject> hitObjs = Raycast3D(worldPos, direction);
        if (hitObjs != null)
        {
            _onTapped.OnNext(hitObjs[0]);
        }
        if (_effectCanvas == null) return;
        if (isStarted && _isEffectView)
        {
            // CanvasのRectTransform内ローカル座標に変換
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                 _effectCanvas.transform as RectTransform,
                 screenPos,
                 null,
                 out Vector2 localPos
             ))
            {
                var obj = TapEffectPool.Instance.GetTapEffectObj();
                obj.Init(localPos);
                obj.CallEffect().Forget();
            }
        }
    }
    /// <summary>
    /// デバイスに応じた座標を返す
    /// </summary>
    /// <returns></returns>
    private Vector3? GetDeviceValue()
    {
        //マウスが接続ならマウス座標を返す
        if (Mouse.current != null)
        {
            return Mouse.current.position.ReadValue();
        }
        //指の座標を返す
        if (Touchscreen.current != null)
        {
            if (Touchscreen.current.touches.Count == 1)
            {
                return Touchscreen.current.primaryTouch.position.ReadValue();
            }
        }
        //例外
        return null;
    }
    private List<GameObject> RaycastUI(Vector2 screenPosition)
    {
        _pointerEventData = new PointerEventData(_eventSystem)
        {
            position = screenPosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        List<List<RaycastResult>> resultDatas = new List<List<RaycastResult>>();
        foreach (var cast in _raycasters)
        {
            cast.Raycast(_pointerEventData, results);
            resultDatas.Add(new List<RaycastResult>(results));
            results.Clear();
        }

        return resultDatas.SelectMany(list => list).Select(r => r.gameObject).ToList();
    }
    private List<GameObject> Raycast3D(
        Vector3 origin,
        Vector3 direction,
        float distance = Mathf.Infinity)
    {
        RaycastHit[] hitObjs = Physics.RaycastAll(origin, direction, distance);
        //Debug.DrawRay(origin, direction, Color.red, 1f);
        if (hitObjs.Length == 0) return null;
        RaycastHit[] sortedHits = hitObjs.OrderBy(hit => hit.distance).ToArray();
        List<GameObject> hisObjs = new List<GameObject>();
        foreach (var hitObj in sortedHits)
        {
            if (hitObj.collider == null) continue;
            hisObjs.Add(hitObj.collider.gameObject);
        }
        return hisObjs;
    }
    private void OnDestroy()
    {
        if (GameStateManager.Instance != null)
        {
            _stateManager.RemoveStateChangedAction(MouseReset);
        }
    }
}
