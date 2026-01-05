using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class PlayerStatusManager : SingletonMonoBehaviour<PlayerStatusManager>
{
    private int _atk;
    private int _addAtkValue;
    private float _atkPersent;
    private float _dashSpeed;
    private float _maxDashSpeed;
    private float _hp;
    private float _maxHp;
    private float _reloadSpeed;
    private float _maxReloadSpeed;
    private int _bulletCount;
    private int _maxBulletCount;
    private bool _isScopeMode;
    private int _buffSelectCount;
    private int _maxBuffSelectCount;

    private bool _isTutorial = false;
    private bool _callDamageFunc = false;
    private GlobalVolumeManager _globalVolumeManager;
    private StatusUI _statusUI;
    public IObservable<float> OnChangeHpRatio => _onHpRatio;
    private Subject<float> _onHpRatio = new Subject<float>();
    protected override void Awake()
    {
        if (CheckInstance() == false) return;
        _globalVolumeManager = GlobalVolumeManager.Instance;
        _statusUI = StatusUI.Instance;
    }
    public void Init()
    {
        _atk = 50;
        _addAtkValue = 0;
        _atkPersent = 0;
        _dashSpeed = 5;
        _maxDashSpeed = 8.5f;
        _maxHp = 20;
        _hp = _maxHp;
        _reloadSpeed = 2;
        _maxReloadSpeed = 7;
        _bulletCount = 3;
        _maxBulletCount = 7;
        _isScopeMode = false;
        _buffSelectCount = 2;
        _maxBuffSelectCount = 3;
        _callDamageFunc = false;
    }
    #region GetParam
    public int GetAtk()
    {
        return Mathf.FloorToInt((_atk + _addAtkValue) * (1 + _atkPersent / 100f));
    }
    public float GetDashSpeed()
    {
        return _dashSpeed;
    }
    public float GetReloadSpeed()
    {
        return _reloadSpeed;
    }
    public float GetHp()
    {
        return _hp;
    }
    public float GetMaxHp()
    {
        return _maxHp;
    }
    public int GetBulletCount()
    {
        return _bulletCount;
    }
    public bool GetIsScopeMode()
    {
        return _isScopeMode;
    }
    public int GetBuffSelectCount()
    {
        return _buffSelectCount;
    }
    #endregion  
    public async UniTask TakeDamageAsync(float damageValue)
    {
        if (_callDamageFunc) return;
        _callDamageFunc = true;
        _hp -= damageValue;
        if (_hp < 0)
        {
            if (_isTutorial == false)
            {
                _hp = 0;
            }
            else
            {
                _hp = 1;
            }
        }
        SoundManager.Instance.PlaySE(SEType.Damage);
        List<UniTask> tasks = new List<UniTask>();
        tasks.Add(_globalVolumeManager.DamageAsync());
        tasks.Add(_statusUI.DamageAsync((float)_hp / _maxHp));
        await UniTask.WhenAll(tasks);
        if (_hp <= 0)
        {
            //死亡処理
            Debug.Log("やられた!");
            return;
        }
        _callDamageFunc = false;
    }
    private void OnHpChange()
    {
        _onHpRatio.OnNext((float)_hp / _maxHp);
    }
    public void  SetTutorial(bool tutorial)
    {
        _isTutorial = tutorial;
    }
    public bool AddBuff(BuffData buffData)
    {
        switch (buffData.BuffType)
        {
            case BuffType.AttackUp:
                _addAtkValue += (int)buffData.Value;
                break;

            case BuffType.SpeedUp:
                _dashSpeed += buffData.Value;
                if (_dashSpeed >= _maxDashSpeed)
                {
                    _dashSpeed = _maxDashSpeed;
                    return true;
                }
                break;

            case BuffType.ReloadUp:
                _reloadSpeed += buffData.Value;
                if (_reloadSpeed >= _maxReloadSpeed)
                {
                    _reloadSpeed = _maxReloadSpeed;
                    return true;
                }
                break;

            case BuffType.Heal:
                _hp += (int)buffData.Value;
                if (_hp >= _maxHp)
                {
                    _hp = _maxHp;
                }
                OnHpChange();
                break;

            case BuffType.ExpUp:
                break;

            case BuffType.HpUp:
                _maxHp += (int)buffData.Value;
                _hp += (int)buffData.Value;
                OnHpChange();
                break;

            case BuffType.BuffSlot:
                break;

            case BuffType.ScopeMode:
                _isScopeMode = true;
                return true;

            case BuffType.BulletUp:
                _bulletCount += (int)buffData.Value;
                if (_bulletCount >= _maxBulletCount)
                {
                    _bulletCount = _maxBulletCount;
                    return true;
                }
                break;

            case BuffType.SelecteBuffUp:
                _buffSelectCount++;
                if (_buffSelectCount >= _maxBuffSelectCount)
                {
                    _buffSelectCount = _maxBuffSelectCount;
                    return true;
                }
                break;
            case BuffType.AttackUpPerSent:
                _atkPersent += buffData.Value;
                break;
        }
        return false;
    }
}
