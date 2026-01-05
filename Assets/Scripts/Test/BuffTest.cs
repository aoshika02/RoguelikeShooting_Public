using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffTest : SingletonMonoBehaviour<BuffTest>
{
    private BuffManager _buffManager;
    private BuffViewManager _viewManager;
    private BuffSlotObj _slotObj;
    void Start()
    {
        _buffManager = BuffManager.Instance;
        _viewManager = BuffViewManager.Instance;
        _slotObj = null;
        BuffFlow().Forget();
    }

    private async UniTask BuffFlow()
    {
        _buffManager.Init();
        var buffDatas = _buffManager.GetBuffTypes();
        await _viewManager.BuffViewAsync(buffDatas);
        await UniTask.WaitUntil(() => _slotObj != null);
        Debug.Log("Call");
        await _viewManager.BuffHideAsync(_slotObj);
    }
    public void SetSlotObj(BuffSlotObj slotObj) => _slotObj = slotObj;
}
