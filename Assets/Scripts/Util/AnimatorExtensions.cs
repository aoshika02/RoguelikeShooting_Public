using System.Linq;
using UnityEngine;

public static class AnimatorExtensions
{
    /// <summary>
    /// 現在再生中のアニメーションクリップ名を取得
    /// </summary>
    public static string GetCurrentClipName(this Animator self, int layerIndex = 0)
    {
        if (self == null || self.runtimeAnimatorController == null)
            return null;

        // 現在のレイヤーで再生されているアニメーションクリップ情報を取得
        AnimatorClipInfo[] clipInfos = self.GetCurrentAnimatorClipInfo(layerIndex);

        if (clipInfos.Length == 0)
            return null;

        return clipInfos[0].clip.name;
    }
    /// <summary>
    /// アニメーションの長さ取得
    /// </summary>
    /// <param name="animationClips"></param>
    /// <param name="clipName"></param>
    /// <returns></returns>
    public static float GetAnimationClipLength(this Animator self, string clipName)
    {
        if (self.runtimeAnimatorController == null)
        {
            Debug.LogWarning("AnimatorController が設定されていません。");
            return 0f;
        }

        AnimationClip clip = self.runtimeAnimatorController.animationClips
           .FirstOrDefault(c => c.name == clipName);

        if (clip == null)
        {
            Debug.LogWarning($"{nameof(AnimationClip)}が見つかりません。clipName: {clipName}");
            return 0f;
        }
        AnimatorStateInfo stateInfo = self.GetCurrentAnimatorStateInfo(0);

        float totalSpeed = self.speed * stateInfo.speed;

        if (Mathf.Approximately(totalSpeed, 0f))
        {
            Debug.LogWarning("Speed が 0 のため、再生時間を求められません。");
            return 0;
        }

        return clip.length / totalSpeed;
    }
}