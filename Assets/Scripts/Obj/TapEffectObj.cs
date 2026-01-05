using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TapEffectObj : MonoBehaviour, IPool
{
    Image _image;
    Material _material;
    private string _radius = "_CircleRadius";
    private string _thickness = "_Thickness";
    private int _radiusID;
    private int _thicknessID;
    public bool IsGenericUse { get; set; }
    public void Init(Vector2 pos)
    {
        if (this == null || gameObject == null) return;
        if (_image == null) _image = GetComponent<Image>();
        _image.rectTransform.anchoredPosition = pos;
        if (_material == null)
        {
            _material = new Material(_image.material);
            _image.material = _material;
            _radiusID = Shader.PropertyToID(_radius);
            _thicknessID = Shader.PropertyToID(_thickness);
        }
        _material.SetFloat(_radiusID, 0);
        _material.SetFloat(_thicknessID, 0.01f);
    }
    public async UniTask CallEffect(float duration = 0.5f)
    {

        try
        {
            await DOVirtual.Float(0, 0.5f, duration, f =>
            {
                if (this == null || _material == null) return;
                _material.SetFloat(_radiusID, f);
            }).ToUniTask();

            await DOVirtual.Float(0.01f, 0f, duration / 5, f =>
            {
                if (this == null || _material == null) return;
                _material.SetFloat(_thicknessID, f);
            }).ToUniTask();
        }
        catch { /* do nothing */ }

        // null だとプールに戻さない
        if (this != null)
            TapEffectPool.Instance.ReleaseTapEffectObj(this);
    }
    public void OnRelease()
    {

    }

    public void OnReuse()
    {

    }
}
