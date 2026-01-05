using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;

public class CallLoadToTitle : MonoBehaviour
{
    private bool _loading = false;
    void Start()
    {
        _loading = true;
        InputManager.Instance.OnCanceled.Subscribe(x =>
        {
            if (x != gameObject) return;
            if (_loading) return;
            _loading = true;
            GameManager.Instance.LoadToTitle().Forget();
        }).AddTo(this);
    }
    public void SetIsLoad(bool isLoad) => _loading = isLoad;

}
