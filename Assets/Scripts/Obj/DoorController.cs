using UnityEngine;

public class DoorController : MonoBehaviour
{
    [SerializeField] private Animator _doorAnimator;
    public void SetAnim(bool isOpen)
    {
        if(_doorAnimator == null) _doorAnimator = GetComponent<Animator>();
        _doorAnimator.SetBool(ParamConsts.OPEN, isOpen);
    }
}
