using System;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : SingletonMonoBehaviour<GameStateManager>
{
    [SerializeField] private GameStateType _stateType;
    public GameStateType StateType => _stateType;
    private event Action<GameStateType> _onGameStateChanged;
    private List<Action<GameStateType>> _onGameStateChangedEvents = new List<Action<GameStateType>>();
    protected override void Awake()
    {
        if (CheckInstance())
        {
            DontDestroyOnLoad(gameObject);
        }
    }
    /// <summary>
    /// ステートの変更
    /// </summary>
    /// <param name="newStateType"></param>
    public void SetGameState(GameStateType newStateType)
    {
        if (_stateType == newStateType) return;
        _stateType = newStateType;
        if (_onGameStateChanged == null) return;
        _onGameStateChanged.Invoke(_stateType);
    }
    /// <summary>
    /// ステート変更時のイベント登録
    /// </summary>
    /// <param name="setEvent"></param>
    public void SetStateChangedAction(Action<GameStateType> setEvent)
    {
        _onGameStateChanged += setEvent;
        _onGameStateChangedEvents.Add(setEvent);
    }
    /// <summary>
    /// イベント解除
    /// </summary>
    /// <param name="setEvent"></param>
    public void RemoveStateChangedAction(Action<GameStateType> setEvent)
    {
        _onGameStateChanged -= setEvent;
        _onGameStateChangedEvents.Remove(setEvent);
    }
    private void OnDestroy()
    {
        foreach (var onGameStateChangedEvent in _onGameStateChangedEvents)
        {
            _onGameStateChanged -= onGameStateChangedEvent;
        }
        _onGameStateChangedEvents.Clear();
    }
}
