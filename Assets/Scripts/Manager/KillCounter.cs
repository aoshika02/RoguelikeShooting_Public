public class KillCounter : SingletonMonoBehaviour<KillCounter>
{
    private int _killCount = 0;
    public void Init() 
    {
        _killCount = 0;
    }
    public void AddKillCount() 
    {
        _killCount++;
    }
    public int GetKillCount()
    {
        return _killCount;
    }
}
