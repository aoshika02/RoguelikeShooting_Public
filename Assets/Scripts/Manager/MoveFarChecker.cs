using UnityEngine;

public class MoveFarChecker : SingletonMonoBehaviour<MoveFarChecker>
{
    private float _targetFar = 15f;
    private float _targetDis = 35f;
    private float _movedDistance;
    private Vector3 _startPos;
    private Vector3 _lastPos;
    private bool _isCheck = false;
    private PlayerMove _playerMove;
    public bool IsDisCompleted { get; private set; }
    public bool IsFarCompleted { get; private set; }

    void Start()
    {
        _playerMove = PlayerMove.Instance;
        _isCheck = false;
        _startPos = _playerMove.transform.position;
        _lastPos = _playerMove.transform.position;
    }
    public void Init(Vector3 start)
    {
        _movedDistance = 0;
        _startPos = start;
        IsDisCompleted = false;
        IsFarCompleted = false;
    }
    public void SetCheck(bool isCheck)
    {
        _isCheck = isCheck;
    }
    void Update()
    {
        if (_isCheck == false) return;
        // フレームごとに移動量を加算（FPS 非依存）
        float distance = Vector3.Distance(_playerMove.transform.position, _lastPos);
        _movedDistance += distance;
        _lastPos = _playerMove.transform.position;
        if (_movedDistance > _targetDis)
        {
            IsDisCompleted = true;
        }

        float dist = Vector3.Distance(_playerMove.transform.position, _startPos);
        if (dist >= _targetFar)
        {
            IsFarCompleted = true;
        }
        if (IsDisCompleted == true && IsFarCompleted == true) 
        {
            _isCheck = false;
        }
    }
}
