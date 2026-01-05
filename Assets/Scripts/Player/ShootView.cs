using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class ShootView : SingletonMonoBehaviour<ShootView>
{
    [SerializeField] private ParticleSystem _muzzle;
    [SerializeField] private Animator _animator;
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private Color _noBulletColor;
    [SerializeField] private Color _bulletColor;
    private Material _material;
    private int _emissionID = Shader.PropertyToID(ParamConsts.EMISSION_COLOR);
    protected override void Awake()
    {
        if (CheckInstance() == false) return;
        _material = new Material(_meshRenderer.material);
        _meshRenderer.material = _material;
    }
    public void Shoot()
    {
        _animator.SetTrigger(ParamConsts.SHOOT);
        _muzzle.Play();
    }
    public async UniTask ShootAsync()
    {
        _animator.SetTrigger(ParamConsts.SHOOT);
        await UniTask.Yield(PlayerLoopTiming.Update);
        //アニメーションの長さ
        var length = 0.1f;
        List<UniTask> tasks = new List<UniTask>();
        tasks.Add(UniTask.WaitForSeconds(length));
        tasks.Add(_muzzle.PlayAsync());
        await UniTask.WhenAll(tasks);
        _animator.ResetTrigger(ParamConsts.SHOOT);
    }
    public void SetRunAnim(bool isRun)
    {
        _animator.SetBool(ParamConsts.RUN, isRun);
    }
    public void SetGunColor(bool isBullet) 
    {
        if (isBullet == false) 
        {
            _material.SetColor(_emissionID,_noBulletColor);
            return;
        }
        _material.SetColor(_emissionID, _bulletColor);
    }
}
public static class ParticleSystemExtensions
{
    public static async UniTask PlayAsync(this ParticleSystem self)
    {
        self.Play();
        await UniTask.WaitUntil(() => !self.IsAlive(true));
    }
}