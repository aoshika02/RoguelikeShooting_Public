using UnityEngine;

public abstract class SingletonMonoBehaviour<T> :MonoBehaviour where T : MonoBehaviour    
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                var t = typeof(T);

                _instance = FindObjectOfType(t) as T;
                if (_instance == null)
                {
                    Debug.LogError($"{t}をアタッチしてある{nameof(gameObject)}がありません");
                }
            }
            return _instance;
        }
    }
    virtual protected void Awake()
    {
        CheckInstance();
    }
    protected bool CheckInstance()
    {
        if (_instance == null)
        {
            _instance = this as T;
            return true;
        }
        else if (Instance == this)
        {
            return true;
        }
        Destroy(this);
        return false;
    }
}
