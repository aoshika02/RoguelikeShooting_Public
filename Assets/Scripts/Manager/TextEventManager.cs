using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class TextEventManager : SingletonMonoBehaviour<TextEventManager>
{
    [SerializeField] private TextEventDatas _textEventDatas;
    //通常のテキスト
    [SerializeField] private TextMeshProUGUI _textEventText;
    //名前テキスト
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private CanvasGroup _nameGroup;

    [SerializeField] private CanvasGroup _skipInfoCanvas;

    //テキストの表示速度
    private float _textSpeed = 0.05f;

    private bool _waitInput = false;

    private TextSystemView _textSystemView;
    protected override void Awake()
    {
        if (!CheckInstance()) return;
        _textEventText.text = "";
        _textEventText.maxVisibleCharacters = 0;
        _nameText.text = "";
        _nameGroup.alpha = 0;
        _skipInfoCanvas.alpha = 0;
        _textSystemView = TextSystemView.Instance;
    }
    public async UniTask ViewText(TextEventType textEventType, float duration = 0.25f) 
    {
        var texteventData = _textEventDatas.TextDatas.FirstOrDefault(x => x.TextEventType == textEventType);
        Clear();
        await _nameGroup.DOFade(1, duration).ToUniTask();
        for (int i = 0; i < texteventData.TextEventDataBases.Count; i++)
        {
            var textBaseData = texteventData.TextEventDataBases[i];
            var texts = TextConverter.GetText(textBaseData.FindKey);
            await ViewText(texts.message,texts.name, textBaseData.Duration);
        }
        List<UniTask> tasks = new List<UniTask>();
        await _nameGroup.DOFade(0, duration).ToUniTask();
        _nameGroup.alpha = 0;
        Clear();
        await _textSystemView.HideAsync();
    }
    private void Clear() 
    {
        _textEventText.text = "";
        _textEventText.maxVisibleCharacters = 0;
        _nameText.text = "";
    }
    private async UniTask ViewText(string text,string name,float duration = 0.25f)
    {
        await SetNameText(name, duration);
        await SetText(text);
        await WaitInput();
    }
    private async UniTask SetNameText(string name, float duration = 0.25f)
    {
        if (_nameText.text == name) return;

        if (string.IsNullOrEmpty(name))
        {
            await _nameGroup.DOFade(0, duration).ToUniTask();
            _nameText.text = "";
        }
        else
        {
            await _nameGroup.DOFade(0, duration * 0.5f).ToUniTask();
            _nameText.text = name;
            await _nameGroup.DOFade(1, duration * 0.5f).ToUniTask();
        }
    }
    private async UniTask SetText(string text)
    {
        _textEventText.maxVisibleCharacters = 0;
        _textEventText.text = text;
        SoundManager.Instance.PlaySE(SEType.TextEvent);

        for (int i = 0; i <= text.Length; i++)
        {
            _textEventText.maxVisibleCharacters = i;
            await UniTask.WaitForSeconds(_textSpeed);
        }
    }
    private async UniTask WaitInput()
    {
        _skipInfoCanvas.alpha = 1;
        _waitInput = false;
        await UniTask.WaitUntil(() => _waitInput);
        _skipInfoCanvas.alpha = 0;
    }
    public void OnClickText()
    {
        _waitInput = true;
    }
}
