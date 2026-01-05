using System;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "EnemyStatesDatas", menuName = "ScriptableObject/EnemyStatesDatas", order = 0)]
public class EnemyStatesDatas : ScriptableObject
{
    public List<EnemyStatesData> EnemyStatesDataSet = new List<EnemyStatesData>();
}
[Serializable]
public class EnemyStatesData
{
    public EnemyType EnemyType;
    public List<EnemyStateType> PatrolStateTypes;
    public List<EnemyStateType> ChaseStateTypes;
}
