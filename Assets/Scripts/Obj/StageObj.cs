using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class StageObj : MonoBehaviour, IPool
{
    [SerializeField] private NavMeshSurface _navMeshSurface;
    private NavMeshData _navMeshData;
    private NavMeshDataInstance _navMeshInstance;
    [SerializeField] private LayerMask _walkableLayer = -1;
    [SerializeField] private Vector3 _boundsSize = new Vector3(100, 50, 100);
    [SerializeField] private List<RouteData> _routeDatas = new List<RouteData>();
    public DoorController EntranceDoorController => _entranceDoorController;
    [SerializeField] private DoorController _entranceDoorController;
    public DoorController ExitDoorController => _exitDoorController;
    [SerializeField] private DoorController _exitDoorController;
    public PlayerInitData PlayerInitData => _playerInitData;
    [SerializeField] private PlayerInitData _playerInitData;
    public StageType StageType => _stageType;
    [SerializeField] private StageType _stageType;
    public bool IsGenericUse { get; set; }
    private List<EnemyObj> _enemyObjs = new List<EnemyObj>();
    private EnemyPool _enemyPool;
    private EnemyStatusManager _enemyStatusManager;
    public int RouteCount => _routeDatas.Count;
    public async UniTask BuildNavMeshRuntime()
    {
        //NavMeshSurface から設定を取得
        var sources = new List<NavMeshBuildSource>();
        NavMeshBuilder.CollectSources(
            new Bounds(Vector3.zero, _boundsSize),
            _walkableLayer,
            NavMeshCollectGeometry.RenderMeshes,
            0,
            new List<NavMeshBuildMarkup>(),
            sources
        );

        // NavMeshData を生成
        var buildSettings = NavMesh.GetSettingsByID(0);
        _navMeshData = NavMeshBuilder.BuildNavMeshData(
            buildSettings,
            sources,
            new Bounds(Vector3.zero, _boundsSize),
            Vector3.zero,
            Quaternion.identity
        );

        if (_navMeshData == null)
        {
            Debug.LogError("NavMeshDataの生成に失敗しました");
            return;
        }

        // NavMesh をシーンに追加
        _navMeshInstance = NavMesh.AddNavMeshData(_navMeshData);

        //フレーム待機
        await UniTask.Yield();
    }
    public async UniTask Init(List<EnemyType> enemyTypes,int index)
    {
        if (enemyTypes == null || enemyTypes.Count == 0)
        {
            Debug.LogWarning($"{nameof(enemyTypes)}が空です");
            return;
        }
        if (_enemyPool == null) _enemyPool = EnemyPool.Instance;
        if (_enemyStatusManager == null) _enemyStatusManager = EnemyStatusManager.Instance;
       await BuildNavMeshRuntime();

        for (int i = 0; i < _routeDatas.Count; i++)
        {
            EnemyType enemyType = enemyTypes[i % enemyTypes.Count];
            var enemyObj = _enemyPool.GetEnemy(enemyType);
            EnemyParam paramData = _enemyStatusManager.GetEnemyParam(enemyType, index);
            if (enemyObj == null)
            {
                Debug.LogWarning($"{nameof(EnemyObj)}の取得に失敗しました");
                continue;
            }
            if (paramData == null)
            {
                Debug.LogWarning($"{nameof(EnemyParam)}の取得に失敗しました -> {enemyType}");
                continue;
            }
            enemyObj.Init(paramData, _routeDatas[i].GetRoutePoints());
            _enemyObjs.Add(enemyObj);
        }
        await UniTask.CompletedTask;
    }
    public async UniTask Flow()
    {
        if (_enemyObjs.Count == 0)
        {
            return;
        }

        foreach (var enemyObj in _enemyObjs)
        {
            EnemyStatesData enemyStatesData = _enemyStatusManager.GetEnemyStatesData(enemyObj.EnemyType);
            if (enemyStatesData == null)
            {
                Debug.LogWarning($"{nameof(EnemyStatesData)}の取得に失敗しました -> {enemyObj.EnemyType}");
                await UniTask.Yield();
                continue;
            }
            enemyObj.FlowAsync(
               enemyStatesData.PatrolStateTypes,
               enemyStatesData.ChaseStateTypes).Forget();
            //Debug.Log(string.Join("->", enemyStatesData.PatrolStateTypes));
        }
        await UniTask.CompletedTask;
    }
    public void OnRelease()
    {
        gameObject.SetActive(false);

        if (_navMeshInstance.valid)
        {
            _navMeshInstance.Remove();
        }

        foreach (var enemy in _enemyObjs)
        {
            _enemyPool.ReleaseEnemy(enemy);
        }
        _enemyObjs.Clear();
    }

    public void OnReuse()
    {
        gameObject.SetActive(true);
    }
}
