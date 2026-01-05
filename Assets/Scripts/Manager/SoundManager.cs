using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
public class SoundManager : SingletonMonoBehaviour<SoundManager>
{
    private List<SoundData> _soundDatas = new List<SoundData>();
    [SerializeField] private SEClips _seClips;
    [SerializeField] private BGMClips _bgmClips;
    protected override void Awake()
    {
        if (CheckInstance() == false)
        {
            return;
        }
        DontDestroyOnLoad(gameObject);
        //データをあらかじめ5個ぐらい生成
        for (int i = 0; i < 5; i++)
        {
            CreateSoundData(_soundDatas);
        }
    }
    [Serializable]
    public class SoundData
    {
        public GameObject GameObject;
        public AudioSource AudioSource;
        public SoundKey SoundKey;
        public bool IsBGM;
    }

    #region Play
    /// <summary>
    /// SE再生
    /// </summary>
    /// <param name="seType">再生するSE</param>
    /// <returns>停止用キー</returns>
    public SoundKey PlaySE(SEType seType)
    {
        AudioClip audioClip = GetSEClip(seType)?.Clip;
        SoundData soundData = GetSoundData(_soundDatas);
        soundData.AudioSource.spatialBlend = 0;
        return Play(soundData, audioClip, false, false);
    }
    public SoundKey PlaySE(SEType seType, Vector3 pos)
    {
        AudioClip audioClip = GetSEClip(seType)?.Clip;
        SoundData soundData = GetSoundData(_soundDatas);
        soundData.AudioSource.spatialBlend = 1;
        soundData.GameObject.transform.position = pos;
        return Play(soundData, audioClip, false, false);
    }
    /// <summary>
    /// BGM再生
    /// </summary>
    /// <param name="bgmType">再生するBGM</param>
    /// <param name="isLoop">ループフラグ</param>
    /// <returns>停止用キー</returns>
    public SoundKey PlayBGM(BGMType bgmType)
    {
        AudioClip audioClip = GetBGMClip(bgmType)?.Clip;
        SoundData soundData = GetSoundData(_soundDatas);
        return Play(soundData, audioClip, true, true);
    }
    private SoundKey Play(SoundData soundData, AudioClip audioClip, bool isLoop, bool isBGM)
    {
        soundData.SoundKey = new SoundKey();

        AudioSource audioSource = soundData.AudioSource;
        audioSource.clip = audioClip;
        audioSource.loop = isLoop;
        audioSource.volume = 1;
        audioSource.Play();
        soundData.IsBGM = isBGM;
        if (isLoop == false)
        {
            ClipRemover(soundData).Forget();
        }
        return soundData.SoundKey;
    }
    #endregion

    #region Stop

    #region StopSE
    /// <summary>
    /// SE停止
    /// </summary>
    /// <param name="soundKey">停止用キー</param>
    /// <param name="duration">停止時間</param>
    /// <returns></returns>
    public async UniTask StopSE(SoundKey soundKey, float duration = 0.25f)
    {
        foreach (var sd in _soundDatas)
        {
            if (sd.IsBGM == true) continue;
            if (sd.SoundKey == soundKey && sd.AudioSource.isPlaying == true)
            {
                await StopAsync(sd, duration);
                break;
            }
        }
    }
    /// <summary>
    /// SE全停止
    /// </summary>
    /// <param name="duration">停止時間</param>
    /// <returns></returns>
    public async UniTask AllStopSE(float duration = 0.25f)
    {
        List<UniTask> tasks = new List<UniTask>();
        foreach (var sd in _soundDatas)
        {
            if (sd.IsBGM == true) continue;
            tasks.Add(StopAsync(sd, duration));
        }
        await UniTask.WhenAll(tasks);
    }

    #endregion

    #region StopBGM
    /// <summary>
    /// BGM停止
    /// </summary>
    /// <param name="soundKey">停止用キー</param>
    /// <param name="duration">停止時間</param>
    /// <returns></returns>
    public async UniTask StopBGM(SoundKey soundKey, float duration = 0.25f)
    {
        foreach (var sd in _soundDatas)
        {
            if (sd.IsBGM == false) continue;
            if (sd.SoundKey == soundKey && sd.AudioSource.isPlaying == true)
            {
                await StopAsync(sd, duration);
                break;
            }
        }
    }
    /// <summary>
    /// BGM全停止
    /// </summary>
    /// <param name="duration">停止時間</param>
    /// <returns></returns>
    public async UniTask AllStopBGM(float duration = 0.25f)
    {
        List<UniTask> tasks = new List<UniTask>();
        foreach (var sd in _soundDatas)
        {
            if (sd.IsBGM == false) continue;
            tasks.Add(StopAsync(sd, duration));
        }
        await UniTask.WhenAll(tasks);
    }
    private async UniTask StopAsync(SoundData soundData, float duration)
    {
        await DOVirtual.Float(1, 0, duration, value =>
        {
            soundData.AudioSource.volume = value;
        }).SetLink(soundData.GameObject).ToUniTask();
        Stop(soundData.AudioSource);
    }
    private void Stop(AudioSource audioSource)
    {
        audioSource.volume = 0;
        audioSource.clip = null;
        audioSource.loop = false;
        audioSource.Stop();
    }
    #endregion

    #endregion

    #region Create
    /// <summary>
    /// AudioSource生成
    /// </summary>
    /// <returns></returns>
    private SoundData CreateSoundData(List<SoundData> soundDatas)
    {
        GameObject obj = new GameObject("AudioSource");
        AudioSource source = obj.AddComponent<AudioSource>();

        float maxDist = 20f; 

        AnimationCurve curve = new AnimationCurve(
            new Keyframe(0f, 1f),         
            new Keyframe(maxDist, 0f)     
        );

        source.rolloffMode = AudioRolloffMode.Custom;
        source.maxDistance = maxDist;
        source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, curve);

        source.loop = false;
        source.playOnAwake = false;
        obj.transform.SetParent(transform);
        SoundData soundData = new SoundData
        {
            GameObject = obj,
            AudioSource = source,
            SoundKey = new SoundKey(),
        };
        soundDatas.Add(soundData);
        return soundData;
    }
    #endregion

    #region Get
    /// <summary>
    /// 未使用AudioSource取得
    /// </summary>
    /// <returns></returns>
    private SoundData GetSoundData(List<SoundData> soundDatas)
    {
        foreach (var soundData in soundDatas)
        {
            if (soundData.AudioSource.isPlaying == false) return soundData;
            if (soundData.AudioSource.clip == null) return soundData;
        }
        return CreateSoundData(soundDatas);
    }
    /// <summary>
    /// SE取得
    /// </summary>
    /// <param name="seType">取得したいSEのキー</param>
    /// <returns></returns>
    private SEClip GetSEClip(SEType seType)
    {
        SEClip seClip = null;
        foreach (var sc in _seClips.SEClipList)
        {
            if (sc.SEType != seType) continue;
            seClip = sc;
            break;
        }
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (seClip == null)
        {
            Debug.LogError($"SEClipsに{seType}が登録されていません");
        }
#endif
        return seClip;
    }
    /// <summary>
    /// BGM取得
    /// </summary>
    /// <param name="bgmType">取得したいBGMのキー</param>
    /// <returns></returns>
    private BGMClip GetBGMClip(BGMType bgmType)
    {
        BGMClip bgmClip = null;
        foreach (var bc in _bgmClips.BGMClipList)
        {
            if (bc.BGMType != bgmType) continue;
            bgmClip = bc;
            break;
        }
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (bgmClip == null)
        {
            Debug.LogError($"BGMClipsに{bgmType}が登録されていません");
        }
#endif
        return bgmClip;
    }
    #endregion

    #region Remove
    /// <summary>
    /// 初期化とRemove
    /// </summary>
    /// <param name="soundData">対象のAudioSourceを含むSoundData</param>
    /// <returns></returns>
    private async UniTask ClipRemover(SoundData soundData)
    {
        await UniTask.WaitWhile(() => soundData.AudioSource.isPlaying);
        Stop(soundData.AudioSource);
    }
    #endregion

    private void OnDestroy()
    {
        //_cancelToken.Cancel();
    }
}
