using UnityEngine;
[RequireComponent(typeof(BoxCollider))]
public class CallExit : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(ParamConsts.PLAYER)) 
        {
            GameManager.Instance.SetExit();
        }
    }
}
