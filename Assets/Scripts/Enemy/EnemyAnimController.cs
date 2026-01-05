using Cysharp.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyAnimController : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    public void SetAnim(string animParameter, bool value)
    {
        if (this == null) return;
        if (_animator == null) return;
        _animator.SetBool(animParameter, value);
    }
    public async UniTask<float> SetAnimAsync(string animParameter, bool value)
    {
        _animator.SetBool(animParameter, value);
        var clipName = _animator.GetCurrentClipName();
        var animLength = _animator.GetAnimationClipLength(clipName);
        await UniTask.WaitForSeconds(animLength);
        return animLength;
    }
}
