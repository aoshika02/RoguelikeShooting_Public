using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultView : SingletonMonoBehaviour<ResultView>
{
    [SerializeField] private GameObject _resultRoot;
    [SerializeField] private List<ResultSet> _resultSets = new List<ResultSet>();
    [SerializeField] private Image _hideImage;
    [SerializeField] private Material _glitchMaterial;
    [SerializeField] private float _scale = 0;
    [SerializeField] private TextMeshProUGUI _resultText;
    [SerializeField] private TextMeshProUGUI _toTitleText;
    [SerializeField] private CallLoadToTitle _loadToTitle;
    private int _resultSlotCount = 4;
    private KeyConverter _keyConverter;
    protected override void Awake()
    {
        if (CheckInstance() == false) return;
        _keyConverter = KeyConverter.Instance;
    }
    public void Init()
    {
        _resultRoot.gameObject.SetActive(false);
        _hideImage.gameObject.SetActive(false);
        _hideImage.material = null;
        _resultText.text = "";
        _toTitleText.text = "";
        _resultSets.ForEach(r =>
        {
            r.Rabel.text = "";
            r.Value.text = "";
        });
    }
    public async UniTask ViewResults(bool isClear, int floorCount, int time, int damageValue, int killCount)
    {
        if (_resultSets.Count != _resultSlotCount) return;
        _resultRoot.gameObject.SetActive(true);
        TextEventType eventType = isClear ? TextEventType.ResultClearRabel : TextEventType.ResultFailedRabel;
        var findKey = _keyConverter.GetFindKey(eventType, 0);
        var texts = TextConverter.GetText(findKey);
        _resultText.maxVisibleCharacters = 0;
        _resultText.text = texts.message;
        for (int i = 0; i <= texts.message.Length; i++)
        {
            _resultText.maxVisibleCharacters = i;
            await UniTask.WaitForSeconds(0.05f);
        }
        await UniTask.WaitForSeconds(0.5f);
        await ViewResultAsync(_resultSets[0], TextEventType.ResultFloorRabel, TextEventType.ResultFloorValue, floorCount.ToString());
        string timeText = time / 60 + " : " + time % 60;
        await ViewResultAsync(_resultSets[1], TextEventType.ResultTimeRabel, TextEventType.ResultTimeValue, timeText);
        await ViewResultAsync(_resultSets[2], TextEventType.ResultDamageRabel, TextEventType.ResultDamageValue, damageValue.ToString());
        await ViewResultAsync(_resultSets[3], TextEventType.ResultEnemyRabel, TextEventType.ResultEnemyValue, killCount.ToString());
        _loadToTitle.SetIsLoad(false);
        findKey = _keyConverter.GetFindKey(TextEventType.TapToTitle, 0);
        texts = TextConverter.GetText(findKey);
        _toTitleText.text = texts.message;
    }
    private async UniTask ViewResultAsync(ResultSet resultSet, TextEventType rabelType, TextEventType valueType, string value)
    {
        _hideImage.material = null;
        _hideImage.rectTransform.anchoredPosition = new Vector2(_hideImage.rectTransform.anchoredPosition.x, resultSet.PosY);
        _hideImage.rectTransform.localScale = new Vector2(0, _hideImage.rectTransform.localScale.y);
        _hideImage.gameObject.SetActive(true);
        SoundManager.Instance.PlaySE(SEType.Result);
        await _hideImage.rectTransform.DOScaleX(_scale, 0.25f).ToUniTask();
        SetText(resultSet, rabelType, valueType, value);
        _hideImage.material = _glitchMaterial;
        var color = _glitchMaterial.GetColor(ParamConsts.COLOR);
        await DOVirtual.Float(1, 0, 0.5f, f =>
        {
            _glitchMaterial.SetColor(ParamConsts.COLOR, new Color(color.r, color.g, color.b, f));
        }).ToUniTask();
        _hideImage.gameObject.SetActive(false);
        _glitchMaterial.SetColor(ParamConsts.COLOR, new Color(color.r, color.g, color.b, 1));
        await UniTask.WaitForSeconds(0.5f);
    }
    private void SetText(ResultSet resultSet, TextEventType rabelType, TextEventType valueType, string value)
    {
        var findKey = _keyConverter.GetFindKey(rabelType, 0);
        var texts = TextConverter.GetText(findKey);
        resultSet.Rabel.text = texts.message;

        findKey = _keyConverter.GetFindKey(valueType, 0);
        texts = TextConverter.GetText(findKey);
        var repalce = texts.message.Replace("{Value}", value);
        resultSet.Value.text = repalce;
    }
}
[Serializable]
public class ResultSet
{
    public float PosY;
    public TextMeshProUGUI Rabel;
    public TextMeshProUGUI Value;
}
