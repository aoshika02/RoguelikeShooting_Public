using UnityEngine;

public class CallEnter : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(ParamConsts.PLAYER))
        {
            GameManager.Instance.SetEnter();
        }
    }
}
