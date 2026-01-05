using UnityEngine;

public class EnemyAttackCall : MonoBehaviour
{
    [SerializeField] private EnemyObj _enemyObj;
    public void SetEnemyObj(EnemyObj enemyObj) => _enemyObj = enemyObj;
    public EnemyObj GetEnemyObj() => _enemyObj;
}
