using System;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "EnemyParamDatas", menuName = "ScriptableObject/EnemyParamDatas", order = 0)]
public class EnemyParamDatas : ScriptableObject
{
    public List<EnemyParam> EnemyParamSet = new List<EnemyParam>();
}
[Serializable]
public class EnemyParam
{
    public EnemyType EnemyType;
    public int Hp;
    public float Atk;
    public int Def;
    public float MoveSpeed;
    public float RunSpeed;
    public float MoveThreshold;
    public float ChaseThreshold;
    public float WaitTime;
}
