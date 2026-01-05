using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class BuffSlotObj : MonoBehaviour, IPool
{
    public bool IsGenericUse { get; set; }
    [SerializeField] private List<Image> _frames;
    [SerializeField] private Image _buffIcon;
    [SerializeField] private Image _selectEffect;
    [SerializeField] private TextMeshProUGUI _buffText;
    [SerializeField] private CanvasGroup _canvasGroup;
    private bool _tappable = false;
    private BuffData _buffData;
    private SoundManager _soundManager;
    private void Awake()
    {
        _soundManager = SoundManager.Instance;
        InputManager.Instance.OnCanceled.Subscribe(x =>
        {
            if (GameStateManager.Instance.StateType != GameStateType.BuffSelect) return;
            if (_tappable == false) return;
            if (x != gameObject) return;
            GameManager.Instance.SetSlotObj(this);
        }).AddTo(this);
    }
    public void Init(Color frameColor, Sprite buffSprite, string buffText, BuffData buffData)
    {
        _tappable = false;
        _frames.ForEach(x => x.color = frameColor);
        _buffIcon.sprite = buffSprite;
        _buffText.text = buffText;
        _buffData = buffData;
        _selectEffect.rectTransform.localScale = Vector3.one;
        _selectEffect.AlphaChange(1);
        _canvasGroup.alpha = 1;
    }
    public BuffData GetBuffData() => _buffData;
    public async UniTask SelectEffect(float duration = 0.5f)
    {
        _soundManager.PlaySE(SEType.Select);
        List<UniTask> tasks = new List<UniTask>();
        tasks.Add(_selectEffect.rectTransform.DOScale(1.2f, duration).ToUniTask());
        tasks.Add(_selectEffect.DOFade(0, duration).ToUniTask());
        await UniTask.WhenAll(tasks);
    }
    public void OnRelease()
    {
        gameObject.SetActive(false);
        _selectEffect.rectTransform.localScale = Vector3.one;
        _selectEffect.SetAlpha(1f);
    }
    public void OnReuse()
    {
        gameObject.SetActive(true);
    }
    public async UniTask FadeAsync(float endValue,float duration = 0.25f)
    {
        await _canvasGroup.DOFade(endValue, duration).ToUniTask();
    }
    public void SetTappable(bool tappable = true)
    {
        _tappable = tappable;
    }
}
public static class ImageExtensions
{
    public static void SetAlpha(this Image self, float alpha)
    {
        var color = self.color;
        color.a = alpha;
        self.color = color;
    }
    public static void SetAlpha(this RawImage self, float alpha)
    {
        var color = self.color;
        color.a = alpha;
        self.color = color;
    }
}