using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NaviSystem : SingletonMonoBehaviour<NaviSystem>
{
    //アニメーター
    [SerializeField] private Animator _animator;
    [SerializeField] private float _textViewLength = 1;
    [SerializeField] private TextMeshProUGUI _naviText;
    [SerializeField] private RectTransform _naviRoot;
    [SerializeField] private float _viewPos;
    [SerializeField] private float _hidePos;
    private Queue<string> _messages = new Queue<string>();
    public bool IsCall => _isCall;
    private bool _isCall = false;
    private KeyConverter _keyConverter;
    private SoundManager _soundManager;
    //テキストセット
    protected override void Awake()
    {
        if (CheckInstance() == false) return;
        _keyConverter = KeyConverter.Instance;
        _soundManager = SoundManager.Instance;
        _naviRoot.anchoredPosition = new Vector2(_hidePos, _naviRoot.anchoredPosition.y);
    }
    public void AddText(TextEventType eventType, string replace)
    {
        for (int i = 0; i < _keyConverter.TextEventCount(eventType); i++)
        {
            var findKey = _keyConverter.GetFindKey(eventType, i);
            var texts = TextConverter.GetText(findKey);
            var repalce = texts.message.Replace("{Value}", replace);
            AddText(repalce);
        }
    }
    public void AddText(TextEventType eventType)
    {
        for (int i = 0; i < _keyConverter.TextEventCount(eventType); i++)
        {
            var findKey = _keyConverter.GetFindKey(eventType, i);
            var texts = TextConverter.GetText(findKey);
            AddText(texts.message);
        }
    }
    public void AddText(string text)
    {
        _messages.Enqueue(text);
        SetText().Forget();
    }
    private async UniTask SetText()
    {
        if (_isCall) return;
        _isCall = true;
        _animator.SetBool(ParamConsts.TALK, true);
        _naviRoot.anchoredPosition = new Vector2(_viewPos, _naviRoot.anchoredPosition.y);

        while (_messages.Count > 0)
        {
            _naviText.maxVisibleCharacters = 0;
            _naviText.text = _messages.Dequeue();
            _soundManager.PlaySE(SEType.Navi);
            for (int i = 0; i <= _naviText.text.Length; i++)
            {
                _naviText.maxVisibleCharacters = i;
                await UniTask.WaitForSeconds(0.05f);
            }
            await UniTask.WaitForSeconds(_textViewLength);
        }
        _isCall = false;
    }
    public void SetViewLength(int length)
    {
        _textViewLength = length;
    }
    public void Clear()
    {
        _messages.Clear();
    }
    public void HideNavi()
    {
        _animator.SetBool(ParamConsts.TALK, false);
        _isCall = false;
        _messages.Clear();
        _naviText.text = "";
        _naviRoot.anchoredPosition = new Vector2(_hidePos, _naviRoot.anchoredPosition.y);
    }
}
