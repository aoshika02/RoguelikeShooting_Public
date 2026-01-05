using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySEHandler : MonoBehaviour
{
    [SerializeField] private SEType _seType;
    private SoundManager _soundManager;
    private void Start()
    {
        _soundManager = SoundManager.Instance;
    }
    public void PlaySE()
    {
        _soundManager.PlaySE(_seType);
    }
}
