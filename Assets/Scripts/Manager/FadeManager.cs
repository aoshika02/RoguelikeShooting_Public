using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FadeManager : SingletonMonoBehaviour<FadeManager>
{
    [SerializeField] private Image _fadeImage;
    private static Image _staticFadeImage;

    protected override void Awake()
    {
        if (!CheckInstance())
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        _staticFadeImage = _fadeImage;

        // 最初は黒にしておく
        _staticFadeImage.color = new Color(0, 0, 0, 1);
    }

    public static async UniTask FadeIn(float duration = 0.5f)
    {
        _staticFadeImage.DOFade(0, duration).SetUpdate(true);
        await UniTask.WaitForSeconds(duration);
    }

    public static async UniTask FadeOut(float duration = 0.5f)
    {
        _staticFadeImage.DOFade(1, duration).SetUpdate(true);
        await UniTask.WaitForSeconds(duration);
    }
}
