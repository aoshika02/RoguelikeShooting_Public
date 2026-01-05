using System.Collections.Generic;

public class StageTypeRandomizer : SingletonMonoBehaviour<StageTypeRandomizer>
{
    private Queue<StageType> _stageTypeQueue = new Queue<StageType>();
    private List<StageType> _allStageTypes = new List<StageType>
        {
        StageType.StageA ,
        StageType.StageB_01,
        StageType.StageB_02,
        StageType.StageB_03,
        StageType.StageB_04,
        StageType.StageC,
    };
    protected override void Awake()
    {
        if (CheckInstance() == false) return;
    }
    public StageType GetStageType(int repeatCount = 2)
    {
        return RandomTypeBase.GetRandomType<StageType>(_stageTypeQueue, _allStageTypes, repeatCount);
    }
}
