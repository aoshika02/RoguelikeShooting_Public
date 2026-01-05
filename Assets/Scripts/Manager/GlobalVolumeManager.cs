using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GlobalVolumeManager : SingletonMonoBehaviour<GlobalVolumeManager>
{
    [SerializeField] private VolumeData _defaultVolumeData;
    [SerializeField] private VolumeData _damageVolumeData;
    private Volume _volume;
    private Vignette _vignette;
    private Tween _volumeTween;

    protected override void Awake()
    {
        _volume = GetComponent<Volume>();
        if (_volume.profile.TryGet(out _vignette))
        {
            Debug.Log("Vignette found!");
        }
        else
        {
            Debug.LogWarning("Vignette not found in Volume Profile.");
        }
    }
    /// <summary>
    /// ダメージ演出
    /// </summary>
    /// <param name="duration"></param>
    /// <returns></returns>
    public async UniTask DamageAsync(float duration = 0.25f)
    {
        await VignetteTweenAsync(_damageVolumeData, Ease.InQuad, 0);
        await VignetteTweenAsync(_defaultVolumeData, Ease.InQuad, duration);
    }
    /// <summary>
    /// ビネットに値を適応
    /// </summary>
    /// <param name="volumeData"></param>
    /// <param name="ease"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    private async UniTask VignetteTweenAsync(VolumeData volumeData, Ease ease, float duration = 0.25f)
    {
        _volumeTween?.Kill();
        _volumeTween = DOTween.Sequence()
               .Join(GetColorTween(volumeData.VignetteColor, duration, ease))
               .Join(GetIntensityTween(volumeData.VignetteIntensity, duration, ease))
               .Join(GetSmoothnessTween(volumeData.VignetteSmoothness, duration, ease)
           ).SetLink(gameObject);

        await _volumeTween.Play();
    }

    #region Vignette
    #region Tween
    private Tween GetColorTween(Color color, float duration, Ease ease)
    {
        Color start = _vignette.color.value;

        return DOTween.To(
            () => start,
            x => SetColor(x),
            color,
            duration).SetEase(ease);

    }
    private Tween GetIntensityTween(float intensity, float duration, Ease ease)
    {
        float start = _vignette.intensity.value;

        return DOTween.To(
            () => start,
            x => SetIntensity(x),
            intensity,
            duration).SetEase(ease);
    }
    private Tween GetSmoothnessTween(float smoothness, float duration, Ease ease)
    {
        float start = _vignette.smoothness.value;

        return DOTween.To(
            () => start,
            x => SetSmoothness(x),
            smoothness,
            duration).SetEase(ease);
    }
    #endregion

    #region Setter
    private void SetColor(Color color)
    {
        if (_vignette == null) return;
        _vignette.color.Override(color);
    }
    private void SetIntensity(float intensity)
    {
        if (_vignette == null) return;
        _vignette.intensity.Override(intensity);
    }
    private void SetSmoothness(float smoothness)
    {
        if (_vignette == null) return;
        _vignette.smoothness.Override(smoothness);
    }
    #endregion

    #endregion
}
[Serializable]
public class VolumeData
{
    public Color VignetteColor;
    public float VignetteIntensity;
    public float VignetteSmoothness;
}
