using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

public class SceneLoadMananger : SingletonMonoBehaviour<SceneLoadMananger>
{
    protected override void Awake()
    {
        if (CheckInstance() == false)
        {
            return;
        }
        DontDestroyOnLoad(gameObject);
    }
    public async UniTask LoadSceneAsync(SceneType sceneType)
    {
        await SceneManager.LoadSceneAsync((int)sceneType);
    }
}

public enum SceneType
{
    None = -1,
    Title = 0,
    Main = 1,
}
