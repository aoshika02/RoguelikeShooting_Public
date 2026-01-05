using System.Collections.Generic;
using UnityEngine;

public class DecalPool : SingletonMonoBehaviour<DecalPool>
{
    [SerializeField] private GameObject _bulletDecalPrefab;
    private GenericObjectPool<BulletDecal> _bulletDecalPool;
    //private List<BulletDecal> _bulletDecals = new List<BulletDecal>();
    //private int _maxCount = 5;
    //private int _useCount = 0;
    protected override void Awake()
    {
        if (CheckInstance() == false) return;
        _bulletDecalPool = new GenericObjectPool<BulletDecal>(_bulletDecalPrefab, transform);
    }
    //public void SetMaxCount(int maxCount) => _maxCount = maxCount;
    public BulletDecal GetBulletDecal()
    {
        //BulletDecal bulletDecal = null;
        //if (_maxCount <= _useCount)
        //{
        //    bulletDecal = _bulletDecals[0];
        //    _bulletDecals.RemoveAt(0);
        //}
        //else
        //{
        //    _useCount++;
        //    bulletDecal = _bulletDecalPool.Get();
        //}
        //_bulletDecals.Add(bulletDecal);
        //return bulletDecal;
        return _bulletDecalPool.Get();
    }
    public void ReleaseBulletDecal(BulletDecal bulletDecal)
    {
        //_useCount--;
        //_bulletDecals.Remove(bulletDecal);
        _bulletDecalPool.Release(bulletDecal);
    }
}
