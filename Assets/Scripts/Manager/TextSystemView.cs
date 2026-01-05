using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
public class TextSystemView : SingletonMonoBehaviour<TextSystemView>
{
    [SerializeField] private RawImage _rawImage;
    private Material _material;
    private int _colorID = Shader.PropertyToID(ParamConsts.COLOR);
    private void Start()
    {
        _material = new Material(_rawImage.material);
        _rawImage.material = _material;
        InputManager.Instance.OnCanceled.Subscribe(x => 
        {
            if (x != gameObject) return;
            TextEventManager.Instance.OnClickText();
        }).AddTo(this);
    }
    public void Show()
    {
        gameObject.SetActive(true);
    }
    public void Hide() 
    {
        gameObject.SetActive(false);
    }
    public async UniTask ShowAsync(float duration = 0.5f) 
    {
        await MaterialFade(1, 0, 0);
        Show();
        await MaterialFade(0, 1, duration);
    }
    public async UniTask HideAsync(float duration = 0.5f) 
    {
        await MaterialFade(1,0,duration);
        Hide();
    }
    private async UniTask MaterialFade(float from,float to,float duration= 0.5f) 
    {
        Color color = _material.GetColor(_colorID);
        await DOVirtual.Float(from, to, duration, f =>
        {
            _material.SetColor(_colorID, new Color(color.r, color.g, color.b, f));
        }).ToUniTask();
    }
}
