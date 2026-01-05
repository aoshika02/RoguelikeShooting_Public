using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;

public class CallSceneLoad : MonoBehaviour
{
    private InputManager _inputManager;
    private SceneLoadMananger _sceneLoadMananger;
    [SerializeField] private bool _isTutorial = false;
    void Start()
    {
        _inputManager = InputManager.Instance;
        _sceneLoadMananger = SceneLoadMananger.Instance;
        _inputManager.OnCanceled.Subscribe(x =>
        {
            if (x != gameObject) return;
            TitleManager.Instance.Load(_isTutorial);
        }).AddTo(this);
    }

}
