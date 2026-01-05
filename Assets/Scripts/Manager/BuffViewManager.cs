using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BuffViewManager : SingletonMonoBehaviour<BuffViewManager>
{
    private BuffSlotPool _pool;
    private KeyConverter _keyConverter;
    private List<BuffSlotObj> _slots = new List<BuffSlotObj>();
    [SerializeField] private List<SlotPosData> _slotPosDatas = new List<SlotPosData>();
    [SerializeField] private List<RankToColor> _rankToColors = new List<RankToColor>();
    [SerializeField] private float _startPos;
    [SerializeField] private float _viewPos;
    [SerializeField] private float _hidePos;
    [SerializeField] private BuffTexDatas _buffTexDatas;
    protected override void Awake()
    {
        if (CheckInstance() == false) return;
        _pool = BuffSlotPool.Instance;
        _keyConverter = KeyConverter.Instance;
        gameObject.SetActive(false);
    }
    public async UniTask BuffViewAsync(List<BuffData> buffDatas, float duration = 0.5f, bool isTappable = true)
    {
        List<UniTask> tasks = new List<UniTask>();
        if (_slots.Count > 0)
        {
            _slots.ForEach(x => _pool.ReleaseBuffSlotObj(x));
            _slots.Clear();
        }
        //初期位置(個数によって変化)
        var posData = _slotPosDatas.FirstOrDefault(x => x.SlotCount == buffDatas.Count);
        for (int i = 0; i < buffDatas.Count; i++)
        {
            //スロット取得
            var slotObj = _pool.GetBuffSlotObj();
            var rankToClor = _rankToColors.FirstOrDefault(x => x.RankType == buffDatas[i].RankType);
            //バフ属性からテキスト吸い上げ
            var findKey = _keyConverter.GetFindKey(buffDatas[i].TextEventType, 0);
            var texts = TextConverter.GetText(findKey);
            var repalce = texts.message.Replace("{Value}", $"<color=#C4000F>" + buffDatas[i].Value + "</color>");
            //バフ属性からアイコン吸い上げ
            var buffToTex = _buffTexDatas.BuffToTexes.FirstOrDefault(x => x.BuffType == buffDatas[i].BuffType);
            var buffSprite = buffToTex?.BuffSprite;
            slotObj.Init(rankToClor.Color, buffSprite, repalce, buffDatas[i]);
            if (slotObj.TryGetComponent<RectTransform>(out var rectTransform))
            {
                rectTransform.anchoredPosition = new Vector2(posData.PosXies[i], _startPos);
                tasks.Add(rectTransform.DOAnchorPosY(_viewPos, duration).ToUniTask());
            }
            _slots.Add(slotObj);
        }
        gameObject.SetActive(true);
        //上から下へアニメーション
        await UniTask.WhenAll(tasks);
        SetTappable(isTappable);
    }
    public void SetTappable(bool isTappable = true) 
    {
        if (_slots == null || _slots.Count == 0) return;
        _slots.ForEach(x => x.SetTappable(isTappable));
    }
    public async UniTask<BuffData> BuffHideAsync(BuffSlotObj buffSlotObj, float duration = 0.5f)
    {
        if (_slots.Count <= 0) return null;
        List<UniTask> tasks = new List<UniTask>();
        foreach (var slotObj in _slots)
        {
            if (slotObj == buffSlotObj) continue;
            if (slotObj.TryGetComponent<RectTransform>(out var rectTransform))
            {
                tasks.Add(rectTransform.DOAnchorPosY(_hidePos, duration / 2).ToUniTask());
            }
        }
        await UniTask.WhenAll(tasks);
        await UniTask.WaitForSeconds(0.25f);
        await buffSlotObj.SelectEffect();
        await UniTask.WaitForSeconds(0.25f);
        await buffSlotObj.FadeAsync(0);
        gameObject.SetActive(false);
        return buffSlotObj.GetBuffData();
    }
}
[Serializable]
public class SlotPosData
{
    public int SlotCount;
    public List<float> PosXies;
}
[Serializable]
public class RankToColor
{
    public RankType RankType;
    public Color Color;
}
public static class ImageExtentions
{
    public static void AlphaChange(this Image self, float alpha)
    {
        Color color = self.color;
        color.a = alpha;
        self.color = color;
    }
}