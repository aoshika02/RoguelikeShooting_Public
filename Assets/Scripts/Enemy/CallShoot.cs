using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CallShoot : MonoBehaviour
{
    [SerializeField] private EnemyObj _enemyObj;
    public void Shoot() 
    {
        _enemyObj.Shoot();
    }
}
