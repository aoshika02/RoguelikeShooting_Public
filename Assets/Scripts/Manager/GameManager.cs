using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    private int _clearFloorCount = 0;
    private int _maxFloorCount = 10;
    private int _enemyCount = 0;
    private bool _isExit = false;
    private bool _isEnter = false;
    private PlayerMove _playerMove;
    private StagePool _stagePool;
    private StageObj _currentStage;
    private EnemyPool _enemyPool;
    private EnemyTypeRandomizer _enemyTypeRandomizer;
    private EnemyStatusManager _enemyStatusManager;
    private PlayerShooting _playerShooting;
    private PlayerStatusManager _playerStatusManager;
    private BuffManager _buffManager;
    private BuffViewManager _buffViewManager;
    private BuffSlotObj _slotObj;
    private GameStateManager _gameStateManager;
    private NaviSystem _naviSystem;
    private StageTypeRandomizer _stageTypeRandomizer;
    private TimerManager _timerManager;
    private KillCounter _killCounter;
    private DamageCounter _damageCounter;
    private ResultView _resultView;
    private TextSystemView _textSystemView;
    private TextEventManager _textEventManager;
    private MoveFarChecker _moveFarChecker;
    private StatusUI _statusUI;
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    protected override void Awake()
    {
        if (CheckInstance() == false) return;
    }
    public async void Init(bool isTutorial)
    {
        _clearFloorCount = 0;
        bool isExist =
            PlayerMove.Instance != null &&
            StagePool.Instance != null &&
            EnemyPool.Instance != null &&
            EnemyTypeRandomizer.Instance != null &&
            EnemyStatusManager.Instance != null &&
            FadeManager.Instance != null &&
            PlayerShooting.Instance != null &&
            PlayerStatusManager.Instance != null &&
            BuffManager.Instance != null &&
            BuffViewManager.Instance != null &&
            BuffSlotPool.Instance != null &&
            GameStateManager.Instance != null &&
            StatusUI.Instance != null &&
            StageTypeRandomizer.Instance != null &&
            TimerManager.Instance != null &&
            KillCounter.Instance != null &&
            DamageCounter.Instance != null &&
            ResultView.Instance != null &&
            TextSystemView.Instance != null &&
            TextEventManager.Instance != null &&
            MoveFarChecker.Instance != null &&
            NaviSystem.Instance != null;
        await UniTask.WaitUntil(() => isExist);
        _playerMove = PlayerMove.Instance;
        _stagePool = StagePool.Instance;
        _enemyPool = EnemyPool.Instance;
        _enemyTypeRandomizer = EnemyTypeRandomizer.Instance;
        _enemyStatusManager = EnemyStatusManager.Instance;
        _playerShooting = PlayerShooting.Instance;
        _playerStatusManager = PlayerStatusManager.Instance;
        _buffManager = BuffManager.Instance;
        _buffViewManager = BuffViewManager.Instance;
        _gameStateManager = GameStateManager.Instance;
        _naviSystem = NaviSystem.Instance;
        _stageTypeRandomizer = StageTypeRandomizer.Instance;
        _timerManager = TimerManager.Instance;
        _killCounter = KillCounter.Instance;
        _damageCounter = DamageCounter.Instance;
        _resultView = ResultView.Instance;
        _textSystemView = TextSystemView.Instance;
        _textEventManager = TextEventManager.Instance;
        _moveFarChecker = MoveFarChecker.Instance;
        _statusUI = StatusUI.Instance;
        _cancellationTokenSource = new CancellationTokenSource();
        _playerStatusManager.Init();
        _playerStatusManager.SetTutorial(false);
        _playerShooting.Init();
        _buffManager.Init();
        _timerManager.SetStartTime(0);
        _killCounter.Init();
        _damageCounter.Init();
        _resultView.Init();
        _textSystemView.Hide();
        BuffSlotPool.Instance.Init();
        _playerShooting.SetCharge(true);
        await _stagePool.Init();
        await _enemyPool.Init();
        await _enemyStatusManager.InitDic();
        if (isTutorial == false)
        {
            Flow(_cancellationTokenSource.Token).Forget();
            GameOver().Forget();
            return;
        }
        TutorialFlow(_cancellationTokenSource.Token).Forget();
    }
    private async UniTask Flow(CancellationToken token)
    {
        try
        {
            List<EnemyType> enemyTypes = new List<EnemyType>();
            //敵カウントリセット
            _enemyCount = 0;

            //Debug.Log(string.Join(", ", enemyTypes));
            //
            await StageSetUp(enemyTypes, _stageTypeRandomizer.GetStageType());
            //フェードイン
            await FadeManager.FadeIn();
            _timerManager.StartTimer();
            while (_clearFloorCount < _maxFloorCount)
            {
                //エネミー行動開始
                await _currentStage.Flow();
                //入口ドア開場
                _currentStage.EntranceDoorController.SetAnim(true);
                _statusUI.ViewFloorCount(_clearFloorCount + 1).Forget();
                _naviSystem.AddText(TextEventType.NaviEnemyKill);
                _naviSystem.AddText(TextEventType.NaviEnemyCount, _enemyCount.ToString());
                _gameStateManager.SetGameState(GameStateType.Action);
                await UniTask.WaitUntil(() => _isEnter, cancellationToken: _cancellationTokenSource.Token);
                if (_cancellationTokenSource.IsCancellationRequested) break;
                //入口ドア閉める
                _currentStage.EntranceDoorController.SetAnim(false);
                //敵が全滅するまで待機
                await UniTask.WaitUntil(() => _enemyCount <= 0, cancellationToken: _cancellationTokenSource.Token);
                if (_cancellationTokenSource.IsCancellationRequested) break;
                if (_clearFloorCount >= _maxFloorCount - 1)
                {
                    _naviSystem.AddText(TextEventType.NaviGameEnd);
                }
                else
                {
                    _naviSystem.AddText(TextEventType.NaviEnemyDown);
                }
                //ドア開場
                _currentStage.ExitDoorController.SetAnim(true);
                //出口に入るまで待機
                await UniTask.WaitUntil(() => _isExit);

                _clearFloorCount++;
                if (_clearFloorCount >= _maxFloorCount)
                {
                    _cancellationTokenSource.Cancel();
                    // ゲームクリア処理
                    _timerManager.StopTimer();
                    await ViewResult(true);
                    break;
                }
                //フェードアウト
                await FadeManager.FadeOut();
                _naviSystem.HideNavi();
                //フロア解放
                _stagePool.ReleaseStage(_currentStage);
                //敵カウントリセット
                _enemyCount = 0;
                //フロアの属性生成
                await StageSetUp(enemyTypes, _stageTypeRandomizer.GetStageType());
                //フェードイン
                await FadeManager.FadeIn();
                //バフ選択
                _gameStateManager.SetGameState(GameStateType.BuffSelect);
                await BuffFlow();
                _playerShooting.ResetBullt();
                //行動開始

            }
            // ゲームクリア
        }
        catch (System.OperationCanceledException)
        {
            Debug.Log("ゲームフロー終了");
        }
    }
    private async UniTask StageSetUp(List<EnemyType> enemyTypes, StageType stageType)
    {
        // フロア生成
        _currentStage = _stagePool.GetStage(stageType);
        //敵の属性生成
        enemyTypes = _enemyTypeRandomizer.GetEnemyTypes(_clearFloorCount, _currentStage.RouteCount);
        //ステージ初期化
        await _currentStage.Init(enemyTypes, _clearFloorCount);
        _currentStage.EntranceDoorController.SetAnim(false);
        _currentStage.ExitDoorController.SetAnim(false);
        //初期位置へ移動
        _playerMove.SetMove(false);
        _playerMove.InitPos(_currentStage.PlayerInitData);
        //出口判定リセット
        _isExit = false;
        _isEnter = false;
    }
    private async UniTask BuffFlow()
    {
        _naviSystem.AddText(TextEventType.NaviBuffSelect);
        //選択完了待機
        var buffDatas = _buffManager.GetBuffTypes(count: _playerStatusManager.GetBuffSelectCount());
        await _buffViewManager.BuffViewAsync(buffDatas);
        await UniTask.WaitUntil(() => _slotObj != null);
        //バフ非表示
        var buffData = await _buffViewManager.BuffHideAsync(_slotObj);
        _naviSystem.AddText(TextEventType.NaviBuffSelected);
        await UniTask.WaitForSeconds(1);
        //バフ適応
        _buffManager.AddActiveBuff(buffData);
        _slotObj = null;
    }
    private async UniTask GameOver()
    {
        await UniTask.WaitUntil(() => _playerStatusManager.GetHp() <= 0, cancellationToken: _cancellationTokenSource.Token);
        if (_cancellationTokenSource.IsCancellationRequested) return;
        _cancellationTokenSource.Cancel();
        // ゲームオーバー処理
        _timerManager.StopTimer();
        await ViewResult(false);

    }
    private async UniTask ViewResult(bool isClear)
    {
        _gameStateManager.SetGameState(GameStateType.TextEvent);
        await _resultView.ViewResults(
            isClear,
            _clearFloorCount,
            (int)_timerManager.GetTime(),
            _damageCounter.GetDamage(),
            _killCounter.GetKillCount());
    }
    public async UniTask LoadToTitle()
    {
        SoundManager.Instance.PlaySE(SEType.TapBotton);
        await FadeManager.FadeOut();
        if (_currentStage != null && _stagePool != null)
        {
            _stagePool.ReleaseStage(_currentStage);
        }
        _playerMove.gameObject.SetActive(false);
        InputManager.Instance.gameObject.SetActive(false);
        Destroy(InputManager.Instance.gameObject);
        GameStateManager.Instance.SetGameState(GameStateType.None);
        await SceneLoadMananger.Instance.LoadSceneAsync(SceneType.Title);
    }
    private async UniTask TutorialFlow(CancellationToken token)
    {
        // チュートリアル処理
        #region 初期セットアップ
        List<EnemyType> enemyTypes = new List<EnemyType>();
        await StageSetUp(enemyTypes, StageType.NonePure);
        _gameStateManager.SetGameState(GameStateType.TextEvent);
        _naviSystem.HideNavi();
        _naviSystem.gameObject.SetActive(false);
        _playerShooting.SetCharge(false);
        _playerStatusManager.SetTutorial(true);
        await FadeManager.FadeIn();
        _isExit = false;

        #endregion

        #region シナリオ～ナビ起動まで
        //フェードイン
        await _textSystemView.ShowAsync();
        //シナリオ処理
        await _textEventManager.ViewText(TextEventType.TutorialScenario);
        _naviSystem.gameObject.SetActive(true);
        _naviSystem.AddText(TextEventType.NaviAccept);
        InputManager.Instance.MouseReset(GameStateType.Action);
        await UniTask.WaitUntil(() => _naviSystem.IsCall == false);
        //入口ドア開場
        _currentStage.EntranceDoorController.SetAnim(true);
        _moveFarChecker.Init(_playerMove.transform.position);
        _moveFarChecker.SetCheck(true);
        _playerMove.SetMove(true);
        _naviSystem.SetViewLength(3);
        #endregion

        #region 移動～フロア1達成まで
        //カメラ移動待機
        //移動待機
        while (!(_moveFarChecker.IsDisCompleted && _moveFarChecker.IsFarCompleted))
        {
            _naviSystem.AddText(TextEventType.NaviMove);
            await UniTask.Yield();
        }
        _naviSystem.Clear();
        //射撃待機
        _naviSystem.AddText(TextEventType.NaviShoot);
        await UniTask.WaitUntil(() => _naviSystem.IsCall == false);
        _statusUI.OnStateChange(GameStateType.Action);
        _playerShooting.OnStateChange(GameStateType.Action);
        await UniTask.WaitUntil(() => _playerShooting.CurrentBullet <= 0);
        _naviSystem.AddText(TextEventType.NaviBullet);
        _playerShooting.SetCharge(true);
        await UniTask.WaitUntil(() => _naviSystem.IsCall == false);
        //ドア開場
        _currentStage.ExitDoorController.SetAnim(true);
        _naviSystem.AddText(TextEventType.NaviNext);
        //出口に入るまで待機
        await UniTask.WaitUntil(() => _isExit);
        #endregion

        #region フロア2セットアップ
        await FadeManager.FadeOut();
        //フロア解放
        _stagePool.ReleaseStage(_currentStage);
        //敵カウントリセット
        _enemyCount = 0;
        // フロア生成
        _currentStage = _stagePool.GetStage(StageType.None);
        //敵の生成
        enemyTypes = new List<EnemyType>() { EnemyType.Robot };
        //ステージ初期化
        await _currentStage.Init(enemyTypes, 0);
        _currentStage.EntranceDoorController.SetAnim(false);
        _currentStage.ExitDoorController.SetAnim(false);
        //初期位置へ移動
        _playerMove.SetMove(false);
        _playerMove.InitPos(_currentStage.PlayerInitData);
        //出口判定リセット
        _isExit = false;
        _isEnter = false;

        _gameStateManager.SetGameState(GameStateType.BuffSelect);
        _slotObj = null;
        await FadeManager.FadeIn();
        #endregion

        #region バフ選択
        var buffDatas = _buffManager.GetBuffTypes(count: _playerStatusManager.GetBuffSelectCount());
        await _buffViewManager.BuffViewAsync(buffDatas, isTappable: false);
        _naviSystem.AddText(TextEventType.NaviBuff);
        await UniTask.WaitUntil(() => _naviSystem.IsCall == false);
        _naviSystem.AddText(TextEventType.NaviBuffSelect);
        _buffViewManager.SetTappable(true);
        //バフ選択待機
        await UniTask.WaitUntil(() => _slotObj != null);
        //バフ非表示
        var buffData = await _buffViewManager.BuffHideAsync(_slotObj);
        _naviSystem.AddText(TextEventType.NaviBuffSelected);
        await UniTask.WaitForSeconds(1);
        //バフ適応
        _buffManager.AddActiveBuff(buffData);
        _slotObj = null;
        #endregion

        #region 敵撃破まで
        InputManager.Instance.MouseReset(GameStateType.Action);
        //エネミー行動開始
        await _currentStage.Flow();
        //入口ドア開場
        _currentStage.EntranceDoorController.SetAnim(true);
        _naviSystem.AddText(TextEventType.NaviFight);
        _naviSystem.AddText(TextEventType.NaviEnemyKill);
        await UniTask.WaitUntil(() => _naviSystem.IsCall == false);
        _naviSystem.AddText(TextEventType.NaviEnemyCount, _enemyCount.ToString());
        _gameStateManager.SetGameState(GameStateType.Action);
        //敵撃破待機
        await UniTask.WaitUntil(() => _enemyCount <= 0);
        _gameStateManager.SetGameState(GameStateType.None);
        _naviSystem.AddText(TextEventType.NaviEnd);
        await UniTask.WaitUntil(() => _naviSystem.IsCall == false);
        await UniTask.WaitForSeconds(0.5f);
        #endregion
        //チュートリアル終了
        await LoadToTitle();
    }
    public void SetSlotObj(BuffSlotObj slotObj) => _slotObj = slotObj;
    public void SetExit() => _isExit = true;
    public void SetEnter() => _isEnter = true;
    public void AddEnemyCount() => _enemyCount++;
    public void RemoveEnemyCount()
    {
        _enemyCount--;
        if (_enemyCount <= 0) return;
        _naviSystem.AddText(TextEventType.NaviEnemyCount, _enemyCount.ToString());
    }
}
