using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;

public class StatusUI : SingletonMonoBehaviour<StatusUI>
{
    [SerializeField] private List<Image> _bulletImags = new List<Image>();
    [SerializeField] private List<Image> _bulletBGImags = new List<Image>();
    [SerializeField] private RectTransform _uiRoot;
    [SerializeField] private Image _hpBar;
    [SerializeField] private Image _hpBG;
    [SerializeField] private float _minHpPos;
    [SerializeField] private float _maxHpPos;
    [SerializeField] private CanvasGroup _statusUIGroup;
    [SerializeField] private TextMeshProUGUI _floorCountText;
    private KeyConverter _keyConverter;
    private void Start()
    {
        _keyConverter = KeyConverter.Instance;
        _floorCountText.text = null;
        GameStateManager.Instance.SetStateChangedAction(OnStateChange);
        PlayerShooting.Instance.BulletCharge.Subscribe(x =>
        {
            SetAllAmount(x);
        }).AddTo(this);
        PlayerShooting.Instance.BulltCount.Subscribe(x =>
        {
            SetAmount(x.Item1, x.Item2);
        }).AddTo(this);
        PlayerShooting.Instance.MaxBulltCount.Subscribe(x =>
        {
            SetBulletCount(x);
        }).AddTo(this);
        PlayerStatusManager.Instance.OnChangeHpRatio.Subscribe(x =>
        {
            ChangeHpUI(x);
        }).AddTo(this);
    }
    public void OnStateChange(GameStateType stateType)
    {
        if (stateType == GameStateType.Action)
        {
            _statusUIGroup.alpha = 1;
            return;
        }
        _statusUIGroup.alpha = 0;
    }

    #region Bullet
    public void SetBulletCount(int count)
    {
        for (int i = 0; i < _bulletImags.Count; i++)
        {
            if (count > i)
            {
                _bulletImags[i].AlphaChange(1f);
                if (_bulletBGImags.Count > i)
                {
                    _bulletBGImags[i].AlphaChange(1f);
                }
                continue;
            }
            _bulletImags[i].AlphaChange(0f);
            if (_bulletBGImags.Count > i)
            {
                _bulletBGImags[i].AlphaChange(0f);
            }
        }
    }
    public void SetAmount(float amount, int index)
    {
        if (_bulletImags.Count > index && index >= 0)
        {
            _bulletImags[index].fillAmount = amount;
        }
    }
    public void SetAllAmount(float amount)
    {
        _bulletImags.ForEach(x => x.fillAmount = amount);
    }
    #endregion

    #region Damage
    private void ChangeHpUI(float value)
    {
        var pos = Mathf.Lerp(_minHpPos, _maxHpPos, value);
        var currentPos = _hpBar.rectTransform.anchoredPosition;
        _hpBar.rectTransform.anchoredPosition = new Vector2(pos, currentPos.y);
        _hpBG.rectTransform.anchoredPosition = new Vector2(pos, currentPos.y);
    }
    public async UniTask DamageAsync(float value)
    {
        await ShakeAsync();
        await ChangeHpUIAsync(value);
    }
    private async UniTask ChangeHpUIAsync(float value, float duration = 0.75f)
    {
        var pos = Mathf.Lerp(_minHpPos, _maxHpPos, value);
        var currentPos = _hpBar.rectTransform.anchoredPosition;
        _hpBar.rectTransform.anchoredPosition = new Vector2(pos, currentPos.y);
        await _hpBG.rectTransform.DOAnchorPosX(pos, duration).ToUniTask();
    }
    private async UniTask ShakeAsync(float duration = 0.25f, float strength = 20f, int vibrato = 30, float randomness = 90f)
    {
        await _uiRoot.DOShakeAnchorPos(duration, strength, vibrato, randomness, false, true)
               .SetEase(Ease.OutQuad).ToUniTask();
    }
    #endregion

    #region FloorView

    public async UniTask ViewFloorCount(int count) 
    {
        var findKey = _keyConverter.GetFindKey(TextEventType.AreaCount, 0);
        var texts = TextConverter.GetText(findKey);
        var repalce = texts.message.Replace("{Value}", count.ToString());
        _floorCountText.text = repalce;
        _floorCountText.SetAlpha(1);
        await UniTask.WaitForSeconds(2);
        await DOVirtual.Float(1, 0, 0.5f, f => 
        {
            _floorCountText.SetAlpha(f);
        }).ToUniTask();
    }
    #endregion

    private void OnDestroy()
    {
        GameStateManager.Instance.RemoveStateChangedAction(OnStateChange);
    }
}
public static class TextExtensions
{
    public static void SetAlpha(this TextMeshProUGUI self, float alpha)
    {
        var color = self.color;
        color.a = alpha;
        self.color = color;
    }
}