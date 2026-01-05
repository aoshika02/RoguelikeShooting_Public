public class DamageCounter : SingletonMonoBehaviour<DamageCounter>
{
    private int _damageValue = 0;
    public void Init() 
    {
        _damageValue = 0;        
    }
    public void AddDamage(int damage) 
    {
        _damageValue += damage;
    }
    public int GetDamage() 
    {
        return _damageValue;
    }
}
