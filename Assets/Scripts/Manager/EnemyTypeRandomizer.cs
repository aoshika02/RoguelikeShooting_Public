using System.Collections.Generic;

public class EnemyTypeRandomizer : SingletonMonoBehaviour<EnemyTypeRandomizer>
{
    private Queue<EnemyType> _enemyTypeQueue = new Queue<EnemyType>();
    private List<EnemyType> _allEnemyTypes = new List<EnemyType>
    {
        EnemyType.Stone,
        EnemyType.Golem,
        EnemyType.Robot,
    };
    protected override void Awake()
    {
        if (CheckInstance() == false) return;
    }
    public List<EnemyType> GetEnemyTypes(int floorCount, int count, int repeatCount = 2)
    {
        if (floorCount == 0)
        {
            return GetEnemyTypes(count, EnemyType.Grunt);
        }
        if (floorCount == 1)
        {
            return GetEnemyTypes(count, EnemyType.Golem);
        }
        if (floorCount == 2)
        {
            return GetEnemyTypes(count, EnemyType.Stone);
        }
        if (floorCount == 3)
        {
            return GetEnemyTypes(count, EnemyType.Robot);
        }
        if (floorCount % 10 == 0)
        {
           // return GetEnemyTypes(count, EnemyType.Dragon);
        }
        return GetEnemyTypes(count, (floorCount % 3 == 0), repeatCount);
    }
    public List<EnemyType> GetEnemyTypes(int count, bool isMix = false, int repeatCount = 2)
    {
        return RandomTypeBase.GetRandomType<EnemyType>(count, _enemyTypeQueue, isMix, _allEnemyTypes, repeatCount);
    }
    public List<EnemyType> GetEnemyTypes(int count, EnemyType enemyType)
    {
        List<EnemyType> returnlist = new List<EnemyType>();
        for (int i = 0; i < count; i++)
        {
            returnlist.Add(enemyType);
        }
        return returnlist;
    }
}
